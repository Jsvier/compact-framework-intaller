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

namespace OpenNETCF.Compression.CAB
{
    internal class FileInfo
    {
        internal CFFILE CFFILE { get; private set; }

        internal FileInfo(CFFILE file)
        {
            CFFILE = file;
        }

        public string Name
        {
            get { return CFFILE.szName; }
        }

        public uint Size
        {
            get { return CFFILE.cbFile; }
        }

        private const int YEARMASK  = 0x0000FE00;
        private const int MONTHMASK = 0x000001E0;
        private const int DAYMASK   = 0x0000001F;
        private const int HOURMASK  = 0x0000F800;
        private const int MINMASK   = 0x000001E0;
        private const int SECMASK   = 0x0000001F;

        public DateTime FileDate
        {
            get
            {
                int year = ((CFFILE.date & YEARMASK) >> 9) + 1980;
                int month = (CFFILE.date & MONTHMASK) >> 5;
                int day = (CFFILE.date & DAYMASK);

                int hour = (CFFILE.time & HOURMASK) >> 11;
                int min = (CFFILE.time & MINMASK) >> 11;
                int sec = (CFFILE.time & SECMASK);

                return new DateTime(year, month, day, hour, min, sec);
            }
        }
    }
}
