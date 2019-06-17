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
    // struct CFFOLDER
    // {
    //     u4  coffCabStart;	/* offset of the first CFDATA block in this folder */
    //     u2  cCFData;	/* number of CFDATA blocks in this folder */
    //     u2  typeCompress;	/* compression type indicator */
    //     u1  abReserve[];	/* (optional) per-folder reserved area */
    // };

    internal class CFFOLDER
    {
        internal static CFFOLDER FromStream(FileStream stream, CFHEADER header)
        {
            CFFOLDER folder = new CFFOLDER();
            BinaryReader reader = new BinaryReader(stream);

            folder.coffCabStart = reader.ReadUInt32();
            folder.cCFData = reader.ReadUInt16();
            folder.typeCompress = reader.ReadUInt16();

            if (((header.CFHEADER_FIXED.flags & CFHEADER_FLAGS.RESERVE_PRESENT) == CFHEADER_FLAGS.RESERVE_PRESENT)
                && header.CFHEADER_OPTIONAL.cbCFFolder != 0)
            {
                folder.abReserve = reader.ReadBytes(header.CFHEADER_OPTIONAL.cbCFFolder);
            }

            return folder;
        }

        public bool Compressed { get { return this.typeCompress != 0; } }

        private CFFOLDER()
        {
        }

        internal uint coffCabStart { private set; get; }
        internal ushort cCFData { private set; get; }
        internal ushort typeCompress { private set; get; }
        internal byte[] abReserve;
    }
}
