namespace AllPawnsMustDie
{
    partial class DisplayFENDialog
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
            this.labelDisplayFEN = new System.Windows.Forms.Label();
            this.textBoxDisplayFEN = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelDisplayFEN
            // 
            this.labelDisplayFEN.AutoSize = true;
            this.labelDisplayFEN.Location = new System.Drawing.Point(13, 13);
            this.labelDisplayFEN.Name = "labelDisplayFEN";
            this.labelDisplayFEN.Size = new System.Drawing.Size(98, 13);
            this.labelDisplayFEN.TabIndex = 0;
            this.labelDisplayFEN.Text = Properties.Resources.FENDisplayLabel;
            // 
            // textBoxDisplayFEN
            // 
            this.textBoxDisplayFEN.Location = new System.Drawing.Point(13, 30);
            this.textBoxDisplayFEN.Name = "textBoxDisplayFEN";
            this.textBoxDisplayFEN.ReadOnly = true;
            this.textBoxDisplayFEN.Size = new System.Drawing.Size(463, 20);
            this.textBoxDisplayFEN.TabIndex = 1;
            this.textBoxDisplayFEN.Text = string.Empty;
            // 
            // DisplayFENDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 69);
            this.Controls.Add(this.textBoxDisplayFEN);
            this.Controls.Add(this.labelDisplayFEN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DisplayFENDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = Properties.Resources.FENDisplayTitle;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDisplayFEN;
        private System.Windows.Forms.TextBox textBoxDisplayFEN;
    }
}