using System;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using OpenNETCF.Compression.CAB;

namespace System.Runtime.CompilerServices
{
  public class ExtensionAttribute : Attribute
  {
  }
}

namespace ONCFInstall
{
  public delegate void FileProgressHandler(int progressPercent);

  public static class Extensions
  {
    public static string Find(this List<string> list, string findString)
    {
      foreach (string file in list)
      {
        if (string.Compare(file, findString, true) == 0)
        {
          return file;
        }
      }
      return null;
    }
  }

  public class CustomCABInstaller : WinCEInstallerFile
  {
    private int m_fileCount = 0;
    private CommandLineArgs m_args;

    public event FileProgressHandler FileProgress;

    public CustomCABInstaller(string cabFileName, CommandLineArgs args)
      : base(cabFileName)
    {
      m_args = args;
      SkipFileNames = m_args.SkipFiles ?? new List<string>();
      PathStringReplacements = m_args.PathStringReplacements ?? new Dictionary<string, string>();
      SkipOSVersionCheck = m_args.SkipOSVersionCheck;
    }

    /// <summary>
    /// List of file names to skip during installation
    /// </summary>
    public List<string> SkipFileNames { get; set; }

    /// <summary>
    /// List of path replacement strings
    /// </summary>
    public Dictionary<string, string> PathStringReplacements { get; set; }

    /// <summary>
    /// If <b>true</b>, the installer will not check to ensure the target meets the installer's version requirements
    /// </summary>
    public bool SkipOSVersionCheck { get; set; }

    /// <summary>
    /// If <b>true</b>, the installer will not check to ensure the target shotcup the installer's version requirements
    /// </summary>
    public bool Shortcut { get; set; }

    public override void OnInstallBegin()
    {
      m_fileCount = 0;
    }

    public override void OnTargetOSVersionCheck()
    {
      // check to see if we should skip the OS version check
      if (!SkipOSVersionCheck)
      {
        base.OnTargetOSVersionCheck();
      }
    }

    public override void OnInstallFile(ref FileInstallInfo fileInfo, out bool skipped)
    {
      // check to see if it's a name we should skip
      if (SkipFileNames.Find(fileInfo.FileName) != null)
      {
        Utility.Output(string.Format("Skipping file '{0}'", fileInfo.FileName));
        skipped = true;
        return;
      }

      // do any path replacements
      foreach (KeyValuePair<string, string> val in PathStringReplacements)
      {
        fileInfo.DestinationFolder = fileInfo.DestinationFolder.Replace(val.Key, val.Value);
      }

      Utility.Output(string.Format("Installing '{0}' to '{1}'", fileInfo.FileName, fileInfo.DestinationFolder));

      base.OnInstallFile(ref fileInfo, out skipped);

      if (FileProgress != null)
      {
        FileProgress((++m_fileCount * 100) / FileCount);
      }
    }
  }
}
