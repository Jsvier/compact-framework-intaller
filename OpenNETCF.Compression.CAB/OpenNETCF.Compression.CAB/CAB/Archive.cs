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
using SIO = System.IO;
using OpenNETCF.Compression;
using OpenNETCF.Compression.CabExtract;

namespace OpenNETCF.Compression.CAB
{
  public delegate void UnpackProgress(string fileName, int percentDone);

  internal class Archive
  {
    private List<CFFOLDER> m_folders = new List<CFFOLDER>();
    private FileInfoCollection m_files = new FileInfoCollection();
    private string m_fileName;

    public string FileName
    {
      get { return m_fileName; }
      private set { m_fileName = value; }
    }

    public Archive(string fileName)
    {
      FileName = fileName;
      using (System.IO.FileStream stream = System.IO.File.Open(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
      {
        CFHEADER header = CFHEADER.FromStream(stream);

        int dataBlocks = 0;

        for (int i = 0; i < header.CFHEADER_FIXED.cFolders; i++)
        {
          CFFOLDER folder = CFFOLDER.FromStream(stream, header);
          m_folders.Add(folder);

          dataBlocks += folder.cCFData;
        }

        stream.Seek(header.CFHEADER_FIXED.coffFiles, System.IO.SeekOrigin.Begin);

        for (int i = 0; i < header.CFHEADER_FIXED.cFiles; i++)
        {
          CFFILE file = CFFILE.FromStream(stream);
          m_files.Add(file);
        }
        stream.Close();
      }
    }

    public virtual FileInfoCollection ContainedFiles
    {
      get
      {
        return m_files;
      }
    }

    public void ExtractFile(FileInfo fileInfo, string targetFileName)
    {
      ExtractFile(fileInfo, targetFileName, false);
    }



    public virtual void ExtractFile(FileInfo fileInfo, string targetFileName, bool overwriteExisting)
    {
      CFFOLDER containingFolder = m_folders[fileInfo.CFFILE.iFolder];

      using (System.IO.Stream input = System.IO.File.Open(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
      {
        int toRead = (int)fileInfo.CFFILE.cbFile;

        CabDecompressor decompressor = new CabDecompressor((CompressionType)containingFolder.typeCompress);
        CabFolderStream folderStream = new CabFolderStream(input, containingFolder.coffCabStart, containingFolder.Compressed);

        // skip to the beginning of the file. this is done by decompressing into a null stream
        if (fileInfo.CFFILE.uoffFolderStart > 0)
        {
          using (NullStream ns = new NullStream())
          {
              decompressor.DecompressBlock((int)fileInfo.CFFILE.uoffFolderStart, folderStream.SizeCompressed, folderStream.SizeUncompressed, folderStream, ns);
          }
        }

        using (SizeLimitedOutputStream output = new SizeLimitedOutputStream(targetFileName, System.IO.FileMode.OpenOrCreate, (int)fileInfo.CFFILE.cbFile))
        {
          while (toRead > 0)
          {
            int nextBlock = Math.Min(toRead, folderStream.SizeUncompressed);
            if (folderStream.SizeUncompressed == -1)
                nextBlock = toRead;
            //System.Diagnostics.Debug.WriteLine(string.Format("Reading {0} bytes", nextBlock));
            decompressor.DecompressBlock(nextBlock, folderStream.SizeCompressed, folderStream.SizeUncompressed, folderStream, output);
            toRead -= nextBlock;
            output.Flush();
          }
        }
      }
    }

    public void Extract(string targetFolder)
    {
      foreach (FileInfo fi in this.ContainedFiles)
      {
        ExtractFile(fi, System.IO.Path.Combine(targetFolder, fi.Name), true);
      }
    }
  }

  public class SizeLimitedOutputStream : SIO.FileStream
  {
    int writeLimit;
    public SizeLimitedOutputStream(string path, SIO.FileMode mode, int writeLimit)
      : base(path, mode, System.IO.FileAccess.Write, System.IO.FileShare.Read)
    {
      this.writeLimit = writeLimit;
    }

    public override void Write(byte[] array, int offset, int count)
    {
      int cb = Math.Min(count, writeLimit - (int)Position);
      if (cb <= 0)
        return;
      base.Write(array, offset, cb);
    }
  }
}
