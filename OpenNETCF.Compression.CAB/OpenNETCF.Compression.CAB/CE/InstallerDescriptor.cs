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
using System.Collections;
using System.Text;
using System.IO;

namespace OpenNETCF.Compression.CAB
{
    internal class InstallerDescriptor
    {
        private InstallerFile[] m_files;
        private InstallerRegistryHive[] m_hives;
        private InstallerRegistryKey[] m_keys;
        private InstallerString[] m_strings;
        private InstallerDirectory[] m_directories;
        private InstallerLink[] m_links;
        private string m_sourceFilePath;
        internal bool m_isNewFormat = false;

        public InstallerDescriptor(string descriptorFilePath)
        {
            // did we get a folder path of the actual file path?  We'll accept either 'cause we're nice like that
            if(File.Exists(descriptorFilePath))
            {
                FileStream fs = File.Open(descriptorFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                m_sourceFilePath = Path.GetDirectoryName(descriptorFilePath);
                InitFromStream(fs);
                fs.Close();
            }
            else
            {
                if (!Directory.Exists(descriptorFilePath))
                {
                    throw new IOException(string.Format("No File or Folder named '{0}' found", descriptorFilePath));
                }

                m_sourceFilePath = descriptorFilePath;
                foreach (string fileName in Directory.GetFiles(descriptorFilePath))
                {
                    if (Path.GetExtension(fileName) == ".000")
                    {
                        m_sourceFilePath = Path.GetDirectoryName(fileName);
                        FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                        InitFromStream(fs);
                        fs.Close();
                        return;
                    }
                }

                throw new IOException(string.Format("No CAB info file found in directory '{0}'", descriptorFilePath));
            }           
        }

        public InstallerDescriptor(FileStream stream)
        {
            InitFromStream(stream);
        }

        private bool VerifyCECAB(FileStream stream)
        {
            byte[] header = new byte[4];

            // header for older CE CABs (no XML descriptor)
            byte[] checkOld = new byte[] { (byte)'M', (byte)'S', (byte)'C', (byte)'E' };
            // header for new CABs
            byte[] checkNew = new byte[] { (byte)'C', (byte)'E', (byte)'4', (byte)'+' };

            stream.Seek(MSCE_OFFSET, SeekOrigin.Begin);
            stream.Read(header, 0, header.Length);
            stream.Seek(0, SeekOrigin.Begin);

            if (BitConverter.ToUInt32(header, 0) == BitConverter.ToUInt32(checkNew, 0))
            {
                m_isNewFormat = true;
                return true;
            }

            if (BitConverter.ToUInt32(header, 0) == BitConverter.ToUInt32(checkOld, 0))
            {
                m_isNewFormat = false;
                return true;
            }
            return false;
        }

        private void InitFromStream(FileStream stream)
        {
            if (!VerifyCECAB(stream))
            {
                throw new IOException(string.Format("{0} is not a valid CE CAB info file", Path.GetFileName(stream.Name)));
            }

            m_data = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(m_data, 0, m_data.Length);

            GetItems(stream, ref m_strings, STRINGS_COUNT_OFFSET, STRINGS_OFFSET);
            GetItems(stream, ref m_directories, DIRS_COUNT_OFFSET, DIRS_OFFSET);
            GetItems(stream, ref m_files, FILES_COUNT_OFFSET, FILES_OFFSET);
            GetItems(stream, ref m_hives, REG_HIVES_COUNT_OFFSET, REG_HIVES_OFFSET);
            GetItems(stream, ref m_keys, REG_KEYS_COUNT_OFFSET, REG_KEYS_OFFSET);
            GetItems(stream, ref m_links, LINKS_COUNT_OFFSET, LINKS_OFFSET);
        }

        private const int MSCE_OFFSET = 0;
        private const int RESERVED1_OFFSET = 4;
        private const int FILE_LENGTH_OFFSET = 8;
        private const int RESERVED2_OFFSET = 12;
        private const int RESERVED3_OFFSET = 16;
        private const int ARCHITECTURE_OFFSET = 20;
        private const int MIN_VER_MAJOR_OFFSET = 24;
        private const int MIN_VER_MINOR_OFFSET = 28;
        private const int MAX_VER_MAJOR_OFFSET = 32;
        private const int MAX_VER_MINOR_OFFSET = 36;
        private const int MIN_VER_BUILD_OFFSET = 40;
        private const int MAX_VER_BUILD_OFFSET = 44;
        private const int STRINGS_COUNT_OFFSET = 48;
        private const int DIRS_COUNT_OFFSET = 50;
        private const int FILES_COUNT_OFFSET = 52;
        private const int REG_HIVES_COUNT_OFFSET = 54;
        private const int REG_KEYS_COUNT_OFFSET = 56;
        private const int LINKS_COUNT_OFFSET = 58;
        private const int STRINGS_OFFSET = 60;
        private const int DIRS_OFFSET = 64;
        private const int FILES_OFFSET = 68;
        private const int REG_HIVES_OFFSET = 72;
        private const int REG_KEYS_OFFSET = 76;
        private const int LINKS_OFFSET = 80;
        private const int APP_NAME_OFFSET = 84;
        private const int APP_NAME_LENGTH_OFFSET = 86;
        private const int PROVIDER_OFFSET = 88;
        private const int PROVIDER_LENGTH_OFFSET = 90;
        private const int UNSUPPORTED_OFFSET = 92;
        private const int UNSUPPORTED_LENGTH_OFFSET = 94;
        private const int RESERVED4_OFFSET = 96;
        private const int RESERVED5_OFFSET = 98;

        public const int Length = 100;

        private byte[] m_data = new byte[Length];

        public string SourceFilesPath
        {
            get { return m_sourceFilePath; }
            set { m_sourceFilePath = value; }
        }

        public string[] UnsupportedPlatforms
        {
            get
            {
                short length = BitConverter.ToInt16(m_data, UNSUPPORTED_LENGTH_OFFSET);
                short offset = BitConverter.ToInt16(m_data, UNSUPPORTED_OFFSET);

                if(length == 0)
                {
                    return null;
                }

                ArrayList platforms = new ArrayList();
                int last = 0;
                for (int i = 0; i < length - 1; i++)
                {
                    if (m_data[offset + i] == 0)
                    {
                        string platform = Encoding.ASCII.GetString(m_data, offset + last, i - last);
                        platforms.Add(platform);
                        last = i + 1;
                    }
                }
                return (string[])platforms.ToArray(typeof(string));
            }
        }

        public string InstallDir
        {
            get
            {
              // according to my sources, the CE source code for wceload just returns the string with ID == 1 as the InstallDir
              // my testing doesn't necessarily equate to that, but I've not yet figured out how to recreate the InstallDir 
              // used in the INF file, so we'll go with this for now
              foreach (InstallerString str in m_strings)
              {
                if (str.ID == 1)
                {
                  return str.Text;
                }
              }
              return string.Empty;
            }
        }

        public string AppName
        {
            get
            {
                short length = BitConverter.ToInt16(m_data, APP_NAME_LENGTH_OFFSET);
                short offset = BitConverter.ToInt16(m_data, APP_NAME_OFFSET);
                return Encoding.ASCII.GetString(m_data, offset, length).Replace("\0", "");
            }
        }

        public string Provider
        {
            get
            {
                short length = BitConverter.ToInt16(m_data, PROVIDER_LENGTH_OFFSET);
                short offset = BitConverter.ToInt16(m_data, PROVIDER_OFFSET);
                return Encoding.ASCII.GetString(m_data, offset, length).Replace("\0", "");
            }
        }

        public int MSCESignature
        {
            get { return BitConverter.ToInt32(m_data, MSCE_OFFSET); }
        }

        public int FileLength
        {
            get { return BitConverter.ToInt32(m_data, FILE_LENGTH_OFFSET); }
        }
        public TargetArchitecture TargetArchitecture
        {
            get { return (TargetArchitecture)BitConverter.ToInt32(m_data, ARCHITECTURE_OFFSET); }
        }

        public Version MinimumVersion
        {
            get
            {
                return new Version(
                    BitConverter.ToInt32(m_data, MIN_VER_MAJOR_OFFSET),
                    BitConverter.ToInt32(m_data, MIN_VER_MINOR_OFFSET),
                    BitConverter.ToInt32(m_data, MIN_VER_BUILD_OFFSET));
            }
        }

        public Version MaximumVersion
        {
            get
            {
                int major = BitConverter.ToInt32(m_data, MAX_VER_MAJOR_OFFSET);
                int minor = BitConverter.ToInt32(m_data, MAX_VER_MINOR_OFFSET);
                int build = BitConverter.ToInt32(m_data, MAX_VER_BUILD_OFFSET);

                if (build < 0)
                {
                    build = 0;
                }

                return new Version(major, minor, build);
            }
        }

        public uint MaxBuild
        {
            get
            {
                return BitConverter.ToUInt32(m_data, MAX_VER_BUILD_OFFSET);
            }
        }

        public InstallerLink[] Shortcuts
        {
            get
            {
                return m_links;
            }
        }

        public InstallerString[] Strings
        {
            get
            {
                return m_strings;
            }
        }

        public InstallerDirectory[] Directories
        {
            get
            {
                return m_directories;
            }
        }

        public InstallerRegistryKey[] RegistryKeys
        {
            get
            {
                return m_keys;
            }
        }

        public InstallerDirectory GetDirectoryByID(short directoryID)
        {
            foreach (InstallerDirectory dir in m_directories)
            {
                if (dir.ID == directoryID) return dir;
            }

            throw new ArgumentException(string.Format("Directory ID {0} not found", directoryID));
        }

        public RegistryHive GetHiveByHiveID(short hiveID)
        {
            foreach (InstallerRegistryHive hive in m_hives)
            {
                if (hive.ID == hiveID)
                {
                    return hive.Hive;
                }
            }

            throw new ArgumentException(string.Format("Hive ID {0} not found", hiveID));
        }

        public InstallerFile GetFileByFileID(short fileID)
        {
            foreach (InstallerFile file in m_files)
            {
                if (file.ID == fileID)
                {
                    return file;
                }
            }

            throw new ArgumentException(string.Format("File ID {0} not found", fileID));
        }

        public string GetSubkeyByHiveID(short hiveID)
        {
            foreach (InstallerRegistryHive hive in m_hives)
            {
                if (hive.ID == hiveID)
                {
                    return hive.GetString(m_strings);
                }
            }

            throw new ArgumentException(string.Format("Hive ID {0} not found", hiveID));
        }

        private void GetItems<T>(FileStream stream, ref T[] items, int countOffset, int dataOffset) where T : InstallerItem, new()
        {
            byte[] buffer = new byte[4];

            // find the number of files
            stream.Seek(countOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, 2);
            short count = BitConverter.ToInt16(buffer, 0);

            if (count == 0)
                return;

            // create the array
            items = new T[count];

            // find the offset to the data
            stream.Seek(dataOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            int offset = BitConverter.ToInt32(buffer, 0);

            // seek to start of the data
            stream.Seek(offset, SeekOrigin.Begin);

            // fill the array
            for (int s = 0; s < items.Length; s++)
            {
                T t = new T();
                t.LoadFromStream(stream);
                items[s] = t;
            }
        }

        public InstallerFile[] Files
        {
            get
            {
                return m_files;
            }
        }

        public void GenerateINF(string path)
        {
        }
    }
    
}
