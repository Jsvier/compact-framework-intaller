using System;

using System.Collections.Generic;
using System.Text;

namespace ONCFInstall
{
  public class CommandLineArgs
  {
    public CommandLineArgs()
    {
      SkipOSVersionCheck = false;
      SkipFiles = new List<string>();
      PathStringReplacements = new Dictionary<string, string>();
    }

    public bool SkipOSVersionCheck { get; set; }
    public List<string> SkipFiles { get; set; }
    public Dictionary<string, string> PathStringReplacements { get; set; }
  }
}
