// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.RedirectionHelper
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

using System;
using System.Reflection;

namespace Boformer.Redirection
{
  public static class RedirectionHelper
  {
    public static RedirectCallsState RedirectCalls(MethodInfo from, MethodInfo to)
    {
      return RedirectionHelper.PatchJumpTo(from.MethodHandle.GetFunctionPointer(), to.MethodHandle.GetFunctionPointer());
    }

    public static RedirectCallsState RedirectCalls(
      RuntimeMethodHandle from,
      RuntimeMethodHandle to)
    {
      return RedirectionHelper.PatchJumpTo(from.GetFunctionPointer(), to.GetFunctionPointer());
    }

    public static void RevertRedirect(MethodInfo from, RedirectCallsState state)
    {
      RedirectionHelper.RevertJumpTo(from.MethodHandle.GetFunctionPointer(), state);
    }

    public static unsafe RedirectCallsState PatchJumpTo(
      IntPtr site,
      IntPtr target)
    {
      RedirectCallsState redirectCallsState = new RedirectCallsState();
      byte* pointer = (byte*) site.ToPointer();
      redirectCallsState.a = *pointer;
      redirectCallsState.b = pointer[1];
      redirectCallsState.c = pointer[10];
      redirectCallsState.d = pointer[11];
      redirectCallsState.e = pointer[12];
      redirectCallsState.f = (ulong) *(long*) (pointer + 2);
      *pointer = (byte) 73;
      pointer[1] = (byte) 187;
      *(long*) (pointer + 2) = target.ToInt64();
      pointer[10] = (byte) 65;
      pointer[11] = byte.MaxValue;
      pointer[12] = (byte) 227;
      return redirectCallsState;
    }

    public static unsafe void RevertJumpTo(IntPtr site, RedirectCallsState state)
    {
      byte* pointer = (byte*) site.ToPointer();
      *pointer = state.a;
      pointer[1] = state.b;
      *(long*) (pointer + 2) = (long) state.f;
      pointer[10] = state.c;
      pointer[11] = state.d;
      pointer[12] = state.e;
    }
  }
}
