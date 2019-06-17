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

namespace OpenNETCF.Compression.CabExtract
{
    public class MsZip
    {
        internal const uint FRAME_SIZE = (32768); /* size of LZ history window */
        internal const int MAX_HUFFBITS = (16);    /* maximum huffman code length */
        internal const int LITERAL_MAXSYMBOLS = (288);   /* literal/length huffman tree */
        internal const uint LITERAL_TABLEBITS = (9);
        internal const int DISTANCE_MAXSYMBOLS = (32);    /* distance huffman tree */
        internal const int DISTANCE_TABLEBITS = (6);

        internal const int LITERAL_TABLESIZE = (LITERAL_MAXSYMBOLS * 4);

        internal const int DISTANCE_TABLESIZE = ((1 << DISTANCE_TABLEBITS) + (DISTANCE_MAXSYMBOLS * 2));

        /* match lengths for literal codes 257.. 285 */
        static internal readonly ushort[] LiteralLengths = new ushort[]
        {
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27,
            31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258
        };

        /* match offsets for distance codes 0 .. 29 */
        static internal readonly ushort[] DistanceOffsets = new ushort[]
        {
            1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385,
            513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
        };

        /* extra bits required for literal codes 257.. 285 */
        static internal readonly byte[] LiteralExtraBits = new byte[]
        {
          0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2,
          2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };

        /* extra bits required for distance codes 0 .. 29 */
        static internal readonly byte[] DistanceExtraBits = new byte[]
        {
          0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6,
          6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };

        /* the order of the bit length Huffman code lengths */
        static internal readonly byte[] BitLengthOrder = new byte[]
        {
          16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15
        };

        /* ANDing with bit_mask[n] masks the lower n bits */
        static internal readonly ushort[] BitMasks = new ushort[]
        {
         0x0000, 0x0001, 0x0003, 0x0007, 0x000f, 0x001f, 0x003f, 0x007f, 0x00ff,
         0x01ff, 0x03ff, 0x07ff, 0x0fff, 0x1fff, 0x3fff, 0x7fff, 0xffff
        };
    }


    public class ZipException : Exception
    {
        public MspackError err;
        public ZipException(MspackError code)
        { err = code; }
    }
}
