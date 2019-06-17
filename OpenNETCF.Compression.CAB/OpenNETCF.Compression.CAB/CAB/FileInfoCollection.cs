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
using System.Text.RegularExpressions;

namespace OpenNETCF.Compression.CAB
{
    internal class FileInfoCollection : IEnumerable<FileInfo>
    {
        private List<FileInfo> m_files = new List<FileInfo>();

        public IEnumerator<FileInfo> GetEnumerator()
        {
            return m_files.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_files.GetEnumerator();
        }

        internal void Add(CFFILE cffile)
        {
            m_files.Add(new FileInfo(cffile));
        }

        public FileInfo this[int index]
        {
            get { return m_files[index]; }
        }

        public FileInfo this[string name]
        {
            get
            {
                FileInfo info = null;

                foreach (FileInfo fi in m_files)
                {
                    if (string.Compare(name, fi.Name, true) == 0)
                    {
                        if (info == null)
                        {
                            info = fi;
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("Ambigous file name.  More than one file named '{0}' is in the archive", name));
                        }
                    }
                }
                return info;
            }
        }

        public FileInfo[] Filter(string filterString)
        {
            List<FileInfo> list = new List<FileInfo>();
            string pattern = Regex.Escape(filterString);
            pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".") + "$";

            foreach (FileInfo fi in m_files)
            {
                if(Regex.IsMatch(fi.Name, pattern, RegexOptions.IgnoreCase))
                {
                    list.Add(fi);
                }
            }
            return list.ToArray();
        }

        public int Count
        {
            get { return m_files.Count; }
        }

        public FileInfo[] ToArray()
        {
            return m_files.ToArray();
        }
    }
}
