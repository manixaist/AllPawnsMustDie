using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /*
     * This file contains commonly defined enums and structs for use throughout
     * the project.  If a struct or enum is fully contained privately within a
     * class, then it doesn't belong here.
     * 
     * If it's accessed by a variety of classes via parameters to methods 
     * or properties, then it very much belongs in here.
     */

    #region Public Enums
    /// <summary>
    /// Chess piece color, only two options, pretty well known....
    /// </summary>
    public enum PieceColor
    {
        /// <summary>White pieces</summary>
        White,
        /// <summary>Black pieces</summary>
        Black
    };

    /// <summary>
    /// Filthy peasant or member of the nobility?
    /// </summary>
    public enum PieceClass
    {
        /// <summary>King - it's good to be him, but movement is limited</summary>
        King,
        /// <summary>Queen - Combines the powers of the Rook and Bishops </summary>
        Queen,
        /// <summary>Rook - Long operator along vert and horz lines</summary>
        Rook,
        /// <summary>Bishop - Long operator along diagonal lines.</summary>
        Bishop,
        /// <summary>Knight - Only piece that can jump in normal play.  Moves like an 'L'</summary>
        Knight,
        /// <summary>Pawn - Peasant class, moves forward only, captures on the diagonal, can be promoted</summary>
        Pawn,
        /// <summary>Not a true piece, but rather a temporary target for Pawns exposed to en-passant capture</summary>
        EnPassantTarget
    };

    /// <summary>
    /// Which way should the board be drawn?
    /// </summary>
    public enum BoardOrientation
    {
        /// <summary>Board is drawn with the White pieces on the bottom</summary>
        WhiteOnBottom,
        /// <summary>Board is drawn with the Black pieces on the bottom</summary>
        BlackOnBottom
    };

    /// <summary>
    /// The Chess Board is split into two sides, King and Queen (left or right)
    /// depending on which color you are.
    /// </summary>
    [Flags]
    public enum BoardSide
    {
        /// <summary>No side specified</summary>
        None = 0,
        /// <summary>Kingside of the board; Files [e-h]</summary>
        King = 1,
        /// <summary>Kingside of the board; Files [a-d]</summary>
        Queen = 2
    };

    /// <summary>
    /// Possible game results
    /// </summary>
    public enum GameResult
    {
        /// <summary>White wins! (black mated)</summary>
        WhiteWins,
        /// <summary>Black wins! (white mated)</summary>
        BlackWins,
        /// <summary>No one wins!</summary>
        Stalemate,
        /// <summary>No one wins by move count or mutual decision!</summary>
        Draw,
        /// <summary>No winner or loser yet</summary>
        Contested,
    };
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
        /// Copy constructor
        /// </summary>
        /// <param name="oldFile"></param>
        public PieceFile(PieceFile oldFile)
        {
            pieceFile = oldFile.pieceFile;
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
            return (Convert.ToInt16(pieceFile) - Convert.ToInt16('a')) + 1;
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

    /// <summary>
    /// Wraps a board location
    /// </summary>
    public struct BoardSquare
    {
        /// <summary>
        /// Checks if a square is valid.  You cannot instantiate an invalid
        /// PieceFile object, so passing that form is meaningless, though
        /// the rank could be bad
        /// </summary>
        /// <param name="file">PieceFile.ToInt() most likely [1-8]</param>
        /// <param name="rank">[1-8]</param>
        /// <returns>true if the square is valid (located on the board)</returns>
        public static bool IsValid(int file, int rank)
        {
            return (((file <= 8) && (file > 0)) &&
                    ((rank <= 8) && (rank > 0)));
        }

        /// <summary>
        /// Save the file, rank for the location
        /// </summary>
        /// <param name="file">[a-h]</param>
        /// <param name="rank">[1-8]</param>
        public BoardSquare(PieceFile file, int rank)
        {
            pieceFile = file;
            pieceRank = rank;
        }

        /// <summary>
        /// returns san string for square e.g. "e2"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(File.ToString(), Rank.ToString());
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="obj">object testing</param>
        /// <returns>true if obj is the same as this instance</returns>
        public override bool Equals(System.Object obj)
        {
            return (obj is BoardSquare) && (this == (BoardSquare)obj); ;
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <returns>hashcode for the object</returns>
        public override int GetHashCode()
        {
            return pieceFile.GetHashCode() | pieceRank;
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="p1">object1</param>
        /// <param name="p2">object2</param>
        /// <returns>true if the value of object1 and object2 are the same</returns>
        public static bool operator ==(BoardSquare p1, BoardSquare p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            return ((p1.File == p2.File) && (p1.Rank == p2.Rank));
        }

        /// <summary>
        /// Override for equality tests
        /// </summary>
        /// <param name="p1">Object1</param>
        /// <param name="p2">Onject2</param>
        /// <returns>True if the values of Object1 and Object2 are NOT the same</returns>
        public static bool operator !=(BoardSquare p1, BoardSquare p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Returns the File for the square
        /// </summary>
        public PieceFile File { get { return pieceFile; } }

        /// <summary>
        /// Returns the Rank for the square
        /// </summary>
        public int Rank { get { return pieceRank; } }

        private PieceFile pieceFile;
        private int pieceRank;
    }

    /// <summary>
    /// Holds more detailed information on a single move for the GUI.
    /// </summary>
    public struct MoveInformation
    {
        /// <summary>
        /// Create a new MoveInformation struct and fill in the minumum
        /// required information
        /// </summary>
        /// <param name="start">starting square</param>
        /// <param name="end">ending square</param>
        /// <param name="deployed">true if piece has ever moved, prior to this move</param>
        /// <param name="oldFEN">last FEN for board</param>
        public MoveInformation(BoardSquare start, BoardSquare end, bool deployed, string oldFEN)
        {
            prevFEN = oldFEN;
            ancillaryPiece = null;
            promotionClass = PieceClass.King; // Invalid promotion job
            startSquare = start;
            endSquare = end;
            firstMove = (deployed == false);
            color = PieceColor.White; // assume white
            castlingRights = BoardSide.None;
            isCapture = false;
            isCastle = false;
        }

        /// <summary>
        /// Return the move in a SAN friendly format
        /// </summary>
        /// <returns>SAN engine-friendly format e.g. e2e4 or d7d8q, etc</returns>
        public override string ToString()
        {
            string result = String.Concat(startSquare.File.ToString(), startSquare.Rank.ToString(),
                endSquare.File.ToString(), endSquare.Rank.ToString());
            if (IsPromotion)
            {
                result = String.Concat(result, ChessGame.PieceClassToPromotionChar(promotionClass));
            }
            return result;
        }

        /// <summary>
        /// If not null, the piece that was captured on this move.
        /// </summary>
        public ChessPiece CapturedPiece
        {
            get
            {
                if (!isCapture) { throw new InvalidOperationException(); }
                return ancillaryPiece;
            }
            set
            {
                if (value != null) // ignore nulls
                {
                    if (isCastle) { throw new InvalidOperationException(); }
                    isCapture = true;
                    ancillaryPiece = value;
                }
            }
        }

        /// <summary>
        /// If not null, the Rook associated with the castle
        /// </summary>
        public ChessPiece CastlingRook
        {
            get
            {
                if (!isCastle) { throw new InvalidOperationException(); }
                return ancillaryPiece;
            }
            set
            {
                if (value != null) // ignore nulls
                {
                    if (isCapture) { throw new InvalidOperationException(); }
                    if (value.Job != PieceClass.Rook) { throw new ArgumentException(); }
                    isCastle = true;
                    ancillaryPiece = value;
                }
            }
        }

        /// <summary>
        /// Property for the promotion job
        /// </summary>
        public PieceClass PromotionJob
        {
            get { return promotionClass; }
            set { promotionClass = value; }
        }

        /// <summary>
        /// Color of the move
        /// </summary>
        public PieceColor Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Castling rights prior to this move
        /// </summary>
        public BoardSide CastlingRights
        {
            get { return castlingRights; }
            set { castlingRights = value; }
        }

        /// <summary>Returns the start square for the move</summary>
        public BoardSquare Start { get { return startSquare; } }

        /// <summary>Returns the ending square for the move</summary>
        public BoardSquare End { get { return endSquare; } }

        /// <summary>True if this is the piece's first move</summary>
        public bool FirstMove { get { return firstMove; } }

        /// <summary>True if the move is a promotion move</summary>
        public bool IsPromotion { get { return (promotionClass != PieceClass.King); } }

        /// <summary>true if move was a capture</summary>
        public bool IsCapture { get { return isCapture; } }

        /// <summary>true if move was a castle</summary>
        public bool IsCastle { get { return isCastle; } }

        /// <summary>board FEN prior to move</summary>
        public string PreviousFEN { get { return prevFEN; } }

        BoardSquare startSquare;
        BoardSquare endSquare;
        ChessPiece ancillaryPiece;
        PieceClass promotionClass;
        PieceColor color;
        BoardSide castlingRights;
        bool firstMove;
        bool isCapture;
        bool isCastle;
        string prevFEN;
    }
    #endregion
}
