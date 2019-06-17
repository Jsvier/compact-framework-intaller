using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace HHT_Base
{
    public class HHT_Registry : HHT_BASE
    {
        public Byte[] toByteArray(string s)
        {
            int i = 0;
            int imax = s.Length;
            Byte[] r = new Byte[imax / 2 ];
            for (i = 0; i <= (imax -2); i++)
            {
                r[i/2] = (byte)int.Parse(s.Substring(i,2), System.Globalization.NumberStyles.HexNumber);
                i++;
            }
            return r;
        }

        /// <summary>
        /// validacion de los registro de implementaciones
        /// </summary>
        /// <param name="registrykey"></param>
        /// <returns></returns>
       public bool ValidateRegistry(RegistryKey registrykey){
           try
           {
               if (registrykey.GetValue("Instl").ToString() == "1")
                   return true;
               else
                   return false;
           }
           catch (Exception)
           {
               return false;
           }
        }

        public void GetRegistro()
        {
            HHT_Helper.SetSecurityFlag(1);

            OnMessage("Comienza Migracion del registro..");

            RegistryKey registryKey;

            //[HKEY_LOCAL_MACHINE\SOFTWARE\Apps\Microsoft .NET CF 3.5 EN-String Resource]
            registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Apps\Microsoft .NET CF 3.5 EN-String Resource");
            if (ValidateRegistry(registryKey))
                HHT_Helper.ListProgams.Add("Microsoft .NET CF 3.5 EN-String Resource");

            //[HKEY_LOCAL_MACHINE\SOFTWARE\Apps\SQLServerCompact 3.5 Core]
            registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Apps\SQLServerCompact 3.5 Core");
            if (ValidateRegistry(registryKey))
                HHT_Helper.ListProgams.Add("SQLServerCompact 3.5 Core");

            //[HKEY_LOCAL_MACHINE\SOFTWARE\Apps\Symbol Managed Class Libraries]
            registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Apps\Symbol Managed Class Libraries");
            if (ValidateRegistry(registryKey))
                HHT_Helper.ListProgams.Add("Symbol Managed Class Libraries");

           
            registryKey = Registry.LocalMachine.CreateSubKey("System\\State\\Hardware");
            registryKey.SetValue("WiFi", "00000017",RegistryValueKind.DWord);
            registryKey.Close();

            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\Tcpip\\Parms");
            registryKey.SetValue("DNSDomain", "makro.com.ar", RegistryValueKind.String);
            registryKey.Close();

            //Antena / Link
            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\SDCSD40N1\\Parms\\Tcpip");
            registryKey.SetValue("Domain", "makro.com.ar", RegistryValueKind.String);
            registryKey.SetValue("AutoInterval", "300", RegistryValueKind.DWord);
            registryKey.SetValue("AutoMask", new string[] { "255.255.0.0" }, RegistryValueKind.MultiString);
            registryKey.SetValue("AutoSubnet", new string[] { "169.254.0.0" }, RegistryValueKind.MultiString);
            registryKey.SetValue("AutoSeed", "0000", RegistryValueKind.QWord);
            registryKey.SetValue("T1", "43200", RegistryValueKind.DWord);
            registryKey.SetValue("T2", "75600", RegistryValueKind.DWord);
            registryKey.SetValue("LeaseObtainedHigh", "86400", RegistryValueKind.DWord);
            registryKey.SetValue("LeaseObtainedLow", "3299823616", RegistryValueKind.String);
            registryKey.SetValue("DhcpDNS", new string[] { "10.49.2.2710.49.2.26"}, RegistryValueKind.MultiString);
            registryKey.SetValue("DhcpDefaultGateway", new string[] { "10.49.6.129" }, RegistryValueKind.MultiString);
            registryKey.SetValue("DhcpServer", new string[] { "10.49.6.129" }, RegistryValueKind.MultiString);
            registryKey.SetValue("DhcpSubnetMask", new string[] { "255.255.255.128" }, RegistryValueKind.MultiString);
            registryKey.SetValue("DhcpIPAddress", new string[] { "10.49.6.190" }, RegistryValueKind.MultiString);

            registryKey.Close();


            //Wifi Config #2
            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\SDCCF10G1\\Parms\\Configs\\Config02");

            registryKey.SetValue("RadioMode","10", RegistryValueKind.DWord);
            registryKey.SetValue("BitRate","0", RegistryValueKind.DWord);
            registryKey.SetValue("WEPType","5", RegistryValueKind.DWord);
            registryKey.SetValue("PowerSave","2", RegistryValueKind.DWord);
            registryKey.SetValue("EAPType","0", RegistryValueKind.DWord);
            registryKey.SetValue("AuthType","0", RegistryValueKind.DWord);
            registryKey.SetValue("TxPower","0", RegistryValueKind.DWord);
            registryKey.SetValue("ClientName","0", RegistryValueKind.String);
            registryKey.SetValue("SSID","MakroST", RegistryValueKind.String);
            registryKey.SetValue("ConfigName","MakroST", RegistryValueKind.String);

            Byte[] Save4 =
            {
                252, 126, 54, 58, 191, 175, 118, 103, 126, 175, 166, 201, 243, 128, 14, 203, 252, 88, 27, 17,
                91, 129, 212, 84, 248, 212, 122, 91, 74, 157, 113, 186, 172, 82, 204, 202, 237, 21, 138, 28, 171, 255,
                117, 61, 0, 50, 39, 121, 61, 193, 120, 144, 38, 86, 117, 98, 103, 45, 10, 90, 54, 19, 122, 195, 216, 178,
                192, 239, 120, 11, 89, 0, 205, 83, 16, 221, 108, 98, 25, 61, 246, 161, 237, 112, 228, 3, 78, 74, 23, 227,
                254, 128, 36, 184, 228, 175, 57, 248, 5, 127, 105, 121, 229, 244, 150, 97, 254, 126, 224, 57, 138, 62,
                216, 16, 83, 72, 90, 241, 143, 55, 83, 46, 136, 3, 95, 194, 245, 196
            };

            registryKey.SetValue("Save4", Save4, RegistryValueKind.Binary);


            Byte[] Save3 =
            {
                175, 88, 144, 198, 99, 167, 249, 6, 203, 83, 98, 215, 220, 52, 237, 193, 112, 30, 118, 85, 8,
                140, 87, 225, 152, 109, 138, 229, 69, 238, 208, 165, 195, 131, 236, 103, 48, 155, 225, 136, 31, 0, 51,
                159, 9, 30, 171, 110, 241, 198, 33, 207, 205, 3, 107, 164, 145, 77, 29, 122, 84, 136, 27, 119, 166, 9,
                75, 93, 245, 233, 76, 142, 130, 125, 63, 116, 64, 203, 208, 15, 194, 90, 40, 192, 40, 41, 4, 87, 0, 68,
                36, 152, 141, 114, 108, 251, 89, 2, 234, 116, 202, 151, 4, 64, 63, 170, 224, 65, 87, 31, 73, 211, 198,
                50, 69, 128, 102, 84, 47, 33, 16, 16, 213, 199, 8, 51, 130, 233
            };

            registryKey.SetValue("Save3", Save3, RegistryValueKind.Binary);

            Byte[] Save2 =
            {
                142, 135, 235, 112, 251, 155, 148, 12, 140, 169, 85, 248, 153, 40, 15, 189, 32, 124, 28, 247,
                53, 221, 28, 231, 169, 198, 7, 202, 128, 249, 131, 102, 7, 177, 17, 91, 198, 160, 91, 244, 243, 202, 213,
                96, 47, 25, 71, 213, 213, 18, 29, 67, 240, 121, 141, 66, 140, 241, 201, 56, 0, 126, 253, 12, 154, 254,
                251, 214, 147, 245, 37, 250, 53, 216, 107, 138, 250, 5, 100, 190, 231, 97, 82, 180, 137, 202, 69, 136,
                238, 222, 5, 253, 200, 91, 211, 245, 221, 24, 62, 206, 147, 235, 149, 217, 250, 189, 31, 23, 185, 228,
                106, 9, 110, 64, 120, 25, 17, 32, 125, 19, 88, 117, 128, 189, 224, 214, 227, 101
            };

            registryKey.SetValue("Save2", Save2, RegistryValueKind.Binary);

            Byte[] Save1 =
            {
                228, 239, 174, 27, 223, 129, 86, 114, 204, 117, 181, 67, 66, 83, 230, 145, 81, 82, 168, 53,
                49, 4, 250, 63, 157, 61, 77, 219, 172, 68, 30, 180, 159, 29, 33, 219, 178, 46, 45, 104, 141, 81, 146, 93,
                31, 2, 27, 91, 85, 9, 132, 224, 51, 234, 155, 165, 227, 235, 70, 81, 114, 21, 25, 15, 54, 79, 76, 193,
                90, 23, 223, 12, 197, 98, 189, 58, 73, 223, 114, 33, 42, 103, 45, 249, 138, 167, 3, 213, 7, 193, 226,
                175, 26, 232, 193, 222, 199, 235, 24, 79, 193, 44, 4, 150, 223, 209, 183, 22, 105, 26, 66, 219, 220, 116,
                202, 104, 20, 237, 123, 20, 214, 185, 200, 108, 204, 189, 239, 97
            };

            registryKey.SetValue("Save1", Save1, RegistryValueKind.Binary);

            registryKey.Close();


            //Wifi Config #3
            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\SDCCF10G1\\Parms\\Configs\\Config03");

            registryKey.SetValue("RadioMode", "10", RegistryValueKind.DWord);
            registryKey.SetValue("BitRate", "0", RegistryValueKind.DWord);
            registryKey.SetValue("WEPType", "5", RegistryValueKind.DWord);
            registryKey.SetValue("PowerSave", "2", RegistryValueKind.DWord);
            registryKey.SetValue("EAPType", "0", RegistryValueKind.DWord);
            registryKey.SetValue("AuthType", "0", RegistryValueKind.DWord);
            registryKey.SetValue("TxPower", "0", RegistryValueKind.DWord);
            registryKey.SetValue("ClientName", "0", RegistryValueKind.String);
            registryKey.SetValue("SSID", "Makro-Inventario", RegistryValueKind.String);
            registryKey.SetValue("ConfigName", "Makro-Inventario", RegistryValueKind.String);

            Byte[] Save4_3 = toByteArray("FC7E363ABFAF76677EAFA6C9F3800ECBFC581B115B81D454F8D47A5B4A9D71BAAC52CCCAED158A1CABFF753D003227793DC1789026567562672D0A5A36137AC3D8B2C0EF780B5900CD5310DD6C62193DF6A1ED70E4034E4A17E3FE8024B8E4AF39F8057F6979E5F49661FE7EE0398A3ED81053485AF18F37532E88035FC2F5C4");
                
            registryKey.SetValue("Save4", Save4_3, RegistryValueKind.Binary);

            Byte[] Save3_3 = toByteArray("AF5890C663A7F906CB5362D7DC34EDC1701E7655088C57E1986D8AE545EED0A598C9E87C309BE18872BB9DF320D2BE7AF1C621CF669492A4914D1D7A54881B77A6094B5DF5E94C8E827D3F7440CBD00FC25A28C028290457004424988D726CFB5902EA74CA9704403FAAE041571F49D3C632458066542F211010D5C7083382E9");
                
            registryKey.SetValue("Save3", Save3_3, RegistryValueKind.Binary);

            Byte[] Save2_3 = toByteArray("8E87EB70FB9B940C8CA955F899280FBD207C1CF735DD1CE7A9C607CA80F9836607B1115BC6A05BF4F3CAD5602F1947D5D5121D43F0798D428CF1C938007EFD0C9AFEFBD693F525FA35D86B8AFA0564BEE76152B489CA4588EEDE05FDC85BD3F5DD183ECE93EB95D9FABD1F17B9E46A096E40781911207D13587580BDE0D6E365");
                 
            registryKey.SetValue("Save2", Save2_3, RegistryValueKind.Binary);

            Byte[] Save1_3 = toByteArray("E4EFAE1BDF815672CC75B5434253E6915152A8353104FA3F9D3D4DDBAC441EB49F1D21DBB22E2D688D51925D1F021B5B550984E033EA9BA5E3EB46517215190F364F4CC15A17DF0CC562BD3A49DF72212A672DF98AA703D507C1E2AF1AE8C1DEC7EB184FC12C0496DFD1B716691A42DBDC74CA6814ED7B14D6B9C86CCCBDEF61");                
         
            registryKey.SetValue("Save1", Save1_3, RegistryValueKind.Binary);

            registryKey.Close();

            //Configuración por defecto
            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\SDCCF10G1\\Parms\\Configs");
            registryKey.SetValue("ActiveConfig","3", RegistryValueKind.DWord);
            registryKey.SetValue("NumConfigs","3", RegistryValueKind.DWord);

            registryKey.Close();

            //Scanner
            registryKey = Registry.LocalMachine.CreateSubKey("Comm\\SDCCF10G1\\Parms\\Configs\\GlobalConfig");
            registryKey.SetValue("ginaVal","5", RegistryValueKind.DWord);
            registryKey.SetValue("scanInterval","5", RegistryValueKind.DWord);

            registryKey.Close();

            //VNC
            registryKey = Registry.CurrentUser.CreateSubKey("Software\\RealVNC\\WinVNC4");
            registryKey.SetValue("ReverseSecurityTypes", "None", RegistryValueKind.String);
            registryKey.SetValue("QueryConnect", "00", RegistryValueKind.DWord);
            registryKey.SetValue("QueryOnlyIfLoggedOn", "00", RegistryValueKind.DWord);

            Byte[] Password =
            {
                167,248,252,134,115,21,183,255
            };

            registryKey.SetValue("Password", Password, RegistryValueKind.Binary);

            registryKey.Close();

            // Process regeditProcess = Process.Start("regedit.exe", string.Format("/s {0}makro-st.reg",HHT_Helper.HHT_PATH + "//"));
            //regeditProcess.WaitForExit();
        
            OnMessage("Fin Migracion del registro..");

        }

        internal void GetRegistroGood()
        {
            RegistryKey registryKey;
            //Instalador reload #1
            registryKey = Registry.CurrentUser.CreateSubKey("Software\\Symbol\\Startup\\Programs\\Prog13");

            registryKey.SetValue("RunFlag", "1", RegistryValueKind.DWord);
          

        }

        /// <summary>
        /// obtiene el flag de instalacion
        /// </summary>
        /// <returns></returns>
        public bool GetRegistroInstalled()
        {
           
            RegistryKey registryKey;

            try
            {
                registryKey = Registry.CurrentUser.OpenSubKey("Software\\Symbol\\Startup\\Programs\\Prog13\\");

                if (registryKey.GetValue("RunFlag").ToString() == "1")
                   return true;
                else
                   return false;

            }
            catch (Exception ex)
            {
                return false;
            }
             
        }
    }
}  
