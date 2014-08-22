namespace MyVMK_Pal
{
    partial class Pirates
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Pirates));
            this.gameBrowser = new Awesomium.Windows.Forms.WebControl(this.components);
            this.SuspendLayout();
            // 
            // gameBrowser
            // 
            this.gameBrowser.Location = new System.Drawing.Point(0, 0);
            this.gameBrowser.Size = new System.Drawing.Size(75, 23);
            this.gameBrowser.TabIndex = 0;
            // 
            // Pirates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(801, 601);
            this.Controls.Add(this.gameBrowser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Pirates";
            this.Text = "MyVMK Pal | Pirates";
            this.Load += new System.EventHandler(this.Pirates_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Awesomium.Windows.Forms.WebControl gameBrowser;
    }
}