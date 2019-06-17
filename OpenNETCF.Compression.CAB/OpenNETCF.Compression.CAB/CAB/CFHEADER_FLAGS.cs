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
    [Flags]
    internal enum CFHEADER_FLAGS : ushort
    {
        None = 0,
        /// <summary>
        /// Set if this cabinet file is not the first in a set of cabinet files. When this bit is set, the szCabinetPrev and szDiskPrev fields are present in this CFHEADER
        /// </summary>
        PREV_CABINET = 0x0001,
        /// <summary>
        /// Set if this cabinet file is not the last in a set of cabinet files. When this bit is set, the szCabinetNext and szDiskNext fields are present in this CFHEADER.
        /// </summary>
        NEXT_CABINET = 0x0002,
        /// <summary>
        /// Set if this cabinet file contains any reserved fields. When this bit is set, the cbCFHeader, cbCFFolder, and cbCFData fields are present in this CFHEADER.
        /// </summary>
        RESERVE_PRESENT = 0x0004
    }
}
