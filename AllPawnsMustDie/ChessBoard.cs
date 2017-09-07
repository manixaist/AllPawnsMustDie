using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Encapsulates a chess board.  The board owns the pieces and the history
    /// </summary>
    public class ChessBoard
    {
        /// <summary>
        /// Create a new chess board
        /// </summary>
        public ChessBoard()
        {
            NewGame();
        }

        /// <summary>
        /// Reset internal fields
        /// </summary>
        private void Reset()
        {
            moveHistory = new List<string>();
            whitePieces = new List<ChessPiece>();
            blackPieces = new List<ChessPiece>();
            activePlayer = PieceColor.White;
            whiteCastlingRights = BoardSide.King | BoardSide.Queen;
            blackCastlingRights = BoardSide.King | BoardSide.Queen;
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void NewGame()
        {
            Reset();
            CreateAndPlacePieces(InitialFENPosition);
        }

        /// <summary>
        /// Start a new game at a specified starting position
        /// </summary>
        /// <param name="fen">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        public void NewPosition(string fen)
        {
            Reset();
            CreateAndPlacePieces(fen);
        }

        /// <summary>
        /// Sets up the board using the given FEN string.  It's possible not all
        /// pieces are present.
        /// </summary>
        /// <param name="fen">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        private void CreateAndPlacePieces(string fen)
        {
            // String starts on the back rank on the A file then moves Left->Right
            // Top->Bottom
            string fenString = fen.Trim();

            int currentRank = 8; // 8 - back rank for Black
            int currentFile = 1; // A

            int index = 0;
            while (index < fenString.Length)
            {
                char fenChar = fenString[index++];

                if (Char.IsLetter(fenChar))
                {
                    PieceColor color = PieceColor.White;
                    // New piece
                    if (Char.IsLower(fenChar))
                    {
                        // Black
                        color = PieceColor.Black;
                    }

                    ChessPiece newPiece = new ChessPiece(color, PieceClassFromFen(fenChar), new PieceFile(currentFile), currentRank);
                    
                    // Add piece
                    if (color == PieceColor.White)
                    {
                        whitePieces.Add(newPiece);
                    }
                    else
                    {
                        blackPieces.Add(newPiece);
                    }

                    currentFile++;
                }
                else if (Char.IsDigit(fenChar))
                {
                    // advance File the amount of the spaces
                    currentFile += Convert.ToUInt16(fenChar);
                }
                else if (fenChar == '/')
                {
                    // decrement Rank
                    currentRank--;
                    // reset File
                    currentFile = 1;
                }
                else if (char.IsWhiteSpace(fenChar))
                {
                    // Stop here for now
                    break;
                }
            }
        }

        /// <summary>
        /// Determine piece class based on the fen character
        /// </summary>
        /// <param name="fenChar">FEN defined char for a piece</param>
        /// <returns>PieceClass for fen, e.g. King for 'k' or 'K'</returns>
        private PieceClass PieceClassFromFen(Char fenChar)
        {
            PieceClass outClass;
            Char inputChar = Char.ToLower(fenChar);
            switch (inputChar)
            {
                case 'k':
                    outClass = PieceClass.King;
                    break;
                case 'q':
                    outClass = PieceClass.Queen;
                    break;
                case 'r':
                    outClass = PieceClass.Rook;
                    break;
                case 'b':
                    outClass = PieceClass.Bishop;
                    break;
                case 'n':
                    outClass = PieceClass.Knight;
                    break;
                case 'p':
                    outClass = PieceClass.Pawn;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return outClass;
        }

        /// <summary>
        /// Calculates the FEN for the board as is and returns it
        /// </summary>
        /// <returns>FEN for the board</returns>
        public string CurrentPositionAsFEN()
        {
            return "TODO";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startFile"></param>
        /// <param name="startRank"></param>
        /// <param name="targetFile"></param>
        /// <param name="targetRank"></param>
        public bool MovePiece(PieceFile startFile, int startRank, PieceFile targetFile, int targetRank)
        {
            // Validated move as legal (if not, return false)
            //    apply the move
            //    add it to the move history

            // Does move have consequences?
            //    Pawn move, or piece capture? (reset 50 move counter)
            //    Capture? remove captured piece
            //    Does this affect castling rights?
            //    Is the piece promotion elligle? (need a return value for this likely)
            return true;
        }

        private BoardOrientation orientation;

        /// <summary>
        /// Orientation of the board (who is on bottom?)
        /// </summary>
        public BoardOrientation Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        private List<string> moveHistory;

        /// <summary>
        /// List of moves in SAN (standard algebraic notation)
        /// </summary>
        public List<string> Moves { get { return moveHistory; } }

        private List<ChessPiece> whitePieces;
        
        /// <summary>
        /// Set of active white pieces
        /// </summary>
        public List<ChessPiece> WhitePieces { get { return whitePieces; } }

        private List<ChessPiece> blackPieces;

        /// <summary>
        /// Set of active black pieces
        /// </summary>
        public List<ChessPiece> BlackPieces { get { return blackPieces; } }

        private PieceColor activePlayer;

        /// <summary>
        /// Current player's turn
        /// </summary>
        public PieceColor ActivePlayer { get { return activePlayer; } }

        private BoardSide whiteCastlingRights;
        private BoardSide blackCastlingRights;

        /// <summary>
        /// Checks if a given player is allowed to castle
        /// </summary>
        /// <param name="playerColor">Player to check</param>
        /// <param name="side">Side of the board to validate.</param>
        /// <returns>True if the player can castle on the given side</returns>
        public bool CanPlayerCastle(PieceColor playerColor, BoardSide side)
        {
            BoardSide playerSide = (playerColor == PieceColor.White) ? whiteCastlingRights : blackCastlingRights;
            return playerSide.HasFlag(side);
        }

        // FEN for the starting position
        private static String InitialFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    }
}
