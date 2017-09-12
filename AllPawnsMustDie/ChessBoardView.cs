using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
    #region Extension Methods
    public static class RectangleExtensionMethods
    {
        /// <summary>
        /// Returns the center point of the Rectangle.  The fact this needs adding
        /// is sad, but extension methods let us correct this injustice
        /// </summary>
        /// <param name="rect">Rectangle to get the center of</param>
        /// <returns>A Point struct for the center of the given rect</returns>
        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2,
                             rect.Top + rect.Height / 2);
        }
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Interface for drawing the chessboard
    /// </summary>
    interface IChessBoardView
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
    #endregion

    /// <summary>
    /// Encapsulates the view for the chess board.  This is the drawing code
    /// </summary>
    public sealed class ChessBoardView : IChessBoardView, IDisposable
    {
        #region Public Methods
        /// <summary>
        /// Create a new view
        /// </summary>
        /// <param name="form">Form the view is "attached" to</param>
        public ChessBoardView(Form form)
        {
            dimensions = new Size(BoardSizeInPixels, BoardSizeInPixels);

            // Must be careful with this, as many operations are not allowed
            // cross thread.  Invalidate is ok though
            viewForm = form;
            squareSizeInPixels = BoardSizeInPixels / SquaresPerSide;

            // Cache font information
            stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Center;
            stringFont = new Font(ChessFont, ChessFontSize);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ChessBoardView()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of disposable objects we own if we haven't already disposed
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                stringFormat.Dispose();
                stringFont.Dispose();
                disposed = true;
                chessPieceImageMap.Clear();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Load piece images from a bitmap sheet.  This overrides the unicode
        /// text drawing for the pieces
        /// </summary>
        /// <param name="pieceImages">Bitmap image that contains the piece data.
        /// It is assumed the pieces are arranged in 2 rows, with white on top
        /// and black on bottom.  The piece order should be K Q R B N P</param>
        /// <param name="pieceSize">The size of a single piece</param>
        void IChessBoardView.SetBitmapImages(Bitmap pieceImages, Size pieceSize)
        {
            // Extract each image from the bitmap and store it in a map (job+color:image)
            chessPieceImageMap = new Dictionary<string, Bitmap>();

            // This will not change in the loop (all destinations are the same size)
            Rectangle destRect = new Rectangle(0, 0, pieceSize.Width, pieceSize.Height);

            // This one will based on where we are in the image loop
            Rectangle srcRect = new Rectangle(0, 0, pieceSize.Width, pieceSize.Height);

            // Array of jobs in the order we expect to find them in the image
            // This is the same for each row (top white, bottom black)
            PieceClass[] jobs = new PieceClass[] { PieceClass.King, PieceClass.Queen,
                PieceClass.Rook, PieceClass.Bishop, PieceClass.Knight, PieceClass.Pawn };

            // Init loop variables
            int jobIndex = 0;
            PieceColor pieceColor = PieceColor.White;

            // Loop over the assumed layout (2x6)
            for (int imageRow = 0; imageRow < 2; imageRow++)
            {
                for (int imageCol = 0; imageCol < 6; imageCol++)
                {
                    // Create a new bitmap of the reequired size
                    Bitmap b = new Bitmap(pieceSize.Width, pieceSize.Height);
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        // Draw the portion of the larger image into the smaller one
                        // we just created for the piece
                        g.DrawImage(pieceImages, destRect, srcRect, GraphicsUnit.Pixel);
                    }

                    // Add the image to the map (mangle the color and the job for the key)
                    chessPieceImageMap.Add(GetPieceImageKey(jobs[jobIndex++], pieceColor), b);

                    // Shift over to the right by one piece width
                    srcRect.X += pieceSize.Width;
                }

                // Reset row variables
                jobIndex = 0;
                srcRect.X = 0;
                srcRect.Y += pieceSize.Height;

                // Switch colors
                pieceColor = PieceColor.Black;
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

            // Draw the board (orientation does not matter)
            DrawBoard(g);

            Rectangle s;
            Rectangle e;
            bool drawLast = LastMoveRects(out s, out e);

            if (drawLast)
            {
                DrawLastMoveStart(g, s);
                DrawLastMoveEnd(g, e);
            }

            // Draw the pieces (orientation does matter)
            foreach (ChessPiece piece in data.WhitePieces)
            {
                DrawPiece(g, piece);
            }

            foreach (ChessPiece piece in data.BlackPieces)
            {
                DrawPiece(g, piece);
            }

            if (drawLast)
            {
                DrawLastMoveLine(g, s, e);
            }

            // TODO.. highlight checks as well?  Data will need to support this
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Draw an arrow for the last move, indicating direction and placement.  
        /// The line connects the center of the given rect and has an arrow to 
        /// indicate direction of the move (though the piece would as well)
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="startRect">Staring rect to use</param>
        /// <param name="endRect">Ending rect to use</param>
        private void DrawLastMoveLine(Graphics g, Rectangle startRect, Rectangle endRect)
        {
            using (Pen p = new Pen(Brushes.MidnightBlue, 6))
            {
                // This is the arrow on the end of the line
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.ArrowAnchor;
                p.CustomEndCap = new AdjustableArrowCap(5, 5);
                p.DashStyle = DashStyle.Solid;

                // Get the center of each rect using our handy extension
                // It's criminal this isn't a part of the default struct...
                Point centerStart = startRect.Center();
                Point centerEnd = endRect.Center();
                
                // Draw a line from A->B
                g.DrawLine(p, centerStart, centerEnd);
            }
        }

        /// <summary>
        /// Renders a visual indicator for the start position of the last move
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="rect">Rectangle to draw within</param>
        private void DrawLastMoveStart(Graphics g, Rectangle rect)
        {
            // Draws a green circle (assuming the rect is square which it will be)
            using (Pen p = new Pen(Brushes.ForestGreen, 4))
            {
                g.DrawEllipse(p, rect);
            }
        }

        /// <summary>
        /// Renders a visual indicator for the end position of the last move
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="rect">Rectangle to draw within</param>
        private void DrawLastMoveEnd(Graphics g, Rectangle rect)
        {
            // Red square for capture
            if (data.LastMoveWasCapture)
            {
                g.FillRectangle(Brushes.Red, rect);
            }
            // Yellow square for a normal move
            else
            {
                g.FillRectangle(Brushes.Yellow, rect);
            }
        }

        /// <summary>
        /// Renders the chess board minus any pieces or highlights
        /// </summary>
        /// <param name="g">Graphics object</param>
        private void DrawBoard(Graphics g)
        {
            // Divide the client area into 8x8 grid, start with the top-left square
            Rectangle rect = new Rectangle(topLeft.X, topLeft.Y, squareSizeInPixels, squareSizeInPixels);

            // Draws a checkerboard pattern - the pattern in the same regardless
            // of board orientation - this draws from the top down
            bool darkSquare = false;
            for (int r = 0; r < 8; r++)
            {
                bool fillRowStart = darkSquare;
                for (int c = 0; c < 8; c++)
                {
                    if (darkSquare)
                    {
                        g.FillRectangle(Brushes.Gray, rect);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.White, rect);
                    }
                    darkSquare = !darkSquare;
                    rect.X += rect.Width;
                }
                darkSquare = !darkSquare;
                fillRowStart = !fillRowStart;

                rect.X = topLeft.X;
                rect.Y += rect.Height;
            }

            // Draws a border around the board
            using (Pen p = new Pen(Color.Black, 1))
            {
                g.DrawRectangle(p, new Rectangle(topLeft.X, topLeft.Y, BoardSizeInPixels, BoardSizeInPixels));
            }
        }

        /// <summary>
        /// Draw a single piece.  This is a simple draw, but the board shows through
        /// the transparent portions of the piece since it's text.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="piece">ChessPiece to draw</param>
        private void DrawPiece(Graphics g, ChessPiece piece)
        {
            if (piece.Visible)
            {
                Rectangle pieceRect = GetRect(piece.File, piece.Rank);
                if (chessPieceImageMap == null)
                {
                    // Find the screen rect for the piece and 'draw' it
                    g.DrawString(PieceChar(piece).ToString(), stringFont, Brushes.Black, pieceRect, stringFormat);
                }
                else
                {
                    Bitmap b = chessPieceImageMap[GetPieceImageKey(piece.Job, piece.Color)];
                    b.MakeTransparent(Color.Magenta);
                    g.DrawImage(b, pieceRect);
                }
            }
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
                X = (file.ToInt()) * squareSizeInPixels;
                Y = (8 - rank) * squareSizeInPixels;
            }
            else
            {
                X = (8- file.ToInt()) * squareSizeInPixels;
                Y = (rank) * squareSizeInPixels;
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

        /// <summary>
        /// Helper method to return the rects of the starting and ending squares
        /// of the last move
        /// </summary>
        /// <param name="startRect">starting position rect</param>
        /// <param name="endRect">ending position rect</param>
        /// <returns></returns>
        private bool LastMoveRects(out Rectangle startRect, out Rectangle endRect)
        {
            bool result = false;
            if (data.Moves.Count > 0) // There has to be at least 1 move
            {
                result = true;
                // Get the move e.g. "e2e4"
                string lastMove = data.Moves.Last();

                // Divide the move into start and end e.g. "e2" and "e4"
                string lastMoveStart = lastMove.Substring(0, 2);
                string lastMoveEnd = lastMove.Substring(2, 2);

                // Convert the text moves to a rank and file
                PieceFile startFile = new PieceFile(lastMoveStart[0]);
                int startRank = Convert.ToInt16((char)lastMoveStart[1]) - Convert.ToInt16('0');
                PieceFile endFile = new PieceFile(lastMoveEnd[0]);
                int endRank = Convert.ToInt16((char)lastMoveEnd[1]) - Convert.ToInt16('0');

                // Get the rects for the squares
                startRect = GetRect(startFile, startRank);
                endRect = GetRect(endFile, endRank);

                // Shrink the start rect (currently draws a smaller circle inside the square)
                startRect.Inflate(new Size(-squareSizeInPixels / 10, -squareSizeInPixels / 10));
            }
            else
            {
                // No moves, so no rects (or 0 sized ones in this case)
                startRect = new Rectangle();
                endRect = new Rectangle();
            }
            return result;
        }

        /// <summary>
        /// Small helper to get the mangled key for an image
        /// </summary>
        /// <param name="job"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static string GetPieceImageKey(PieceClass job, PieceColor color)
        {
            return String.Concat(job.ToString(), color.ToString());
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Sets the starting offset for the board inside the Form client
        /// </summary>
        Point IChessBoardView.Offset
        {
            get { return topLeft; }
            set { topLeft = value; }
        }

        /// <summary>
        /// Size of the board
        /// </summary>
        Size IChessBoardView.Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        /// <summary>
        /// The data that drives the view, the ChessBoard class
        /// </summary>
        ChessBoard IChessBoardView.ViewData
        {
            get { return data; }
            set { data = value; }
        }
        #endregion

        #region Public Fields
        /// <summary>
        /// The size of one edge of the board in pixels.  A square is 1/8 of this
        /// </summary>
        public static int BoardSizeInPixels = 640;
        #endregion

        #region Private Fields
        // Standard chessboard is (8 x 8)
        private static int SquaresPerSide = 8; 
        
        // Using Unicode font for the chess pieces
        private static int ChessFontSize = 48;  
        private static string ChessFont = "Segoe UI Symbols";
        private readonly StringFormat stringFormat;
        private readonly Font stringFont;

        private bool disposed = false;      // Disposed flag

        // Pixel size of a single chess square edge
        private int squareSizeInPixels;

        // Overall size and offset from the topLeft of the form
        private Point topLeft = new Point(0, 0);
        private Size dimensions;

        // Form this class renders to
        private Form viewForm;

        // Data that drives the view
        private ChessBoard data;

        // Holds images for the pieces if specified
        private Dictionary<string, Bitmap> chessPieceImageMap;

        #endregion
    }
}
