using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using ONCFInstall;
using System.IO;

namespace HHT_Base
{
    public partial class FrmBase : Form
    {

        public FrmBase()
        {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {

            lbITems.Items.Clear();
             
            HHT_Directory directory = new HHT_Directory();
            directory.Message += new HHT_BASE.EventHandler(dir_Message);

            HHT_Registry registry = new HHT_Registry();
            registry.Message += new HHT_BASE.EventHandler(dir_Message);

            registry.GetRegistro();
            directory.CreateFolder();
            directory.CopyFile();
       
            directory.RunCAB();

            Thread.Sleep(3000);

            registry.GetRegistroGood();
          
            HHT_Helper.Reset();

        }

        void dir_Message(string message)
        {
            lbITems.Items.Add(message);
            lbITems.Refresh();
        }

    }
}