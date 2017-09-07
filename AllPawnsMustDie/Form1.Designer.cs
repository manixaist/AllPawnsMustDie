namespace AllPawnsMustDie
{
    partial class APMD_Form
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

            if (disposing && (chessGame != null))
            {
                chessGame.Dispose();
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
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPositiionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameToolStripNewGame = new System.Windows.Forms.ToolStripMenuItem();
            this.newPositionToolStripNewPosition = new System.Windows.Forms.ToolStripMenuItem();
            this.loadEngineToolStripLoadEngine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripExit = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFENToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // newGameToolStripMenuItem
            // 
            this.newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // newPositiionToolStripMenuItem
            // 
            this.newPositiionToolStripMenuItem.Name = "newPositiionToolStripMenuItem";
            this.newPositiionToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(484, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newGameToolStripNewGame,
            this.newPositionToolStripNewPosition,
            this.loadEngineToolStripLoadEngine,
            this.toolStripSeparator2,
            this.exitToolStripExit});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem1.Text = "&File";
            // 
            // newGameToolStripNewGame
            // 
            this.newGameToolStripNewGame.Name = "newGameToolStripNewGame";
            this.newGameToolStripNewGame.Size = new System.Drawing.Size(153, 22);
            this.newGameToolStripNewGame.Text = "&New Game";
            this.newGameToolStripNewGame.Click += new System.EventHandler(this.newGameToolStripNewGame_Click);
            // 
            // newPositionToolStripNewPosition
            // 
            this.newPositionToolStripNewPosition.Name = "newPositionToolStripNewPosition";
            this.newPositionToolStripNewPosition.Size = new System.Drawing.Size(153, 22);
            this.newPositionToolStripNewPosition.Text = "New &Position...";
            this.newPositionToolStripNewPosition.Click += new System.EventHandler(this.newPositionToolStripNewPosition_Click);
            // 
            // loadEngineToolStripLoadEngine
            // 
            this.loadEngineToolStripLoadEngine.Name = "loadEngineToolStripLoadEngine";
            this.loadEngineToolStripLoadEngine.Size = new System.Drawing.Size(153, 22);
            this.loadEngineToolStripLoadEngine.Text = "&Load Engine...";
            this.loadEngineToolStripLoadEngine.Click += new System.EventHandler(this.loadEngineToolStripLoadEngine_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(150, 6);
            // 
            // exitToolStripExit
            // 
            this.exitToolStripExit.Name = "exitToolStripExit";
            this.exitToolStripExit.Size = new System.Drawing.Size(153, 22);
            this.exitToolStripExit.Text = "E&xit";
            this.exitToolStripExit.Click += new System.EventHandler(this.exitToolStripExit_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFENToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // showFENToolStripMenuItem
            // 
            this.showFENToolStripMenuItem.Enabled = false;
            this.showFENToolStripMenuItem.Name = "showFENToolStripMenuItem";
            this.showFENToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.showFENToolStripMenuItem.Text = "Show &FEN";
            // 
            // APMD_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "APMD_Form";
            this.Text = "All Pawns Must Die";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.APMD_Form_FormClosing);
            this.Load += new System.EventHandler(this.APMD_Form_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.APMD_Form_Paint);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.APMD_Form_MouseUp);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newPositiionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripNewGame;
        private System.Windows.Forms.ToolStripMenuItem newPositionToolStripNewPosition;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripExit;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFENToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadEngineToolStripLoadEngine;
    }
}

