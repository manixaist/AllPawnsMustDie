using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Interface for drawing the chessboard
    /// </summary>
    public interface IChessBoardView
    {
        /// <summary>
        /// Load piece images from a bitmap sheet.  This overrides the unicode
        /// text drawing for the pieces
        /// </summary>
        /// <param name="pieceImages">Bitmap image that contains the piece data.
        /// It is assumed the pieces are arranged in 2 rows, with white on top
        /// and black on bottom.  The piece order should be K Q R B N P</param>
        /// <param name="pieceSize">The size of a single piece</param>
        void SetBitmapImages(Bitmap pieceImages, Size pieceSize);

        /// <summary>
        /// Render the board at its location
        /// </summary>
        /// <param name="o">Rendering object</param>
        void Render(object o);

        /// <summary>
        /// Returns a piece given an X,Y in relation to the board+offset
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>ChessPiece found or null if none</returns>
        ChessPiece GetPiece(int x, int y);

        /// <summary>
        /// Returns the BoardSquare at the given x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        BoardSquare GetSquare(int x, int y);

        /// <summary>
        /// Highlight a single square
        /// </summary>
        /// <param name="file">[a-h] file</param>
        /// <param name="rank">[1-8] rank</param>
        void HighlightSquare(PieceFile file, int rank);

        /// <summary>
        /// Highlight a set of squares
        /// </summary>
        /// <param name="squares">List of BoardSquares to highlight</param>
        void HighlightSquares(ref List<BoardSquare> squares);

        /// <summary>
        /// Removes all squares marked for highlighting
        /// </summary>
        void ClearHiglightedSquares();

        /// <summary>
        /// Tell the view it needs to redraw
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Abstracts the UI of selecting a promotion type (e.g. Queen 99.999%)
        /// </summary>
        /// <returns>New PieceClass to promote to</returns>
        PieceClass ChoosePromotionJob();
        
        /// <summary>
        /// Property for the data that drives the view
        /// </summary>
        ChessBoard ViewData { get; set; }

        /// <summary>
        /// TopLeft corner of the board
        /// </summary>
        Point Offset { get; set; }

        /// <summary>
        /// Property to set the size of the view
        /// </summary>
        Size Dimensions { get; set; }

        /// <summary>
        /// Returns a rect (including the offset) for the board
        /// </summary>
        Rectangle BoardRect { get; }
    }
}
