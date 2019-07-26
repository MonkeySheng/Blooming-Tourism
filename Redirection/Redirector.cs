// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.Redirector
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

using System;
using System.Reflection;

namespace Boformer.Redirection
{
  public class Redirector
  {
    private RedirectCallsState state;
    private readonly IntPtr site;
    private readonly IntPtr target;

    public Redirector(MethodInfo from, MethodInfo to)
    {
      this.site = from.MethodHandle.GetFunctionPointer();
      this.target = to.MethodHandle.GetFunctionPointer();
    }

    public void Apply()
    {
      if (this.Deployed)
        return;
      this.state = RedirectionHelper.PatchJumpTo(this.site, this.target);
      this.Deployed = true;
    }

    public void Revert()
    {
      if (!this.Deployed)
        return;
      RedirectionHelper.RevertJumpTo(this.site, this.state);
      this.Deployed = false;
    }

    public bool Deployed { get; private set; }
  }
}
