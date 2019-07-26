// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.RedirectionUtil
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Boformer.Redirection
{
  public static class RedirectionUtil
  {
    private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags RedirectorFieldFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public static Dictionary<MethodInfo, Redirector> RedirectType(
      System.Type type,
      bool onCreated = false)
    {
      Dictionary<MethodInfo, Redirector> redirects = new Dictionary<MethodInfo, Redirector>();
      object[] customAttributes = type.GetCustomAttributes(typeof (TargetTypeAttribute), false);
      if (customAttributes.Length != 1)
        return (Dictionary<MethodInfo, Redirector>) null;
      System.Type type1 = ((TargetTypeAttribute) customAttributes[0]).Type;
      RedirectionUtil.RedirectMethods(type, type1, redirects, onCreated);
      RedirectionUtil.RedirectReverse(type, type1, redirects, onCreated);
      return redirects;
    }

    private static void RedirectMethods(
      System.Type type,
      System.Type targetType,
      Dictionary<MethodInfo, Redirector> redirects,
      bool onCreated)
    {
      foreach (MethodInfo method in ((IEnumerable<MethodInfo>) type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).Where<MethodInfo>((Func<MethodInfo, bool>) (method =>
      {
        object[] customAttributes = method.GetCustomAttributes(typeof (RedirectMethodAttribute), false);
        if (customAttributes.Length != 1)
          return false;
        return ((RedirectMethodAttribute) customAttributes[0]).OnCreated == onCreated;
      })))
      {
        Debug.Log((object) ("Redirecting " + targetType.Name + "#" + method.Name + "..."));
        Redirector redirector = RedirectionUtil.RedirectMethod(targetType, method, redirects, false);
        FieldInfo field = type.GetField(method.Name + "Redirector", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof (Redirector))
        {
          Debug.Log((object) "Redirector field found!");
          field.SetValue((object) null, (object) redirector);
        }
      }
    }

    private static void RedirectReverse(
      System.Type type,
      System.Type targetType,
      Dictionary<MethodInfo, Redirector> redirects,
      bool onCreated)
    {
      foreach (MethodInfo method in ((IEnumerable<MethodInfo>) type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).Where<MethodInfo>((Func<MethodInfo, bool>) (method =>
      {
        object[] customAttributes = method.GetCustomAttributes(typeof (RedirectReverseAttribute), false);
        if (customAttributes.Length == 1)
          return ((RedirectReverseAttribute) customAttributes[0]).OnCreated == onCreated;
        return false;
      })))
      {
        Debug.Log((object) ("Redirecting reverse " + targetType.Name + "#" + method.Name + "..."));
        RedirectionUtil.RedirectMethod(targetType, method, redirects, true);
      }
    }

    private static Redirector RedirectMethod(
      System.Type targetType,
      MethodInfo method,
      Dictionary<MethodInfo, Redirector> redirects,
      bool reverse = false)
    {
      Tuple<MethodInfo, Redirector> tuple = RedirectionUtil.RedirectMethod(targetType, method, reverse);
      redirects.Add(tuple.First, tuple.Second);
      return tuple.Second;
    }

    private static Tuple<MethodInfo, Redirector> RedirectMethod(
      System.Type targetType,
      MethodInfo detour,
      bool reverse)
    {
      ParameterInfo[] parameters = detour.GetParameters();
      System.Type[] types = parameters.Length == 0 || (targetType.IsValueType || parameters[0].ParameterType != targetType) && (!targetType.IsValueType || parameters[0].ParameterType != targetType.MakeByRefType()) ? ((IEnumerable<ParameterInfo>) parameters).Select<ParameterInfo, System.Type>((Func<ParameterInfo, System.Type>) (p => p.ParameterType)).ToArray<System.Type>() : ((IEnumerable<ParameterInfo>) parameters).Skip<ParameterInfo>(1).Select<ParameterInfo, System.Type>((Func<ParameterInfo, System.Type>) (p => p.ParameterType)).ToArray<System.Type>();
      MethodInfo method = targetType.GetMethod(detour.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, types, (ParameterModifier[]) null);
      if (method == null && detour.Name.EndsWith("Alt"))
        method = targetType.GetMethod(detour.Name.Substring(0, detour.Name.Length - 3), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, types, (ParameterModifier[]) null);
      Redirector second = reverse ? new Redirector(detour, method) : new Redirector(method, detour);
      second.Apply();
      return Tuple.New<MethodInfo, Redirector>(method, second);
    }
  }
}
