using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HHT_Base
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            HHT_Registry registry = new HHT_Registry();

            if (!registry.GetRegistroInstalled())
                Application.Run(new FrmBase());
        }
    }
}