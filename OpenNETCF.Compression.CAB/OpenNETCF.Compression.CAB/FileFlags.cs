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
  /// <summary>
  /// Flags used in a CAB archive to describe file actions
  /// </summary>
  [Flags]
  public enum FileFlags : uint
  {
    None = 0,
    /// <summary>
    /// File is reference shared and is only removed at uninstall if the ref count is zero
    /// </summary>
    Shared = unchecked((uint)(1 << 31)),
    /// <summary>
    /// Always overwrite any existing file with the same name
    /// </summary>
    AlwaysOverwrite = (1 << 30),
    /// <summary>
    /// Overwrite the existing file only if the CAB file is newer
    /// </summary>
    OverwriteIfNewer = (1 << 29),
    /// <summary>
    /// Only copy to the target if the file already exists
    /// </summary>
    OnlyCopyExisting = (1 << 10),
    /// <summary>
    /// Do not overwrite the target file if it already exists
    /// </summary>
    DoNotOverwrite = (1 << 4),
    /// <summary>
    /// If this is set, the user cannot skip this file and coditions that would cause a skip are an install failure
    /// </summary>
    CannotSkip = (1 << 1),
    /// <summary>
    ///  Warn the user if this is skipped
    /// </summary>
    WarnIfSkipped = (1 << 0)
  }
}
