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

namespace OpenNETCF.Compression.CAB
{
    internal class InstallerLink : InstallerItem
    {
        private short m_baseDirectory;
        private short m_targetFileID;
        private LinkType m_linkType;
        private short m_specLength;

        private const int m_reservedOffset = 2;
        private const int m_baseDirectoryOffset = 4;
        private const int m_targetOffset = 6;
        private const int m_linkTypeOffset = 8;
        private const int m_speclengthOffset = 10;
        private const int m_specdataOffset = 12;

        public string ShortcutLocation
        {
            get { return string.Format("%CE{0}%", m_baseDirectory); }
        }

        public override void LoadFromStream(FileStream stream)
        {
            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);

            m_id = BitConverter.ToInt16(buffer, m_idOffset);
            m_baseDirectory = BitConverter.ToInt16(buffer, m_baseDirectoryOffset);
            m_targetFileID = BitConverter.ToInt16(buffer, m_targetOffset);
            m_linkType = (LinkType)BitConverter.ToInt16(buffer, m_linkTypeOffset);
            m_specLength = BitConverter.ToInt16(buffer, m_speclengthOffset);

            // create m_data
            m_data = new byte[buffer.Length + m_specLength];

            // copy our header into it
            Buffer.BlockCopy(buffer, 0, m_data, 0, buffer.Length);

            // copy the text into it - this seeks the stream to the end
            stream.Read(m_data, buffer.Length, m_specLength);
        }

        public short TargetFileID
        {
            get
            {
                return m_targetFileID;
            }
        }

        public LinkType LinkType
        {
            get
            {
                return (LinkType)m_linkType;
            }
        }

        public string GetString(InstallerString[] strings)
        {
            StringBuilder sb = new StringBuilder();

            int length = 0;
            int offset = m_speclengthOffset;

            length = BitConverter.ToInt16(m_data, offset);

            // last spec is always 0
            while (length > 1)
            {
                offset += 2;

                foreach (InstallerString s in strings)
                {
                    if (s.ID == BitConverter.ToInt16(m_data, offset))
                    {
                        sb.Append(s.Text);
                        break;
                    }
                }

                length--;
            }

            if (sb.Length == 0)
                return "";

            return sb.ToString();
        }
    }
}
