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
    /// Interface for drawing the chessboard
    /// </summary>
    interface IChessBoardView
    {
        /// <summary>
        /// Invalidate the view, this should force a redraw
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Render the board at its location
        /// </summary>
        /// <param name="g">Graphics object for the form</param>
        void Render(Graphics g);

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
    }

    /// <summary>
    /// Encapsulates the view for the chess board.  This is the drawing code
    /// </summary>
    public sealed class ChessBoardView : IChessBoardView, IDisposable
    {
        private static int BoardSizeInPixels = 640;
        private static int SquaresPerSide = 8;
        private static int ChessFontSize = 48;
        private static string ChessFont = "Segoe UI Symbols";

        /// <summary>
        /// Create a new view
        /// </summary>
        /// <param name="form">Form the view is "attached" to</param>
        public ChessBoardView(Form form)
        {
            dimensions = new Size(BoardSizeInPixels, BoardSizeInPixels);
            viewForm = form;
            squareSizeInPixels = BoardSizeInPixels / SquaresPerSide;

            stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Center;
            stringFont = new Font(ChessFont, ChessFontSize);
        }

        ~ChessBoardView()
        {
            Dispose();
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                stringFormat.Dispose();
                stringFont.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Redraw the view on the form
        /// </summary>
        void IChessBoardView.Invalidate()
        {
            // Only need to invalidate the board portion of the form
            viewForm.Invalidate(new Rectangle(topLeft.X, topLeft.Y, 
                BoardSizeInPixels, BoardSizeInPixels), true);
        }

        /// <summary>
        /// Draw the chess board
        /// </summary>
        /// <param name="g">Graphics object for the form</param>
        void IChessBoardView.Render(Graphics g)
        {
            // Dimensions and ViewData should be set before this call
            IChessBoardView view = (IChessBoardView)this;

            // Get the pieces
            List<ChessPiece> pieces = data.WhitePieces;
            pieces.AddRange(data.BlackPieces);

            // Draw the board (orientation does not matter)
            DrawBoard(g);

            // Draw the pieces (orientation does matter)
            foreach (ChessPiece piece in pieces)
            {
                DrawPiece(g, piece);
            }

            // Draw others?  Highlighting legal moves maybe?
            //...
        }

        private void DrawBoard(Graphics g)
        {
            // Draw the board
            // Divide the client area into 8x8 grid
            Rectangle rect = new Rectangle(topLeft.X, topLeft.Y, squareSizeInPixels, squareSizeInPixels);
            
            // Draws a checkerboard pattern - the pattern in the same regardless
            // of board orientation - this draws from the top down
            bool fillRect = false;
            for (int r = 0; r < 8; r++)
            {
                bool fillRowStart = fillRect;
                for (int c = 0; c < 8; c++)
                {
                    if (fillRect == true)
                    {
                        g.FillRectangle(Brushes.Gray, rect);
                    }
                    fillRect = !fillRect;
                    rect.X += rect.Width;
                }
                fillRect = !fillRect;
                fillRowStart = !fillRowStart;

                rect.X = topLeft.X;
                rect.Y += rect.Height;
            }

            // Draws a border around the board
            Pen p = new Pen(Color.Black, 1);
            g.DrawRectangle(p, new Rectangle(topLeft.X, topLeft.Y, BoardSizeInPixels, BoardSizeInPixels));
            p.Dispose();
        }

        private readonly StringFormat stringFormat;
        private readonly Font stringFont;

        /// <summary>
        /// Draw a single piece
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="piece">ChessPiece to draw</param>
        private void DrawPiece(Graphics g, ChessPiece piece)
        {
            // Fetch the unicode for the given piece and draw it in the rect
            Rectangle pieceRect = GetRect(piece.File, piece.Rank);
            g.DrawString(PieceChar(piece).ToString(), stringFont, Brushes.Black, pieceRect, stringFormat);
        }

        /// <summary>
        /// Find the rect for a given location on the board
        /// </summary>
        /// <param name="file">File for the piece</param>
        /// <param name="rank">Rank for the piece</param>
        /// <returns>Rectangle for the board location in client coordinates</returns>
        private Rectangle GetRect(PieceFile file, int rank)
        {
            // This simple diagram helps demonstrate the inverted relationship
            // (not pictured is the overall board offset)
            //
            //       Black                White
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 8 | |X| |X| |X| |X|   | |X| |X| |X| |X| 1
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 7 |X| |X| |X| |X| |   |X| |X| |X| |X| | 2
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 6 | |X| |X| |X| |X|   | |X| |X| |X| |X| 3
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 5 |X| |X| |X| |X| |   |X| |X| |X| |X| | 4
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 4 | |X| |X| |X| |X|   | |X| |X| |X| |X| 5
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 3 |X| |X| |X| |X| |   |X| |X| |X| |X| | 6
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 2 | |X| |X| |X| |X|   | |X| |X| |X| |X| 7
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            // 1 |X| |X| |X| |X| |   |X| |X| |X| |X| | 8
            //   +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
            //    a b c d e f g h     h g f e d c b a
            //       White                Black
            int X;
            int Y;

            if (data.Orientation == BoardOrientation.WhiteOnBottom)
            {
                X = (file.ToInt() - 1) * squareSizeInPixels;
                Y = (8 - rank) * squareSizeInPixels;
            }
            else
            {
                X = (8- file.ToInt()) * squareSizeInPixels;
                Y = (rank - 1) * squareSizeInPixels;
            }
            
            // Return the calculated rect offset from the overall topLeft location
            return new Rectangle(X + topLeft.X, Y + topLeft.Y, squareSizeInPixels, squareSizeInPixels);
        }

        /// <summary>
        /// Returns the Unicode character for the given piece
        /// </summary>
        /// <param name="piece">ChessPiece to convert</param>
        /// <returns>Unicode for the piece</returns>
        private Char PieceChar(ChessPiece piece)
        {
            bool isBlack = piece.Color == PieceColor.Black;
            switch (piece.Job)
            {
                case PieceClass.King:
                    return isBlack ? '\u265A' : '\u2654';
                case PieceClass.Queen:
                    return isBlack ? '\u265B' : '\u2655';
                case PieceClass.Rook:
                    return isBlack ? '\u265C' : '\u2656';
                case PieceClass.Bishop:
                    return isBlack ? '\u265D' : '\u2657';
                case PieceClass.Knight:
                    return isBlack ? '\u265E' : '\u2658';
                case PieceClass.Pawn:
                    return isBlack ? '\u265F' : '\u2659';
            }
            throw new ArgumentOutOfRangeException();
        }

        private Point topLeft = new Point(0, 0);
        
        /// <summary>
        /// Sets the starting offset for the board inside the Form client
        /// </summary>
        Point IChessBoardView.Offset
        {
            get { return topLeft; }
            set { topLeft = value; }
        }

        private Size dimensions;

        /// <summary>
        /// Size of the board
        /// </summary>
        Size IChessBoardView.Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        private ChessBoard data;

        /// <summary>
        /// The data that drives the view, the ChessBoard class
        /// </summary>
        ChessBoard IChessBoardView.ViewData
        {
            get { return data; }
            set { data = value; }
        }

        private Form viewForm;
        private int squareSizeInPixels;
    }
}
