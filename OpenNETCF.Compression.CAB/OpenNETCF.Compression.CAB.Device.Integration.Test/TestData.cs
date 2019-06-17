using System;

using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  internal class TestData
  {
    public TestData()
    {
      InstalledFiles = new List<InstalledFileInfo>();
      InstalledRegKeys = new List<InstalledRegInfo>();
      InstalledShortcuts = new List<InstalledShortcutInfo>();
    }

    public List<InstalledFileInfo> InstalledFiles { set; get; }
    public List<InstalledRegInfo> InstalledRegKeys { set; get; }
    public List<InstalledShortcutInfo> InstalledShortcuts { set; get; }
  }
}
