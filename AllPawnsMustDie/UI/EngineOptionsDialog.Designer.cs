namespace AllPawnsMustDie
{
    partial class EngineOptionsDialog
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
            this.checkBoxReduceEngineStrength = new System.Windows.Forms.CheckBox();
            this.ToolTipCheckBox = new System.Windows.Forms.ToolTip(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxReduceEngineStrength
            // 
            this.checkBoxReduceEngineStrength.AutoSize = true;
            this.checkBoxReduceEngineStrength.Location = new System.Drawing.Point(13, 13);
            this.checkBoxReduceEngineStrength.Name = "checkBoxReduceEngineStrength";
            this.checkBoxReduceEngineStrength.Size = new System.Drawing.Size(176, 17);
            this.checkBoxReduceEngineStrength.TabIndex = 0;
            this.checkBoxReduceEngineStrength.Text = global::AllPawnsMustDie.Properties.Resources.ReduceEngineStrengthCheckboxLabel;
            this.ToolTipCheckBox.SetToolTip(this.checkBoxReduceEngineStrength, global::AllPawnsMustDie.Properties.Resources.ReduceEngineStrenghExplanationHelpText);
            this.checkBoxReduceEngineStrength.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(197, 56);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = global::AllPawnsMustDie.Properties.Resources.Ok;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // EngineOptionsDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 91);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxReduceEngineStrength);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EngineOptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxReduceEngineStrength;
        private System.Windows.Forms.ToolTip ToolTipCheckBox;
        private System.Windows.Forms.Button buttonOK;
    }
}