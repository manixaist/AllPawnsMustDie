namespace AllPawnsMustDie
{
    partial class NewGameDialog
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
            this.groupBoxPlayerColor = new System.Windows.Forms.GroupBox();
            this.radioButtonBlack = new System.Windows.Forms.RadioButton();
            this.radioButtonWhite = new System.Windows.Forms.RadioButton();
            this.labelThinkTime = new System.Windows.Forms.Label();
            this.numericUpDownThinkTime = new System.Windows.Forms.NumericUpDown();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelFEN = new System.Windows.Forms.Label();
            this.textBoxFEN = new System.Windows.Forms.TextBox();
            this.groupBoxPlayerColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThinkTime)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxPlayerColor
            // 
            this.groupBoxPlayerColor.Controls.Add(this.radioButtonBlack);
            this.groupBoxPlayerColor.Controls.Add(this.radioButtonWhite);
            this.groupBoxPlayerColor.Location = new System.Drawing.Point(12, 12);
            this.groupBoxPlayerColor.Name = "groupBoxPlayerColor";
            this.groupBoxPlayerColor.Size = new System.Drawing.Size(181, 72);
            this.groupBoxPlayerColor.TabIndex = 0;
            this.groupBoxPlayerColor.TabStop = false;
            this.groupBoxPlayerColor.Text = Properties.Resources.PlayerColorGroupBox;
            // 
            // radioButtonBlack
            // 
            this.radioButtonBlack.AutoSize = true;
            this.radioButtonBlack.Location = new System.Drawing.Point(7, 42);
            this.radioButtonBlack.Name = "radioButtonBlack";
            this.radioButtonBlack.Size = new System.Drawing.Size(52, 17);
            this.radioButtonBlack.TabIndex = 1;
            this.radioButtonBlack.TabStop = true;
            this.radioButtonBlack.Text = global::AllPawnsMustDie.Properties.Resources.PlayerColorSelectBlack;
            this.radioButtonBlack.UseVisualStyleBackColor = true;
            // 
            // radioButtonWhite
            // 
            this.radioButtonWhite.AutoSize = true;
            this.radioButtonWhite.Location = new System.Drawing.Point(6, 19);
            this.radioButtonWhite.Name = "radioButtonWhite";
            this.radioButtonWhite.Size = new System.Drawing.Size(53, 17);
            this.radioButtonWhite.TabIndex = 0;
            this.radioButtonWhite.TabStop = true;
            this.radioButtonWhite.Text = global::AllPawnsMustDie.Properties.Resources.PlayerColorSelectWhite;
            this.radioButtonWhite.UseVisualStyleBackColor = true;
            // 
            // labelThinkTime
            // 
            this.labelThinkTime.AutoSize = true;
            this.labelThinkTime.Location = new System.Drawing.Point(9, 99);
            this.labelThinkTime.Name = "labelThinkTime";
            this.labelThinkTime.Size = new System.Drawing.Size(121, 13);
            this.labelThinkTime.TabIndex = 1;
            this.labelThinkTime.Text = Properties.Resources.EngineThinkTimeLabel;
            // 
            // numericUpDownThinkTime
            // 
            this.numericUpDownThinkTime.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownThinkTime.Location = new System.Drawing.Point(139, 97);
            this.numericUpDownThinkTime.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownThinkTime.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownThinkTime.Name = "numericUpDownThinkTime";
            this.numericUpDownThinkTime.Size = new System.Drawing.Size(54, 20);
            this.numericUpDownThinkTime.TabIndex = 2;
            this.numericUpDownThinkTime.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(119, 170);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = Properties.Resources.Ok;
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(38, 170);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = Properties.Resources.Cancel;
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelFEN
            // 
            this.labelFEN.AutoSize = true;
            this.labelFEN.Location = new System.Drawing.Point(9, 128);
            this.labelFEN.Name = "labelFEN";
            this.labelFEN.Size = new System.Drawing.Size(116, 13);
            this.labelFEN.TabIndex = 5;
            this.labelFEN.Text = Properties.Resources.FENInputLabel;
            // 
            // textBoxFEN
            // 
            this.textBoxFEN.Location = new System.Drawing.Point(12, 144);
            this.textBoxFEN.Name = "textBoxFEN";
            this.textBoxFEN.Size = new System.Drawing.Size(181, 20);
            this.textBoxFEN.TabIndex = 6;
            // 
            // NewGameDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(206, 201);
            this.Controls.Add(this.textBoxFEN);
            this.Controls.Add(this.labelFEN);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.numericUpDownThinkTime);
            this.Controls.Add(this.labelThinkTime);
            this.Controls.Add(this.groupBoxPlayerColor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewGameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = Properties.Resources.NewGameTitle;
            this.groupBoxPlayerColor.ResumeLayout(false);
            this.groupBoxPlayerColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThinkTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxPlayerColor;
        private System.Windows.Forms.RadioButton radioButtonBlack;
        private System.Windows.Forms.RadioButton radioButtonWhite;
        private System.Windows.Forms.Label labelThinkTime;
        private System.Windows.Forms.NumericUpDown numericUpDownThinkTime;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelFEN;
        private System.Windows.Forms.TextBox textBoxFEN;
    }
}