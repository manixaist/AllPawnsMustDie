namespace AllPawnsMustDie
{
    partial class ColorSelectDialog
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
            this.groupBoxSelectColor = new System.Windows.Forms.GroupBox();
            this.radioButtonBlack = new System.Windows.Forms.RadioButton();
            this.radioButtonWhite = new System.Windows.Forms.RadioButton();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBoxSelectColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSelectColor
            // 
            this.groupBoxSelectColor.Controls.Add(this.radioButtonBlack);
            this.groupBoxSelectColor.Controls.Add(this.radioButtonWhite);
            this.groupBoxSelectColor.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSelectColor.Name = "groupBoxSelectColor";
            this.groupBoxSelectColor.Size = new System.Drawing.Size(109, 75);
            this.groupBoxSelectColor.TabIndex = 0;
            this.groupBoxSelectColor.TabStop = false;
            this.groupBoxSelectColor.Text = "Select Color";
            // 
            // radioButtonBlack
            // 
            this.radioButtonBlack.AutoSize = true;
            this.radioButtonBlack.Location = new System.Drawing.Point(6, 42);
            this.radioButtonBlack.Name = "radioButtonBlack";
            this.radioButtonBlack.Size = new System.Drawing.Size(52, 17);
            this.radioButtonBlack.TabIndex = 1;
            this.radioButtonBlack.Text = "&Black";
            this.radioButtonBlack.UseVisualStyleBackColor = true;
            // 
            // radioButtonWhite
            // 
            this.radioButtonWhite.AutoSize = true;
            this.radioButtonWhite.Checked = true;
            this.radioButtonWhite.Location = new System.Drawing.Point(6, 19);
            this.radioButtonWhite.Name = "radioButtonWhite";
            this.radioButtonWhite.Size = new System.Drawing.Size(53, 17);
            this.radioButtonWhite.TabIndex = 0;
            this.radioButtonWhite.TabStop = true;
            this.radioButtonWhite.Text = "&White";
            this.radioButtonWhite.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(46, 93);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "&Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // ColorSelectDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(133, 128);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxSelectColor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ColorSelectDialog";
            this.Text = "Player Color";
            this.groupBoxSelectColor.ResumeLayout(false);
            this.groupBoxSelectColor.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSelectColor;
        private System.Windows.Forms.RadioButton radioButtonBlack;
        private System.Windows.Forms.RadioButton radioButtonWhite;
        private System.Windows.Forms.Button buttonOk;
    }
}