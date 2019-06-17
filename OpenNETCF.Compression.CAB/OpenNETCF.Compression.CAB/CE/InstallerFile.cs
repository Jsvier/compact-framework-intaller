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
using System.IO;
using OpenNETCF.Compression.CAB;

namespace OpenNETCF.Compression.CAB
{
    internal class InstallerFile : InstallerItem
    {
        private short m_directoryID;
        private short m_physicalFileNumber;
        private FileFlags m_flags;
        private int m_nameLength;

        private const int m_directoryOffset = 2;
        private const int m_fileNumberOffset = 4;
        private const int m_flagsOffset = 6;
        private const int m_lengthOffset = 10;
        private const int m_nameOffset = 12;

        public short DirectoryID
        {
            get { return m_directoryID; }
        }

        public short PhysicalFileNumber
        {
            get { return m_physicalFileNumber; }
        }

        public FileFlags Flags
        {
            get { return m_flags; }
        }

        public override string ToString()
        {
            return FileName;
        }

        public override void LoadFromStream(FileStream stream)
        {
            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);

            m_id = BitConverter.ToInt16(buffer, m_idOffset);
            m_directoryID = BitConverter.ToInt16(buffer, m_directoryOffset);
            m_physicalFileNumber = BitConverter.ToInt16(buffer, m_fileNumberOffset);
            m_flags = (FileFlags)BitConverter.ToInt32(buffer, m_flagsOffset);
            m_nameLength = BitConverter.ToInt16(buffer, m_lengthOffset);

            // create m_data
            m_data = new byte[buffer.Length + m_nameLength];

            // copy our header into it
            Buffer.BlockCopy(buffer, 0, m_data, 0, buffer.Length);

            // copy the text into it - this seeks the stream to the end
            stream.Read(m_data, buffer.Length, m_nameLength);
        }

        public string FileName
        {
            get
            {
                return Encoding.ASCII.GetString(m_data, m_nameOffset, m_nameLength).TrimEnd(new char[] { '\0' });
            }
        }
    }
}
