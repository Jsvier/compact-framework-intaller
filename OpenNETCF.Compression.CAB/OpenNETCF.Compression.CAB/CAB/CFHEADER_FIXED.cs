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

namespace OpenNETCF.Compression.CAB
{
    internal struct CFHEADER_FIXED
    {
        CFHEADER_FIXED(
        uint signature, 
        uint reserved1,
        uint cbCabinet,
        uint reserved2,
        uint coffFiles,
        uint reserved3,
        byte versionMinor,
        byte versionMajor,
        ushort cFolders,
        ushort cFiles,
        CFHEADER_FLAGS flags,
        ushort setID,
        ushort iCabinet)
        {
            this.signature = this.reserved1 = this.cbCabinet =
            this.reserved2 = this.coffFiles = this.reserved3 =
            this.cFolders = this.cFiles = this.setID = 
            this.iCabinet = this.versionMinor = this.versionMajor = 0;

            this.flags = CFHEADER_FLAGS.None;
        }
//    u1  signature[4];	/* cabinet file signature */
//    u4  reserved1;	/* reserved */
//    u4  cbCabinet;	/* size of this cabinet file in bytes */
//    u4  reserved2;	/* reserved */
//    u4  coffFiles;	/* offset of the first CFFILE entry */
//    u4  reserved3;	/* reserved */
//    u1  versionMinor;	/* cabinet file format version, minor */
//    u1  versionMajor;	/* cabinet file format version, major */
//    u2  cFolders;	/* number of CFFOLDER entries in this cabinet */
//    u2  cFiles;	/* number of CFFILE entries in this cabinet */
//    u2  flags;	/* cabinet file option indicators */
//    u2  setID;	/* must be the same for all cabinets in a set */
//    u2  iCabinet;	/* number of this cabinet file in a set */

        public uint signature;	/* cabinet file signature */
        public uint reserved1;	/* reserved */
        public uint cbCabinet;	/* size of this cabinet file in bytes */
        public uint reserved2;	/* reserved */
        public uint coffFiles;	/* offset of the first CFFILE entry */
        public uint reserved3;	/* reserved */
        public byte versionMinor;	/* cabinet file format version, minor */
        public byte versionMajor;	/* cabinet file format version, major */
        public ushort cFolders;	/* number of CFFOLDER entries in this cabinet */
        public ushort cFiles;	/* number of CFFILE entries in this cabinet */
        public CFHEADER_FLAGS flags;	/* cabinet file option indicators */
        public ushort setID;	/* must be the same for all cabinets in a set */
        public ushort iCabinet;	/* number of this cabinet file in a set */
    }
}
