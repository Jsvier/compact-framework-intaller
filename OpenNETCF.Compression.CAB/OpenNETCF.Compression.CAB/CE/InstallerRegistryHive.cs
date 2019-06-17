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
    internal class InstallerRegistryHive : InstallerItem
    {
        private int m_specLength;

        public RegistryHive m_hive;

        private const int m_hiveOffset = 2;
        private const int m_reservedOffset = 4;
        private const int m_lengthOffset = 6;

        public RegistryHive Hive
        {
            get { return m_hive; }
        }

        public override void LoadFromStream(FileStream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            m_id = BitConverter.ToInt16(buffer, m_idOffset);
            m_hive = (RegistryHive)BitConverter.ToInt16(buffer, m_hiveOffset);
            m_specLength = BitConverter.ToInt16(buffer, m_lengthOffset);

            // create m_data
            m_data = new byte[buffer.Length + m_specLength];

            // copy our header into it
            Buffer.BlockCopy(buffer, 0, m_data, 0, buffer.Length);

            // copy the text into it - this seeks the stream to the end
            stream.Read(m_data, buffer.Length, m_specLength);
        }

        public string GetString(InstallerString[] strings)
        {
            StringBuilder sb = new StringBuilder();

            int spec = 0;
            int offset = 8;

            spec = BitConverter.ToInt16(m_data, offset);

            while (spec != 0)
            {
                offset += 2;

                foreach (InstallerString s in strings)
                {
                    if (s.ID == spec)
                    {
                        sb.Append("\\");
                        sb.Append(s.Text);
                        break;
                    }
                }

                spec = BitConverter.ToInt16(m_data, offset);
            }

            if (sb.Length == 0)
                return "\\";

            return sb.ToString();
        }

    }
}
