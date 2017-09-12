using System;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
    #region Public Enums
    /// <summary>
    /// Chess piece color, only two options, pretty well known....
    /// </summary>
    public enum PieceColor { White, Black };

    /// <summary>
    /// Filthy peasant or member of the nobility?
    /// </summary>
    public enum PieceClass { King, Queen, Rook, Bishop, Knight, Pawn, EnPassantTarget };

    /// <summary>
    /// Which way should the board be drawn?
    /// </summary>
    public enum BoardOrientation { WhiteOnBottom, BlackOnBottom };

    /// <summary>
    /// The Chess Board is split into two sides, King and Queen (left or right)
    /// depending on which color you are.
    /// </summary>
    [Flags] public enum BoardSide { None = 0, King = 1, Queen = 2 };
    #endregion

    #region Public Structs
    /// <summary>
    /// Encapsulates a file on the Chess Board.  files are [a-h] and are basically
    /// "columns" on the board.
    /// </summary>
    public struct PieceFile
    {
        /// <summary>
        /// Initialize the file.  If the char is not in [a-h], ArgumentOutOfRangeException
        /// is thrown. 
        /// </summary>
        /// <param name="file">name of the file [a-h]. This is case-insensitive</param>
        public PieceFile(char file)
        {
            char f = Char.ToLower(file);
            if (f < 'a' || f > 'h')
            {
                throw new ArgumentOutOfRangeException();
            }
            pieceFile = f;
        }

        /// <summary>
        /// Initialize the file.  If the int is not in [1-8], ArgumentOutOfRangeException
        /// </summary>
        /// <param name="file">int version of the file [1-8]</param>
        public PieceFile(int file)
        {
            if (file < 1 || file > 8)
            {
                throw new ArgumentOutOfRangeException();
            }
            pieceFile = Convert.ToChar(Convert.ToInt16('a') + file - 1);
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="obj">object testing</param>
        /// <returns>true if obj is the same as this instance</returns>
        public override bool Equals(System.Object obj)
        {
            return (obj is PieceFile) && (this == (PieceFile)obj);
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <returns>hashcode for the object</returns>
        public override int GetHashCode()
        {
            return pieceFile.GetHashCode();
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="p1">object1</param>
        /// <param name="p2">object2</param>
        /// <returns>true if the value of object1 and object2 are the same</returns>
        public static bool operator ==(PieceFile p1, PieceFile p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            return p1.File == p2.File;
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="p1">Object1</param>
        /// <param name="p2">Onject2</param>
        /// <returns>True if the values of Object1 and Object2 are NOT the same</returns>
        public static bool operator !=(PieceFile p1, PieceFile p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Returns the numeric version of the file.  Files are mapped [a-h]->[1-8]
        /// and align with the rank.  This makes the board a conceptual 2D array
        /// with 1-based offsets
        /// </summary>
        /// <returns>An integer between 1 and 8 inclusive.</returns>
        public int ToInt()
        {
            return Convert.ToInt16(pieceFile) - Convert.ToInt16('a');
        }

        /// <summary>
        /// Override to return the string version of the file e.g. "a", "b"..."h"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Convert.ToString(pieceFile);
        }

        /// <summary>
        /// Returns the stored file.  It must have been valid on creation, so this
        /// is ensured to return a char [a-h] and it will always be lowercase
        /// </summary>
        public char File { get { return pieceFile; } }

        /// <summary>
        /// The stored file (verified as valid)
        /// </summary>
        private char pieceFile;
    }
    #endregion

    /// <summary>
    /// ChessGame encapsulates a game "session".  It can either start from scratch
    /// or from a given starting position.  It is responsible for tracking all 
    /// aspects of the game.  A non-exhasutive list: The board (owns the pieces,
    /// and moves, and more), time controls, all UI components like a move history
    /// or captured list, and of course the chess engine.
    /// 
    /// ChessGame is also responsible for routing applicable events from the UI
    /// to the engine, like when a player clicks on a square.
    /// </summary>
    public sealed class ChessGame : IDisposable
    {
        #region Public Events
        /// <summary>
        /// EventHandler (delegate(s)) that will get the response event
        /// this is only used internally to the class
        /// </summary>
        public event EventHandler<EventArgs> OnChessGameSelfPlayGameOver;
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new ChessGame object
        /// </summary>
        /// <param name="clientForm">Windows Form the game will draw to</param>
        /// <param name="fullPathToEngine">Full path the chess engine exe</param>
        public ChessGame(Form clientForm, string fullPathToEngine)
        {
            // Save the form
            form = clientForm;

            // Create the board, the view, and the engine
            chessEngine = new ChessEngineWrapper(fullPathToEngine);
            
            // Subscribe to events from the engine (commands and verbose)
            chessEngine.Engine.OnChessEngineResponseReceived += ChessEngineResponseReceivedEventHandler;
            chessEngine.Engine.OnChessEngineVerboseOutputReceived += ChessEngineVerboseOutputReceivedEventHandler;

            // Initialize the chess engine with optional parameters
            chessEngine.Engine.SendCommandAsync("setoption name Skill Level value 15", "");
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ChessGame()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of disposable objects and unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                chessEngine.Engine.OnChessEngineResponseReceived -= ChessEngineResponseReceivedEventHandler;
                chessEngine.Engine.OnChessEngineVerboseOutputReceived -= ChessEngineVerboseOutputReceivedEventHandler;
                ((IDisposable)view).Dispose();
                ((ChessEngineWrapper)chessEngine).Dispose();
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="playerColor">Color for the human player</param>
        public void NewGame(PieceColor playerColor)
        {
            this.playerColor = playerColor; // save the player color

            // Create the board and view
            board = new ChessBoard();
            view = new ChessBoardView(form);

            // Initialize the board and view
            board.NewGame();

            // Set the data for the view
            ((IChessBoardView)view).ViewData = board;

            // Set the Offset for the view
            ((IChessBoardView)view).Offset = new Point(25, 50);

            // Create and initialize the board and view
            ((IChessBoardView)view).ViewData = board;

            // Override the unicode drawing with bmp images
            ((IChessBoardView)view).SetBitmapImages(new Bitmap(Properties.Resources.chesspieces), new Size(64,64));

            // Initialize the engine with a new game
            chessEngine.NewGame();
        }

        /// <summary>
        /// Start requesting moves from the engine for the board.  This will alternate
        /// and apply moves for both sides, not just the player color (human)
        /// </summary>
        public void StartEngineSelfPlay()
        {
            selfPlay = true;
            UpdateEnginePosition();
        }

        /// <summary>
        /// Ask the engine to evaluate the current board for the best move.  This
        /// is an asynchronous call.  ChessEngineResponseReceivedEventHandler will
        /// fired when the command is processed
        /// </summary>
        public void GetBestMoveAsync()
        {
            // TODO - Really this layer should not have this knoweledge, but the only
            // supported engine type is UCI right now
            chessEngine.Engine.SendCommandAsync(MoveCommandWithTime, UCIChessEngine.BestMoveResponse);
        }

        /// <summary>
        /// Create a new position based on a FEN
        /// </summary>
        /// <param name="playerColor">Color for the human player</param>
        /// <param name="fenNotation">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        public void NewPosition(PieceColor playerColor, string fenNotation)
        {
            // Create a whole new game using the playerColor as the human
            // but staring with the position given
            this.playerColor = playerColor;
            chessEngine.NewPosition(fenNotation);
        }

        /// <summary>
        /// User clicked somewhere on the client area for ChessGame
        /// </summary>
        /// <param name="x">x coordinate relative to top-left</param>
        /// <param name="y">y coordinate relative to top-left</param>
        public void ProcessClick(int x, int y)
        {
            // Do nothing at all here for now.  The application only supports
            // the engine playing with itself for now, so clicking is meaningless

            // Later, this is where we would convert this (X,Y) to a square and
            // then based on the current state, check if this is a valid piece, 
            // or a legal followup move, or just a click elsewhere
            // TODO...
        }

        /// <summary>
        /// Tell the view to render itself
        /// </summary>
        /// <param name="g">Graphics object for the form</param>
        public void Render(Graphics g)
        {
            ((IChessBoardView)view).Render(g);
        }

        /// <summary>
        /// Quit
        /// </summary>
        public void Quit()
        {
            // Close the engine (external process)
            chessEngine.Quit();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// EventHandler for the chess engine.  This handler is invoked (after
        /// subscription) when the engine has finished processing a command
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Contains the final response string</param>
        private void ChessEngineResponseReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
        {
            Debug.WriteLine(String.Concat("Response: ", e.Response));

            // If this is true, it means we're updating our position with the engine
            // (e.g. syncing up after a move was applied locally to our object)
            if (updatingPosition)
            {
                updatingPosition = false;
                // Now trigger getting the next move from the engine and exit
                GetBestMoveAsync();
            }
            else if (selfPlay && e.Response.StartsWith("Checkers:"))
            {
                // Fire event if there is a listener
                if (OnChessGameSelfPlayGameOver != null)
                {
                    OnChessGameSelfPlayGameOver(this, null);
                }
            }
            // If this is a "bestmove" response, apply it, then ask for the next one
            // We also need to update the ChessBoard class so the view updates
            else if (selfPlay && e.Response.StartsWith("bestmove"))
            {
                thinkingIndex = 0;  // index counter for simple progress text

                // Get it from the engine
                string bestMove = chessEngine.BestMove;
                if ((String.Compare(bestMove, "(none)") == 0) || // Probably mate
                    (board.HalfMoveCount >= HalfMovesUntilDraw)) // Propably spinning
                {
                    if (board.HalfMoveCount >= HalfMovesUntilDraw)
                    {
                        Debug.WriteLine("Draw by 50 moves rule...");
                    }

                    // Debug at the end to compare the board states
                    chessEngine.Engine.SendCommandAsync("d", "Checkers:");
                }
                else
                {
                    PieceFile startFile = new PieceFile(bestMove[0]);
                    int startRank = Convert.ToInt16(bestMove[1]) - Convert.ToInt16('0');
                    PieceFile destFile = new PieceFile(bestMove[2]);
                    int destRank = Convert.ToInt16(bestMove[3]) - Convert.ToInt16('0');

                    // When coming from the engine, we get the promotion detection for free
                    if (bestMove.Length == 5)
                    {
                        // Applied on the next move
                        board.PromotePiece(startFile, startRank, destFile, destRank, ChessBoard.PieceClassFromFen(bestMove[4]));
                    }

                    // Always returns true now
                    board.MovePiece(startFile, startRank, destFile, destRank);
                    board.Moves.Add(bestMove);
                    form.Invalidate();
                    
                    Debug.WriteLine(String.Format("Fullmoves: {0}", board.FullMoveCount));
                    Debug.WriteLine(String.Format("Halfmoves: {0}", board.HalfMoveCount));

                    // Start getting the next move
                    UpdateEnginePosition();
                }
            }
        }

        /// <summary>
        /// EventHandler for the chess engine.This handler is invoked(after
        /// subscription) when the engine has received any output from the engine
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">string response from the chess engine</param>
        private void ChessEngineVerboseOutputReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
        {
            // Removing the thinking lines to declutter the debug outbput
            if (!e.Response.StartsWith("info"))
            {
                Debug.WriteLine(String.Concat("<=Engine: ", e.Response));
            }

            // Get the control name w're using to output verbose for now (it's a label)
            Control verboseControl = form.Controls[APMD_Form.VerboseOutputControlName];

            // Build the progress text bar
            StringBuilder sb = new StringBuilder("Thinking: [");
            sb.Append('\u25AB', 75);
            sb.Append(']');
            // Replace the current spinning index to the marker character
            sb.Replace('\u25AB', '\u25AA', 11, thinkingIndex);
            thinking = sb.ToString();

            // Wrap the progress counter index.  It moves between the '[' and ']' chars
            if (thinkingIndex < 75)
            {
                thinkingIndex++;
            }
            else
            {
                thinkingIndex = 0;
            }

            // Check if we're on the UI thread, the answer is almostly certainly no
            if (verboseControl.InvokeRequired)
            {
                // Invoke is synchronous - this will block this thread
                verboseControl.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread now, so this is safe
                    verboseControl.Text = thinking;
                });
            }
            else
            {
                // If for some reason we are on the UI thread, then we can just update it
                verboseControl.Text = thinking;
            }
        }

        /// <summary>
        /// Update the position with the chess engine.  Once a move is applied wit the
        /// board, then engine needs to know, so it can analyze the next move whether
        /// this came from a player or the engine iteself
        /// </summary>
        private void UpdateEnginePosition()
        {
            // Get the current moves list from the board
            // build the command string to find the best move
            // send the command.  This block of code will be reused in
            // ChessEngineResponseReceivedEventHandler as well
            //chessEngine.Engine.SendCommand()
            string movesList = String.Join(" ", board.Moves.ToArray());

            updatingPosition = true;
            chessEngine.Engine.SendCommandAsync(String.Concat("position startpos moves ", movesList), "");
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The ClientSize that ChessGame would like.  Some extra room for now.
        /// </summary>
        public static Size RequestedSize
        {
            get
            {
                int delta = ChessBoardView.BoardSizeInPixels + 50;
                return new Size(delta, delta + 50);
            }

        }

        /// <summary>
        /// Color for the human player
        /// </summary>
        public PieceColor PlayerColor
        {
            get { return playerColor; }
        }

        /// <summary>
        /// Color for the active player.
        /// </summary>
        public PieceColor ActivePlayer
        {
            get { return board.ActivePlayer; }
        }
        #endregion

        #region Private Fields
        private bool disposed = false;
        private bool selfPlay = false;
        private string thinking = String.Empty;
        private int thinkingIndex = 0;
        private bool updatingPosition = false;
        private PieceColor playerColor;
        private ChessBoard board;
        private ChessBoardView view;
        private ChessEngineWrapper chessEngine;
        private Form form;
        private static int HalfMovesUntilDraw = 50;
        private static string MoveCommandWithTime = "go movetime 250";
        #endregion
    }
}
