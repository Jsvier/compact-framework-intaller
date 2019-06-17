using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace ONCFInstall
{
  static class Utility
  {
    public static void Output(string text)
    {
      Console.WriteLine(text);
      Debug.WriteLine(text);
    }
  }
}
