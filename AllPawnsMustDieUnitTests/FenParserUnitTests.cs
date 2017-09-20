using System;
using System.Diagnostics;
using AllPawnsMustDie;
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
    /// Holds tests related to the FenParser class
    /// </summary>
    [TestClass]
    public class FenParserUnitTests
    {
        /// <summary>
        /// FenParser constructor tests - really the only public methods, either
        /// constructed with a FEN string or with a ChessBoard object
        /// </summary>
        [TestClass]
        public class FenParserConstructorTests
        {
            /// <summary>
            /// FenParser(fen string) tests
            /// </summary>
            [TestMethod]
            public void FenParserStringInitTestMethod()
            {
                // List of valid FENs to parse, each item added to this list should get
                // an item added to the testVerifications Tuple below for the expected results
                string[] testFENs =
                {
                    "5rk1/pbq2ppn/1p1rp2p/3p4/2P2Q2/1P1B4/P2N1PPP/3RR1K1 b - - 1 24",
                    "5rk1/1pq1ppbp/p5p1/3pnP2/3BnQP1/1PP4P/4B3/4RRK1 b - - 0 23",
                    "r1bqkb1r/pp1n1ppp/2p1pn2/3p4/2PP4/2N1PN2/PP3PPP/R1BQKB1R w KQkq - 1 6",
                };

                // Should match the array above, just some basic read back of properties
                // once parsed
                Tuple<PieceColor, BoardSide, BoardSide, int, int>[] testVerifications =
                {
                    // ActivePlayer, White Castle, Black Castle, Half moves, Full moves
                    new Tuple<PieceColor, BoardSide, BoardSide, int, int>(PieceColor.Black, BoardSide.None, BoardSide.None, 1, 24),
                    new Tuple<PieceColor, BoardSide, BoardSide, int, int>(PieceColor.Black, BoardSide.None, BoardSide.None, 0, 23),
                    new Tuple<PieceColor, BoardSide, BoardSide, int, int>(PieceColor.White, (BoardSide.King | BoardSide.Queen), (BoardSide.King | BoardSide.Queen), 1, 6),
                };

                // Make sure these are always matching in length (test bug)
                Assert.AreEqual(testFENs.Length, testVerifications.Length);

                for (int index=0;index<testFENs.Length;index++)
                {
                    string fen = testFENs[index];
                    Trace.WriteLine(String.Format("Initializing FenParser with fen: {0}...", fen));
                    FenParser parser = new FenParser(fen);

                    Trace.WriteLine(String.Format("Verifying calculated FEN matches the original...", fen));
                    Assert.IsTrue(String.Compare(parser.FEN, fen) == 0);

                    Trace.WriteLine("Verifying additional properties of the parser...");
                    Trace.WriteLine(String.Format("...ActivePlayer == {0}", testVerifications[index].Item1.ToString()));
                    Assert.AreEqual(parser.ActivePlayer, testVerifications[index].Item1);

                    Trace.WriteLine(String.Format("...WhiteCastlingRights == {0}", testVerifications[index].Item2.ToString()));
                    Assert.AreEqual(parser.WhiteCastlingRights, testVerifications[index].Item2);

                    Trace.WriteLine(String.Format("...BlackCastlingRights == {0}", testVerifications[index].Item3.ToString()));
                    Assert.AreEqual(parser.BlackCastlingRights, testVerifications[index].Item3);

                    Trace.WriteLine(String.Format("...HalfMoves == {0}", testVerifications[index].Item4));
                    Assert.AreEqual(parser.HalfMoves, testVerifications[index].Item4);

                    Trace.WriteLine(String.Format("...FullMoves == {0}", testVerifications[index].Item5));
                    Assert.AreEqual(parser.FullMoves, testVerifications[index].Item5);
                }
            }

            /// <summary>
            /// FenParser(ChessBoard) tests
            /// </summary>
            [TestMethod]
            public void FenParserChessBoardInitDefautlBoardTestMethod()
            {
                Trace.WriteLine("Creating a default chess board...");
                ChessBoard testBoard = new ChessBoard();
                testBoard.NewGame();

                Trace.WriteLine("Creating a FenParser based on the ChessBoard object...");
                FenParser parser = new FenParser(testBoard);

                Trace.WriteLine("Verifying default board parsed FEN...");
                Assert.IsTrue(String.Compare(parser.FEN, ChessBoard.InitialFENPosition) == 0);
            }

            /// <summary>
            /// Tests various positions not just the default one
            /// </summary>
            [TestMethod]
            public void FenParserChessBoardInitVariousBoardsTestMethod()
            {
                // List of FENs to verify
                // We'll initialize a board with a FEN, then ask for the 
                // calculated FEN back, they should match
                string[] testFENs =
                {
                    "5rk1/pbq2ppn/1p1rp2p/3p4/2P2Q2/1P1B4/P2N1PPP/3RR1K1 b - - 1 24",
                    "5rk1/1pq1ppbp/p5p1/3pnP2/3BnQP1/1PP4P/4B3/4RRK1 b - - 0 23",
                    "r1bqkb1r/pp1n1ppp/2p1pn2/3p4/2PP4/2N1PN2/PP3PPP/R1BQKB1R w KQkq - 1 6",
                };

                foreach (string fen in testFENs)
                {
                    ChessBoard testBoard = new ChessBoard();
                    Trace.WriteLine(String.Format("Initializing ChessBoard with FEN: {0}", fen));
                    testBoard.NewPosition(fen);
                    Trace.WriteLine(String.Format("Initializing FenParser with ChessBoard...", fen));
                    FenParser parser = new FenParser(testBoard);
                    Trace.WriteLine(String.Format("Verifying calculated FEN matches the original...", fen));
                    Assert.IsTrue(String.Compare(parser.FEN, fen) == 0);
                }
            }

            /// <summary>
            /// Initialize a position, then make move(s) and retrieve the updated
            /// FEN and veify it
            /// </summary>
            [TestMethod]
            public void FenParserChessBoardInitPositionThenMoveTestMethod()
            {
                string fenBefore = "rnbqkb1r/pp3ppp/4pn2/2p5/2BP4/4PN2/PP3PPP/RNBQK2R w KQkq - 0 6";
                // 6. O-O (Castle Kingside) => new fen...
                string fenAfter = "rnbqkb1r/pp3ppp/4pn2/2p5/2BP4/4PN2/PP3PPP/RNBQ1RK1 b kq - 1 6";

                ChessBoard testBoard = new ChessBoard();
                testBoard.NewPosition(fenBefore);

                // Castle Kingside O-O or "e1g1"
                Trace.WriteLine("Castling Kingside...\"e1g1\"");
                ChessBoard.MoveInformation moveInfo = new ChessBoard.MoveInformation(
                    new ChessBoard.BoardSquare(new PieceFile('e'), 1), new ChessBoard.BoardSquare(new PieceFile('g'), 1), false);
                testBoard.MovePiece(ref moveInfo);

                // Now create a parser from the board - this should calculate the expecte FEN correctly
                Trace.WriteLine("Creating parser from updated ChessBoard...");
                FenParser parser = new FenParser(testBoard);

                Trace.WriteLine(String.Format("Verifying calculated FEN after move matches the expected...{0}", fenAfter));
                Assert.IsTrue(String.Compare(parser.FEN, fenAfter) == 0);
            }
        }
    }
}
