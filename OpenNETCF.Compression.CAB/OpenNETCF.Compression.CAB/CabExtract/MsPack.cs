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
    internal delegate int ReadDelegate(byte[] buffer, int offset, int bytes);
    internal delegate int WriteDelegate(byte[] buffer, int offset, int bytes);

    /* --- file I/O abstraction ------------------------------------------------ */

    /**
     * A structure which abstracts file I/O and memory management.
     *
     * The library always uses the mspack_system structure for interaction
     * with the file system and to allocate, free and copy all memory. It also
     * uses it to send literal messages to the library user.
     *
     * When the library is compiled normally, passing NULL to a compressor or
     * decompressor constructor will result in a default mspack_system being
     * used, where all methods are implemented with the standard C library.
     * However, all constructors support being given a custom created
     * mspack_system structure, with the library user's own methods. This
     * allows for more abstract interaction, such as reading and writing files
     * directly to memory, or from a network socket or pipe.
     *
     * Implementors of an mspack_system structure should read all
     * documentation entries for every structure member, and write methods
     * which conform to those standards.
     */
    struct mspack_system
    {

        /**
         * Reads a given number of bytes from an open file.
         *
         * @param file    the file to read from
         * @param buffer  the location where the read bytes should be stored
         * @param bytes   the number of bytes to read from the file.
         * @return the number of bytes successfully read (this can be less than
         *         the number requested), zero to mark the end of file, or less
         *         than zero to indicate an error.
         * @see open(), write()
         */
        public ReadDelegate read;

        /**
         * Writes a given number of bytes to an open file.
         *
         * @param file    the file to write to
         * @param buffer  the location where the written bytes should be read from
         * @param bytes   the number of bytes to write to the file.
         * @return the number of bytes successfully written, this can be less
         *         than the number requested. Zero or less can indicate an error
         *         where no bytes at all could be written. All cases where less
         *         bytes were written than requested are considered by the library
         *         to be an error.
         * @see open(), read()
         */
        public WriteDelegate write;


        /**
         * Copies from one region of memory to another.
         * 
         * The regions of memory are guaranteed not to overlap, are usually less
         * than 256 bytes, and may not be aligned. Please note that the source
         * parameter comes before the destination parameter, unlike the standard
         * C function memcpy().
         *
         * @param src   the region of memory to copy from
         * @param dest  the region of memory to copy to
         * @param bytes the size of the memory region, in bytes
         */
        public void Copy(byte[] src,
                int offset,
                 byte[] dest,
                 int bytes)
        {
            Buffer.BlockCopy(src, offset, dest, 0, bytes);
        }

        public void Copy(byte[] src,
                int offset,
                 byte[] dest,
                    int destOffset,
                 int bytes)
        {
            Buffer.BlockCopy(src, offset, dest, destOffset, bytes);
        }

        /**
         * A null pointer to mark the end of mspack_system. It must equal NULL.
         *
         * Should the mspack_system structure extend in the future, this NULL
         * will be seen, rather than have an invalid method pointer called.
         */
        //void *null_ptr;
    };

    public enum InflateError
    {
        /* inflate() error codes */
        BLOCKTYPE = (-1),  /* unknown block type                      */
        COMPLEMENT = (-2),  /* block size complement mismatch          */
        FLUSH = (-3),  /* error from flush_window() callback      */
        BITBUF = (-4),  /* too many bits in bit buffer             */
        SYMLENS = (-5),  /* too many symbols in blocktype 2 header  */
        BITLENTBL = (-6),  /* failed to build bitlens huffman table   */
        LITERALTBL = (-7),  /* failed to build literals huffman table  */
        DISTANCETBL = (-8),  /* failed to build distance huffman table  */
        BITOVERRUN = (-9),  /* bitlen RLE code goes over table size    */
        BADBITLEN = (-10), /* invalid bit-length code                 */
        LITCODE = (-11), /* out-of-range literal code               */
        DISTCODE = (-12), /* out-of-range distance code              */
        DISTANCE = (-13), /* somehow, distance is beyond 32k         */
        HUFFSYM = (-14), /* out of bits decoding huffman symbol     */
    }

    public enum MspackError
    {
        /* --- error codes --------------------------------------------------------- */

        /** Error code: no error */
        OK = (0),
        /** Error code: bad arguments to method */
        ARGS = (1),
        /** Error code: error opening file */
        OPEN = (2),
        /** Error code: error reading file */
        READ = (3),
        /** Error code: error writing file */
        WRITE = (4),
        /** Error code: seek error */
        SEEK = (5),
        /** Error code: out of memory */
        NOMEMORY = (6),
        /** Error code: bad "magic id" in file */
        SIGNATURE = (7),
        /** Error code: bad or corrupt file format */
        DATAFORMAT = (8),
        /** Error code: bad checksum or CRC */
        CHECKSUM = (9),
        /** Error code: error during compression */
        CRUNCH = (10),
        /** Error code: error during decompression */
        DECRUNCH = (11),
    }
}
