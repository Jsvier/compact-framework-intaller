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
    internal class CFHEADER_OPTIONAL
    {
        // u2  cbCFHeader; 	/* (optional) size of per-cabinet reserved area */
        // u1  cbCFFolder; 	/* (optional) size of per-folder reserved area */
        // u1  cbCFData; 	/* (optional) size of per-datablock reserved area */
        // u1  abReserve[];	/* (optional) per-cabinet reserved area */
        // u1  szCabinetPrev[];	/* (optional) name of previous cabinet file */
        // u1  szDiskPrev[];	/* (optional) name of previous disk */
        // u1  szCabinetNext[];	/* (optional) name of next cabinet file */
        // u1  szDiskNext[];	/* (optional) name of next disk */

        public static CFHEADER_OPTIONAL FromStream(FileStream stream, CFHEADER_FLAGS fixedHeaderFlags)
        {
            CFHEADER_OPTIONAL optionalHeader = new CFHEADER_OPTIONAL();
            BinaryReader reader = new BinaryReader(stream);

            // TODO: bounds checking
            if ((fixedHeaderFlags & CFHEADER_FLAGS.RESERVE_PRESENT) == CFHEADER_FLAGS.RESERVE_PRESENT)
            {
                // cbCFHeader present
                optionalHeader.cbCFHeader = reader.ReadUInt16();

                // cbCFFolder present
                optionalHeader.cbCFFolder = reader.ReadByte();


                // cbCFData present
                optionalHeader.cbCFData = reader.ReadByte();

                // abReserve present
                optionalHeader.abReserve = reader.ReadBytes(optionalHeader.cbCFHeader);
            }
            else
            {
                // cbCFHeader not present
                optionalHeader.cbCFHeader = 0;

                // cbCFFolder not present
                optionalHeader.cbCFFolder = 0;

                // cbCFData not present
                optionalHeader.cbCFData = 0;

                // abReserve not present
                optionalHeader.abReserve = null;
            }

            if ((fixedHeaderFlags & CFHEADER_FLAGS.PREV_CABINET) == CFHEADER_FLAGS.PREV_CABINET)
            {
                // not needed for CE cabs so skip for now
                throw new NotSupportedException("CFHEADER_FLAGS.PREV_CABINET FLAG NOT SUPPORTED");
            }
            else
            {
                optionalHeader.szCabinetPrev = null;
                optionalHeader.szDiskPrev = null;
                optionalHeader.szCabinetNext = null;
                optionalHeader.szDiskNext = null;
            }

            if ((fixedHeaderFlags & CFHEADER_FLAGS.NEXT_CABINET) == CFHEADER_FLAGS.NEXT_CABINET)
            {
                // not needed for CE cabs so skip for now
                throw new NotSupportedException("CFHEADER_FLAGS.NEXT_CABINET FLAG NOT SUPPORTED");
            }

            // move to next DWORD boundary (struct alignment)
            // stream.Seek((4 - (stream.Position % 4)), SeekOrigin.Current);

            return optionalHeader;
        }

        private CFHEADER_OPTIONAL()
        {
        }

        /// <summary>
        /// If flags.RESERVE_PRESENT is not set, this field is not present, and the value of cbCFHeader defaults to zero.  
        /// Indicates the size in bytes of the abReserve field in this CFHEADER.  Values for cbCFHeader range from 0 to 60,000.
        /// </summary>
        public ushort cbCFHeader { get; private set; }

        /// <summary>
        /// If flags.RESERVE_PRESENT is not set, then this field is not present, and the value of cbCFFolder defaults to zero.  
        /// Indicates the size in bytes of the abReserve field in each CFFOLDER entry.  Values for cbCFFolder range from 0 to 255.
        /// </summary>
        public byte cbCFFolder { get; private set; }

        /// <summary>
        /// If flags.RESERVE_PRESENT is set, then this field is not present, and the value for cbCFData defaults to zero.  
        /// Indicates the size in bytes of the abReserve field in each CFDATA entry.  Values for cbCFData range from 0 to 255.        
        /// </summary>
        public byte cbCFData { get; private set; }

        /// <summary>
        /// If flags.RESERVE_PRESENT is set and cbCFHeader is non-zero, then this field contains per-cabinet-file application information.  
        /// This field is defined by the application and used for application-defined purposes.
        /// </summary>
        public byte[] abReserve { get; private set; }

        /// <summary>
        /// If flags.PREV_CABINET is not set, then this field is not present.  
        /// NUL-terminated ASCII string containing the file name of the logically-previous cabinet file.  
        /// May contain up to 255 bytes plus the NUL byte.  
        /// Note that this gives the name of the most-recently-preceding cabinet file that contains the initial instance of a file entry.  
        /// This might not be the immediately previous cabinet file, when the most recent file spans multiple cabinet files.  
        /// If searching in reverse for a specific file entry, or trying to extract a file which is reported to begin in the “previous cabinet”, 
        /// szCabinetPrev would give the name of the cabinet to examine.If flags.cfhdrPREV_CABINET is not set, then this field is not present.  
        /// </summary>
        public string szCabinetPrev { get; private set; }

        /// <summary>
        /// If flags.cfhdrPREV_CABINET is not set, then this field is not present.  
        /// NUL-terminated ASCII string containing a descriptive name for the media containing the file named in szCabinetPrev, such as the text on the diskette label.  
        /// This string can be used when prompting the user to insert a diskette.  
        /// May contain up to 255 bytes plus the NUL byte.
        /// </summary>
        public string szDiskPrev { get; private set; }

        /// <summary>
        /// If flags.NEXT_CABINET is not set, then this field is not present.  
        /// NUL-terminated ASCII string containing the file name of the next cabinet file in a set.  
        /// May contain up to 255 bytes plus the NUL byte.  
        /// Files extending beyond the end of the current cabinet file are continued in the named cabinet file.
        /// </summary>
        public string szCabinetNext { get; private set; }

        /// <summary>
        /// If flags.cfhdrNEXT_CABINET is not set, then this field is not present.  
        /// NUL-terminated ASCII string containing a descriptive name for the media containing the file named in szCabinetNext, such as the text on the diskette label.  
        /// May contain up to 255 bytes plus the NUL byte.  
        /// This string can be used when prompting the user to insert a diskette.
        /// </summary>
        public string szDiskNext { get; private set; }
    }
}
