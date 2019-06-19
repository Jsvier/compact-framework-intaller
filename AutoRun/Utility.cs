using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace HHT_Base
{
  static class Utility
  {
        public static void Output(string text)
        {
          Console.WriteLine(text);
          Debug.WriteLine(text);
        }

        /// url - URL of the file to download
        /// destination - Full path of the destination of the file we are downloading
        /// returns - flag indicating whether the file download was successful
        public static bool DownloadFile(string url, string destination)
        {
            bool success;

            WebResponse response = null;
            Stream responseStream = null;
            FileStream fileStream = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = 100000; // 100 seconds
                request.ContentType = "application/octet-stream";
                response = request.GetResponse();

                responseStream = response.GetResponseStream();

                fileStream = System.IO.File.Open(destination, FileMode.Create, FileAccess.Write, FileShare.None);

                // read up to ten kilobytes at a time
                const int maxRead = 10240;
                var buffer = new byte[maxRead];
                int bytesRead;

                // loop until no data is returned
                while ((bytesRead = responseStream.Read(buffer, 0, maxRead)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }

                // we got to this point with no exception. Ok.
                success = true;
                return success;
            }
            catch (Exception exp)
            {
                // something went terribly wrong.
                success = false;
                Debug.WriteLine(exp);
                return success;
            }
            finally
            {
                // cleanup all potentially open streams.

                if (null != responseStream)
                    responseStream.Close();
                if (null != response)
                    response.Close();
                if (null != fileStream)
                    fileStream.Close();
            }
        }
  }
}

