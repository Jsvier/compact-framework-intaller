using System;

using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  internal class InstalledShortcutInfo : ShortcutInstallInfo
  {
    public InstalledShortcutInfo(string shortcutLocation, string shorcutName, string targetLocation)
      : base(shortcutLocation, shorcutName, targetLocation)
    {
    }

    public bool Installed { get; set; }

    public int CompareTo(InstalledShortcutInfo info)
    {
      int result = string.Compare(this.ShortcutName, info.ShortcutName, true);
      if (result != 0) return result;

      result = string.Compare(this.ShortcutLocation, info.ShortcutLocation, true);
      if (result != 0) return result;

      result = string.Compare(this.TargetLocation, info.TargetLocation, true);
      if (result != 0) return result;

      return 0;
    }
  }
}
