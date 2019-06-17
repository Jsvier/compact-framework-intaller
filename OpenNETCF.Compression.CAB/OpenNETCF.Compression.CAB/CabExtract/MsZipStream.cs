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
    internal delegate int FlushWindowDelegate(uint u);
    internal class MsZipStream
    {
        mspack_system sys;            /* I/O routines          */
        uint window_posn;             /* offset within window  */

        /* inflate() will call this whenever the window should be emptied. */
        FlushWindowDelegate flush_window;

        MspackError error;
        int repair_mode, bytes_output;

        /* I/O buffering */
        byte[] inbuf;
        internal int i_ptr, i_end, o_ptr, o_end;
        internal uint bit_buffer, bits_left, inbuf_size;


        /* huffman code lengths */
        byte[] LITERAL_len = new byte[MsZip.LITERAL_MAXSYMBOLS];
        byte[] DISTANCE_len = new byte[MsZip.DISTANCE_MAXSYMBOLS];

        /* huffman decoding tables */
        ushort[] LITERAL_table = new ushort[MsZip.LITERAL_TABLESIZE];
        ushort[] DISTANCE_table = new ushort[MsZip.DISTANCE_TABLESIZE];

        /* 32kb history window */
        byte[] window = new byte[MsZip.FRAME_SIZE];

        internal void STORE_BITS(int i_ptr, int i_end, uint _bit_buffer, uint bits_left)
        {
            this.i_ptr = i_ptr;
            this.i_end = i_end;
            this.bit_buffer = _bit_buffer;
            this.bits_left = bits_left;
        }

        internal void RESTORE_BITS(out int _i_ptr, out int _i_end, out uint _bit_buffer, out uint _bits_left)
        {
            _i_ptr = this.i_ptr;
            _i_end = this.i_end;
            _bit_buffer = this.bit_buffer;
            _bits_left = this.bits_left;
        }

        internal void ENSURE_BITS(uint nbits, ref int _i_ptr, ref int _i_end, ref uint _bit_buffer, ref uint _bits_left)
        {
            while (_bits_left < (nbits))
            {
                if (_i_ptr >= _i_end)
                {
                    if (zipd_read_input() != MspackError.OK)
                        throw new ZipException(error);
                    _i_ptr = this.i_ptr;
                    _i_end = this.i_end;
                }
                _bit_buffer |= (uint)((uint)inbuf[_i_ptr++] << (byte)_bits_left);
                _bits_left += 8;
            }
        }

        MspackError zipd_read_input()
        {
            int read = sys.read(inbuf, 0, (int)inbuf_size);
            if (read < 0) return error = MspackError.READ;
            //System.Diagnostics.Debug.WriteLine(string.Format("zip_read_input: read {0:X} bytes", read));
            i_ptr = 0;
            i_end = read;

            return MspackError.OK;
        }

        /* make_decode_table(nsyms, nbits, length[], table[])
         *
         * This function was coded by David Tritscher. It builds a fast huffman
         * decoding table out of just a canonical huffman code lengths table.
         *
         * NOTE: this is NOT identical to the make_decode_table() in lzxd.c. This
         * one reverses the quick-lookup bit pattern. Bits are read MSB to LSB in LZX,
         * but LSB to MSB in MSZIP.
         *
         * nsyms  = total number of symbols in this huffman tree.
         * nbits  = any symbols with a code length of nbits or less can be decoded
         *          in one lookup of the table.
         * length = A table to get code lengths from [0 to nsyms-1]
         * table  = The table to fill up with decoded symbols and pointers.
         *
         * Returns 0 for OK or 1 for error
         */

        static readonly byte[] bit4Reverse = {
			0,
			8,
			4,
			12,
			2,
			10,
			6,
			14,
			1,
			9,
			5,
			13,
			3,
			11,
			7,
			15
		};
        public static short BitReverse(int toReverse)
        {
            return (short)(bit4Reverse[toReverse & 0xF] << 12 |
                            bit4Reverse[(toReverse >> 4) & 0xF] << 8 |
                            bit4Reverse[(toReverse >> 8) & 0xF] << 4 |
                            bit4Reverse[toReverse >> 12]);
        }

        static int make_decode_table(uint nsyms, uint nbits,
                         byte[] length, ushort[] table)
        {
            uint leaf, reverse, fill;
            uint sym, next_sym;
            uint bit_num;
            uint pos = 0; /* the current position in the decode table */
            uint table_mask = (uint)(1 << (byte)nbits);
            uint bit_mask = table_mask >> 1; /* don't do 0 length codes */

            /* fill entries for codes short enough for a direct mapping */
            for (bit_num = 1; bit_num <= nbits; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;

                    /* reverse the significant bits */
                    fill = length[sym]; reverse = pos >> (byte)(nbits - fill); leaf = 0;
                    do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill != 0);

                    if ((pos += bit_mask) > table_mask) return 1; /* table overrun */

                    /* fill all possible lookups of this symbol with the symbol itself */
                    fill = bit_mask; next_sym = (ushort)(1 << (byte)bit_num);
                    do {
                        table[leaf] = (ushort)sym; 
                        leaf += next_sym; 
                    } while (--fill != 0);
                }
                bit_mask >>= 1;
            }

            /* exit with success if table is now complete */
            if (pos == table_mask) return 0;

            /* mark all remaining table entries as unused */
            for (sym = (ushort)pos; sym < table_mask; sym++)
            {
                reverse = sym; leaf = 0; fill = nbits;
                do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill != 0);
                table[leaf] = 0xFFFF;
            }

            /* where should the longer codes be allocated from? */
            next_sym = (ushort)(((table_mask >> 1) < (ushort)nsyms) ? nsyms : (table_mask >> 1));

            /* give ourselves room for codes to grow by up to 16 more bits.
             * codes now start at bit nbits+16 and end at (nbits+16-codelength) */
            pos <<= 16;
            table_mask <<= 16;
            bit_mask = 1 << 15;

            for (bit_num = (byte)(nbits + 1); bit_num <= MsZip.MAX_HUFFBITS; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;

                    /* leaf = the first nbits of the code, reversed */
                    reverse = pos >> 16; leaf = 0; fill = nbits;
                    do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill != 0);

                    for (fill = 0; fill < (bit_num - nbits); fill++)
                    {
                        /* if this path hasn't been taken yet, 'allocate' two entries */
                        if (table[leaf] == 0xFFFF)
                        {
                            table[(next_sym << 1)] = 0xFFFF;
                            table[(next_sym << 1) + 1] = 0xFFFF;

                            table[leaf] = (ushort)next_sym++;
                        }
                        /* follow the path and select either left or right for next bit */
                        leaf = (uint)(table[leaf] << 1) | ((pos >> (byte)(15 - fill)) & 1);
                    }

                    if (leaf == 0xef)
                        leaf = 0xef;
                    table[leaf] = (ushort)sym;

                    if ((pos += bit_mask) > table_mask) return 1; /* table overflow */
                }
                bit_mask >>= 1;
            }

            /* full table? */
            return (pos != table_mask) ? 1 : 0;
        }


        /* READ_HUFFSYM(tablename, var) decodes one huffman symbol from the
         * bitstream using the stated table and puts it in var.
         */
        int READ_HUFFSYM_Literal(ref int _i_ptr, ref int _i_end, ref ushort sym, ref uint i, ref uint _bit_buffer, ref uint _bits_left)
        {
            int var;
            /* huffman symbols can be up to 16 bits long */
            ENSURE_BITS(MsZip.MAX_HUFFBITS, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
            /* immediate table lookup of [tablebits] bits of the code */
            sym = LITERAL_table[(_bit_buffer & ((1 << (byte)MsZip.LITERAL_TABLEBITS) - 1))];
            /* is the symbol is longer than [tablebits] bits? (i=node index) */
            if (sym >= MsZip.LITERAL_MAXSYMBOLS)
            {
                /* decode remaining bits by tree traversal */
                i = MsZip.LITERAL_TABLEBITS;
                i--;
                do
                {
                    /* check next bit. error if we run out of bits before decode */
                    if (i++ > MsZip.MAX_HUFFBITS)
                    {
                        //D(("out of bits in huffman decode"))                            
                        return (int)InflateError.HUFFSYM;
                    }
                    /* double node index and add 0 (left branch) or 1 (right) */
                    sym = LITERAL_table[((uint)sym << 1) | ((_bit_buffer >> (byte)i) & 1)];
                    /* while we are still in node indicies, not decoded symbols */
                } while (sym >= MsZip.LITERAL_MAXSYMBOLS);
            }
            /* result */
            (var) = sym;
            /* look up the code length of that symbol and discard those bits */
            i = LITERAL_len[sym];
            _bit_buffer >>= (byte)(i);
            _bits_left -= (uint)i;
            return var;
        }

        //#define PEEK_BITS(nbits)   (bit_buffer & ((1<<(nbits))-1))
        int READ_HUFFSYM_Distance(ref int _i_ptr, ref int _i_end, ref ushort sym, ref uint i, ref uint _bit_buffer, ref uint _bits_left)
        {
            int var;
            /* huffman symbols can be up to 16 bits long */
            ENSURE_BITS(MsZip.MAX_HUFFBITS, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
            /* immediate table lookup of [tablebits] bits of the code */
            sym = DISTANCE_table[(_bit_buffer & ((1 << (byte)MsZip.DISTANCE_TABLEBITS) - 1))];
            /* is the symbol is longer than [tablebits] bits? (i=node index) */
            if (sym >= MsZip.DISTANCE_MAXSYMBOLS)
            {
                /* decode remaining bits by tree traversal */
                i = MsZip.DISTANCE_TABLEBITS;
                i--;
                do
                {
                    /* check next bit. error if we run out of bits before decode */
                    if (i++ > MsZip.MAX_HUFFBITS)
                    {
                        //D(("out of bits in huffman decode"))                            
                        return (int)InflateError.HUFFSYM;
                    }
                    /* double node index and add 0 (left branch) or 1 (right) */
                    sym = DISTANCE_table[((uint)sym << 1) | ((_bit_buffer >> (byte)i) & 1)];
                    /* while we are still in node indicies, not decoded symbols */
                } while (sym >= MsZip.DISTANCE_MAXSYMBOLS);
            }
            /* result */
            (var) = sym;
            /* look up the code length of that symbol and discard those bits */
            i = DISTANCE_len[sym];
            _bit_buffer >>= (byte)(i);
            _bits_left -= (uint)i;
            return var;
        }

        //#define PEEK_BITS_T(nbits) (bit_buffer & bit_mask[(nbits)])

        //#define REMOVE_BITS(nbits) ((bit_buffer >>= (nbits)), (bits_left -= (nbits)))

        uint READ_BITS(int nbits, ref int _i_ptr, ref int _i_end, ref uint _bit_buffer, ref uint _bits_left)
        {
            uint val = 0;
            while (_bits_left < (nbits))
            {
                if (_i_ptr >= _i_end)
                {
                    if (zipd_read_input() != MspackError.OK)
                        throw new ZipException(error);
                    _i_ptr = i_ptr;
                    _i_end = i_end;
                }
                _bit_buffer |= (uint)(inbuf[_i_ptr++] << (byte)_bits_left);
                _bits_left += 8;
            }
            val = (uint)(_bit_buffer & ((1 << (nbits)) - 1));
            _bit_buffer >>= (nbits);
            _bits_left -= (uint)nbits;

            return val;
        }

        uint READ_BITS_T(int nbits, ref int _i_ptr, ref int _i_end, ref uint _bit_buffer, ref uint _bits_left)
        {
            uint val = 0;
            while (_bits_left < (nbits))
            {
                if (_i_ptr >= _i_end)
                {
                    if (zipd_read_input() != MspackError.OK)
                        throw new ZipException(error);
                    _i_ptr = i_ptr;
                    _i_end = i_end;
                }
                _bit_buffer |= (uint)(inbuf[_i_ptr++] << (byte)_bits_left);
                _bits_left += 8;
            }
            val = (uint)(_bit_buffer & MsZip.BitMasks[nbits]);
            _bit_buffer >>= (byte)nbits;
            _bits_left -= (uint)nbits;
            return val;
        }


        int zip_read_lens()
        {
            /* for the bit buffer and huffman decoding */
            uint _bit_buffer;
            uint _bits_left;
            int _i_ptr, _i_end;

            /* bitlen Huffman codes -- immediate lookup, 7 bit max code length */
            ushort[] bl_table = new ushort[(1 << 7)];
            byte[] bl_len = new byte[19];

            byte[] lens = new byte[MsZip.LITERAL_MAXSYMBOLS + MsZip.DISTANCE_MAXSYMBOLS];
            uint lit_codes, dist_codes, code, last_code = 0, bitlen_codes, i, run;

            _i_ptr = this.i_ptr;
            _i_end = this.i_end;
            _bit_buffer = this.bit_buffer;
            _bits_left = this.bits_left;

            /* read the number of codes */
            lit_codes = READ_BITS(5, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); 
            lit_codes += 257;
            dist_codes = READ_BITS(5, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); 
            dist_codes += 1;
            bitlen_codes = READ_BITS(4, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); 
            bitlen_codes += 4;
            if (lit_codes > MsZip.LITERAL_MAXSYMBOLS) return (int)InflateError.SYMLENS;
            if (dist_codes > MsZip.DISTANCE_MAXSYMBOLS) return (int)InflateError.SYMLENS;

            /* read in the bit lengths in their unusual order */
            for (i = 0; i < bitlen_codes; i++)
                bl_len[MsZip.BitLengthOrder[i]] = (byte)READ_BITS(3, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
            while (i < 19) bl_len[MsZip.BitLengthOrder[i++]] = 0;

            /* create decoding table with an immediate lookup */
            if (make_decode_table(19, 7, bl_len, bl_table) != 0)
            {
                return (int)InflateError.BITLENTBL;
            }

            /* read literal / distance code lengths */
            for (i = 0; i < (lit_codes + dist_codes); i++)
            {
                /* single-level huffman lookup */
                ENSURE_BITS(7, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
                code = bl_table[(_bit_buffer & ((1 << (7)) - 1))];
                _bit_buffer >>= (byte)bl_len[code];
                _bits_left -= (uint)bl_len[code];

                if (code < 16) lens[i] = (byte)(last_code = code);
                else
                {
                    switch (code)
                    {
                        case 16: run = READ_BITS(2, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); run += 3; code = last_code; break;
                        case 17: run = READ_BITS(3, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); run += 3; code = 0; break;
                        case 18: run = READ_BITS(7, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left); run += 11; code = 0; break;
                        default:
                            //D(("bad code!: %u", code)) 
                            return (int)InflateError.BADBITLEN;
                    }
                    if ((i + run) > (lit_codes + dist_codes)) return (int)InflateError.BITOVERRUN;
                    while (run-- != 0) lens[i++] = (byte)code;
                    i--;
                }
            }

            /* copy LITERAL code lengths and clear any remaining */
            i = lit_codes;
            sys.Copy(lens, 0, LITERAL_len, (int)i);
            while (i < MsZip.LITERAL_MAXSYMBOLS) LITERAL_len[i++] = 0;

            i = dist_codes;
            sys.Copy(lens, (int)lit_codes, DISTANCE_len, (int)i);
            while (i < MsZip.DISTANCE_MAXSYMBOLS) DISTANCE_len[i++] = 0;

            STORE_BITS(_i_ptr, _i_end, _bit_buffer, _bits_left);
            return 0;
        }

        /* a clean implementation of RFC 1951 / inflate */
        internal MspackError inflate()
        {
            uint last_block, block_type, distance, length, this_run, i;

            /* for the bit buffer and huffman decoding */
            uint _bit_buffer = 0;
            uint _bits_left;
            ushort sym = 0;
            int _i_ptr, _i_end;

            RESTORE_BITS(out _i_ptr, out _i_end, out _bit_buffer, out _bits_left);

            do
            {
                /* read in last block bit */
                last_block = READ_BITS(1, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);

                /* read in block type */
                block_type = READ_BITS(2, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
                //System.Diagnostics.Debug.WriteLine(string.Format(("block_type={0} last_block={1}"), block_type, last_block));

                if (block_type == 0)
                {
                    /* uncompressed block */
                    byte[] lens_buf = new byte[4];

                    /* go to byte boundary */
                    i = _bits_left & 7;
                    bit_buffer >>= (byte)i;
                    _bits_left -= (uint)i;


                    /* read 4 bytes of data, emptying the bit-buffer if necessary */
                    for (i = 0; (_bits_left >= 8); i++)
                    {
                        if (i == 4) return (MspackError)InflateError.BITBUF;
                        lens_buf[i] = (byte)(bit_buffer & ((1 << (8)) - 1));
                        bit_buffer >>= (8);
                        _bits_left -= (uint)8;
                    }
                    if (_bits_left != 0) return (MspackError)InflateError.BITBUF;
                    while (i < 4)
                    {
                        if (_i_ptr >= _i_end)
                        {
                            if (zipd_read_input() != MspackError.OK) return error;
                            _i_ptr = this.i_ptr;
                            _i_end = this.i_end;
                        }
                        lens_buf[i++] = inbuf[_i_ptr++];
                    }

                    /* get the length and its complement */
                    length = (uint)(lens_buf[0] | (lens_buf[1] << 8));
                    i = (uint)(lens_buf[2] | (lens_buf[3] << 8));
                    if (length != (~i & 0xFFFF)) return (MspackError)InflateError.COMPLEMENT;

                    /* read and copy the uncompressed data into the window */
                    while (length > 0)
                    {
                        if (_i_ptr >= _i_end)
                        {
                            if (zipd_read_input() != MspackError.OK) return error;
                            _i_ptr = this.i_ptr;
                            _i_end = this.i_end;
                        }

                        this_run = length;
                        if (this_run > (uint)(_i_end - _i_ptr)) this_run = (uint)(_i_end - _i_ptr);
                        if (this_run > (MsZip.FRAME_SIZE - window_posn))
                            this_run = MsZip.FRAME_SIZE - window_posn;

                        sys.Copy(inbuf, _i_ptr, window, (int)window_posn, (int)this_run);
                        window_posn += this_run;
                        _i_ptr += (int)this_run;
                        length -= this_run;

                        if (window_posn == MsZip.FRAME_SIZE)
                        {
                            if (flush_window((uint)MsZip.FRAME_SIZE) != 0) return (MspackError)InflateError.FLUSH;
                            window_posn = 0;
                        }
                    }
                }
                else if ((block_type == 1) || (block_type == 2))
                {
                    /* Huffman-compressed LZ77 block */
                    uint _window_posn, match_posn, code;

                    if (block_type == 1)
                    {
                        /* block with fixed Huffman codes */
                        i = 0;
                        while (i < 144) LITERAL_len[i++] = 8;
                        while (i < 256) LITERAL_len[i++] = 9;
                        while (i < 280) LITERAL_len[i++] = 7;
                        while (i < 288) LITERAL_len[i++] = 8;
                        for (i = 0; i < 32; i++) DISTANCE_len[i] = 5;
                    }
                    else
                    {
                        /* block with dynamic Huffman codes */
                        STORE_BITS(_i_ptr, _i_end, _bit_buffer, _bits_left);
                        if ((i = (uint)zip_read_lens()) != 0) return (MspackError)(int)i;
                        RESTORE_BITS(out _i_ptr, out _i_end, out _bit_buffer, out _bits_left);
                    }

                    /* now huffman lengths are read for either kind of block, 
                     * create huffman decoding tables */
                    if (make_decode_table(MsZip.LITERAL_MAXSYMBOLS, MsZip.LITERAL_TABLEBITS,
                              LITERAL_len, LITERAL_table) != 0)
                    {
                        return (MspackError)InflateError.LITERALTBL;
                    }

                    if (make_decode_table(MsZip.DISTANCE_MAXSYMBOLS, MsZip.DISTANCE_TABLEBITS,
                              DISTANCE_len, DISTANCE_table) != 0)
                    {
                        return (MspackError)InflateError.DISTANCETBL;
                    }

                    /* decode forever until end of block code */
                    _window_posn = window_posn;
                    while (true)
                    {
                        code = (uint)READ_HUFFSYM_Literal(ref _i_ptr, ref _i_end, ref sym, ref i, ref _bit_buffer, ref _bits_left);
                        //System.Diagnostics.Debug.WriteLine(string.Format(
                        //    "code: {0}, _i_ptr: {1}, _i_end: {2}, sym: {3}, i: {4}, _window_posn: {5}", code, _i_ptr, _i_end, sym, i, _window_posn));
                        if (_window_posn == 1688)
                            _window_posn = 1688;
                        if (code < 256)
                        {
                            window[_window_posn++] = (byte)code;
                            if (_window_posn == MsZip.FRAME_SIZE)
                            {
                                if (flush_window(MsZip.FRAME_SIZE) != 0) return (MspackError)InflateError.FLUSH;
                                _window_posn = 0;
                            }
                        }
                        else if (code == 256)
                        {
                            /* END OF BLOCK CODE: loop break point */
                            break;
                        }
                        else
                        {
                            code -= 257;
                            if (code > 29) return (MspackError)InflateError.LITCODE;
                            length = READ_BITS_T(MsZip.LiteralExtraBits[code], ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
                            length += MsZip.LiteralLengths[code];

                            code = (uint)READ_HUFFSYM_Distance(ref _i_ptr, ref _i_end, ref sym, ref i, ref _bit_buffer, ref _bits_left);
                            if (code > 30) return (MspackError)InflateError.DISTCODE;
                            distance = READ_BITS_T(MsZip.DistanceExtraBits[code], ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
                            distance += MsZip.DistanceOffsets[code];

                            /* match position is window position minus distance. If distance
                             * is more than window position numerically, it must 'wrap
                             * around' the frame size. */
                            match_posn = (uint)(((distance > _window_posn) ? MsZip.FRAME_SIZE : 0)
                              + _window_posn - distance);

                            /* copy match */
                            if (length < 12)
                            {
                                /* short match, use slower loop but no loop setup code */
                                while (length-- != 0)
                                {
                                    window[_window_posn++] = window[match_posn++];
                                    unchecked { match_posn &= (uint)((int)MsZip.FRAME_SIZE - 1); }

                                    if (_window_posn == MsZip.FRAME_SIZE)
                                    {
                                        if (flush_window(MsZip.FRAME_SIZE) != 0)
                                            return (MspackError)InflateError.FLUSH;
                                        _window_posn = 0;
                                    }
                                }
                            }
                            else
                            {
                                /* longer match, use faster loop but with setup expense */
                                uint runsrc, rundest;
                                do
                                {
                                    this_run = length;
                                    if ((match_posn + this_run) > MsZip.FRAME_SIZE)
                                        this_run = MsZip.FRAME_SIZE - match_posn;
                                    if ((_window_posn + this_run) > MsZip.FRAME_SIZE)
                                        this_run = MsZip.FRAME_SIZE - _window_posn;

                                    rundest = _window_posn; _window_posn += this_run;
                                    runsrc = match_posn; match_posn += this_run;
                                    length -= this_run;
                                    while (this_run-- != 0) window[rundest++] = window[runsrc++];

                                    /* flush if necessary */
                                    if (_window_posn == MsZip.FRAME_SIZE)
                                    {
                                        if (flush_window(MsZip.FRAME_SIZE) != 0)
                                            return (MspackError)InflateError.FLUSH;
                                        _window_posn = 0;
                                    }
                                    if (match_posn == MsZip.FRAME_SIZE) match_posn = 0;
                                } while (length > 0);
                            }

                        } /* else (code >= 257) */

                    } /* while (forever) -- break point at 'code == 256' */
                    window_posn = _window_posn;
                }
                else
                {
                    /* block_type == 3 -- bad block type */
                    return (MspackError)InflateError.BLOCKTYPE;
                }
            } while (last_block == 0);

            /* flush the remaining data */
            if (window_posn != 0)
            {
                if (flush_window(window_posn) != 0) return (MspackError)InflateError.FLUSH;
            }
            STORE_BITS(_i_ptr, _i_end, _bit_buffer, _bits_left);

            /* return success */
            return 0;
        }

        internal int mszipd_flush_window(uint data_flushed)
        {
            this.bytes_output += (int)data_flushed;
            if (this.bytes_output > MsZip.FRAME_SIZE)
            {
                //D(("overflow: %u bytes flushed, total is now %u", data_flushed, this.bytes_output))
                return 1;
            }
            return 0;
        }

        internal void mszipd_init(mspack_system system,
                          int input_buffer_size,
                          int repair_mode)
        {
            input_buffer_size = (input_buffer_size + 1) & -2;
            if (input_buffer_size == 0) throw new Exception();


            /* allocate input buffer */
            this.inbuf = new byte[input_buffer_size];

            /* initialise decompression state */
            this.sys = system;
            this.inbuf_size = (uint)input_buffer_size;
            this.error = MspackError.OK;
            this.repair_mode = repair_mode;
            this.flush_window = new FlushWindowDelegate(mszipd_flush_window);

            this.i_ptr = this.i_end = 0;
            this.o_ptr = this.o_end = -1;
            this.bit_buffer = 0; this.bits_left = 0;
        }

        internal MspackError mszipd_decompress(int out_bytes)
        {
            /* for the bit buffer */
            uint _bit_buffer;
            uint _bits_left;
            int _i_ptr, _i_end;

            int i, state;
            MspackError error = MspackError.OK;

            /* easy answers */
            if ((out_bytes < 0)) return MspackError.ARGS;
            if (error != MspackError.OK) return error;

            /* flush out any stored-up bytes before we begin */
            i = this.o_end - this.o_ptr;
            if (i > out_bytes) i = (int)out_bytes;
            if (i != 0)
            {
                if (this.sys.write(this.window, this.o_ptr, i) != i)
                {
                    return this.error = MspackError.WRITE;
                }
                this.o_ptr += i;
                out_bytes -= i;
            }
            if (out_bytes == 0) return MspackError.OK;


            while (out_bytes > 0)
            {
                /* unpack another block */
                RESTORE_BITS(out _i_ptr, out _i_end, out _bit_buffer, out _bits_left);

                /* skip to next read 'CK' header */
                i = (int)_bits_left & 7; 
                /* align to bytestream */
                _bit_buffer >>= (i);
                _bits_left -= (uint)i;
                
                state = 0;
                do
                {
                    i = (int)READ_BITS(8, ref _i_ptr, ref _i_end, ref _bit_buffer, ref _bits_left);
                    if (i == 'C') state = 1;
                    else if ((state == 1) && (i == 'K')) state = 2;
                    else state = 0;
                } while (state != 2);

                /* inflate a block, repair and realign if necessary */
                this.window_posn = 0;
                this.bytes_output = 0;
                STORE_BITS(_i_ptr, _i_end, _bit_buffer, _bits_left);
                if ((error = inflate()) != MspackError.OK)
                {
                    //D(("inflate error %d", i))
                    if (this.repair_mode != 0)
                    {
                        //  this.sys->message(NULL, "MSZIP error, %u bytes of data lost.", MSZIP_FRAME_SIZE - this.bytes_output);
                        for (i = this.bytes_output; i < MsZip.FRAME_SIZE; i++)
                        {
                            this.window[i] = 0;
                        }
                        this.bytes_output = (int)MsZip.FRAME_SIZE;
                    }
                    else
                    {
                        return this.error = (error > 0) ? error : MspackError.DECRUNCH;
                    }
                }
                this.o_ptr = 0;
                this.o_end = this.bytes_output;

                /* write a frame */
                i = (out_bytes < this.bytes_output) ?
                  (int)out_bytes : this.bytes_output;
                if (this.sys.write(this.window, this.o_ptr, i) != i)
                {
                    return this.error = MspackError.WRITE;
                }

                /* mspack errors (i.e. read errors) are fatal and can't be recovered */
                if ((error > 0) && this.repair_mode != 0) return error;

                this.o_ptr += i;
                out_bytes -= i;
            }

            if (out_bytes != 0)
            {
                //D(("bytes left to output"))
                return this.error = MspackError.DECRUNCH;
            }
            return MspackError.OK;
        }

        void mszipd_free()
        {
        }

    }
}
