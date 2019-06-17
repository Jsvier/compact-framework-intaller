namespace HHT_Base
{
    partial class FrmBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBase));
            this.btnInstall = new System.Windows.Forms.Button();
            this.lbITems = new System.Windows.Forms.ListBox();
            this.pblogo = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnInstall.Location = new System.Drawing.Point(129, 3);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(68, 45);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "Instalar";
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // lbITems
            // 
            this.lbITems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbITems.Location = new System.Drawing.Point(0, 80);
            this.lbITems.Name = "lbITems";
            this.lbITems.Size = new System.Drawing.Size(200, 142);
            this.lbITems.TabIndex = 1;
            // 
            // pblogo
            // 
            this.pblogo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pblogo.Image = ((System.Drawing.Image)(resources.GetObject("pblogo.Image")));
            this.pblogo.Location = new System.Drawing.Point(0, 0);
            this.pblogo.Name = "pblogo";
            this.pblogo.Size = new System.Drawing.Size(123, 72);
            // 
            // FrmBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(200, 220);
            this.Controls.Add(this.pblogo);
            this.Controls.Add(this.lbITems);
            this.Controls.Add(this.btnInstall);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "FrmBase";
            this.Text = "HHT Instalador";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.ListBox lbITems;
        private System.Windows.Forms.PictureBox pblogo;
    }
}

