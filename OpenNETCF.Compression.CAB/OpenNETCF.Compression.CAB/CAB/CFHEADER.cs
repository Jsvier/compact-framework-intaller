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
using System.Runtime.InteropServices;
using System.IO;

namespace OpenNETCF.Compression.CAB
{
    internal class CFHEADER
    {
        internal CFHEADER_FIXED CFHEADER_FIXED { get; private set; }
        internal CFHEADER_OPTIONAL CFHEADER_OPTIONAL { get; private set; }

        private CFHEADER(CFHEADER_FIXED fixedHeader, CFHEADER_OPTIONAL optionalHeader)
        {
            this.CFHEADER_FIXED = fixedHeader;
            this.CFHEADER_OPTIONAL = optionalHeader;
        }

        internal unsafe static CFHEADER FromStream(FileStream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            int offset = 0;

            offset += sizeof(CFHEADER_FIXED);
            try
            {
                byte[] buffer = reader.ReadBytes(offset);

                CFHEADER_FIXED fixedHeader;

                GCHandle pinnedBytes = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    fixedHeader = (CFHEADER_FIXED)Marshal.PtrToStructure(pinnedBytes.AddrOfPinnedObject(), typeof(CFHEADER_FIXED));
                }
                finally
                {
                    pinnedBytes.Free();
                }

                if (fixedHeader.signature != 0x4643534d)
                {                    
                    throw new OpenNETCF.Compression.CAB.InvalidArchiveException();
                }

                CFHEADER_OPTIONAL optionalHeader = CFHEADER_OPTIONAL.FromStream(stream, fixedHeader.flags);

                return new CFHEADER(fixedHeader, optionalHeader);
            }
            catch (System.IO.EndOfStreamException)
            {
                throw new OpenNETCF.Compression.CAB.InvalidArchiveException();
            }
        }

    }
}
