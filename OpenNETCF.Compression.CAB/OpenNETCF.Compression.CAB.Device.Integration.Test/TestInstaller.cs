using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  internal class TestInstaller : WinCEInstallerFile
  {
    private TestData m_testData;

    public TestInstaller(string cabFileName, TestData data)
      : base(cabFileName)
    {
      m_testData = data;
    }

    public override void OnInstallFile(ref FileInstallInfo fileInfo, out bool skipped)
    {
      // check to see that the file is in the expected list
      InstalledFileInfo info = new InstalledFileInfo(fileInfo);

      bool found = false;
      foreach (InstalledFileInfo searchFor in m_testData.InstalledFiles)
      {
        // TODO: for now ignore date and flags, but these need to be validated in a future test
        if (info.CompareTo(searchFor, true, true) == 0)
        {
          found = true;
          // mark it as installed - we'll look at these later in the test to make sure everything was installed
          searchFor.Installed = true;
          break;
        }
      }

      if (!found)
      {
        Assert.Fail(string.Format("File '{0}' or one of its properties was unexpected", fileInfo.FileName));
      }

      // lie about us skipping the file so the underlying installer doesn't know
      skipped = false;

      // don't call the base - we're just testing
    }

    public override void OnInstallRegValue(ref RegistryInstallInfo registryInfo)
    {
      // check to see that the file is in the expected list
      InstalledRegInfo info = new InstalledRegInfo(registryInfo.KeyName, registryInfo.ValueName, registryInfo.ValueKind);

      bool found = false;
      foreach (InstalledRegInfo searchFor in m_testData.InstalledRegKeys)
      {
        // TODO: for now ignore date and flags, but these need to be validated in a future test
        if (info.CompareTo(searchFor, true, true) == 0)
        {
          found = true;
          // mark it as installed - we'll look at these later in the test to make sure everything was installed
          searchFor.Installed = true;
          break;
        }
      }

      if (!found)
      {
        Assert.Fail(string.Format("Value '{0}' or one of its properties was unexpected", registryInfo.ValueName));
      }

      // don't call the base - we're just testing
    }

    public override void OnInstallShortcut(ref ShortcutInstallInfo shortcutInfo)
    {
      // check to see that the file is in the expected list
      InstalledShortcutInfo info = new InstalledShortcutInfo(shortcutInfo.ShortcutLocation, shortcutInfo.ShortcutName, shortcutInfo.TargetLocation);

      bool found = false;
      foreach (InstalledShortcutInfo searchFor in m_testData.InstalledShortcuts)
      {
        // TODO: for now ignore date and flags, but these need to be validated in a future test
        if (info.CompareTo(searchFor) == 0)
        {
          found = true;
          // mark it as installed - we'll look at these later in the test to make sure everything was installed
          searchFor.Installed = true;
          break;
        }
      }

      if (!found)
      {
        Assert.Fail(string.Format("Shortcut '{0}' or one of its properties was unexpected", shortcutInfo.ShortcutName));
      }

      // don't call the base - we're just testing
    }

    public override bool OnRunInstallerDllInit()
    {
      // TODO:
      // don't call the base - we're just testing

      return true;
    }

    public override bool OnRunInstallerDllExit()
    {
      // TODO:
      // don't call the base - we're just testing
      return true;
    }

    public override void OnTargetArchitectureCheck()
    {
      // TODO:
      // don't call the base - we're just testing
    }

    public override void OnTargetOSVersionCheck()
    {
      // TODO:
      // don't call the base - we're just testing
    }

    public override void RegisterInstallation()
    {
      // TODO:
      // don't call the base - we're just testing
    }
  }
}
