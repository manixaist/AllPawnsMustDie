using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Encapsulates a chess piece
    /// </summary>
    public class ChessPiece
    {
        #region Public Methods
        /// <summary>
        /// Checks if the piece is on the back rank (color matters)
        /// </summary>
        /// <param name="piece">ChessPiece to check</param>
        /// <returns>true if the piece is on the back rank (8 for white, 1 for black)</returns>
        public static bool IsOnBackRank(ChessPiece piece)
        {
            return (piece.Color == PieceColor.White) ? (piece.Rank == 8) : (piece.Rank == 1);
        }

        /// <summary>
        /// Create a new ChessPiece
        /// </summary>
        /// <param name="pieceColor">Color</param>
        /// <param name="pieceClass">King, Queen, Rook, etc</param>
        /// <param name="pieceFile">file location</param>
        /// <param name="pieceRank">rank location</param>
        public ChessPiece(PieceColor pieceColor, PieceClass pieceClass, PieceFile pieceFile, int pieceRank)
        {
            if (pieceClass == PieceClass.EnPassantTarget)
            {
                throw new ArgumentException();
            }

            // Save the parameters as fields
            color = pieceColor;
            job = pieceClass;
            rank = pieceRank;
            file = pieceFile;
            deployed = false;
            captured = false;
            isReadyForPromotion = false;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="oldPiece">ChessPiece to copy</param>
        public ChessPiece(ChessPiece oldPiece)
        {
            // Save the parameters as fields
            color = oldPiece.Color;
            job = oldPiece.Job;
            rank = oldPiece.Rank;
            file = new PieceFile(oldPiece.File);
            deployed = oldPiece.Deployed;
            captured = oldPiece.Captured;
            isReadyForPromotion = oldPiece.isReadyForPromotion;
        }

        /// <summary>
        /// Moves the piece, but this is intended to be short lived.  Reset with
        /// ResetTempMove method
        /// </summary>
        /// <param name="newFile">new File</param>
        /// <param name="newRank">new Rank</param>
        public void TempMove(PieceFile newFile, int newRank)
        {
            tempFile = file;
            tempRank = rank;
            file = newFile;
            rank = newRank;
        }

        /// <summary>
        /// Reset the piece to its 'real' position.  Should be called after TempMove
        /// only and always paired with it.
        /// </summary>
        public void ResetTempMove()
        {
            file = tempFile;
            rank = tempRank;
        }

        /// <summary>
        /// Moves the piece to a new location. Does not validate if move is legal
        /// </summary>
        /// <param name="newFile">new file for the piece</param>
        /// <param name="newRank">new rank for the piece</param>
        public void Move(PieceFile newFile, int newRank)
        {
            // Pieces are marked for promotion prior to the move being applied
            // if the current piece is so marked, promote it now.
            if (isReadyForPromotion)
            {
                job = promotionClass;
            }
            rank = newRank;
            file = newFile;
            deployed = true;
        }

        /// <summary>
        /// Promote the piece to a new class
        /// </summary>
        /// <param name="newLotInLife">PieceClass after promotion e.g. Queen 
        /// 99.99999% of the time</param>
        public void PromoteOnNextMove(PieceClass newLotInLife)
        {
            isReadyForPromotion = true;
            promotionClass = newLotInLife;
        }

        /// <summary>
        /// Revert to a pawn....used to takeback moves
        /// </summary>
        public void Demote()
        {
            // These should be impossible cases
            if ((job == PieceClass.Pawn) || (job == PieceClass.King) || 
                (job == PieceClass.EnPassantTarget))
            {
                throw new InvalidOperationException();
            }
            job = PieceClass.Pawn;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Piece color
        /// </summary>
        public PieceColor Color { get { return color; } }

        /// <summary>
        /// Piece class (King, Queen, etc)
        /// </summary>
        public PieceClass Job { get { return job; } }

        /// <summary>
        /// Piece Rank [1-8]
        /// </summary>
        public int Rank { get { return rank; } }

        /// <summary>
        /// Piece file [a-h]
        /// </summary>
        public PieceFile File { get { return file; } }

        /// <summary>
        /// Has the piece ever moved?  Used for pawns and
        /// castling rights
        /// </summary>
        public bool Deployed { get { return deployed; } set { deployed = value; } }

        /// <summary>
        /// Is the piece captured?
        /// </summary>
        public bool Captured
        {
            get { return captured; }
            set { captured  = value; }
        }
        #endregion

        #region Private Fields
        private PieceClass promotionClass;
        private bool isReadyForPromotion;
        private PieceColor color;
        private PieceClass job;
        private int rank;
        private PieceFile file;
        private int tempRank;
        private PieceFile tempFile;
        private bool deployed;
        private bool captured;
        #endregion
    }
}
