using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  internal class InstalledRegInfo : RegistryInstallInfo
  {
    public InstalledRegInfo(string keyName, string valueName, RegistryValueKind valueKind)
      : base(keyName, valueName, valueKind)
    {
    }

    public bool Installed { get; set; }

    public int CompareTo(InstalledRegInfo info, bool ignoreNoClobber, bool ignoreData)
    {
      int result = string.Compare(this.KeyName, info.KeyName, true);
      if (result != 0) return result;

      result = string.Compare(this.ValueName, info.ValueName, true);
      if (result != 0) return result;

      result = this.ValueKind.CompareTo(info.ValueKind);
      if (result != 0) return result;

      if (!ignoreData)
      {
        // todo:
        throw new NotSupportedException();
      }

      if (!ignoreNoClobber)
      {
        result = this.NoClobber.CompareTo(info.NoClobber);
        if (result != 0) return result;
      }

      return 0;
    }
  }
}
