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
        #region Public Methods
        /// <summary>
        /// Initialize the main form
        /// </summary>
        public APMD_Form()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Methods
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
            ColorSelectDialog playerColorSelectionDialog = new ColorSelectDialog();
            playerColorSelectionDialog.ShowDialog(this);
            NewGame(String.Empty, playerColorSelectionDialog.PlayerColor);
        }

        /// <summary>
        /// Opens a dialog to accept a FEN string.  If that is successful, it sends
        /// the FEN string to the ChessGame instance (assuming we have one)
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void newPositionToolStripNewPosition_Click(object sender, EventArgs e)
        {
            // Opens a dialog to read the FEN string and then passes it to engine
            FenInputDialog fenDialog = new FenInputDialog();
            DialogResult result = fenDialog.ShowDialog(); // Modal

            if (result == DialogResult.OK)
            {
                string fenInput = fenDialog.FEN;
                // For now just pass it and assume it's valid
                NewGame(fenInput, PieceColor.White);

                // TODO - validate the FEN input - sounds like a job for another class...
                // Calculate the active player
                // Extract other relevant info (castling rights, move counts, etc)

                // Really the FEN does not indicate anything for the 'human'
                // player, but for now we will consider the 'player to move'
                // in the FEN as the player
            }
        }

        /// <summary>
        /// Menu handler for File->Exit
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void exitToolStripExit_Click(object sender, EventArgs e)
        {
            if (chessGame != null)
            {
                chessGame.OnChessGameSelfPlayGameOver -= ChessGameSelfPlayGameOverEventHandler;
            }
            chessGame?.Quit();  // Quit the game
            Close();            // Close the form
        }

        /// <summary>
        /// Menu handler for "File->Load Engine..."
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void loadEngineToolStripLoadEngine_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select your chess engine exe";
            openFileDialog.Filter = "Exe Files (*.exe) | *.exe";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                fullPathToChessExe = openFileDialog.FileName;

                // Resize the form if needed
                ClientSize = ChessGame.RequestedSize;

                // Trigger Paint event
                Invalidate();
            }
        }

        /// <summary>
        /// Menu handler for File->Self Play
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void selfPlayToolStripSelfPlay_Click(object sender, EventArgs e)
        {
            NewGame(String.Empty, PieceColor.White);
            chessGame?.StartEngineSelfPlay();
        }

        /// <summary>
        /// Event handler fired when a self play game has finished
        /// </summary>
        /// <param name="sender">Ignored/passed through</param>
        /// <param name="e">Ignored/passed through</param>
        private void ChessGameSelfPlayGameOverEventHandler(object sender, EventArgs e)
        {
            // Start a new game - for now just forward it to the menu handler.
            // Really we could subcribe there directly, but this will allow more
            // to happen on this path later
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread now, so this is safe
                    selfPlayToolStripSelfPlay_Click(sender, e);
                });
            }
        }

        /// <summary>
        /// Starts a new game if an engine is loaded
        /// </summary>
        private void NewGame(string fen, PieceColor playerColor)
        {
            if (fullPathToChessExe != null)
            {
                // Old game is dead to us
                if (chessGame != null)
                {
                    chessGame.OnChessGameSelfPlayGameOver -= ChessGameSelfPlayGameOverEventHandler;
                }
                chessGame?.Dispose();

                // Now we have the engine path, so create an instance of the game class
                chessGame = new ChessGame(this, fullPathToChessExe);
                chessGame.OnChessGameSelfPlayGameOver += ChessGameSelfPlayGameOverEventHandler;

                if (fen == String.Empty)
                {
                    chessGame.NewGame(playerColor);
                }
                else
                {
                    chessGame.NewPosition(playerColor, fen);
                }

                // Trigger Paint event (draws the initial board)
                Invalidate();
            }
            else
            {
                // No engine loaded, so no new game can be created.  Inform the user
                MessageBox.Show(this, APMD_ErrorNoEngineLoaded, APMD_ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Called when the form is closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void APMD_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (chessGame != null)
            {
                chessGame.OnChessGameSelfPlayGameOver -= ChessGameSelfPlayGameOverEventHandler;
            }
            chessGame?.Quit(); // Quit any current game
        }

        /// <summary>
        /// Attempt to undo the last move made
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void UndoLastMoveToolStripMenuItemUndoLastMove_Click(object sender, EventArgs e)
        {
            chessGame?.UndoLastMove();
            Invalidate();
        }
        #endregion

        #region Public Fields
        /// <summary>
        /// Verbose text label control name - used to write verbose output from the
        /// engine, though currently, it only shows a progress string
        /// </summary>
        public static string VerboseOutputControlName = "labelVerbose";
        #endregion

        #region Private Fields
        private static string APMD_ErrorNoEngineLoaded = "No Engine is loaded yet...";
        private static string APMD_ErrorTitle = "Error";
        private string fullPathToChessExe;
        private ChessGame chessGame;
        #endregion
    }
}
