namespace AllPawnsMustDie
{
    partial class PromotionDialog
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
            this.radioButtonQueen = new System.Windows.Forms.RadioButton();
            this.radioButtonBishop = new System.Windows.Forms.RadioButton();
            this.radioButtonRook = new System.Windows.Forms.RadioButton();
            this.radioButtonKnight = new System.Windows.Forms.RadioButton();
            this.groupBoxPromotion = new System.Windows.Forms.GroupBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBoxPromotion.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtonQueen
            // 
            this.radioButtonQueen.AutoSize = true;
            this.radioButtonQueen.Checked = true;
            this.radioButtonQueen.Location = new System.Drawing.Point(6, 26);
            this.radioButtonQueen.Name = "radioButtonQueen";
            this.radioButtonQueen.Size = new System.Drawing.Size(57, 17);
            this.radioButtonQueen.TabIndex = 0;
            this.radioButtonQueen.TabStop = true;
            this.radioButtonQueen.Text = global::AllPawnsMustDie.Properties.Resources.PromotionQueen;
            this.radioButtonQueen.UseVisualStyleBackColor = true;
            // 
            // radioButtonBishop
            // 
            this.radioButtonBishop.AutoSize = true;
            this.radioButtonBishop.Location = new System.Drawing.Point(6, 72);
            this.radioButtonBishop.Name = "radioButtonBishop";
            this.radioButtonBishop.Size = new System.Drawing.Size(57, 17);
            this.radioButtonBishop.TabIndex = 2;
            this.radioButtonBishop.TabStop = true;
            this.radioButtonBishop.Text = global::AllPawnsMustDie.Properties.Resources.PromotionBishop;
            this.radioButtonBishop.UseVisualStyleBackColor = true;
            // 
            // radioButtonRook
            // 
            this.radioButtonRook.AutoSize = true;
            this.radioButtonRook.Location = new System.Drawing.Point(6, 49);
            this.radioButtonRook.Name = "radioButtonRook";
            this.radioButtonRook.Size = new System.Drawing.Size(51, 17);
            this.radioButtonRook.TabIndex = 1;
            this.radioButtonRook.TabStop = true;
            this.radioButtonRook.Text = global::AllPawnsMustDie.Properties.Resources.PromotionRook;
            this.radioButtonRook.UseVisualStyleBackColor = true;
            // 
            // radioButtonKnight
            // 
            this.radioButtonKnight.AutoSize = true;
            this.radioButtonKnight.Location = new System.Drawing.Point(6, 95);
            this.radioButtonKnight.Name = "radioButtonKnight";
            this.radioButtonKnight.Size = new System.Drawing.Size(55, 17);
            this.radioButtonKnight.TabIndex = 3;
            this.radioButtonKnight.TabStop = true;
            this.radioButtonKnight.Text = global::AllPawnsMustDie.Properties.Resources.PromotionKnight;
            this.radioButtonKnight.UseVisualStyleBackColor = true;
            // 
            // groupBoxPromotion
            // 
            this.groupBoxPromotion.Controls.Add(this.radioButtonKnight);
            this.groupBoxPromotion.Controls.Add(this.radioButtonRook);
            this.groupBoxPromotion.Controls.Add(this.radioButtonBishop);
            this.groupBoxPromotion.Controls.Add(this.radioButtonQueen);
            this.groupBoxPromotion.Location = new System.Drawing.Point(12, 12);
            this.groupBoxPromotion.Name = "groupBoxPromotion";
            this.groupBoxPromotion.Size = new System.Drawing.Size(92, 127);
            this.groupBoxPromotion.TabIndex = 4;
            this.groupBoxPromotion.TabStop = false;
            this.groupBoxPromotion.Text = "Choose...";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(12, 145);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(92, 23);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = global::AllPawnsMustDie.Properties.Resources.Ok;
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PromotionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(118, 182);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxPromotion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PromotionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Promotion";
            this.groupBoxPromotion.ResumeLayout(false);
            this.groupBoxPromotion.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonQueen;
        private System.Windows.Forms.RadioButton radioButtonBishop;
        private System.Windows.Forms.RadioButton radioButtonRook;
        private System.Windows.Forms.RadioButton radioButtonKnight;
        private System.Windows.Forms.GroupBox groupBoxPromotion;
        private System.Windows.Forms.Button buttonOk;
    }
}