// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.Tuple`2
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

namespace Boformer.Redirection
{
  public class Tuple<T1, T2>
  {
    public T1 First { get; private set; }

    public T2 Second { get; private set; }

    internal Tuple(T1 first, T2 second)
    {
      this.First = first;
      this.Second = second;
    }
  }
}
