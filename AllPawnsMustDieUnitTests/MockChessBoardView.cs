using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllPawnsMustDie;

namespace AllPawnsMustDieUnitTests
{
    /// <summary>
    /// Mock implementation of IChessBoardView, does as little as possible
    /// </summary>
    internal class MockChessBoardView : IChessBoardView
    {
        void IChessBoardView.SetBitmapImages(Bitmap pieceImages, Size pieceSize) { }
        void IChessBoardView.Render(object o) { }

        ChessPiece IChessBoardView.GetPiece(int x, int y)
        {
            return new ChessPiece(PieceColor.White, PieceClass.Pawn, new PieceFile('a'), 1);
        }

        BoardSquare IChessBoardView.GetSquare(int x, int y)
        {
            return new BoardSquare(new PieceFile('a'), 1);
        }

        void IChessBoardView.HighlightSquare(PieceFile file, int rank) { }
        void IChessBoardView.HighlightSquares(ref List<BoardSquare> squares) { }
        void IChessBoardView.ClearHiglightedSquares() { }
        ChessBoard IChessBoardView.ViewData { get { return null;} set { ; } }
        Point IChessBoardView.Offset { get { return new Point(0,0); } set { ; } }
        Size IChessBoardView.Dimensions { get { return new Size(0, 0); } set { ; } }
        Rectangle IChessBoardView.BoardRect { get { return new Rectangle(); } }
    }
}
