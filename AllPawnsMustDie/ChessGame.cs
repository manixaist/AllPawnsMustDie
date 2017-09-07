using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
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
    [Flags] public enum BoardSide { King, Queen };

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

        public PieceFile(int file)
        {
            if (file < 1 || file > 8)
            {
                throw new ArgumentOutOfRangeException();
            }
            pieceFile = Convert.ToChar(Convert.ToInt16('a') - file);
        }

        /// <summary>
        /// Returns the numeric version of the file.  Files are mapped [a-h]->[1-8]
        /// and align with the rank.  This makes the board a conceptual 2D array
        /// with 1-based offsets
        /// </summary>
        /// <returns>An integer between 1 and 8 inclusive.</returns>
        public int ToInt()
        {
            return Convert.ToInt16('a') - Convert.ToInt16(pieceFile);
        }

        /// <summary>
        /// The stored file (verified as valid)
        /// </summary>
        private char pieceFile;

        /// <summary>
        /// Returns the stored file.  It must have been valid on creation, so this
        /// is ensured to return a char [a-h] and it will always be lowercase
        /// </summary>
        public char File { get { return pieceFile; } }
    }


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
        /// <summary>
        /// Callback for the engine.  This only gets invoked for final responses
        /// and not every output from the engine.
        /// </summary>
        /// <param name="commandResponse">The final response string</param>
        public delegate void EngineCallback(string commandResponse);

        /// <summary>
        /// The ClientSize that ChessGame would like.  Some extra room for now.
        /// </summary>
        public static Size RequestedSize
        {
            get { return new Size(720, 720); }
        }

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
            board = new ChessBoard();
            view = new ChessBoardView(form);

            chessEngine = new ChessEngineWrapper(fullPathToEngine);

            // Uncomment to test orientation flip (defaults to WhiteOnBottom)
            //board.Orientation = BoardOrientation.BlackOnBottom;

            // Set the data for the view
            ((IChessBoardView)view).ViewData = board;

            // Testing the offset code, it could easily draw at (0, 0) for now
            ((IChessBoardView)view).Offset = new Point(25, 50);

            // Tell the engine we want the responses from the commands processed
            chessEngine.Engine.OnChessEngineResponseReceived += ChessEngineResponseReceivedEventHandler;

            chessEngine.Engine.SendCommand("isready", "readyok");
            chessEngine.Engine.SendCommand("uci", "uciok");
            chessEngine.Engine.SendCommand("setoption name Skill Level value 0", "");
            chessEngine.Engine.SendCommand("ucinewgame", "");
        }

        ~ChessGame()
        {
            Dispose();
        }

        public void Dispose()
        {
            ((IDisposable)view).Dispose();
            ((ChessEngineWrapper)chessEngine).Dispose();
            GC.SuppressFinalize(this);
        }

        private void ChessEngineResponseReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
        {
            // Raised on completion of commands

            // Right now this can be "uciready" "readyok" or "bestmove"
            // many commands give no response, so "isready/readyok" is used
            // to sync up with the chess engine
        }

        private PieceColor playerColor;
        
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

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="playerColor">Color for the human player</param>
        public void NewGame(PieceColor playerColor)
        {
            this.playerColor = playerColor;
            chessEngine.NewGame();
        }

        /// <summary>
        /// Create a new position
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
            // State dependent - but a minumum is converting this to a board cell
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

        private ChessBoard board;
        private ChessBoardView view;
        private ChessEngineWrapper chessEngine;
        private Form form;
    }
}
