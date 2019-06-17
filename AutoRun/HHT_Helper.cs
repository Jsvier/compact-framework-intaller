using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace HHT_Base
{
    public static class HHT_Helper
    {
        private static string hht_path = null;

        public  static string  HHT_PATH
        {
            get
            {
                if (hht_path == null)
                {
                    hht_path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                }
                return hht_path;
            }

        }

        private static  List<string> listprogams;
        public static List<string> ListProgams
        {
            get
            {
                if (listprogams == null)
                {
                    listprogams = new List<string>();
                }
                return listprogams;
            }
            set { listprogams = value; }
        }

        /// <summary>
        /// Set security question flag in the registry
        /// Security\Policies\Policies
        /// 0000101a
        /// </summary>
        /// <param name="flagValue">
        /// 0 means security question on
        /// 1 means security question off
        /// </param>
        public static void SetSecurityFlag(int flagValue)
        {
            try
            {
                RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"Security\Policies\Policies", true);

                rKey.SetValue("0000101a", flagValue, RegistryValueKind.DWord);
                rKey.Close();
            }
            catch { /*silent exception, for Windows CE 2003 */ }
        }
        public static void Reset()
        {

            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = string.Format(@"{0}\Setting\Warm Reset.exe",HHT_Helper.HHT_PATH);

            try
            {
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                
            }
        }

    }
}
