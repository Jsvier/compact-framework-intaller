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
using System.Runtime.InteropServices;

namespace OpenNETCF.Compression.CAB
{
  internal static class NativeMethods
  {
    #region --- P/Invoke declarations ---
    /*
            codeINSTALL_INIT
            Install_Init(
                HWND        hwndParent,
                BOOL        fFirstCall,     // is this the first time this function is being called?
                BOOL        fPreviouslyInstalled,
                LPCTSTR     pszInstallDir
            )        
        */
    [DllImport("oncfinstaller.dll", SetLastError = true)]
    internal static extern InstallResult Install_Init(IntPtr hwndParent, int fFirstCall, int fPreviouslyInstalled, string pszInstallDir);

    /*
        codeINSTALL_EXIT
        Install_Exit(
            HWND    hwndParent,
            LPCTSTR pszInstallDir,
            WORD    cFailedDirs,
            WORD    cFailedFiles,
            WORD    cFailedRegKeys,
            WORD    cFailedRegVals,
            WORD    cFailedShortcuts
        )
    */
    [DllImport("oncfinstaller.dll", SetLastError = true)]
    internal static extern InstallResult Install_Exit(IntPtr hwndParent, string pszInstallDir, short cFailedDirs, short cFailedFiles, short cFailedRegKeys, short cFailedRegVals, short cFailedShortcuts);

    // TODO: add uninstall support
    /*
        codeUNINSTALL_INIT
        Uninstall_Init(
            HWND        hwndParent,
            LPCTSTR     pszInstallDir
        )

        codeUNINSTALL_EXIT
        Uninstall_Exit(
            HWND    hwndParent
        )
    */

    internal enum ProcessorArchitecture
    {
      x86 = 0,
      MIPS = 1,
      SHx = 4,
      ARM = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_INFO
    {
      [MarshalAs(UnmanagedType.U4)]
      public int dwOemId;
      [MarshalAs(UnmanagedType.U4)]
      public int dwPageSize;
      public uint lpMinimumApplicationAddress;
      public uint lpMaximumApplicationAddress;
      [MarshalAs(UnmanagedType.U4)]
      public int dwActiveProcessorMask;
      [MarshalAs(UnmanagedType.U4)]
      public int dwNumberOfProcessors;
      [MarshalAs(UnmanagedType.U4)]
      public ProcessorArchitecture dwProcessorType;
      [MarshalAs(UnmanagedType.U4)]
      public int dwAllocationGranularity;
      [MarshalAs(UnmanagedType.U2)]
      public short dwProcessorLevel;
      [MarshalAs(UnmanagedType.U2)]
      public short dwProcessorRevision;
    }

    [DllImport("coredll", SetLastError = true)]
    internal static extern void GetSystemInfo(out SYSTEM_INFO pSi);

    #endregion
  }
}
