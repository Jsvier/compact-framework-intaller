using OpenNETCF.Compression.CAB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  [TestClass()]
  public class WinCEInstallerFileTest
  {
    public TestContext TestContext { get; set; }

    private void TestInit(TestData expectedData)
    {
      // TODO: add flags
      expectedData.InstalledFiles = new List<InstalledFileInfo>(
        new InstalledFileInfo[] {
          new InstalledFileInfo(@"\Sample Install Dir\Folder", "File1.txt", FileFlags.None),
          new InstalledFileInfo(@"\Sample Install Dir\Folder", "LargeFile.txt", FileFlags.None),
          new InstalledFileInfo(@"\Sample Install Dir\Folder\SubFolder", "File2.xml", FileFlags.None),
        });

      // TODO: add data, flags
      expectedData.InstalledRegKeys = new List<InstalledRegInfo>(
        new InstalledRegInfo[] {
          new InstalledRegInfo(@"HKEY_LOCAL_MACHINE\AppRegKey", "StringVal", RegistryValueKind.String),
          new InstalledRegInfo(@"HKEY_LOCAL_MACHINE\AppRegKey", "DwordVal", RegistryValueKind.DWord),
        });

      expectedData.InstalledShortcuts = new List<InstalledShortcutInfo>(
        new InstalledShortcutInfo[] {
          new InstalledShortcutInfo(@"\Program Files\Games", @"Shortcut Name.lnk", @"\Sample Install Dir\Folder\File1.txt"),
        });
    }
    
    /// <summary>
    /// Note that this test does *NOT* verify that a CAB actually installs properly, it only verifies the extraction of the CAB info itself
    /// Another test should be generated (or this test extended) to verify actual installation
    /// </summary>
    [TestMethod()]
    [DeploymentItem("SampleCAB_C.CAB")]
    public void VerifyCompressedCAB()
    {
      VerifyCAB("samplecab_c.cab");
    }

    /// <summary>
    /// Note that this test does *NOT* verify that a CAB actually installs properly, it only verifies the extraction of the CAB info itself
    /// Another test should be generated (or this test extended) to verify actual installation
    /// </summary>
    [TestMethod()]
    [DeploymentItem("SampleCAB.CAB")]
    public void VerifyUncompressedCAB()
    {
      VerifyCAB("samplecab.cab");
    }

    private void VerifyCAB(string cabName)
    {
      TestData results = new TestData();
      TestInit(results);

      string cabPath = Path.Combine(TestContext.TestDeploymentDir, cabName);
      Assert.IsTrue(File.Exists(cabPath), "CAB not deployed with test");

      TestInstaller installer = new TestInstaller(cabPath, results);
      Assert.IsNotNull(installer);

      // check the file count
      Assert.AreEqual(results.InstalledFiles.Count, installer.FileCount, "CAB File Count is wrong");

      installer.FileFailure += new ErrorDetectedHandler(installer_FileFailure);
      installer.RegistryFailure += new ErrorDetectedHandler(installer_RegistryFailure);
      installer.ShortcutFailure += new ErrorDetectedHandler(installer_ShortcutFailure);

      // start the install
      installer.Install();

      // verify all expected files were actually installed
      foreach (InstalledFileInfo info in results.InstalledFiles)
      {
        Assert.IsTrue(info.Installed, string.Format("File '{0}' was not installed as expected", info.FileName));
      }

      // verify all expected reg keys were actually installed
      foreach (InstalledRegInfo info in results.InstalledRegKeys)
      {
        Assert.IsTrue(info.Installed, string.Format("Key '{0}' was not installed as expected", info.ValueName));
      }

      // verify all expected shortcuts were actually installed
      foreach (InstalledShortcutInfo info in results.InstalledShortcuts)
      {
        Assert.IsTrue(info.Installed, string.Format("Shortcut '{0}' was not installed as expected", info.ShortcutName));
      }
    }

    void installer_FileFailure(object info, Exception ex, ref bool cancel)
    {
      Assert.Fail(string.Format("File '{0}' failed to install: {1}", ((FileInstallInfo)info).FileName, ex.Message));
    }

    void installer_RegistryFailure(object info, Exception ex, ref bool cancel)
    {
      Assert.Fail(string.Format("Key '{0}' failed to install: {1}", ((RegistryInstallInfo)info).ValueName, ex.Message));
    }

    void installer_ShortcutFailure(object info, Exception ex, ref bool cancel)
    {
      Assert.Fail(string.Format("Shortcut '{0}' failed to install: {1}", ((ShortcutInstallInfo)info).ShortcutName, ex.Message));
    }
  }
}
