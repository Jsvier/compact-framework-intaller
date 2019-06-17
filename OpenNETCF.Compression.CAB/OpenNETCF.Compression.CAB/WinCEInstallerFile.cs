// Copyright (c) 2008-2012 OpenNETCF Consulting, LLC
// http://www.opennetcf.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to do 
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Compression.CAB;
using System.Runtime.InteropServices;
using SI = System.IO;
using Microsoft.Win32;

namespace OpenNETCF.Compression.CAB
{
  public delegate void ErrorDetectedHandler(object info, Exception ex, ref bool cancel);

  /// <summary>
  /// Encapsulates the functionalities of a Windows CE installer (CAB) file
  /// </summary>
  [CLSCompliant(true)]
  public class WinCEInstallerFile : IDisposable
  {
    private Archive m_cab;
    private string m_tempFolder;
    InstallerDescriptor m_descriptor;
    private string m_appNameOverride;
    private string m_installDirOverride;

    private short m_failedFiles = 0;
    private short m_failedRegVals = 0;
    private short m_failedShortcuts = 0;
    private object m_fileSyncRoot = new object();

    public event ErrorDetectedHandler ShortcutFailure;
    public event ErrorDetectedHandler RegistryFailure;
    public event ErrorDetectedHandler FileFailure;

    /// <summary>
    /// Creates a WinCEInstallerFile class instance
    /// </summary>
    /// <param name="cabFileName"></param>
    public WinCEInstallerFile(string cabFileName)
    {
      if (cabFileName == null) throw new ArgumentNullException("cabFileName");
      if (cabFileName == string.Empty) throw new ArgumentException("cabFileName cannot be empty");

      // see if the file exists
      if (!System.IO.File.Exists(cabFileName)) throw new System.IO.FileNotFoundException();

      m_cab = new Archive(cabFileName);

      // verify it's a CE cab
      FileInfo[] files = m_cab.ContainedFiles.Filter("*.000");
      if (files.Length != 1) throw new ArgumentException("Source file is not a CE installation CAB");

      // current implementation will be to extract the CAB to a temp folder, the parse it from there
      // TODO: would be more efficient to do it all in-place in RAM, but also more work.  Update it in a future version

      Guid g = Guid.NewGuid();

      // create a temp folder name
      if (Environment.OSVersion.Platform == PlatformID.WinCE)
      {
        TempRoot = "\\Windows\\Temp";
      }
      else
      {
        TempRoot = "c:\\Windows\\Temp";
      }

      m_tempFolder = System.IO.Path.Combine(TempRoot, g.ToString());
      System.IO.Directory.CreateDirectory(m_tempFolder);

      // extract the CAB to the temp folder
      m_cab.Extract(m_tempFolder);

      // open the cab contensts as a CE installer
      m_descriptor = new InstallerDescriptor(m_tempFolder);
    }

    /// <summary>
    /// The root location where files will be temporarily extracted before final placement
    /// </summary>
    public string TempRoot { get; set; }

    /// <summary>
    /// Gets the total number of files defined in the installer
    /// </summary>
    public int FileCount
    {
      get { return m_descriptor.Files.Length; }
    }

    /// <summary>
    /// Gets the total number of registry values defined in the installer
    /// </summary>
    public int RegValueCount
    {
      get { return m_descriptor.RegistryKeys.Length; }
    }

    /// <summary>
    /// Gets the total number of shortcuts defined in the installer
    /// </summary>
    public int ShortcutCount
    {
      get { return m_descriptor.Shortcuts.Length; }
    }

    /// <summary>
    /// Gets a list of all directories define in the installer
    /// </summary>
    /// <returns></returns>
    public string[] GetDirectories()
    {
      List<string> names = new List<string>();

      foreach (InstallerDirectory dir in m_descriptor.Directories)
      {
        names.Add(dir.GetString(m_descriptor.Strings));
      }

      return names.ToArray();
    }

    #region --- Properties ---
    /// <summary>
    /// Gets or sets the name of the application
    /// </summary>
    public string ApplicationName
    {
      get { return (m_appNameOverride == null) ? m_descriptor.AppName : m_appNameOverride; }
      set { m_appNameOverride = value; }
    }

    /// <summary>
    /// Gets the name of the application publisher
    /// </summary>
    public string ApplicationPublisher
    {
      get { return m_descriptor.Provider; }
    }

    /// <summary>
    /// Gets or sets the application install directory
    /// </summary>
    public string InstallDirectory
    {
      get { return (m_installDirOverride == null) ? m_descriptor.InstallDir : m_installDirOverride; }
      set { m_installDirOverride = value; }
    }

    /// <summary>
    /// Gets the installer's defined target processor architecture
    /// </summary>
    public string TargetArchitecture
    {
      get { return m_descriptor.TargetArchitecture.ToString(); }
    }

    /// <summary>
    /// Gets a list of the platforms marked as unsupported by the installer
    /// </summary>
    public string[] UnsupportedPlatforms
    {
      get { return m_descriptor.UnsupportedPlatforms; }
    }

    /// <summary>
    /// Gets the maximum version the installer can be run on
    /// </summary>
    public Version MaximumVersion
    {
      get { return m_descriptor.MaximumVersion; }
    }

    /// <summary>
    /// Gets the minimum version the installer can be run on
    /// </summary>
    public Version MinimumVersion
    {
      get { return m_descriptor.MinimumVersion; }
    }

    /// <summary>
    /// Gets the maximum build the installer can be run on
    /// </summary>    
    public UInt32 MaximumBuild
    {
      get { return m_descriptor.MaxBuild; }
    }
    #endregion

    /// <summary>
    /// Installs the Installer File contents to the local device
    /// </summary>
    public void Install()
    {
      OnInstallBegin();

      OnOSCheck();

      //OnTargetOSVersionCheck();

      OnTargetArchitectureCheck();

      if (!OnRunInstallerDllInit()) return;

      InstallFiles();
      InstallRegValues();
      InstallShortcuts();
      if (!OnRunInstallerDllExit()) return;

      RegisterInstallation();

      OnInstallComplete();
    }

    /// <summary>
    /// Called before any installation steps begin
    /// </summary>
    public virtual void OnInstallBegin()
    {
    }

    /// <summary>
    /// Called after the last installation step is complete
    /// </summary>
    public virtual void OnInstallComplete()
    {
    }

    /// <summary>
    /// Ensures that the target platform's OS version falls within the installer's requirements
    /// </summary>
    public virtual void OnTargetOSVersionCheck()
    {
      if (Environment.OSVersion.Version > this.MaximumVersion) throw new PlatformNotSupportedException("Device version is greater than the installer's defined maximum");

      if (Environment.OSVersion.Version < this.MinimumVersion) throw new PlatformNotSupportedException("Device version is less than the installer's defined minimum");
    }

    /// <summary>
    /// Checks to ensure that the Installer File installation is being run on a Windows CE-based device 
    /// </summary>
    public virtual void OnOSCheck()
    {
      if (Environment.OSVersion.Platform != PlatformID.WinCE)
      {
        throw new InvalidArchiveException("Current Platform is not Windows CE-based");
      }
    }

    /// <summary>
    /// Ensures that the target platform's processor architecture matches the installer's requirements
    /// </summary>
    public virtual void OnTargetArchitectureCheck()
    {
      NativeMethods.SYSTEM_INFO si;
      NativeMethods.GetSystemInfo(out si);

      if (m_descriptor.TargetArchitecture != OpenNETCF.Compression.CAB.TargetArchitecture.unspecified)
      {
        switch (si.dwProcessorType)
        {
          case NativeMethods.ProcessorArchitecture.ARM:
            switch (m_descriptor.TargetArchitecture)
            {
              case OpenNETCF.Compression.CAB.TargetArchitecture.ARM720:
              case OpenNETCF.Compression.CAB.TargetArchitecture.ARM820:
              case OpenNETCF.Compression.CAB.TargetArchitecture.ARM920:
              case OpenNETCF.Compression.CAB.TargetArchitecture.StrongARM:
              case OpenNETCF.Compression.CAB.TargetArchitecture.ARM7TDMI:
                break;
              default:
                throw new PlatformNotSupportedException("Installer file is marked for processor architecture: " + this.TargetArchitecture);
            }
            break;
          case NativeMethods.ProcessorArchitecture.MIPS:
            switch (m_descriptor.TargetArchitecture)
            {
              case OpenNETCF.Compression.CAB.TargetArchitecture.MIPS4000:
                break;
              default:
                throw new PlatformNotSupportedException("Installer file is marked for processor architecture: " + this.TargetArchitecture);
            }
            break;
          case NativeMethods.ProcessorArchitecture.SHx:
            switch (m_descriptor.TargetArchitecture)
            {
              case OpenNETCF.Compression.CAB.TargetArchitecture.SH3:
              case OpenNETCF.Compression.CAB.TargetArchitecture.SH4:
              case OpenNETCF.Compression.CAB.TargetArchitecture.HitachiSH3:
              case OpenNETCF.Compression.CAB.TargetArchitecture.HitachiSH3E:
              case OpenNETCF.Compression.CAB.TargetArchitecture.HitachiSH4:
                break;
              default:
                throw new PlatformNotSupportedException("Installer file is marked for processor architecture: " + this.TargetArchitecture);
            }
            break;
          case NativeMethods.ProcessorArchitecture.x86:
            switch (m_descriptor.TargetArchitecture)
            {
              case OpenNETCF.Compression.CAB.TargetArchitecture.Intel386:
              case OpenNETCF.Compression.CAB.TargetArchitecture.Intel486:
              case OpenNETCF.Compression.CAB.TargetArchitecture.Pentium:
                break;
              default:
                throw new PlatformNotSupportedException("Installer file is marked for processor architecture: " + this.TargetArchitecture);
            }
            break;
        }
      }
    }

    /// <summary>
    /// Generates an INF file based on a CAB file contents
    /// </summary>
    /// <param name="destinationPath"></param>
    public void GenerateINFFile(string destinationPath)
    {
      m_descriptor.GenerateINF(destinationPath);
    }

    /// <summary>
    /// Called after an Installer File extracts all files, shortcuts and registry entries.  This method registers the installation with the device for future uninstallation.
    /// </summary>
    public virtual void RegisterInstallation()
    {
      string isvfile = null;
      string keyname = string.Format("SOFTWARE\\Apps\\{0} {1}", m_descriptor.Provider, m_descriptor.AppName);

      using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyname))
      {
        key.SetValue("Instl", 1, Microsoft.Win32.RegistryValueKind.DWord);
        key.SetValue("CmdFile", string.Format("\\Windows\\AppMgr\\{0} {1}.DAT", m_descriptor.Provider, m_descriptor.AppName), Microsoft.Win32.RegistryValueKind.String);
        key.SetValue("CabFile", m_cab.FileName, Microsoft.Win32.RegistryValueKind.String);
        key.SetValue("InstlDirCnt", 0, Microsoft.Win32.RegistryValueKind.DWord);
        key.SetValue("InstallDir", m_descriptor.InstallDir, Microsoft.Win32.RegistryValueKind.String);
        key.SetValue("InstlDir", m_descriptor.InstallDir, Microsoft.Win32.RegistryValueKind.String);

        // see if there is an installer DLL in the archive
        FileInfo fi = GetFileByExtensionIndex(999);
        if (fi != null)
        {
          // move the setup.dll file to the \Windows\AppMgr folder with the proper name
          isvfile = string.Format("\\Windows\\AppMgr\\{0} {1}.dll", m_descriptor.Provider, m_descriptor.AppName);
          if (SI.File.Exists(isvfile)) SI.File.Delete(isvfile);

          // copy the installer to a known location so we can call it
          string src = System.IO.Path.Combine(m_tempFolder, fi.Name);
          SI.File.Copy(src, isvfile);

          key.SetValue("IsvFile", isvfile, Microsoft.Win32.RegistryValueKind.String);
        }
        key.Close();
      }
      // set the reg entries
    }

    /// <summary>
    /// Calls Install_Init on the archive's setup.dll if it exists, otherwise does nothing
    /// </summary>
    /// <returns><b>true</b> if setup.dll doesn't exist, Install_Init returned CONTINUE or the current platform OS is not Windows CE, otherwise <b>false</b>.</returns>
    /// <remarks>Returning false from this method will abort the installation process</remarks>
    public virtual bool OnRunInstallerDllInit()
    {
      if (Environment.OSVersion.Platform != PlatformID.WinCE) return true;

      // clean up from any previous install
      string dest = "\\Windows\\oncfinstaller.dll";
      //TODO:VER CON MAS TIEMPO
      // if (SI.File.Exists(dest)) SI.File.Delete(dest);

      // see if there is an installer DLL in the archive
      FileInfo fi = GetFileByExtensionIndex(999);
      if (fi == null) return true;

      // copy the installer to a known location so we can call it
      // this is required because the CF won't let us dynamically declare a P/Invoke or get a managed function pointer from a handle
      string src = System.IO.Path.Combine(m_tempFolder, fi.Name);
      SI.File.Copy(src, dest);

      InstallResult state = NativeMethods.Install_Init(IntPtr.Zero, 1, 0, this.InstallDirectory);

      return (state == InstallResult.Continue);
    }

    /// <summary>
    /// Executes Install_Exit in the archive's setup.dll if it exists, otherwise does nothing
    /// </summary>
    /// <returns><b>true</b> if setup.dll doesn't exist or if Install_Exit returned CONTINUE, otherwise <b>false</b>.</returns>
    /// <remarks>Returning false from this method will abort the installation process</remarks>
    public virtual bool OnRunInstallerDllExit()
    {
      if (Environment.OSVersion.Platform != PlatformID.WinCE) return true;

      string dest = "\\Windows\\oncfinstaller.dll";
      if (!SI.File.Exists(dest)) return true;

      // TODO: get the success/fail counts
      InstallResult state = NativeMethods.Install_Exit(IntPtr.Zero, this.InstallDirectory, 0, m_failedFiles, 0, m_failedRegVals, 0);

      return (state == InstallResult.Continue);
    }

    /// <summary>
    /// Installs all shortcuts packaged in the installer to the local device
    /// </summary>
    public void InstallShortcuts()
    {
      m_failedShortcuts = 0;

      if (m_descriptor.Shortcuts == null) return;

      foreach (InstallerLink link in m_descriptor.Shortcuts)
      {
        InstallerFile file = m_descriptor.GetFileByFileID(link.TargetFileID);
        string targetFolder = ReplaceSpecialFolderStrings(string.Copy(m_descriptor.GetDirectoryByID(file.DirectoryID).GetString(m_descriptor.Strings)));
        string targetFile = string.Copy(file.FileName);

        string shortcutName = link.GetString(m_descriptor.Strings);
        string shortcutLocation = ReplaceSpecialFolderStrings(link.ShortcutLocation.ToString());

        ShortcutInstallInfo shortcutInfo = null;

        try
        {
          shortcutInfo = new ShortcutInstallInfo(shortcutLocation, shortcutName, SI.Path.Combine(targetFolder, targetFile));
          OnInstallShortcut(ref shortcutInfo);
        }
        catch (Exception ex)
        {
          if (ShortcutFailure != null)
          {
            bool cancel = false;

            ShortcutFailure(shortcutInfo, ex, ref cancel);
            if (!cancel) m_failedShortcuts++;
          }
          else
          {
            m_failedShortcuts++;
          }
        }
      }
    }

    /// <summary>
    /// Called when a shortcut is extracted from the installer and needs to be installed to the device OS
    /// </summary>
    /// <param name="shortcutInfo"></param>
    public virtual void OnInstallShortcut(ref ShortcutInstallInfo shortcutInfo)
    {
      string shortcutFile = SI.Path.Combine(shortcutInfo.ShortcutLocation, shortcutInfo.ShortcutName);

      if (!SI.File.Exists(shortcutInfo.TargetLocation))
      {
        if (ShortcutFailure != null)
        {
          bool cancel = false;

          ShortcutFailure(shortcutInfo, new SI.FileNotFoundException("Target file does not exist"), ref cancel);
          if (!cancel) m_failedShortcuts++;
        }
        else
        {
          m_failedShortcuts++;
        }

        return;
      }

      using (SI.TextWriter writer = SI.File.CreateText(shortcutFile))
      {
        int length = shortcutInfo.TargetLocation.Length;
        length += (length.ToString().Length + 1);

        writer.Write(string.Format("{0}#{1}", length, shortcutInfo.TargetLocation));
      }
    }

    #region --- private utility methods ---
    private FileInfo GetFileByExtensionIndex(int index)
    {
      if ((index < 0) || (index > 999)) throw new ArgumentOutOfRangeException();

      FileInfo[] files = m_cab.ContainedFiles.Filter(string.Format("*.{0:000}", index));

      if (files.Length != 1) return null;

      return files[0];
    }

    /// <summary>
    /// Replaces special folder strings with their proper device folder name (e.g. "%CE1%" becomes "\Program Files")
    /// </summary>
    /// <param name="sourceString"></param>
    /// <returns></returns>
    public string ReplaceSpecialFolderStrings(string sourceString)
    {
      // TODO: need to localize these
      // TODO: determine PPC/WinMo/Generic CE platform

      // for now we'll map for PPC/WinMo
      //%CE1% \Program Files 
      sourceString = sourceString.Replace("%CE1%", "\\Program Files");
      //%CE2% \Windows 
      sourceString = sourceString.Replace("%CE2%", "\\Windows");
      //%CE3% \Windows\Desktop 
      sourceString = sourceString.Replace("%CE3%", "\\Windows\\Desktop");
      // %CE4% \Windows\StartUp 
      sourceString = sourceString.Replace("%CE4%", "\\Windows\\StartUp");
      // %CE5% \My Documents 
      sourceString = sourceString.Replace("%CE5%", "\\My Documents");
      // %CE6% \Program Files\Accessories 
      sourceString = sourceString.Replace("%CE6%", "\\Program Files\\Accessories");
      // %CE7% \Program Files\Communications 
      sourceString = sourceString.Replace("%CE7%", "\\Program Files\\Communications");
      // %CE8% \Program Files\Games 
      sourceString = sourceString.Replace("%CE8%", "\\Program Files\\Games");
      // %CE9% \Program Files\Pocket Outlook 
      sourceString = sourceString.Replace("%CE9%", "\\Program Files\\Pocket Outlook");
      // %CE10% \Program Files\Office 
      sourceString = sourceString.Replace("%CE10%", "\\Program Files\\Office");
      // %CE15% \Windows\Fonts 
      sourceString = sourceString.Replace("%CE15%", "\\Windows\\Fonts");
      // %CE16% \Windows\Recent 
      sourceString = sourceString.Replace("%CE16%", "\\Windows\\Recent");

      // Pocket-PC Specific
      // %CE11% \Windows\Programs 
      sourceString = sourceString.Replace("%CE11%", "\\Windows\\Programs");
      // %CE12% \Windows\Programs\Accessories 
      sourceString = sourceString.Replace("%CE12%", "\\Windows\\Programs\\Accessories");
      // %CE13% \Windows\Programs\Communications 
      sourceString = sourceString.Replace("%CE13%", "\\Windows\\Programs\\Communications");
      // %CE14% \Windows\Programs\Games 
      sourceString = sourceString.Replace("%CE14%", "\\Windows\\Programs\\Games");
      // %CE17% \Windows\Favorites 
      sourceString = sourceString.Replace("%CE17%", "\\Windows\\Favorites");


      // TODO: determine platform
      /* These are for generic CE
      // %CE11% \Windows\Start Menu\Programs 
      sourceString = sourceString.Replace("%CE11%", "\\Windows\\Start Menu\\Programs");
      // %CE12% \Windows\Start Menu\Accessories 
      sourceString = sourceString.Replace("%CE12%", "\\Windows\\Start Menu\\Accessories");
      // %CE13% \Windows\Start Menu\Communications 
      sourceString = sourceString.Replace("%CE13%", "\\Windows\\Start Menu\\Communications");
      // %CE14% \Windows\Start Menu\Games 
      sourceString = sourceString.Replace("%CE14%", "\\Windows\\Start Menu\\Games");
      // %CE17% \Windows\Start Menu 
      sourceString = sourceString.Replace("%CE17%", "\\Windows\\Start Menu");
      */

      // TODO: check for user-defined replacements - are they done in cabwiz or here?
      sourceString = sourceString.Replace("%InstallDir%", this.InstallDirectory);

      return sourceString;
    }
    #endregion

    #region --- Reg entries ---
    /// <summary>
    /// Installs all registry values packaged in the installer to the local device
    /// </summary>
    public void InstallRegValues()
    {
      m_failedRegVals = 0;

      if (m_descriptor.RegistryKeys == null) return;

      foreach (InstallerRegistryKey key in m_descriptor.RegistryKeys)
      {
        string keyName = string.Format("{0}{1}", m_descriptor.GetHiveByHiveID(key.HiveID).ToString(), m_descriptor.GetSubkeyByHiveID(key.HiveID));
        string valueName = string.Copy(key.ValueName);
        Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.Unknown;

        switch (key.KeyType & (~RegistyKeyType.NoClobber))
        {
          case RegistyKeyType.Binary:
            valueKind = Microsoft.Win32.RegistryValueKind.Binary;
            break;
          case RegistyKeyType.MultiString:
            valueKind = Microsoft.Win32.RegistryValueKind.MultiString;
            break;
          case RegistyKeyType.Number:
            valueKind = Microsoft.Win32.RegistryValueKind.DWord;
            break;
          case RegistyKeyType.String:
            valueKind = Microsoft.Win32.RegistryValueKind.String;
            break;
        }
        byte[] data = (byte[])key.Data.Clone();

        RegistryInstallInfo registryInfo = new RegistryInstallInfo(((key.KeyType & RegistyKeyType.NoClobber) == RegistyKeyType.NoClobber), keyName, valueName, valueKind, data);
        try
        {
          OnInstallRegValue(ref registryInfo);
        }
        catch (Exception ex)
        {
          if (RegistryFailure != null)
          {
            bool cancel = false;

            RegistryFailure(registryInfo, ex, ref cancel);
            if (!cancel) m_failedRegVals++;
          }
          else
          {
            m_failedRegVals++;
          }
        }
      }
    }

    /// <summary>
    /// Called when a registry value is extracted from the installer and needs to be installed to the device OS
    /// </summary>
    /// <param name="registryInfo"></param>
    public virtual void OnInstallRegValue(ref RegistryInstallInfo registryInfo)
    {
      if (registryInfo.NoClobber)
      {
        // see if the key is already there
        if (Microsoft.Win32.Registry.GetValue(registryInfo.KeyName, registryInfo.ValueName, null) != null) return;
      }

      switch (registryInfo.ValueKind)
      {
        case RegistryValueKind.Binary:
          Registry.SetValue(registryInfo.KeyName, registryInfo.ValueName, registryInfo.Data, RegistryValueKind.Binary);
          break;
        case RegistryValueKind.String:
          Registry.SetValue(registryInfo.KeyName, registryInfo.ValueName,
              ReplaceSpecialFolderStrings(Encoding.ASCII.GetString(registryInfo.Data, 0, registryInfo.Data.Length - 1)),
              RegistryValueKind.String);
          break;
        case RegistryValueKind.DWord:
          Registry.SetValue(registryInfo.KeyName, registryInfo.ValueName, BitConverter.ToInt32(registryInfo.Data, 0), RegistryValueKind.DWord);
          break;
        case RegistryValueKind.MultiString:
          List<string> stringList = new List<string>();

          int start = 0;
          int end = 0;
          while (registryInfo.Data[end] != 0)
          {
            for (int i = start; i < registryInfo.Data.Length; i++)
            {
              if (registryInfo.Data[start + i] == 0)
              {
                end += i;
                break;
              }
            }

            string item = ReplaceSpecialFolderStrings(Encoding.ASCII.GetString(registryInfo.Data, start, end - start));
            stringList.Add(item);
            start += end;
          }

          Registry.SetValue(registryInfo.KeyName, registryInfo.ValueName, stringList, RegistryValueKind.MultiString);
          break;
      }
    }
    #endregion

    #region --- Files ---
    /// <summary>
    /// Installs all files packaged in the installer to the local device
    /// </summary>
    public void InstallFiles()
    {
      m_failedFiles = 0;

      // get the list of target directories
      string[] directoryList = this.GetDirectories();

      // extract the files
      foreach (InstallerFile file in m_descriptor.Files)
      {
        // This is *not* physical number - don't be fooled by any docs that say it is
        FileInfo info = GetFileByExtensionIndex(file.ID);

        // copy the string so derived classes can alter them
        string targetFolder = string.Copy(m_descriptor.GetDirectoryByID(file.DirectoryID).GetString(m_descriptor.Strings));
        targetFolder = ReplaceSpecialFolderStrings(targetFolder);
        targetFolder = targetFolder.Replace("\\\\", "\\");

        string targetFile = string.Copy(file.FileName);

        FileInstallInfo installInfo = null;
        try
        {
          lock (m_fileSyncRoot)
          {
            m_installInfo = info;
            bool skipped;
      
            installInfo = new FileInstallInfo(targetFile, targetFolder, file.Flags, info.FileDate);

            OnInstallFile(ref installInfo, out skipped);
          }
        }
        catch(Exception ex)
        {
          if (FileFailure != null)
          {
            bool cancel = false;

            FileFailure(installInfo, ex, ref cancel);
            if (!cancel) m_failedFiles++;
          }
          else
          {
            m_failedFiles++;
          }
        }
      }
    }

    FileInfo m_installInfo;

    /// <summary>
    /// Called when a file is extracted from the installer and needs to be installed to the device OS
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="skipped"></param>
    public virtual void OnInstallFile(ref FileInstallInfo fileInfo, out bool skipped)
    {
      skipped = false;

      string destFile = SI.Path.Combine(fileInfo.DestinationFolder, fileInfo.FileName);

      if (((fileInfo.Flags & FileFlags.DoNotOverwrite) == FileFlags.DoNotOverwrite)
          && (SI.File.Exists(destFile)))
      {
        skipped = true;
        return;
      }

      if (((fileInfo.Flags & FileFlags.OnlyCopyExisting) == FileFlags.OnlyCopyExisting)
          && (!SI.File.Exists(destFile)))
      {
        skipped = true;
        return;
      }

      if (((fileInfo.Flags & FileFlags.OverwriteIfNewer) == FileFlags.OverwriteIfNewer)
          && (SI.File.Exists(destFile)))
      {
        SI.FileInfo destInfo = new System.IO.FileInfo(destFile);
        if (destInfo.CreationTime > fileInfo.FileDate)
        {
          skipped = true;
          return;
        }
      }

      if (SI.File.Exists(destFile))
      {
        SI.File.Delete(destFile);
      }

      if (!SI.Directory.Exists(fileInfo.DestinationFolder))
      {
        SI.Directory.CreateDirectory(fileInfo.DestinationFolder);
      }

      SI.File.Copy(SI.Path.Combine(m_tempFolder, m_installInfo.Name), destFile);
    }
    #endregion

    #region --- Destructor and Dispose ---
    ~WinCEInstallerFile()
    {
      Dispose();
    }

    /// <summary>
    /// Frees the resources allocated by the installer file during installation
    /// </summary>
    public void Dispose()
    {
      GC.SuppressFinalize(this);

      if (m_tempFolder != null)
      {
        foreach (string file in System.IO.Directory.GetFiles(m_tempFolder))
        {
          try
          {
            System.IO.File.Delete(file);
          }
          catch
          {
            // TODO: should we do anything here? for now just leave it and let the user/system clean up
          }
        }
        try
        {
          System.IO.Directory.Delete(m_tempFolder);
        }
        catch
        {
          // TODO: should we do anything here? for now just leave it and let the user/system clean up
        }
      }
    }
    #endregion
  }
}
