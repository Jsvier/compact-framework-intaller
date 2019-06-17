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
    internal class InstallerRegistryKey : InstallerItem
    {
        private short m_hiveID;
        private RegistyKeyType m_keyType;
        private int m_dataLength;
        private int m_keyDataOffset; // calculated

        private const int m_hiveIDOffset = 2;
        private const int m_reservedOffset = 4;
        private const int m_keyTypeOffset = 6;
        private const int m_lengthOffset = 10;
        private const int m_dataOffset = 12;
         
        public short HiveID
        {
            get { return m_hiveID; }
        }

        public RegistyKeyType KeyType
        {
            get { return m_keyType; }
        }

        public override void LoadFromStream(FileStream stream)
        {
            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);

            m_id = BitConverter.ToInt16(buffer, m_idOffset);
            m_hiveID = BitConverter.ToInt16(buffer, m_hiveIDOffset);
            m_keyType = (RegistyKeyType)BitConverter.ToInt32(buffer, m_keyTypeOffset);
            m_dataLength = BitConverter.ToInt16(buffer, m_lengthOffset);

            // create m_data
            m_data = new byte[buffer.Length + m_dataLength];

            // copy our header into it
            Buffer.BlockCopy(buffer, 0, m_data, 0, buffer.Length);

            // copy the text into it - this seeks the stream to the end
            stream.Read(m_data, buffer.Length, m_dataLength);

            // find the end of the name
            m_keyDataOffset = m_dataOffset;

            while(m_data[m_keyDataOffset] != 0)
            {
                m_keyDataOffset++;
            }

            // move past null terminator
            m_keyDataOffset++;
        }

        public string ValueName
        {
            get
            {
                // name is the first null terminated section of the data
                // if it's empty, then it's the default key
                if (m_keyDataOffset == m_dataOffset + 1)
                    return null;

                return Encoding.ASCII.GetString(m_data, m_dataOffset, m_keyDataOffset - m_dataOffset).TrimEnd(new char[] { '\0' });
            }
        }

        public byte[] Data
        {
            get
            {
                int length = m_data.Length - m_keyDataOffset;
                byte[] data = new byte[length];
                Buffer.BlockCopy(m_data, m_keyDataOffset, data, 0, length);
                return data;
            }
        }

        public string ValueAsString
        {            
            get
            {
                StringBuilder value = new StringBuilder(Data.Length);
    
                switch (KeyType & (~RegistyKeyType.NoClobber))
                {
                    case RegistyKeyType.Binary:
                        for (int i = 0; i < Data.Length; i++)
                        {
                            value.Append(string.Format("{0:X2}", Data[i]));
                            if (i < Data.Length)
                            {
                                value.Append(",");
                            }
                        }
                        return value.ToString();
                    case RegistyKeyType.MultiString:
                        ArrayList stringList = new ArrayList();
                        int start = 0;
                        int end = 0;
                        while (Data[end] != 0)
                        {
                            for (int i = start; i < Data.Length; i++)
                            {
                                if (Data[start + i] == 0)
                                {
                                    end += i;
                                    break;
                                }
                            }

                            string item = Encoding.ASCII.GetString(Data, start, end - start);
                            value.Append(string.Format("{0}", item));
                            start += end;
                            if (Data[end] != 0)
                            {
                                value.Append(",");
                            }
                        }
                    return value.ToString();
                        
                    case RegistyKeyType.Number:
                        return BitConverter.ToUInt32(Data, 0).ToString();
                    case RegistyKeyType.String:
                        value.Append(string.Format("{0}", Encoding.ASCII.GetString(Data, 0, Data.Length - 1)));
                        return value.ToString();
                }

                return null;
            }
        }
    }
}
