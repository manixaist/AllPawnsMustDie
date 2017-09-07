using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
    /// <summary>
    /// This is the main Window (Form) for the Application.  A minimal amount of
    /// work should be done here, routing most UI events to a subclass to deal
    /// with.  This is (or runs on) the UI thread, so no blocking work.
    /// </summary>
    public partial class APMD_Form : Form
    {
        public APMD_Form()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Form needs painting.  This routes to the ChessGame class
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">EventArgs, mostly we care about Graphics</param>
        private void APMD_Form_Paint(object sender, PaintEventArgs e)
        {
            chessGame?.Render(e.Graphics);
        }

        /// <summary>
        /// Set the form client size if we know it.  Otherwise we must wait until
        /// we load an engine
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void APMD_Form_Load(object sender, EventArgs e)
        {
            // Set the size of the client area - we won't have this class yet
            // since there is no caching of the last used engine so this cannot
            // be created (since we don't have the path to the engine)
            if (chessGame != null)
            {
                ClientSize = ChessGame.RequestedSize;
            }
        }

        /// <summary>
        /// Invoked when the mouse button is released
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Used to get (X, Y) of the mouse</param>
        private void APMD_Form_MouseUp(object sender, MouseEventArgs e)
        {
            // MouseUp allows getting the button and the coordinates
            // MouseDown does as well, but Click and MouseClick do not allow both
            if (e.Button == MouseButtons.Left)
            {
                chessGame?.ProcessClick(e.X, e.Y);
            }
        }

        /// <summary>
        /// Menu handler for "File->New Game"
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void newGameToolStripNewGame_Click(object sender, EventArgs e)
        {
            // If we have a game instance, then start a new game
            // It's possible we don't have this yet if an engine is not loaded.
            if (chessGame != null)
            {
                chessGame.NewGame(PieceColor.White);
            }
            else
            {
                MessageBox.Show(this, "No Engine is loaded yet...", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a dialog to accept a FEN string.  If that is successful, it sends
        /// the FEN string to the ChessGame instance (assuming we have one)
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void newPositionToolStripNewPosition_Click(object sender, EventArgs e)
        {
            if (chessGame != null)
            {
                // Opens a dialog to read the FEN string and then passes it to engine
                FenInputDialog fenDialog = new FenInputDialog();
                DialogResult result = fenDialog.ShowDialog(); // Modal

                if (result == DialogResult.OK)
                {
                    string fenInput = fenDialog.FEN;

                    // TODO - validate the FEN input - sounds like a job for another class...
                    // Calculate the active player
                    // Extract other relevant info (castling rights, move counts, etc)

                    // Really the FEN does not indicate anything for the 'human'
                    // player, but for now we will consider the 'player to move'
                    // in the FEN as the player

                    //chessGame.NewPosition(extractedPlayerColor, fenInput);
                }
            }
            else
            {
                MessageBox.Show(this, "No Engine is loaded yet...", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripExit_Click(object sender, EventArgs e)
        {
            chessGame?.Quit();
            Close(); // Form
        }

        /// <summary>
        /// Menu handler for "File->Load Engine..."
        /// </summary>
        /// <param name="sender">Control sending command, ignored in code</param>
        /// <param name="e">Event sent, also ignored, it was a click</param>
        private void loadEngineToolStripLoadEngine_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select your chess engine exe";
            openFileDialog.Filter = "Exe Files (*.exe) | *.exe";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                fullPathToChessExe = openFileDialog.FileName;

                // Now we have the engine, so create an instance of the game class
                chessGame = new ChessGame(this, fullPathToChessExe);

                // Resize the form if needed
                ClientSize = ChessGame.RequestedSize;

                // Forces a redraw
                Invalidate();
            }
        }

        private string fullPathToChessExe;
        private ChessGame chessGame;

        private void APMD_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            chessGame?.Quit();
        }
    }
}
