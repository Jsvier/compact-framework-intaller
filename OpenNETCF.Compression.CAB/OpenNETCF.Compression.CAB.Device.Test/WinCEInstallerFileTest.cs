using OpenNETCF.Compression.CAB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OpenNETCF.Compression.CAB.Device.Test
{
  [TestClass()]
  public class WinCEInstallerFileTest
  {
    public TestContext TestContext { get; set; }

    [TestMethod()]
    public void ConstructorTestNullPathTest()
    {
      ArgumentNullException expected = null;

      try
      {
        WinCEInstallerFile file = new WinCEInstallerFile(null);
      }
      catch (ArgumentNullException ex)
      {
        expected = ex;
      }

      Assert.IsNotNull(expected);
    }

    [TestMethod()]
    public void ConstructorTestEmptyPathTest()
    {
      ArgumentException expected = null;

      try
      {
        WinCEInstallerFile file = new WinCEInstallerFile(string.Empty);
      }
      catch (ArgumentException ex)
      {
        expected = ex;
      }

      Assert.IsNotNull(expected);
    }

    [TestMethod()]
    public void ConstructorTestBadPathTest()
    {
      FileNotFoundException expected = null;

      try
      {
        WinCEInstallerFile file = new WinCEInstallerFile("\\Windows\\NoExistentfile.cab");
      }
      catch (FileNotFoundException ex)
      {
        expected = ex;
      }

      Assert.IsNotNull(expected);
    }

    [TestMethod()]
    public void ConstructorTestInvalidCABTest()
    {
      InvalidArchiveException expected = null;

      string target = Path.Combine(TestContext.TestDeploymentDir, "invalid.cab");
      using (StreamWriter writer = File.CreateText(target))
      {
        writer.Write("This is not really a CAB file");
      }

      try
      {
        WinCEInstallerFile file = new WinCEInstallerFile(target);
      }
      catch (InvalidArchiveException ex)
      {
        expected = ex;
      }

      Assert.IsNotNull(expected);
    }

    [TestMethod()]
    [DeploymentItem("SampleCAB.CAB")]
    public void NOPInstallerPositiveTest()
    {
      string target = Path.Combine(TestContext.TestDeploymentDir, "SampleCAB.CAB");
      NOPInstaller installer = new NOPInstaller(target);
      installer.Install();

      // no exception == pass
    }
  }
}
