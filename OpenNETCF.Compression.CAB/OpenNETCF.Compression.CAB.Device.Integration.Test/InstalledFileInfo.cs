using System;

using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.Compression.CAB.Device.Integration.Test
{
  internal class InstalledFileInfo
  {
    public InstalledFileInfo(string path, string fileName, FileFlags flags)
      : this(new FileInstallInfo(fileName, path, flags, DateTime.MinValue))
    {
    }

    public InstalledFileInfo(FileInstallInfo fileInfo)
    {
      Path = fileInfo.DestinationFolder;
      FileName = fileInfo.FileName;
      Flags = fileInfo.Flags;
      FileDate = fileInfo.FileDate;
    }

    public string Path { get; set; }
    public string FileName { get; set; }
    public FileFlags Flags { get; set; }
    public DateTime FileDate { get; set; }
    public bool Installed { get; set; }

    public int CompareTo(InstalledFileInfo info, bool ignoreDate, bool ignoreFlags)
    {
      int result = string.Compare(this.FileName, info.FileName, true);
      if (result != 0) return result;

      result = string.Compare(this.Path, info.Path, true);
      if (result != 0) return result;

      if (!ignoreDate)
      {
        result = this.FileDate.CompareTo(info.FileDate);
        if (result != 0) return result;
      }

      if (!ignoreFlags)
      {
        result = this.Flags.CompareTo(info.Flags);
        if (result != 0) return result;
      }

      return 0;
    }
  }
}
