namespace AllPawnsMustDie
{
    partial class AboutDialog
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
            this.linkLabelGitHub = new System.Windows.Forms.LinkLabel();
            this.labelAbout = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelBuildVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // linkLabelGitHub
            // 
            this.linkLabelGitHub.AutoSize = true;
            this.linkLabelGitHub.Location = new System.Drawing.Point(12, 67);
            this.linkLabelGitHub.Name = "linkLabelGitHub";
            this.linkLabelGitHub.Size = new System.Drawing.Size(230, 13);
            this.linkLabelGitHub.TabIndex = 0;
            this.linkLabelGitHub.TabStop = true;
            this.linkLabelGitHub.Text = "https://github.com/manixaist/AllPawnsMustDie";
            this.linkLabelGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGitHub_LinkClicked);
            // 
            // labelAbout
            // 
            this.labelAbout.AutoSize = true;
            this.labelAbout.Location = new System.Drawing.Point(12, 9);
            this.labelAbout.Name = "labelAbout";
            this.labelAbout.Size = new System.Drawing.Size(245, 26);
            this.labelAbout.TabIndex = 1;
            this.labelAbout.Text = "All Pawns Must Die is free and open-source under \r\nthe MIT license.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "GitHub Repository:";
            // 
            // labelBuildVersion
            // 
            this.labelBuildVersion.AutoSize = true;
            this.labelBuildVersion.Location = new System.Drawing.Point(12, 102);
            this.labelBuildVersion.Name = "labelBuildVersion";
            this.labelBuildVersion.Size = new System.Drawing.Size(41, 13);
            this.labelBuildVersion.TabIndex = 3;
            this.labelBuildVersion.Text = "<build>";
            // 
            // AboutDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 124);
            this.Controls.Add(this.labelBuildVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelAbout);
            this.Controls.Add(this.linkLabelGitHub);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelGitHub;
        private System.Windows.Forms.Label labelAbout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelBuildVersion;
    }
}