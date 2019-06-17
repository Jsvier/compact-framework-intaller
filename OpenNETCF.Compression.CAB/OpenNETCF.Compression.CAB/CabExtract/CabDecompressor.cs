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
using System.IO;
using System.Text;

namespace OpenNETCF.Compression.CabExtract
{
  public enum CompressionType : ushort
  {
    Unknown = 0xffff,
    Uncompressed = 0,
    MSZIP = 1
  }

  public class CabDecompressor
  {
    private MsZipStream m_zstm;
    private mspack_system m_system;
    private CompressionType m_compressionType;

    private Stream inputStream, outputStream;
    private int m_sizeCompressed;
    private int m_sizeUncompressed;

    public CabDecompressor(CompressionType type)
    {
      // only create the compressor stuff if the target is compressed
      if (type == CompressionType.MSZIP)
      {
        m_zstm = new MsZipStream();
        m_system = new mspack_system();
        m_system.write = new WriteDelegate(Writer);
        m_system.read = new ReadDelegate(Reader);
        m_zstm.mszipd_init(m_system, 4096, 0);
      }

      m_compressionType = type;
    }

    public void DecompressBlock(int decompressSize, int compressedBlockSize, int uncompressedBlockSize, Stream stmIn, Stream stmOut)
    {
      inputStream = stmIn;
      outputStream = stmOut;

      m_sizeCompressed = compressedBlockSize;
      m_sizeUncompressed = uncompressedBlockSize;

      switch (m_compressionType)
      {
        case CompressionType.Uncompressed: // uncompressed
          byte[] buffer = new byte[decompressSize];
          decompressSize = stmIn.Read(buffer, 0, decompressSize);
          stmOut.Write(buffer, 0, decompressSize);
          return;
        case CompressionType.MSZIP: // MSZIP
          MspackError err = m_zstm.mszipd_decompress(decompressSize);
          if (err != MspackError.OK)
            throw new ZipException(err);
          break;
        default:
          throw new NotSupportedException("Only uncompressed and MSZIP cabs are supported");
      }
    }

    internal int Writer(byte[] buffer, int offset, int count)
    {
      outputStream.Write(buffer, offset, count);
      return count;
    }
    internal int Reader(byte[] buffer, int offset, int count)
    {
      int cb = inputStream.Read(buffer, offset, count);
      m_sizeCompressed -= cb;
      return cb;
    }
  }
}
