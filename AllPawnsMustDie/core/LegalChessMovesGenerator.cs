using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Static helpers to generate legal moves for chess pieces
    /// </summary>
    public class LegalChessMovesGenerator
    {
        /// <summary>
        /// Helper to check if a king of a given color is in check on a given square
        /// </summary>
        /// <param name="board">ChessBoard to check against</param>
        /// <param name="file">ChessFile to check against</param>
        /// <param name="rank">Rank to check against</param>
        /// <param name="kingColor">Color of king to check against</param>
        /// <returns>true if the king would be in check on this square</returns>
        public static bool IsSquareInCheck(ChessBoard board, PieceFile file, int rank, PieceColor kingColor)
        {
            bool result = false;
            List<ChessPiece> opponentPieces = (kingColor == PieceColor.White) ? board.BlackPieces : board.WhitePieces;
            foreach (ChessPiece piece in opponentPieces)
            {
                if (CanPieceTargetSquare(board, piece, file, rank))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// Checks if a given piece can target a specified square.  It is not required
        /// that the square be occupied by an enemy piece, just potentially reachable
        /// </summary>
        /// <param name="board">ChessBoard to check against</param>
        /// <param name="piece">ChessPiece to check</param>
        /// <param name="targetFile">file to target</param>
        /// <param name="targetRank">rank to target</param>
        /// <returns>true if the piece can target the given square</returns>
        public static bool CanPieceTargetSquare(ChessBoard board, ChessPiece piece, PieceFile targetFile, int targetRank)
        {
            bool result = false;

            if (piece.Captured == true)
            {
                return false;
            }

            BoardSquare targetSquare = new BoardSquare(targetFile, targetRank);
            List<BoardSquare> moves = new List<BoardSquare>();
            switch (piece.Job)
            {
                case PieceClass.Pawn:
                    // Can't reuse the normal helper as it requires the space to be occupied
                    // also w/o en-passant and double moves, this can be simpler
                    int pawnTargetRank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;
                    if (targetRank == pawnTargetRank)
                    {
                        if (((piece.File.ToInt() - 1) == targetFile.ToInt()) ||
                            ((piece.File.ToInt() + 1) == targetFile.ToInt()))
                        {
                            result = true;
                        }
                    }
                    break;
                case PieceClass.Knight:
                    moves = GetLegalMoves_Knight(piece, board);
                    break;
                case PieceClass.Bishop:
                    moves = GetLegalMoves_Bishop(piece, board);
                    break;
                case PieceClass.Rook:
                    moves = GetLegalMoves_Rook(piece, board);
                    break;
                case PieceClass.Queen:
                    moves = GetLegalMoves_Queen(piece, board);
                    break;
                case PieceClass.King:
                    // don't recurse into the normal call, also alternate method to examine
                    // These are pairs of offsets (-1, 0), (-1, 1),...etc so there are twice
                    // as many of these as squares to check
                    int[] offsets = new int[] { -1, 0, -1, 1, -1, -1, 1, 0, 1, 1, 1, -1, 0, 1, 0, -1 };
                    for (int index = 0; index < offsets.Length / 2; index++)
                    {
                        int fileOffset = offsets[index * 2];
                        int rankOffset = offsets[(index * 2) + 1];
                        // Test the validity of the square offset
                        if (BoardSquare.IsValid(piece.File.ToInt() + fileOffset, piece.Rank + rankOffset))
                        {
                            BoardSquare testSquare =
                                new BoardSquare(new PieceFile(piece.File.ToInt() + fileOffset), piece.Rank + rankOffset);
                            if (testSquare == targetSquare)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // King is special above
            if (piece.Job != PieceClass.King)
            {
                // Check moves for the target square
                foreach (BoardSquare square in moves)
                {
                    if (square == targetSquare)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a square is either empty, or has an opponent piece in it
        /// It also performs bounds checking
        /// </summary>
        /// <param name="col">int based col (can be out of bounds)</param>
        /// <param name="row">int based row (can be out of bounds)</param>
        /// <param name="board">ChessBoard to check</param>
        /// <param name="playerColor">PieceColor of the player</param>
        /// <param name="occupied">set to true if an opponent piece is also there</param>
        /// <returns>true if the square is empty or contains an opponent piece</returns>
        public static bool SquareIsFreeOrContainsOpponent(int col, int row, ChessBoard board, PieceColor playerColor, out bool occupied)
        {
            bool result = false;
            occupied = false;
            if (BoardSquare.IsValid(col, row))
            {
                // Get the piece at the square if any
                ChessPiece tPiece = board.FindPieceAt(new PieceFile(col), row);
                // No piece...
                if (tPiece == null)
                {
                    result = true;
                    occupied = false;
                }
                else // ...or opponent piece
                {
                    PieceColor opponentColor = (playerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (tPiece.Color == opponentColor)
                    {
                        result = true;
                        occupied = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Helper to return all legal moves for the given piece.
        /// </summary>
        /// <param name="piece">piece to check, assumed to be valid</param>
        /// <param name="board">board to check agains, also assumed valid</param>
        /// <returns>List of squares the piece can legally move to</returns>
        public static List<BoardSquare> GetLegalMoves(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> preCheckMoves = new List<BoardSquare>();

            // Get a list of legal moves ignoring check violations
            switch (piece.Job)
            {
                case PieceClass.Pawn:
                    preCheckMoves = GetLegalMoves_Pawn(piece, board);
                    break;
                case PieceClass.Knight:
                    preCheckMoves = GetLegalMoves_Knight(piece, board);
                    break;
                case PieceClass.Bishop:
                    preCheckMoves = GetLegalMoves_Bishop(piece, board);
                    break;
                case PieceClass.Rook:
                    preCheckMoves = GetLegalMoves_Rook(piece, board);
                    break;
                case PieceClass.Queen:
                    preCheckMoves = GetLegalMoves_Queen(piece, board);
                    break;
                case PieceClass.King:
                    preCheckMoves = GetLegalMoves_King(piece, board);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Eliminate any move found that would place the King in check
            List<BoardSquare> legalMoves = new List<BoardSquare>();
            ChessPiece playerKing = board.GetKing(piece.Color);
            foreach (BoardSquare square in preCheckMoves)
            {
                // Move the piece for the check - also remove any capture for the test
                ChessPiece tempCapture = board.FindPieceAt(square.File, square.Rank);
                piece.TempMove(square.File, square.Rank);
                if (tempCapture != null)
                {
                    tempCapture.Captured = true;
                }

                if (!IsSquareInCheck(board, playerKing.File, playerKing.Rank, playerKing.Color))
                {
                    legalMoves.Add(square);
                }

                if (tempCapture != null)
                {
                    tempCapture.Captured = false;
                }

                // reset the piece (this bypasses the ChessBoard class)
                piece.ResetTempMove();
            }
            return legalMoves;
        }

        /// <summary>
        /// Helper delegate to check targets (lambdas in each specific method)
        /// </summary>
        /// <param name="p">ChessPiece to check</param>
        /// <param name="fileOffset">file offset from piece</param>
        /// <param name="rankOffset">rank offset from piece</param>
        /// <param name="board">ChessBoard piece resides on</param>
        /// <param name="validMoves">filled in with valid moves</param>
        public delegate void CheckPieceTargets(ChessPiece p, int fileOffset,
            int rankOffset, ChessBoard board, List<BoardSquare> validMoves);

        /// <summary>
        /// Checks for valid moves along a linear path.  The path starts at the piece
        /// and moves outward in one of 8 direction until either a piece is reached
        /// or the edge of the board
        /// </summary>
        public static CheckPieceTargets CheckLinearTargets = (p, fileDelta, rankDelta, b, m) =>
        {
            int startCol = p.File.ToInt();
            int startRow = p.Rank;
            int endCol = startCol + fileDelta;
            int endRow = startRow + rankDelta;

            // As long as we're still on the board...
            while (BoardSquare.IsValid(endCol, endRow))
            {
                bool occupied;
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                    if (occupied) // Must be an opponent, so this move is valid
                    {
                        break;  // stop though, no more can be valid along this path
                    }
                }
                else
                {
                    break;
                }
                endCol += fileDelta; // advance along our 'slope'
                endRow += rankDelta;
            }
        };

        /// <summary>
        /// Returns a list of valid squares a pawn can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_Pawn(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Pawns can move one space forward on any move provided the square is empty
            * and 2 squares if it's the first move the pawn has ever made.
            * A pawn may move diagonally if the square is occupied by an opponent piece (capture)
            * or if the space behind the diagonal is occuped by an opponent pawn that
            * just moved 2 spaces (en-passant)
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |  C = Capture only
            * +---+---+---+---+---+---+---+---+  T = Move or capture
            * |   |   |   | M |   |   |   |   |  M = Move Only
            * +---+---+---+---+---+---+---+---+
            * |   |   | C | M | C |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   | P |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            */
            // One rank "forward" which depends on your color
            int rank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;
            if (null == board.FindPieceAt(piece.File, rank))
            {
                moves.Add(new BoardSquare(piece.File, rank));

                // The 2nd move is only valid of the 1st one was (i.e. you can't move through
                // another piece on your first pawn move)
                if (piece.Deployed == false)
                {
                    rank += (piece.Color == PieceColor.White) ? 1 : -1;
                    if (null == board.FindPieceAt(piece.File, rank))
                    {
                        moves.Add(new BoardSquare(piece.File, rank));
                    }
                }
            }

            // Get the en-passant target if it exists, most of the time it will not
            // it only exists the move after an enemy pawn has jumped 2 spaces on
            // its initial move.
            BoardSquare enPassantTarget;
            bool enPassantValid = board.GetEnPassantTarget(out enPassantTarget);

            // Targets will ALWAYS be 1 rank away (enPassant target is behind piece captured)
            rank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;

            // Lambda helper
            CheckPieceTargets checkPawnTargets = (p, fileOffset, rankOffset, b, m) =>
            {
                int newFileIndex = p.File.ToInt() + fileOffset;

                // Can't have diagonal targets on the back rank, or if we're up
                // against the edge we want to check towards
                if (!ChessPiece.IsOnBackRank(piece) && (newFileIndex > 0) && (newFileIndex <= 8))
                {
                    PieceFile tFile = new PieceFile(p.File.ToInt() + fileOffset);
                    BoardSquare targetSquare = new BoardSquare(tFile, rank);
                    ChessPiece tPiece = b.FindPieceAt(tFile, rank);

                    // Either there is a piece of the opposing color on this square
                    // or the sqaure is a valid enpassant target
                    if (((tPiece != null) && (tPiece.Color != p.Color)) ||
                        ((targetSquare == enPassantTarget) && enPassantValid))
                    {
                        m.Add(new BoardSquare(tFile, rank));
                    }
                }
            };

            // There are 2 possible Files (L,R or Kingside, Queenside, etc)
            // Diagonal left (lower file) PieceFile.ToInt() is 0-based since the 
            // drawing code used it first...so adjust by 1 here
            checkPawnTargets(piece, -1, 0, board, moves);
            checkPawnTargets(piece, 1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a knight can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_Knight(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Knights are the only piece that can jump over other pieces, and move
             * in an L-shape (2:1) or (1:2) with a maximum of 8 valid squares.
             * Because they can jump, the only requirement is the target square
             * be empty or contain an opposing piece (and lie within the boundaries
             * of the board)
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+
             * |   | T |   |   |   | T |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | N |   |   |   |   |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | T |   |   |   | T |   |   |     | N |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |     |   |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     |   | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Deltas should be 1 or -1 to indicate direction
            // Checks one quadrant
            CheckPieceTargets checkKnightTargets = (p, fileDelta, rankDelta, b, m) =>
            {
                // Verify targets are reachable (not off edge)
                int startCol = p.File.ToInt();
                int startRow = p.Rank;

                // #1
                int endCol = startCol + (fileDelta * 1);
                int endRow = startRow + (rankDelta * 2);
                bool occupied; // ignored here
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }

                // #2
                endCol = startCol + (fileDelta * 2);
                endRow = startRow + (rankDelta * 1);
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }
            };

            // Check each quadrant
            checkKnightTargets(piece, 1, 1, board, moves);
            checkKnightTargets(piece, 1, -1, board, moves);
            checkKnightTargets(piece, -1, 1, board, moves);
            checkKnightTargets(piece, -1, -1, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a bishop can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_Bishop(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Bishops are long operators that move diagonaly only.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture.  Just as in the case
             * of the knight, the total number of moves reduces as you near the
             * edge, and further more if you approach the corner.
             * 
             * +---+---+---+---+---+---+---+---+
             * | T |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   | T |   |   |   | P |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  q = enemy queen (capture/block))
             * |   |   | T |   | T |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | B |   |   |   |   |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |     | B |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | q |   |   |   | T |   |   |     |   | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   | T |   |     |   |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check each linear direction (away from piece as center)
            CheckLinearTargets(piece, 1, 1, board, moves);
            CheckLinearTargets(piece, 1, -1, board, moves);
            CheckLinearTargets(piece, -1, 1, board, moves);
            CheckLinearTargets(piece, -1, -1, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a rook can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_Rook(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Rooks are long operators that move horizontaly only.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture. 
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | q |   |   |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  q = enemy queen (capture/block))
             * |   |   |   | T |   |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   | P | T | R | T | T | T | T |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | R | T | T | T | T | T |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | T |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | T |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check each linear direction (away from piece as center)
            // Just like a bishop, but the slope is different
            CheckLinearTargets(piece, 0, 1, board, moves);
            CheckLinearTargets(piece, 0, -1, board, moves);
            CheckLinearTargets(piece, 1, 0, board, moves);
            CheckLinearTargets(piece, -1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a queen can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_Queen(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Queens are long operators that combine the bishop and rook.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture. 
             * 
             * +---+---+---+---+---+---+---+---+
             * | T |   |   |   |   |   | T |   |
             * +---+---+---+---+---+---+---+---+
             * |   | T |   | r |   | T |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  r = enemy rook (capture/block))
             * |   |   | T | T | T |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   | P | T | Q | T | T | T | T |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T | T | T |   |   |   |     | Q | T | T | T | T | T |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | T |   | T |   | T |   |   |     | T | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * | T |   |   | T |   |   | T |   |     | T |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check all 8 linear paths away from the Queen
            // "Bishopy" paths (diagonal)
            CheckLinearTargets(piece, 1, 1, board, moves);
            CheckLinearTargets(piece, 1, -1, board, moves);
            CheckLinearTargets(piece, -1, 1, board, moves);
            CheckLinearTargets(piece, -1, -1, board, moves);
            // "Rooky" paths (horz/vert)
            CheckLinearTargets(piece, 0, 1, board, moves);
            CheckLinearTargets(piece, 0, -1, board, moves);
            CheckLinearTargets(piece, 1, 0, board, moves);
            CheckLinearTargets(piece, -1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a king can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        public static List<BoardSquare> GetLegalMoves_King(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* The King is special.  He can move one square in any direction 
             * (provided it is on the board) so long as the square is empty or
             * it has an opponent piece on it.  However, the King can never
             * move into check, even if the square or capture would be legal
             * otherwise, so it requires some extra checking.
             * 
             * Further complicating things, if the king is castling, he cannot
             * move THROUGH check either (basically check those squares as if
             * they were final destinations)
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+  r = enemy rook (block)
             * |   |   |   |   |   | p |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  p = enemy pawn (block)
             * |   |   | T | T | X |   | b |   |  b = enemy bishop (block)
             * +---+---+---+---+---+---+---+---+  X = illegal move
             * |   |   | T | K | T |   |   |   |     
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T | T | X |   |   |   |     | K | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     | X | X |   |   |   | r |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Cannot castle if in check
            if (!IsSquareInCheck(board, piece.File, piece.Rank, piece.Color))
            {
                /* Castling may only be done if the king has never moved, the rook involved has never moved, 
                 * the squares between the king and the rook involved are unoccupied, the king is not in check, 
                 * and the king does not cross over or end on a square in which it would be in check.
                 *
                 * The ChessBoard will keep track of the castling rights when various pieces move, but it
                 * won't constantly update the legality of the move
                 */

                // Build a list of squares to check
                BoardSide castlingRights = (piece.Color == PieceColor.White) ? board.WhiteCastlingRights : board.BlackCastlingRights;
                BoardSide[] sidesToCheck = new BoardSide[2] { BoardSide.King, BoardSide.Queen };
                foreach (BoardSide sideToCheck in sidesToCheck)
                {
                    // Backrank depends on color
                    int kingRank = (piece.Color == PieceColor.White) ? 1 : 8;
                    BoardSquare[] squares = null;

                    // First check if we still have the right, if not, no need to persue it
                    if (castlingRights.HasFlag(sideToCheck))
                    {
                        squares = new BoardSquare[2];
                        // The target Files depend on the side of the board we're checking
                        // put the final target in [0]
                        if (sideToCheck == BoardSide.King)
                        {
                            squares[0] = new BoardSquare(new PieceFile(7), kingRank);
                            squares[1] = new BoardSquare(new PieceFile(6), kingRank);
                        }
                        else // Queenside
                        {
                            squares[0] = new BoardSquare(new PieceFile(3), kingRank);
                            squares[1] = new BoardSquare(new PieceFile(4), kingRank);
                        }
                    }

                    // There should be 2 and only 2 from above if we found potential targets
                    if (squares != null)
                    {
                        // must be empty and not in check - empty is faster so verify it first
                        if ((board.FindPieceAt(squares[0].File, squares[0].Rank) == null) &&
                            (board.FindPieceAt(squares[1].File, squares[1].Rank) == null))
                        {
                            // Now make sure neither square is in check
                            if (!IsSquareInCheck(board, squares[0].File, squares[0].Rank, piece.Color) &&
                                !IsSquareInCheck(board, squares[1].File, squares[1].Rank, piece.Color))
                            {
                                // King can still castle to this side, add the move option
                                moves.Add(squares[0]);
                            }
                        }
                    }
                }

            }
            // Check each of the 8 squares around the king.  If it's free or has
            // an enemy piece, then check if it's targetable by the opponent
            // (moving into check)  If not, then add it to the list
            CheckPieceTargets checkKingTargets = (p, fileDelta, rankDelta, b, m) =>
            {
                // Verify targets are reachable (not off edge)
                int startCol = p.File.ToInt();
                int startRow = p.Rank;

                int endCol = startCol + (fileDelta);
                int endRow = startRow + (rankDelta);
                bool occupied; // ignored here
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }
            };

            // Check all 8 squares around the king
            checkKingTargets(piece, 0, 1, board, moves);
            checkKingTargets(piece, 0, -1, board, moves);
            checkKingTargets(piece, 1, 0, board, moves);
            checkKingTargets(piece, -1, 0, board, moves);
            checkKingTargets(piece, 1, 1, board, moves);
            checkKingTargets(piece, -1, -1, board, moves);
            checkKingTargets(piece, 1, -1, board, moves);
            checkKingTargets(piece, -1, 1, board, moves);

            // Check violations are handled by the common caller for regulatr moves
            return moves;
        }
    }
}
