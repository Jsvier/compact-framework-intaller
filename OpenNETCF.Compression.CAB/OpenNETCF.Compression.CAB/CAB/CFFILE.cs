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
    [Flags]
    internal enum CFFILE_ATTRIBS : ushort
    {
        None = 0,
        ReadOnly = 1,
        Hidden = 2,
        System = 4,
        Archive = 0x20,
        ExecuteAfterExtract = 0x40,
        NameIsUTF = 0x80
        // #define _A_RDONLY      (0x01)	/* file is read-only */
        // #define _A_HIDDEN      (0x02)	/* file is hidden */
        // #define _A_SYSTEM      (0x04)	/* file is a system file */
        // #define _A_ARCH        (0x20)	/* file modified since last backup */
        // #define _A_EXEC        (0x40)	/* run after extraction */
        // #define _A_NAME_IS_UTF (0x80)	/* szName[] contains UTF */

    }

    // struct CFFILE
    // {
    //     u4  cbFile;	/* uncompressed size of this file in bytes */
    //     u4  uoffFolderStart;	/* uncompressed offset of this file in the folder */
    //     u2  iFolder;	/* index into the CFFOLDER area */
    //     u2  date;	/* date stamp for this file */
    //     u2  time;	/* time stamp for this file */
    //     u2  attribs;	/* attribute flags for this file */
    //     u1  szName[];	/* name of this file */
    // };

    internal class CFFILE
    {
        internal static CFFILE FromStream(FileStream stream)
        {
            CFFILE file = new CFFILE();
            BinaryReader reader = new BinaryReader(stream);

            file.cbFile = reader.ReadUInt32();
            file.uoffFolderStart = reader.ReadUInt32();
            file.iFolder = reader.ReadUInt16();
            file.date = reader.ReadUInt16();
            file.time = reader.ReadUInt16();
            file.attribs = (CFFILE_ATTRIBS)reader.ReadUInt16();

            List<byte> nameBytes = new List<byte>();
            while(reader.PeekChar() != 0)
            {
                nameBytes.Add(reader.ReadByte());
            }

            // move past the null terminator
            reader.ReadByte();

            byte[] byteArray = nameBytes.ToArray();
            if ((file.attribs & CFFILE_ATTRIBS.NameIsUTF) == CFFILE_ATTRIBS.NameIsUTF)
            {
                file.szName = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            }
            else
            {
                file.szName = Encoding.ASCII.GetString(byteArray, 0, byteArray.Length);
            }

            return file;
        }

        private CFFILE()
        {
        }

        internal uint cbFile { private set; get; }
        /// <summary>
        /// Uncompressed byte offset of the start of this file's data.  
        /// For the first file in each folder, this value will usually be zero.  
        /// Subsequent files in the folder will have offsets that are typically the running sum of the cbFile values. 
        /// </summary>
        internal uint uoffFolderStart { private set; get; }

        /// <summary>
        /// Index of the folder containing this file’s data. 
        /// A value of zero indicates this is the first folder in this cabinet file.  
        /// The special iFolder values ifoldCONTINUED_FROM_PREV and ifoldCONTINUED_PREV_AND_NEXT indicate that the folder index is actually zero, 
        /// but that extraction of this file would have to begin with the cabinet named in CFHEADER.szCabinetPrev.  
        /// The special iFolder values ifoldCONTINUED_PREV_AND_NEXT and ifoldCONTINUED_TO_NEXT indicate that the folder index is actually one less than CFHEADER.cFolders, 
        /// and that extraction of this file will require continuation to the cabinet named in CFHEADER.szCabinetNext.
        // TODO:not used in CE CAB files, so skip for now        
        // #define ifoldCONTINUED_FROM_PREV      (0xFFFD)
        // #define ifoldCONTINUED_TO_NEXT        (0xFFFE)
        // #define ifoldCONTINUED_PREV_AND_NEXT  (0xFFFF)
        /// </summary>
        /// 
        internal ushort iFolder { private set; get; }
        
        /// <summary>
        /// Date of this file, in the format ((year–1980) << 9)+(month << 5)+(day), where month={1..12} and day={1..31}.  This “date” is typically considered the “last modified” date in local time, but the actual definition is application-defined.
        /// </summary>
        internal ushort date { private set; get; }
        
        /// <summary>
        /// Time of this file, in the format (hour << 11)+(minute << 5)+(seconds/2), where hour={0..23}.  This “time” is typically considered the “last modified” time in local time, but the actual definition is application-defined.
        /// </summary>
        internal ushort time { private set; get; }

        internal CFFILE_ATTRIBS attribs { private set; get; }
        
        internal string szName { private set; get; }

        public override string ToString()
        {
            return szName;
        }
    }
}
