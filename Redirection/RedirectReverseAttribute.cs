// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.RedirectReverseAttribute
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

using System;

namespace Boformer.Redirection
{
  [AttributeUsage(AttributeTargets.Method)]
  internal class RedirectReverseAttribute : Attribute
  {
    public RedirectReverseAttribute()
    {
      this.OnCreated = false;
    }

    public RedirectReverseAttribute(bool onCreated)
    {
      this.OnCreated = onCreated;
    }

    public bool OnCreated { get; }
  }
}
