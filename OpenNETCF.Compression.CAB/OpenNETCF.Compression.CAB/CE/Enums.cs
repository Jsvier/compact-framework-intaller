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
    internal enum SpecialFolder
    {
        /// <summary>
        /// %InstallDir%
        /// </summary>
        InstallDir = 0,
        /// <summary>
        /// %CE1% = \Program Files 
        /// </summary>
        ProgramFiles = 1,
        /// <summary>
        /// %CE2% = \Windows 
        /// </summary>
        Windows = 2,
        /// <summary>
        /// %CE3% = \Windows\Desktop 
        /// </summary>
        Windows_Desktop = 3,
        /// <summary>
        /// %CE4% = \Windows\StartUp 
        /// </summary>
        Windows_Startup = 4,
        /// <summary>
        /// %CE5% = \My Documents 
        /// </summary>
        MyDocuments = 5,
        /// <summary>
        /// %CE6% = \Program Files\Accessories 
        /// </summary>
        ProgramFiles_Accessories = 6,
        /// <summary>
        /// %CE7% = \Program Files\Communications
        /// </summary>
        ProgramFiles_Communications = 7,
        /// <summary>
        /// %CE8% = \Program Files\Games 
        /// </summary>
        ProgramFiles_Games = 8,
        /// <summary>
        /// %CE9% = \Program Files\Pocket Outlook 
        /// </summary>
        ProgramFiles_PocketOutlook = 9,
        /// <summary>
        /// %CE10% = \Program Files\Office 
        /// </summary>
        ProgramFiles_Office = 10,
        /// <summary>
        /// %CE11% = \Windows\Programs
        /// </summary>
        Windows_Programs = 11,
        /// <summary>
        /// %CE12% = \Windows\Programs\Accessories 
        /// </summary>
        Windows_Programs_Accessories = 12,
        /// <summary>
        /// %CE13% = \Windows\Programs\Communications 
        /// </summary>
        Windows_Programs_Communications = 13,
        /// <summary>
        /// %CE14% = \Windows\Programs\Games 
        /// </summary>
        Windows_Programs_Games = 14,
        /// <summary>
        /// %CE15% = \Windows\Fonts 
        /// </summary>
        Windows_Fonts = 15,
        /// <summary>
        /// %CE16% = \Windows\Recent 
        /// </summary>
        Windows_Recent = 16,
        /// <summary>
        /// %CE17% = \Windows\Favorites 
        /// </summary>
        Windows_Favorites = 17

    }

    internal enum LinkType
    {
        Directory = 1,
        File = 0
    }

    internal enum RegistryHive
    {
        HKEY_CLASSES_ROOT = 1,
        HKEY_CURRENT_USER = 2,
        HKEY_LOCAL_MACHINE = 3,
        HKEY_USERS = 4
    }

    [Flags]
    internal enum RegistyKeyType
    {
        /// <summary>
        /// This key is of type TYPE_DWORD, a 32-bit integer.
        /// </summary>
        Number = 0x00010001,
        /// <summary>
        /// This key is of type TYPE_SZ, a null terminated ASCII string.
        /// </summary>
        String = 0x00000000,
        /// <summary>
        /// This key is of type TYPE_MULTI_SZ, a list of null terminated ASCII strings.
        /// </summary>
        MultiString = 0x00010000,
        /// <summary>
        /// This key is of type TYPE_BINARY, raw binary data in no particular format. 
        /// </summary>
        Binary = 0x00000001,
        /// <summary>
        /// This is the only known flag, NOCLOBBER. If this bit is set, the registry entry should not be overwritten if it already exists in the registry. 
        /// </summary>
        NoClobber = 0x00000002
    }

    internal enum TargetArchitecture : int
    {
        unspecified = 0,
        SH3 = 103,
        SH4 = 104,
        Intel386 = 386,
        Intel486 = 486,
        Pentium = 586,
        PowerPC601 = 601,
        PowerPC603 = 603,
        PowerPC604 = 604,
        PowerPC620 = 620,
        Motorola821 = 821,
        ARM720 = 1824,
        ARM820 = 2080,
        ARM920 = 2336,
        StrongARM = 2577,
        MIPS4000 = 4000,
        HitachiSH3 = 10003,
        HitachiSH3E = 10004,
        HitachiSH4 = 10005,
        Alpha21064 = 21064,
        ARM7TDMI = 70001
    }



}
