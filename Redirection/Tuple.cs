// Decompiled with JetBrains decompiler
// Type: Boformer.Redirection.Tuple
// Assembly: WG_CitizenEdit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8BC69FF0-89F1-47E1-8598-58845EA70EFD
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\255710\654707599\WG_CitizenEdit.dll

namespace Boformer.Redirection
{
  public static class Tuple
  {
    public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
    {
      return new Tuple<T1, T2>(first, second);
    }
  }
}
