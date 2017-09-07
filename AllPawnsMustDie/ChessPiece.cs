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
        /// <summary>
        /// Create a new ChessPiece
        /// </summary>
        /// <param name="pieceColor">Color</param>
        /// <param name="pieceClass">King, Queen, Rook, etc</param>
        /// <param name="pieceFile">file location</param>
        /// <param name="pieceRank">rank location</param>
        public ChessPiece(PieceColor pieceColor, PieceClass pieceClass, PieceFile pieceFile, int pieceRank)
        {
            // Save the parameters as fields
            color = pieceColor;
            job = pieceClass;
            rank = pieceRank;
            file = pieceFile;
            deployed = false;   // Initially not moved
        }

        /// <summary>
        /// Moves the piece to a new location
        /// </summary>
        /// <param name="newFile">new file for the piece</param>
        /// <param name="newRank">new rank for the piece</param>
        public void Move(PieceFile newFile, int newRank)
        {
            // Does not validate is move is legal
            rank = newRank;
            file = newFile;
        }

        /// <summary>
        /// Promote the piece to a new class
        /// </summary>
        /// <param name="promotionClass"></param>
        public void Promote(PieceClass promotionClass)
        {
            job = promotionClass;
        }

        private PieceColor color;
        /// <summary>
        /// Piece color
        /// </summary>
        public PieceColor Color { get { return color; } }

        private PieceClass job;
        /// <summary>
        /// Piece class (King, Queen, etc)
        /// </summary>
        public PieceClass Job { get { return job; } }

        private int rank;
        /// <summary>
        /// Piece Rank [1-8]
        /// </summary>
        public int Rank { get { return rank; } }

        private PieceFile file;
        /// <summary>
        /// Piece file [a-h]
        /// </summary>
        public PieceFile File { get { return file; } }

        private bool deployed;
        /// <summary>
        /// Has the piece ever moved?  Used for pawns and
        /// castling rights
        /// </summary>
        public bool Deployed { get { return deployed; } }
    }
}
