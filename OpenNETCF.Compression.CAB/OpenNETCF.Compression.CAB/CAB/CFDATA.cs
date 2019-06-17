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
    // struct CFDATA
    // {
    //     u4  csum;	/* checksum of this CFDATA entry */
    //     u2  cbData;	/* number of compressed bytes in this block */
    //     u2  cbUncomp;	/* number of uncompressed bytes in this block */
    //     u1  abReserve[];	/* (optional) per-datablock reserved area */
    //     u1  ab[cbData];	/* compressed data bytes */
    // };

    internal class CFDATA
    {
        private CFDATA()
        {
        }

        internal static CFDATA FromStream(FileStream stream, CFHEADER header)
        {
            CFDATA cfdata = new CFDATA();

            BinaryReader reader = new BinaryReader(stream);

            cfdata.csum = reader.ReadUInt32();
            cfdata.cbData = reader.ReadUInt16();
            cfdata.cbUncomp = reader.ReadUInt16();
            cfdata.abReserve = reader.ReadBytes(header.CFHEADER_OPTIONAL.cbCFData);
            cfdata.ab = reader.ReadBytes(cfdata.cbData);

            return cfdata;
        }

        internal uint csum { get; private set; }
        internal ushort cbData { get; private set; }
        internal ushort cbUncomp { get; private set; }
        internal byte[] abReserve { get; private set; }
        internal byte[] ab { get; private set; }

    }
}
