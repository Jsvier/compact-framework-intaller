using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using ONCFInstall;

namespace HHT_Base
{
    public class HHT_Directory : HHT_BASE
    {
        /// <summary>
        /// Genera las carpetas
        /// </summary>
        public void CreateFolder()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = string.Format (@"{0}\Setting\clean.bat", HHT_Helper.HHT_PATH);
      
            try
            {
                OnMessage("Comienza limpieza..");
                Process process = Process.Start(processStartInfo);

                if (process != null)
                {
                    process.WaitForExit();
                }

                OnMessage("Termina limpieza..");
            }
            catch (Exception ex)
            {
                OnMessage("Error limpieza: " + ex.Message);
            }
        }

        /// <summary>
        /// Copiar los archivos
        /// </summary>
        public void CopyFile()
        {
            OnMessage("Comienza Copia de archivos ..");

            CopyFile(string.Format(@"{0}\Setting\StartUp\AutoRun.lnk", HHT_Helper.HHT_PATH), @"\Application\StartUp\AutoRun.lnk");
            CopyFile(string.Format(@"{0}\Setting\StartUp\winvnc.lnk", HHT_Helper.HHT_PATH), @"\Application\StartUp\winvnc.lnk");
            CopyFile(string.Format (@"{0}\Setting\StartUp\winvnc.lnk", HHT_Helper.HHT_PATH), @"\Application\StartUp\winvnc.lnk");
            CopyFile(string.Format (@"{0}\Setting\StartUp\ScanWedge.lnk", HHT_Helper.HHT_PATH), @"\Application\StartUp\ScanWedge.lnk");
            CopyFile(string.Format (@"{0}\Setting\VNC\vncconfig.exe", HHT_Helper.HHT_PATH), @"\Application\VNC\vncconfig.exe");
            CopyFile(string.Format (@"{0}\Setting\VNC\winvnc.exe", HHT_Helper.HHT_PATH), @"\Application\VNC\winvnc.exe");
            CopyFile(string.Format(@"{0}\Setting\Samples.C\ScanWedge.exe", HHT_Helper.HHT_PATH), @"\Application\Samples.C\ScanWedge.exe");

            CopyFile(string.Format(@"{0}\Setting\MBSce.lnk", HHT_Helper.HHT_PATH), @"\windows\Desktop\MBSce.lnk");
            CopyFile(string.Format(@"{0}\Setting\fast-track.lnk", HHT_Helper.HHT_PATH), @"\windows\Desktop\fast-track.lnk");
            CopyFile(string.Format(@"{0}\Setting\MbsInventary.lnk", HHT_Helper.HHT_PATH), @"\windows\Desktop\MbsInventary.lnk");

            OnMessage("Fin Copia de archivos ..");
        }

        /// <summary>
        /// Copia los archivos locales
        /// </summary>
        /// <param name="source">Origen</param>
        /// <param name="destination">Destino</param>
        private void CopyFile(string source, string destination)
        {
            try
            {
                FileInfo fi = new FileInfo(source);
                fi.CopyTo(destination, true);
            }
            catch (Exception)
            {

            }
   
        }

        /// <summary>
        /// Instalacion de los CAB
        /// </summary>
        public void RunCAB()
        {
            OnMessage("Comienza la Instalacion de CAB de Mensajeria .. ");

            if (!HHT_Helper.ListProgams.Contains("Microsoft .NET CF 3.5 EN-String Resource"))
                Install_CAB_ZIP(@"Program Files\hht_base\Setting\NETCFv35.Messages.EN.cab", false);
            
            OnMessage("Comienza la Instalacion de CAB de base de datos .. ");

            if (!HHT_Helper.ListProgams.Contains("SQLServerCompact 3.5 Core"))
                Install_CAB_ZIP(HHT_Helper.HHT_PATH + "\\Setting\\sqlce.wce5.armv4i.cab", false);
            
            OnMessage("Comienza la Instalacion de CAB de escaner .. ");

            if (!HHT_Helper.ListProgams.Contains("Symbol Managed Class Libraries"))
                Install_CAB_ZIP(HHT_Helper.HHT_PATH + "\\Setting\\symbol.cab", false);


            //TODO:DESCARGAR DE URL
            OnMessage("Comienza la Instalacion de CAB de MBS .. ");
            Install_CAB_ZIP(HHT_Helper.HHT_PATH + "\\Setting\\MBSce.CAB", true);

            OnMessage("Comienza la Instalacion de CAB de Fast-track .. ");
            Install_CAB_ZIP(HHT_Helper.HHT_PATH + "\\Setting\\mbsfasttrack.CAB", true);

            OnMessage("Comienza la Instalacion de CAB de Inventario .. ");
            Install_CAB_ZIP(HHT_Helper.HHT_PATH + "\\Setting\\Inventario.CAB", true);

            OnMessage("Fin la Instalacion de CAB .. ");  
        }

        /// <summary>
        /// Ejecuta los cab de instalacion
        /// </summary>
        /// <param name="filemane"></param>
        /// <param name="argument"></param>
        private void RunCAB(string filemane, string argument)
        {
            try
            {

                ProcessStartInfo processInfo = new ProcessStartInfo(filemane,argument);
                Process process = Process.Start(processInfo);
                
                while (!process.HasExited)                                 // <-- Wait untill process exited
                {
                    process.WaitForExit(3000);
                }

            }
            catch (Exception ex)
            { 
                OnMessage("Error CAB: " + ex.Message);   
            }
        }

        private void Install_CAB_ZIP(string source, bool  shortcut)
        {
            string[] args = new string[] { source, shortcut.ToString() };
            CommandLineArgs commandArgs;

            try
            {
                commandArgs = ParseArgs(args);
                CustomCABInstaller installer = new CustomCABInstaller(args[0], commandArgs);
                installer.Install();
            }
            catch
            {
                return;
            }
    
        }

        private CommandLineArgs ParseArgs(string[] argList)
        {
            // see if we have args and not too many
            if ((argList == null) || (argList.Length == 0) || (argList.Length > MAX_ARGS_SUPPORTED)) throw new Exception();

            // first arg must be a path to an existing CAB
            if (!File.Exists(argList[0])) throw new FileNotFoundException();

            CommandLineArgs args = new CommandLineArgs();

            // find skip args
            for (int i = 1; i < argList.Length; i++)
            {
                if (argList[i].IndexOf("/skip", 0) == 0)
                {
                    ParseSkip(argList[i], args);
                }
                else if (argList[i].IndexOf("/replace", 0) == 0)
                {
                    ParseReplace(argList[i], args);
                }
                else if (argList[i].IndexOf("/sv", 0) == 0)
                {
                    args.SkipOSVersionCheck = true;
                }
                else
                {
                    //throw new Exception();
                }
            }

            return args;
        }

        private void ParseSkip(string skipstring, CommandLineArgs args)
        {
            if (skipstring == null) throw new ArgumentNullException("skipstring");
            if (args == null) throw new ArgumentNullException("args");
            int colonIndex = skipstring.IndexOf(':');
            if (colonIndex < 0) throw new ArgumentException("no colon found");

            skipstring = skipstring.Substring(colonIndex + 1);
            string[] skips = skipstring.Split(new char[] { ',' });

            foreach (string skip in skips)
            {
                if (!string.IsNullOrEmpty(skip))
                {
                    args.SkipFiles.Add(skip);
                }
            }
        }

        private void ParseReplace(string replacestring, CommandLineArgs args)
        {
            if (replacestring == null) throw new ArgumentNullException("skipstring");
            if (args == null) throw new ArgumentNullException("args");
            int colonIndex = replacestring.IndexOf(':');
            if (colonIndex < 0) throw new ArgumentException("no colon found");

            replacestring = replacestring.Substring(colonIndex + 1);
            string[] replacements = replacestring.Split(new char[] { ',' });

            foreach (string replacement in replacements)
            {
                if (!string.IsNullOrEmpty(replacement))
                {
                    string[] values = replacement.Split(new char[] { '|' });
                    if (values.Length != 2) throw new ArgumentException();
                    args.PathStringReplacements.Add(values[0], values[1]);
                }
            }

        }
    }
}
