using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Encapsulates FEN functionality
    /// https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
    /// </summary>
    class FENString
    {
        private readonly string fen;

        /// <summary>
        /// Accepts a raw FEN string and just saves it as its own
        /// </summary>
        /// <param name="forsythEdwardsNotation">FEN string describing the board</param>
        public FENString(string forsythEdwardsNotation)
        {
            fen = forsythEdwardsNotation;
        }

        /// <summary>
        /// Calculate the internal FEN string based on a ChessBoard given
        /// </summary>
        /// <param name="board">ChessBoard to evaluate</param>
        public FENString(ChessBoard board)
        {
            // Calculate the FEN from the board position
            // TODO...just insert a test one for now, just a few moves
            fen = "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
        }

        /// <summary>
        /// Validates the FEN stored as valid, or not
        /// </summary>
        /// <returns>true if the string is a valid FEN, false otherwise</returns>
        public bool IsValidNotation()
        {
            // Parse the FEN and make sure the string is, in fact, valid
            // TODO...
            return true;
        }

        public List<PieceLocation> Pieces
        {
            get { return null; }
        }

        public PieceColor ActivePlayer
        {
            // TODO - logic - for now return white
            get { return PieceColor.White; }
        }

        public BoardSide WhiteCastlingRights
        {
            // TODO - logic - for now return all
            get { return BoardSide.King | BoardSide.Queen; }
        }

        public BoardSide BlackCastlingRights
        {
            // TODO - logic - for now return all
            get { return BoardSide.King | BoardSide.Queen; }
        }

        public PieceLocation EnPassantTarget
        {
            // Just insert something - we're really describing an empty square 
            // here, the one behind the pawn that just moved 2 spaces.
            // If there is no target, this is null
            get { return null; }
        }

        /// <summary>
        /// Gets the number of half moves since a pawn advance or any capture
        /// </summary>
        public int HalfMoves { get { return 0; } }

        /// <summary>
        /// Gets the number of full moves
        /// </summary>
        public int FullMoves { get { return 0; } }

        /// <summary>
        /// Holds enough information to place a piece on the board (type, rank, file)
        /// </summary>
        public class PieceLocation
        {
            PieceClass job;
            PieceFile file;
            int rank;

            /// <summary>
            /// Store the values for the piece positional data
            /// </summary>
            /// <param name="pieceJob">King, Queen, etc</param>
            /// <param name="pieceFile">[a-h] ([1-8])</param>
            /// <param name="pieceRank">[1-8]</param>
            public PieceLocation(PieceClass pieceJob, PieceFile pieceFile, int pieceRank)
            {
                job = pieceJob;
                rank = pieceRank;
                file = pieceFile;
            }
            
            /// <summary>
            /// Gets the PieceClass, e.g. King, Queen, Rook, Bishop, Knight, or Pawn
            /// </summary>
            PieceClass Job { get { return job; } }

            /// <summary>
            /// Gets the PieceFile which represents the column [a-h] also mapped to [1-8]
            /// </summary>
            PieceFile File { get { return file; } }

            /// <summary>
            /// Gets the rank of the piece as an integer [1-8]
            /// </summary>
            int Rank { get { return rank; } }
        }
    }
}
