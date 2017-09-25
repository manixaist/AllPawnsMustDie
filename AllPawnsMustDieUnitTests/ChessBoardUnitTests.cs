using System;
using System.Diagnostics;
using AllPawnsMustDie;
using CB = AllPawnsMustDie.ChessBoard;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/* The organization for UnitTests should follow the schema below, which was
 * "borrowed" from this blog post:
 * http://haacked.com/archive/2012/01/02/structuring-unit-tests.aspx/
 * 
 * Basically a class for each class tested, then nested sub-classes for each
 * method being tested, and then methods for each test on that method.  Read
 * the link for more reasons why, but it's a nice organization scheme for 
 * managed code at least.
 * 
 * [TestClass]
 * class NameOfProductionClassTests
 * {
 *     [TestClass]
 * ....class NameOfClassMethodMethod
 *     {
 *         [TestMethod]
 *     ....public void TestName() { ... }
 *         [TestMethod]
 *     ....public void TestName() { ... }
 *         [TestMethod]
 *     ....public void TestName() { ... }
 *     }
 *
 *     [TestClass]
 * ....class NameOfClassMethod@Method
 *     {
 *         [TestMethod]
 *     ....public void TestName() { ... }
 *     }
 * }
 */

namespace AllPawnsMustDieUnitTests
{
    /// <summary>
    /// Holds unit tests related to the ChessBoard class
    /// </summary>
    [TestClass]
    public class ChessBoardUnitTests
    {
        #region HELPER CONSTANTS
        private const PieceColor WH = PieceColor.White;
        private const PieceColor BL = PieceColor.Black;
        private const PieceClass PAWN = PieceClass.Pawn;
        private const PieceClass ROOK = PieceClass.Rook;
        private const PieceClass KNHT = PieceClass.Knight;
        private const PieceClass BISH = PieceClass.Bishop;
        private const PieceClass QUEN = PieceClass.Queen;
        private const PieceClass KING = PieceClass.King;
        private static PieceFile A_FILE = new PieceFile('a');
        private static PieceFile B_FILE = new PieceFile('b');
        private static PieceFile C_FILE = new PieceFile('c');
        private static PieceFile D_FILE = new PieceFile('d');
        private static PieceFile E_FILE = new PieceFile('e');
        private static PieceFile F_FILE = new PieceFile('f');
        private static PieceFile G_FILE = new PieceFile('g');
        private static PieceFile H_FILE = new PieceFile('h');
        #endregion

        // The idea is we might need different implementations.  If not, then convert
        // VerifyChessBoardPieces to a normal static method
        private delegate void BasicBoardCheck(ChessBoard board, CB.BoardSquare square, PieceColor color, PieceClass job);

        /// <summary>
        /// Checks if a piece of the given color and job exists on the given board
        /// </summary>
        static BasicBoardCheck VerifyChessBoardPieces = (board, square, color, job) =>
        {
            Trace.WriteLine(String.Format("Looking for piece at [{0}:{1}]...", square.File.ToString(), square.Rank));
            ChessPiece piece = board.FindPieceAt(square.File, square.Rank);
            try
            {
                Trace.WriteLine(String.Format("...found it, checking Color({0}) and Job({1})", color.ToString(), job.ToString()));
                Assert.AreEqual(piece.Color, color);
                Assert.AreEqual(piece.Job, job);
                Assert.IsTrue(piece.Visible);
            }
            catch (NullReferenceException)
            {
                Assert.Fail("piece was not found...it should have been.");
            }
        };

        /// <summary>
        /// ChessBoard.NewGame() tests
        /// </summary>
        [TestClass]
        public class NewGameMethodTests
        {
            /// <summary>
            /// Basic tests for NewGame() method
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // Standard board setup
                Tuple<CB.BoardSquare, PieceColor, PieceClass>[] defaultBoardData =
                {
                    // WHITE PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 1), WH, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 1), WH, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 1), WH, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 1), WH, QUEN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 1), WH, KING),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 1), WH, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 1), WH, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 1), WH, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 2), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 2), WH, PAWN),
                    // BLACK PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 8), BL, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 8), BL, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 8), BL, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 8), BL, QUEN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 8), BL, KING),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 8), BL, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 8), BL, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 8), BL, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 7), BL, PAWN),
                };

                // Create a new ChessBoard and call NewGame() to initialize it
                Trace.WriteLine("Creating a new ChessBoard...");
                ChessBoard testBoard = new ChessBoard();
                Trace.WriteLine("ChessBoard.NewGame()...");
                testBoard.NewGame();

                Trace.WriteLine("Verifying contents of default ChessBoard...");
                foreach (Tuple<CB.BoardSquare, PieceColor, PieceClass> tuple in defaultBoardData)
                {
                    VerifyChessBoardPieces(testBoard, tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }
        }

        /// <summary>
        /// ChessBoard.NewPosition(string) tests
        /// </summary>
        [TestClass]
        public class NewPositionMethodTests
        {
            /// <summary>
            /// Basic tests for NewPosition(string) method
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // position fen "3q2k1/1br2pb1/p5p1/1p2N1Pn/3PpP2/PP2N3/1B3Q2/5RK1 w - -2 36"
                // StockFish8 debug output
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | q |   |   | k |   |
                // +---+---+---+---+---+---+---+---+
                // |   | b | r |   |   | p | b |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   |   |   |   | p |   |
                // +---+---+---+---+---+---+---+---+
                // |   | p |   |   | N |   | P | n |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | P | p | P |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | P |   |   | N |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   | B |   |   |   | Q |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   | R | K |   |
                // +---+---+---+---+---+---+---+---+
                string testFEN = "3q2k1/1br2pb1/p5p1/1p2N1Pn/3PpP2/PP2N3/1B3Q2/5RK1 w - - 2 36";
                Tuple<CB.BoardSquare, PieceColor, PieceClass>[] positionalBoardData =
                {
                    // WHITE PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 3), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 3), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 4), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 4), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 5), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 2), WH, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 3), WH, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 5), WH, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 2), WH, QUEN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 1), WH, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 1), WH, KING),
                    // BLACK PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 6), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 5), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 4), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 6), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 7), BL, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 7), BL, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 5), BL, KNHT),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 8), BL, QUEN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 7), BL, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 8), BL, KING),
                };

                // Very similar to NewGame, but we could create many more positions to verify
                // For now, use one I created playing a game on lichess.org
                ChessBoard testBoard = new ChessBoard();
                Trace.WriteLine("ChessBoard.NewPosition(fen)...");
                testBoard.NewPosition(testFEN);

                Trace.WriteLine("Verifying contents of test ChessBoard...");
                foreach (Tuple<CB.BoardSquare, PieceColor, PieceClass> tuple in positionalBoardData)
                {
                    VerifyChessBoardPieces(testBoard, tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }
        }

        /// <summary>
        /// ChessBoard.PieceClassFromFenTests static Method tests
        /// </summary>
        [TestClass]
        public class PieceClassFromFenTests
        {
            /// <summary>
            /// Basic tests for PieceClassFromFen() Method.  It basically 
            /// translates a character to an enum
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // Valid: { k, q, r, b, n, p }
                // Others: ArgumentOutOfRangeException
                Tuple<char, PieceClass>[] testDataValid =
                {
                    new Tuple<char, PieceClass>('k', KING),
                    new Tuple<char, PieceClass>('q', QUEN),
                    new Tuple<char, PieceClass>('r', ROOK),
                    new Tuple<char, PieceClass>('b', BISH),
                    new Tuple<char, PieceClass>('n', KNHT),
                    new Tuple<char, PieceClass>('p', PAWN),
                };

                foreach (Tuple<char, PieceClass> tuple in testDataValid)
                {
                    Trace.WriteLine(String.Format("Verifying {0} == {1}", tuple.Item1, tuple.Item2.ToString()));
                    Assert.AreEqual(ChessBoard.PieceClassFromFen(tuple.Item1), tuple.Item2);
                }

                // Not a complete list, but the rest of [a-z][A-Z] that isn't valid plus some others
                string testDataInvalid = "acdefghijlmostuvwxyzACDEFGHIJLMOSTUVWXYZ0123456789!@#$%^&*";
                foreach (char c in testDataInvalid)
                {
                    bool caught = false;
                    try
                    {
                        Trace.WriteLine(String.Format("Verifying {0} throws ArgumentOutOfRangeException", c));
                        PieceClass neverGetsAssigned = ChessBoard.PieceClassFromFen(c);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        caught = true;
                    }
                    Assert.IsTrue(caught);
                }
            }
        }

        /// <summary>
        /// ChessBoard.CurrentPositionAsFen() tests
        /// </summary>
        [TestClass]
        public class CurrentPositionAsFenTests
        {
            /// <summary>
            /// Basic tests for ChessBoard.CurrentPositionAsFen()
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // Production Method is NYI
                Trace.WriteLine("CurrentPositionAsFenTests are NYI...");
            }
        }

        /// <summary>
        /// ChessBoard.MovePiece() tests
        /// </summary>
        [TestClass]
        public class MovePieceTests
        {
            /// <summary>
            /// Basic tests for MovePiece
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // ChessBoard.MovePiece() assumes the move is valid, but it will
                // handle some side-effects, like moving the rook on a king move
                // that is castling, en-passant captures, and promotions if marked
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewGame();

                // Basic opening pawn move "e2e4"
                Trace.WriteLine("Applying a basic pawn move...e2e4");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 2), new ChessBoard.BoardSquare(E_FILE, 4), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                Trace.WriteLine("Verify pawn is found at new location...");
                ChessPiece piece = testBoard.FindPieceAt(E_FILE, 4);
                Assert.AreEqual(piece.Job, PieceClass.Pawn);
                Assert.AreEqual(piece.Color, PieceColor.White);
            }

            /// <summary>
            /// Verifies castling moves
            /// </summary>
            [TestMethod]
            public void CastlingMoveTests()
            {
                // There are 4 possible castles (Wh:Bl)(Kingside:Queenside)
                // White to move: "r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8"
                // Black to move: "r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R b KQkq - 6 8"
                //+---+---+---+---+---+---+---+---+
                //| r |   |   |   | k |   |   | r |
                //+---+---+---+---+---+---+---+---+
                //| p | p | p | q |   | p | p | p |
                //+---+---+---+---+---+---+---+---+
                //|   |   | n | b | b | n |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | p | p |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | P | P |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   | N | B | B | N |   |   |
                //+---+---+---+---+---+---+---+---+
                //| P | P | P | Q |   | P | P | P |
                //+---+---+---+---+---+---+---+---+
                //| R |   |   |   | K |   |   | R |
                //+---+---+---+---+---+---+---+---+
                // Reset board
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8");

                // Castle White Kingside
                Trace.WriteLine("Castle White Kingside - e1g1");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 1), new ChessBoard.BoardSquare(G_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the rook moved as well
                Trace.WriteLine("Verifying the rook moved as well");
                ChessPiece whiteKingsideRook = testBoard.FindPieceAt(F_FILE, 1);
                Assert.AreEqual(whiteKingsideRook.Job, PieceClass.Rook);
                Trace.WriteLine("Verifying the castling rights are updated");
                Assert.IsFalse(testBoard.WhiteCastlingRights.HasFlag(BoardSide.King));

                // Reset board
                testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8");

                // Castle White Queenside
                Trace.WriteLine("Castle White Kingside - e1c1");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 1), new ChessBoard.BoardSquare(C_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the rook moved as well
                Trace.WriteLine("Verifying the rook moved as well");
                ChessPiece whiteQueensideRook = testBoard.FindPieceAt(D_FILE, 1);
                Assert.AreEqual(whiteQueensideRook.Job, PieceClass.Rook);
                Trace.WriteLine("Verifying the castling rights are updated");
                Assert.IsFalse(testBoard.WhiteCastlingRights.HasFlag(BoardSide.Queen));

                // Reset board - note black to play in fen
                testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R b KQkq - 6 8");
                
                // Castle Black Kingside
                Trace.WriteLine("Castle White Kingside - e8g8");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 8), new ChessBoard.BoardSquare(G_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the rook moved as well
                Trace.WriteLine("Verifying the rook moved as well");
                ChessPiece blackKingsideRook = testBoard.FindPieceAt(F_FILE, 8);
                Assert.AreEqual(blackKingsideRook.Job, PieceClass.Rook);
                Trace.WriteLine("Verifying the castling rights are updated");
                Assert.IsFalse(testBoard.BlackCastlingRights.HasFlag(BoardSide.King));

                // Reset board - note black to play in fen
                testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R b KQkq - 6 8");

                // Castle Black Queenside
                Trace.WriteLine("Castle White Kingside - e8c8");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 8), new ChessBoard.BoardSquare(C_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the rook moved as well
                Trace.WriteLine("Verifying the rook moved as well");
                ChessPiece blackQueensideRook = testBoard.FindPieceAt(D_FILE, 8);
                Assert.AreEqual(blackQueensideRook.Job, PieceClass.Rook);
                Trace.WriteLine("Verifying the castling rights are updated");
                Assert.IsFalse(testBoard.BlackCastlingRights.HasFlag(BoardSide.Queen));
            }

            /// <summary>
            /// Verifies En-Passant captures
            /// </summary>
            [TestMethod]
            public void EnPassantMoveTests()
            {
                // Prepares ..d7d5 for en-passant capture
                // "r1bqkbnr/pppppppp/n7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2"
                // +---+---+---+---+---+---+---+---+
                // | r |   | b | q | k | b | n | r |
                // +---+---+---+---+---+---+---+---+
                // | p | p | p | p | p | p | p | p |
                // +---+---+---+---+---+---+---+---+
                // | n |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   | P |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | P | P | P |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // | R | N | B | Q | K | B | N | R |
                // +---+---+---+---+---+---+---+---+
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r1bqkbnr/pppppppp/n7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");

                // Prepare our en-passant target
                Trace.WriteLine("Preparing en-passant victim d7d5...");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(D_FILE, 7), new ChessBoard.BoardSquare(D_FILE, 5), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Now attempt the en-passant e5d6
                Trace.WriteLine("Capture the black pawn by en-passant e5d6...");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 5), new ChessBoard.BoardSquare(D_FILE, 6), true, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the capture
                Trace.WriteLine("Verify the captured pawn");
                Assert.IsTrue(moveInfo.IsCapture);
                Assert.AreEqual(moveInfo.CapturedPiece.Job, PieceClass.Pawn);
                Assert.AreEqual(moveInfo.CapturedPiece.Color, PieceColor.Black);
            }
        }

        /// <summary>
        /// ChessBoard.GetEnPassantTarget() tests
        /// </summary>
        [TestClass]
        public class GetEnPassantTargetTests
        {
            /// <summary>
            /// Basic tests for GetEnPassantTarget()
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // Prepares ..d7d5 for en-passant capture
                // "r1bqkbnr/pppppppp/n7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2"
                // +---+---+---+---+---+---+---+---+
                // | r |   | b | q | k | b | n | r |
                // +---+---+---+---+---+---+---+---+
                // | p | p | p | p | p | p | p | p |
                // +---+---+---+---+---+---+---+---+
                // | n |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   | P |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | P | P | P |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // | R | N | B | Q | K | B | N | R |
                // +---+---+---+---+---+---+---+---+
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r1bqkbnr/pppppppp/n7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");

                // Prepare our en-passant target
                Trace.WriteLine("Preparing en-passant victim d7d5...");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(D_FILE, 7), new ChessBoard.BoardSquare(D_FILE, 5), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the target was set
                Trace.WriteLine("Verifying target is at d7d6 (behind moved pawn)");
                CB.BoardSquare enPassantTarget;
                Assert.IsTrue(testBoard.GetEnPassantTarget(out enPassantTarget));
                Assert.AreEqual(enPassantTarget, new CB.BoardSquare(D_FILE, 6));

                // Make another mover
                Trace.WriteLine("ignore capture opportunity a2a3...");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(A_FILE, 2), new ChessBoard.BoardSquare(A_FILE, 3), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the target was cleared
                Trace.WriteLine("Verifying target is now cleared");
                Assert.IsFalse(testBoard.GetEnPassantTarget(out enPassantTarget));
            }
        }

        /// <summary>
        /// ChessBoard.CanPlayerCastle() tests
        /// </summary>
        [TestClass]
        public class CanPlayerCastleTests
        {
            /// <summary>
            /// CanPlayerCastle() checks only the rights, not the validity of the
            /// open spaces or check violations, moves will update however
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // There are 4 possible castles (Wh:Bl)(Kingside:Queenside)
                // White to move: "r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8"
                // Black to move: "r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R b KQkq - 6 8"
                //+---+---+---+---+---+---+---+---+
                //| r |   |   |   | k |   |   | r |
                //+---+---+---+---+---+---+---+---+
                //| p | p | p | q |   | p | p | p |
                //+---+---+---+---+---+---+---+---+
                //|   |   | n | b | b | n |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | p | p |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | P | P |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   | N | B | B | N |   |   |
                //+---+---+---+---+---+---+---+---+
                //| P | P | P | Q |   | P | P | P |
                //+---+---+---+---+---+---+---+---+
                //| R |   |   |   | K |   |   | R |
                //+---+---+---+---+---+---+---+---+
                // Reset board
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8");

                // Verify all rights intact
                Trace.WriteLine("Verifying all castling rights are still valid...");
                Assert.IsTrue(testBoard.CanPlayerCastle(PieceColor.White, BoardSide.King));
                Assert.IsTrue(testBoard.CanPlayerCastle(PieceColor.White, BoardSide.Queen));
                Assert.IsTrue(testBoard.CanPlayerCastle(PieceColor.Black, BoardSide.King));
                Assert.IsTrue(testBoard.CanPlayerCastle(PieceColor.Black, BoardSide.Queen));

                // Move the white kingside rook h1g1
                Trace.WriteLine("Moving kingside rook, h1g1...");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(H_FILE, 1), new ChessBoard.BoardSquare(G_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify white kingside rights are gone
                Trace.WriteLine("Verifying castling rights... white kingside cleared");
                Assert.IsFalse(testBoard.WhiteCastlingRights.HasFlag(BoardSide.King));

                // Move the black kingside rook h8g8
                Trace.WriteLine("Moving kingside rook, h8g8...");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(H_FILE, 8), new ChessBoard.BoardSquare(G_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify black kingside rights are gone
                Trace.WriteLine("Verifying castling rights... black kingside cleared");
                Assert.IsFalse(testBoard.BlackCastlingRights.HasFlag(BoardSide.King));

                // Move the white queenside rook a1b1
                Trace.WriteLine("Moving queenside rook, a1b1...");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(A_FILE, 1), new ChessBoard.BoardSquare(B_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify white queenside rights are gone
                Trace.WriteLine("Verifying castling rights... white queenside cleared");
                Assert.AreEqual(testBoard.WhiteCastlingRights, BoardSide.None);

                // Move the black queenside rook a8b8
                Trace.WriteLine("Moving queenside rook, a8b8...");
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(A_FILE, 8), new ChessBoard.BoardSquare(B_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify black queenside rights are gone
                Trace.WriteLine("Verifying castling rights... black queenside cleared");
                Assert.AreEqual(testBoard.BlackCastlingRights, BoardSide.None);
            }
        }

        /// <summary>
        /// ChessBoard.PromotPiece() tests
        /// </summary>
        [TestClass]
        public class PromotePieceTests
        {
            /// <summary>
            /// Basic tests for PromotePiece
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // PromotePiece does some verification, it must be a pawn and it must
                // be on a back rank
                // r2k2r1/p2n1P2/b3q2p/2p1p3/Pb1P4/R1N5/1pQ2PPP/3R2K1 w - - 0 22
                // +---+---+---+---+---+---+---+---+
                // | r |   |   | k |   |   | r |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   | n |   | P |   |   |
                // +---+---+---+---+---+---+---+---+
                // | b |   |   |   | q |   |   | p |
                // +---+---+---+---+---+---+---+---+
                // |   |   | p |   | p |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | b |   | P |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | R |   | N |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   | p | Q |   |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | R |   |   | K |   |
                // +---+---+---+---+---+---+---+---+

                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r2k2r1/p2n1P2/b3q2p/2p1p3/Pb1P4/R1N5/1pQ2PPP/3R2K1 w - - 0 22");

                // setup the promotion move f7g8n
                // Don't promote to Queen or Rook as the next move will be illegal with Black
                // King in check
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(F_FILE, 7), new ChessBoard.BoardSquare(G_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.PromotePiece(F_FILE, 7, G_FILE, 8, PieceClass.Knight, ref moveInfo);
                testBoard.MovePiece(ref moveInfo);

                // Verify the promoted piece
                ChessPiece piece = testBoard.FindPieceAt(G_FILE, 8);
                Assert.AreEqual(PieceClass.Knight, piece.Job);
                Assert.AreEqual(PieceColor.White, piece.Color);

                // Promote black pawn b2b1q
                moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(B_FILE, 2), new ChessBoard.BoardSquare(B_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.PromotePiece(B_FILE, 2, B_FILE, 1, PieceClass.Queen, ref moveInfo);
                testBoard.MovePiece(ref moveInfo);

                // Verify the promoted piece
                piece = testBoard.FindPieceAt(B_FILE, 1);
                Assert.AreEqual(PieceClass.Queen, piece.Job);
                Assert.AreEqual(PieceColor.Black, piece.Color);
            }
        }

        /// <summary>
        /// ChessBoard.FindPieceAt() tests
        /// </summary>
        [TestClass]
        public class FindPieceAtTests
        {
            /// <summary>
            /// Basic tests - This gets coverage in other tests as a matter of course
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // position fen "r5Q1/p2nk3/b3q2p/2P1p3/Pb6/R1N5/2Q2PPP/1q1R2K1 w - - 0 24"
                // StockFish8 debug output
                // +---+---+---+---+---+---+---+---+
                // | r |   |   |   |   |   | Q |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   | n | k |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | b |   |   |   | q |   |   | p |
                // +---+---+---+---+---+---+---+---+
                // |   |   | P |   | p |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | b |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | R |   | N |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   | Q |   |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // |   | q |   | R |   |   | K |   |
                // +---+---+---+---+---+---+---+---+
                Tuple<CB.BoardSquare, PieceColor, PieceClass>[] positionalBoardData =
                {
                    // WHITE PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 4), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 3), WH, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 5), WH, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 3), WH, KNHT ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(C_FILE, 2), WH, QUEN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 1), WH, ROOK ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(F_FILE, 2), WH, PAWN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 8), WH, QUEN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 2), WH, PAWN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(G_FILE, 1), WH, KING ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 2), WH, PAWN ),
                    // BLACK PIECES
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 8), BL, ROOK),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 7), BL, PAWN),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(A_FILE, 6), BL, BISH),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 4), BL, BISH ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(B_FILE, 1), BL, QUEN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(D_FILE, 7), BL, KNHT ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 7), BL, KING ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 6), BL, QUEN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(E_FILE, 5), BL, PAWN ),
                    new Tuple<CB.BoardSquare, PieceColor, PieceClass>(new CB.BoardSquare(H_FILE, 6), BL, PAWN ),
                };

                // Very similar to NewGame, but we could create many more positions to verify
                // For now, use one I created playing a game on lichess.org
                ChessBoard testBoard = new ChessBoard();
                Trace.WriteLine("ChessBoard.NewPosition(fen)...");
                testBoard.NewPosition("r5Q1/p2nk3/b3q2p/2P1p3/Pb6/R1N5/2Q2PPP/1q1R2K1 w - - 0 24");

                Trace.WriteLine("Verifying contents of test ChessBoard...");
                foreach (Tuple<CB.BoardSquare, PieceColor, PieceClass> tuple in positionalBoardData)
                {
                    VerifyChessBoardPieces(testBoard, tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }
        }

        /// <summary>
        /// ChessBoard.GetKing() tests
        /// </summary>
        [TestClass]
        public class GetKingTests
        {
            /// <summary>
            /// Basic tests
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // Any valid chessboard will have the kings on it since they
                // are never captured.
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewGame();

                Trace.WriteLine("Getting White King...");
                ChessPiece whiteKing = testBoard.GetKing(PieceColor.White);
                Assert.AreEqual(whiteKing.Job, PieceClass.King);

                Trace.WriteLine("Getting Black King...");
                ChessPiece blackKing = testBoard.GetKing(PieceColor.Black);
                Assert.AreEqual(blackKing.Job, PieceClass.King);
            }
        }

        /// <summary>
        /// ChessBoard.RevertLastMove() tests
        /// </summary>
        [TestClass]
        public class RevertLastMoveTests
        {
            /// <summary>
            /// Basic revert moves tests
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // RevertLastMove() does just what it says.  To test it we can apply
                // a move, then revert it and verify the board state
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewGame();

                // Basic opening pawn move "e2e4"
                Trace.WriteLine("Applying a basic pawn move...e2e4");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 2), new ChessBoard.BoardSquare(E_FILE, 4), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                Trace.WriteLine("Verify pawn is found at new location...");
                ChessPiece piece = testBoard.FindPieceAt(E_FILE, 4);
                Assert.AreEqual(piece.Job, PieceClass.Pawn);
                Assert.AreEqual(piece.Color, PieceColor.White);

                // Now revert it
                Trace.WriteLine("Reverting the move...");
                testBoard.RevertLastMove();

                Trace.WriteLine("Verify pawn is not at e4");
                piece = testBoard.FindPieceAt(E_FILE, 4);
                Assert.IsNull(piece);

                Trace.WriteLine("Verify pawn is back at e2, and not deployed");
                piece = testBoard.FindPieceAt(E_FILE, 2);
                Assert.AreEqual(piece.Job, PieceClass.Pawn);
                Assert.IsFalse(piece.Deployed);
            }

            /// <summary>
            /// Reverts a castling move
            /// </summary>
            [TestMethod]
            public void RevertCastlingMoveTest()
            {
                // There are 4 possible castles (Wh:Bl)(Kingside:Queenside)
                // White to move: "r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8"
                //+---+---+---+---+---+---+---+---+
                //| r |   |   |   | k |   |   | r |
                //+---+---+---+---+---+---+---+---+
                //| p | p | p | q |   | p | p | p |
                //+---+---+---+---+---+---+---+---+
                //|   |   | n | b | b | n |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | p | p |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   |   | P | P |   |   |   |
                //+---+---+---+---+---+---+---+---+
                //|   |   | N | B | B | N |   |   |
                //+---+---+---+---+---+---+---+---+
                //| P | P | P | Q |   | P | P | P |
                //+---+---+---+---+---+---+---+---+
                //| R |   |   |   | K |   |   | R |
                //+---+---+---+---+---+---+---+---+
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r3k2r/pppq1ppp/2nbbn2/3pp3/3PP3/2NBBN2/PPPQ1PPP/R3K2R w KQkq - 6 8");

                // Castle White Kingside
                Trace.WriteLine("Castle White Kingside - e1g1");
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(E_FILE, 1), new ChessBoard.BoardSquare(G_FILE, 1), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify the rook moved as well
                Trace.WriteLine("Verifying the rook moved as well");
                ChessPiece whiteKingsideRook = testBoard.FindPieceAt(F_FILE, 1);
                Assert.AreEqual(whiteKingsideRook.Job, PieceClass.Rook);
                Trace.WriteLine("Verifying the castling rights are updated");
                Assert.IsFalse(testBoard.WhiteCastlingRights.HasFlag(BoardSide.King));

                // Revert it
                testBoard.RevertLastMove();

                // Verify the king is back home
                Trace.WriteLine("Verify the king is back home");
                ChessPiece king = testBoard.GetKing(PieceColor.White);
                Assert.AreEqual(king.Job, PieceClass.King);
                Assert.AreEqual(king.Color, PieceColor.White);
                Assert.AreEqual(king.File, E_FILE);
                Assert.AreEqual(king.Rank, 1);
                Assert.IsFalse(king.Deployed);

                // Verify the rook is back home
                Trace.WriteLine("Verify the rook is back home");
                ChessPiece rook = testBoard.FindPieceAt(H_FILE, 1);
                Assert.AreEqual(rook.Job, PieceClass.Rook);
                Assert.AreEqual(rook.Color, PieceColor.White);
                Assert.AreEqual(rook.File, H_FILE);
                Assert.AreEqual(rook.Rank, 1);
                Assert.IsFalse(rook.Deployed);
            }

            /// <summary>
            /// Reverts a promotion move
            /// </summary>
            [TestMethod]
            public void RevertPromotionMoveTest()
            {
                // PromotePiece does some verification, it must be a pawn and it must
                // be on a back rank
                // r2k2r1/p2n1P2/b3q2p/2p1p3/Pb1P4/R1N5/1pQ2PPP/3R2K1 w - - 0 22
                // +---+---+---+---+---+---+---+---+
                // | r |   |   | k |   |   | r |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   | n |   | P |   |   |
                // +---+---+---+---+---+---+---+---+
                // | b |   |   |   | q |   |   | p |
                // +---+---+---+---+---+---+---+---+
                // |   |   | p |   | p |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | b |   | P |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | R |   | N |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   | p | Q |   |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | R |   |   | K |   |
                // +---+---+---+---+---+---+---+---+
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r2k2r1/p2n1P2/b3q2p/2p1p3/Pb1P4/R1N5/1pQ2PPP/3R2K1 w - - 0 22");

                // setup the promotion move f7g8n
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(F_FILE, 7), new ChessBoard.BoardSquare(G_FILE, 8), false, testBoard.CurrentFEN);
                testBoard.PromotePiece(F_FILE, 7, G_FILE, 8, PieceClass.Knight, ref moveInfo);
                testBoard.MovePiece(ref moveInfo);

                // Verify the promoted piece
                ChessPiece piece = testBoard.FindPieceAt(G_FILE, 8);
                Assert.AreEqual(PieceClass.Knight, piece.Job);
                Assert.AreEqual(PieceColor.White, piece.Color);

                // Revert the move
                testBoard.RevertLastMove();

                // Verify the piece is a pawn again and back at F7
                piece = testBoard.FindPieceAt(F_FILE, 7);
                Assert.AreEqual(PieceClass.Pawn, piece.Job);
            }
        }

        /// <summary>
        /// Public Properties tests
        /// </summary>
        [TestClass]
        public class PropertiesTests
        {
            // Listed for reference:
            //================================
            // LastMoveWasCapture
            // Orientation
            // Moves
            // WhitePieces
            // BlackPieces
            // ActivePlayer
            // HalfMoveCount
            // FullMoveCount
            // WhiteCastlingRights
            // BlackCastlingRights
            // ActivePlayerCastlingRights
            // OpponentPlayerCastlingRights

            /// <summary>
            /// Most of these are covered in other tests
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // r4rk1/4bpp1/pqbppnPp/1p6/4P3/P1N1B3/1PPNB1PP/R3QRK1 b - - 0 17
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition("r4rk1/4bpp1/pqbppnPp/1p6/4P3/P1N1B3/1PPNB1PP/R3QRK1 b - - 0 17");

                // Black queen takes white bishop
                ChessBoard.MoveInformation moveInfo = new CB.MoveInformation(
                    new ChessBoard.BoardSquare(B_FILE, 6), new ChessBoard.BoardSquare(E_FILE, 3), false, testBoard.CurrentFEN);
                testBoard.MovePiece(ref moveInfo);

                // Verify Properties
                Trace.WriteLine("Verifying LastMoveWasCapture is true...");
                Assert.IsTrue(testBoard.LastMoveWasCapture);

                Trace.WriteLine("Verifying move counts...");
                Assert.AreEqual(testBoard.HalfMoveCount, 0);
                Assert.AreEqual(testBoard.FullMoveCount, 18);

                Trace.WriteLine("Verifying castling rights...");
                Assert.AreEqual(testBoard.WhiteCastlingRights, BoardSide.None);
                Assert.AreEqual(testBoard.BlackCastlingRights, BoardSide.None);

                Trace.WriteLine("Verifying orientation...");
                Assert.AreEqual(testBoard.Orientation, BoardOrientation.WhiteOnBottom);
                Assert.AreEqual(testBoard.BlackCastlingRights, BoardSide.None);
            }
        }
    }
}