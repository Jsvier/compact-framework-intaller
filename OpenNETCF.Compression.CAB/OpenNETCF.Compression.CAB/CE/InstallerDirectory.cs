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
    internal class InstallerDirectory : InstallerItem
    {
        private const int m_lengthOffset = 2;
        private const int m_specificationOffset = 4;

        private int m_specLength;

        private ArrayList m_specificationList = new ArrayList();

        public override void LoadFromStream(FileStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            m_id = BitConverter.ToInt16(buffer, m_idOffset);
            m_specLength = BitConverter.ToInt16(buffer, m_lengthOffset);

            // create m_data
            m_data = new byte[buffer.Length + m_specLength];

            // copy our header into it
            Buffer.BlockCopy(buffer, 0, m_data, 0, buffer.Length);

            // copy the text into it - this seeks the stream to the end
            stream.Read(m_data, buffer.Length, m_specLength);

            // load the specification list
            int offset = m_specificationOffset;

            short spec = 0;
            do
            {
                spec = BitConverter.ToInt16(m_data, offset);
                if (spec != 0)
                {
                    m_specificationList.Add(spec);
                }
                offset += 2;
            } while (spec != 0);

        }

        public string GetString(InstallerString[] strings)
        {
            StringBuilder sb = new StringBuilder();

            foreach (short spec in m_specificationList)
            {
                foreach (InstallerString s in strings)
                {
                    if (s.ID == spec)
                    {
                        sb.Append("\\");
                        sb.Append(s.Text);
                        break;
                    }
                }
            }

            if (sb.Length == 0)
                return "\\";

            return sb.ToString();
        }

        public static string GetSpecialFolderString(SpecialFolder folder)
        {
            if (folder == SpecialFolder.InstallDir)
            {
                return "%InstallDir%";
            }

            return string.Format("%CE{0}%", (int)folder);
        }
    }
}
