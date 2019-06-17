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
    public class CabFolderStream: Stream
    {
        private Stream innerStream;
        private bool compressed;

        // Current folder
        private uint folderOffset;
        // Current CFDATA block
        private int sizeCompressed;

        public int SizeCompressed
        {
            get { return sizeCompressed; }
        }
        private int sizeUncompressed;

        public int SizeUncompressed
        {
            get { return sizeUncompressed; }
        }
        private int positionInBlock;

        public CabFolderStream(Stream stm, uint folderOffset, bool compressed)
        {
            this.folderOffset = folderOffset;
            this.innerStream = stm;
            this.compressed = compressed;
            innerStream.Seek((long)folderOffset, SeekOrigin.Begin);
            ReadNextBlock();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("CabFolderStream: Reading {0} bytes at offset {1}", count, innerStream.Position));
            return ReadInternal(buffer, offset, count);
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                buffer = new byte[count + offset];
            
            int numRead = 0;
            while (count > 0)
            {
                if (positionInBlock == sizeCompressed) // advance
                {
                    try
                    {
                        ReadNextBlock();
                    }
                    catch (IOException)
                    {
                        return numRead;
                    }
                }
                if (SizeCompressed == -1)
                    return numRead;
                int toRead = Math.Min(count, sizeCompressed - positionInBlock);
                int cb = innerStream.Read(buffer, offset, toRead);
                positionInBlock += cb;
                numRead += cb;
                count -= cb;
                offset += cb;
                if (cb == 0)
                    break;
            }

            return numRead;
        }

        private void ReadNextBlock()
        {
            innerStream.Seek(4, SeekOrigin.Current); // CRC
            sizeCompressed = innerStream.ReadByte() | (innerStream.ReadByte() << 8);
            sizeUncompressed = innerStream.ReadByte() | (innerStream.ReadByte() << 8);
            positionInBlock = 0;
            System.Diagnostics.Debug.WriteLine(string.Format("Reading block. Ofsset: {0:X}, Comp: {1}, Uncomp: {2}, Sig:{3:X}", innerStream.Position - 10, sizeCompressed, sizeUncompressed, 0x4b43));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                innerStream.Seek(folderOffset, SeekOrigin.Begin);
                sizeCompressed = sizeUncompressed = positionInBlock = 0;
            }
            else if (origin == SeekOrigin.End)
                throw new NotSupportedException();

            int moveDistance = 0;
            while (moveDistance < offset)
            {
                if (positionInBlock == sizeCompressed) // advance
                {
                    try
                    {
                        innerStream.Seek(4, SeekOrigin.Current); // CRC
                    }
                    catch (IOException)
                    {
                        return moveDistance;
                    }
                    sizeCompressed = innerStream.ReadByte() | (innerStream.ReadByte() << 8);
                    sizeUncompressed = innerStream.ReadByte() | (innerStream.ReadByte() << 8);

                    //int sig = innerStream.ReadByte() | (innerStream.ReadByte() << 8);
                    //if (sig != 0x4b43)
                    //    throw new Exception();
                    //innerStream.Seek(-2, SeekOrigin.Current);
                    positionInBlock = 0;
                }
                int toMove = (int)Math.Min(offset, sizeCompressed - positionInBlock);
                try
                {
                    innerStream.Seek(toMove, SeekOrigin.Current);
                }
                catch (IOException){ break;}
                positionInBlock += toMove;
                moveDistance += toMove;
            }

            return moveDistance;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
