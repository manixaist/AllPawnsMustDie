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
        /// Tests for the static helpers that manipulate the FEN string directly
        /// </summary>
        [TestClass]
        public class FenParserStaticMethodTests
        {
            /// <summary>
            /// Holds tests for FenParser.ExpandRank()
            /// </summary>
            [TestMethod]
            public void ExpandRankTests()
            {
                string[] testFENS =
                {
                    "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1",
                    "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6",
                    "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7",
                };

                // index to testFENS, rank to expand, expected result
                Tuple<int, int, string>[] testData =
                {
                    // Expand each rank and check it 
                    // 0
                    new Tuple<int, int, string>(0, 8, "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 7, "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 6, "rnbqkbnr/pppppppp/11111111/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 5, "rnbqkbnr/pppppppp/8/11111111/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 4, "rnbqkbnr/pppppppp/8/8/111P1111/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 3, "rnbqkbnr/pppppppp/8/8/3P4/11111111/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 2, "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<int, int, string>(0, 1, "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    // 1
                    new Tuple<int, int, string>(1, 8, "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 7, "rnbqkb1r/pp111ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 6, "rnbqkb1r/pp3ppp/11p1pn11/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 5, "rnbqkb1r/pp3ppp/2p1pn2/111111B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 4, "rnbqkb1r/pp3ppp/2p1pn2/6B1/11pP1111/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 3, "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/11N1PN11/PP3PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 2, "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP111PPP/R2QKB1R b KQkq - 0 6"),
                    new Tuple<int, int, string>(1, 1, "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R11QKB1R b KQkq - 0 6"),
                    // 2
                    new Tuple<int, int, string>(2, 8, "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 7, "rnb1kb1r/pp111ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 6, "rnb1kb1r/pp3ppp/11p1pn11/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 5, "rnb1kb1r/pp3ppp/2p1pn2/111p1111/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 4, "rnb1kb1r/pp3ppp/2p1pn2/3p4/111PP111/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 3, "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/111BBN11/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 2, "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7"),
                    new Tuple<int, int, string>(2, 1, "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R11QK11R w KQkq - 2 7"),
                };

                foreach (Tuple<int, int, string> tuple in testData)
                {
                    string result = FenParser.ExpandRank(testFENS[tuple.Item1], tuple.Item2);
                    Trace.WriteLine(String.Format("Verifying {0} @ {1} => {2}", testFENS[tuple.Item1], tuple.Item2, tuple.Item3));
                    Assert.IsTrue(String.Compare(result, tuple.Item3) == 0);
                }
            }

            /// <summary>
            /// Holds tests for FenParser.CollapseRank()
            /// </summary>
            [TestMethod]
            public void CollapseRankTests()
            {
                string[] testFENS =
                {
                    "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1",
                    "rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6",
                    "rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7",
                };

                // input, rank, expected index
                Tuple<string, int, int>[] testData =
                {
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 8, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 7, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/11111111/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 6, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/11111111/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 5, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/111P1111/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 4, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/3P4/11111111/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 3, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 2, 0),
                    new Tuple<string, int, int>("rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1", 1, 0),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6", 8, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp111ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6", 7, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/11p1pn11/6B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6", 6, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/111111B1/2pP4/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6", 5, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/6B1/11pP1111/2N1PN2/PP3PPP/R2QKB1R b KQkq - 0 6", 4, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/11N1PN11/PP3PPP/R2QKB1R b KQkq - 0 6", 3, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP111PPP/R2QKB1R b KQkq - 0 6", 2, 1),
                    new Tuple<string, int, int>("rnbqkb1r/pp3ppp/2p1pn2/6B1/2pP4/2N1PN2/PP3PPP/R11QKB1R b KQkq - 0 6", 1, 1),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 8, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp111ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 7, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/11p1pn11/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 6, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/111p1111/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 5, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/3p4/111PP111/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 4, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/111BBN11/PqPN1PPP/R2QK2R w KQkq - 2 7", 3, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R2QK2R w KQkq - 2 7", 2, 2),
                    new Tuple<string, int, int>("rnb1kb1r/pp3ppp/2p1pn2/3p4/3PP3/3BBN2/PqPN1PPP/R11QK11R w KQkq - 2 7", 1, 2),
                };

                foreach (Tuple<string, int, int> tuple in testData)
                {
                    string result = FenParser.CollapseRank(tuple.Item1, tuple.Item2);
                    Trace.WriteLine(String.Format("Verifying {0} @ {1} => {2} : {3}", tuple.Item1, tuple.Item2, testFENS[tuple.Item3], result));
                    Assert.IsTrue(String.Compare(result, testFENS[tuple.Item3]) == 0);
                }
            }

            /// <summary>
            /// Holds tests for FenParser.PieceAtBoardPosition()
            /// </summary>
            [TestMethod]
            public void PieceAtBoardPositionTests()
            {
                //"r1bq1rk1/pp3ppp/3b1n2/3pN3/8/3B4/PPPN1PPP/R1BQ1RK1 b - - 0 11";
                // This method assumes the FEN is already expanded on the correct rank, so just expand the 
                // whole thing for the tests.  Expansion has its own tests.
                string expandedFEN = "r1bq1rk1/pp111ppp/111b1n11/111pN111/11111111/111B1111/PPPN1PPP/R1BQ1RK1 b - - 0 11";
                // Board position (WHITE IS UPPERCASE)
                // +---+---+---+---+---+---+---+---+
                // | r |   | b | q |   | r | k |   |
                // +---+---+---+---+---+---+---+---+
                // | p | p |   |   |   | p | p | p |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | b |   | n |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | p | N |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | B |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // | P | P | P | N |   | P | P | P |
                // +---+---+---+---+---+---+---+---+
                // | R |   | B | Q |   | R | K |   |
                // +---+---+---+---+---+---+---+---+

                // File, Rank, Expected
                Tuple<int, int, char>[] testData =
                {
                    new Tuple<int, int, char>(1, 1, 'R'),
                    new Tuple<int, int, char>(3, 1, 'B'),
                    new Tuple<int, int, char>(4, 1, 'Q'),
                    new Tuple<int, int, char>(6, 1, 'R'),
                    new Tuple<int, int, char>(7, 1, 'K'),
                    new Tuple<int, int, char>(1, 2, 'P'),
                    new Tuple<int, int, char>(2, 2, 'P'),
                    new Tuple<int, int, char>(3, 2, 'P'),
                    new Tuple<int, int, char>(4, 2, 'N'),
                    new Tuple<int, int, char>(5, 2, '1'), // Empty space since expanded
                    new Tuple<int, int, char>(6, 2, 'P'),
                    new Tuple<int, int, char>(7, 2, 'P'),
                    new Tuple<int, int, char>(8, 2, 'P'),
                    new Tuple<int, int, char>(4, 3, 'B'),
                    new Tuple<int, int, char>(4, 5, 'p'),
                    new Tuple<int, int, char>(5, 5, 'N'),
                    new Tuple<int, int, char>(4, 6, 'b'),
                    new Tuple<int, int, char>(6, 6, 'n'),
                    new Tuple<int, int, char>(1, 8, 'r'),
                    new Tuple<int, int, char>(8, 8, '1'),
                };

                foreach (Tuple<int, int, char> tuple in testData)
                {
                    Trace.WriteLine(String.Format("Veriying {0} found at [{1}:{2}]", tuple.Item3, tuple.Item1, tuple.Item2));
                    Assert.AreEqual(tuple.Item3, FenParser.PieceAtBoardPosition(expandedFEN, tuple.Item1, tuple.Item2));
                }
                
            }

            /// <summary>
            /// Holds tests for FenParser.InsertPiece()
            /// </summary>
            [TestMethod]
            public void InsertPieceTests()
            {
                // Note this is raw manipulation of the string, not logic, i.e. inserting a piece
                // with this method does not have side effects like updating the move counts or
                // castling rights
                //"r3r1k1/p2b1ppp/1p1q1n2/3p4/2P5/3BRN1P/PP1Q1PP1/R5K1 w - - 0 19";
                string expandedFEN = "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19";
                // +---+---+---+---+---+---+---+---+
                // | r |   |   |   | r |   | k |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   | b |   | p | p | p |
                // +---+---+---+---+---+---+---+---+
                // |   | p |   | q |   | n |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | p |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   | P |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | B | R | N |   | P |
                // +---+---+---+---+---+---+---+---+
                // | P | P |   | Q |   | P | P |   |
                // +---+---+---+---+---+---+---+---+
                // | R |   |   |   |   |   | K |   |
                // +---+---+---+---+---+---+---+---+

                // Piece, file, rank, expected
                Tuple<char, int, int, string>[] testData =
                {
                    new Tuple<char, int, int, string>('Q', 3, 1, "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R1Q111K1 w - - 0 19"),
                    new Tuple<char, int, int, string>('P', 2, 8, "rP11r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                    new Tuple<char, int, int, string>('b', 5, 5, "r111r1k1/p11b1ppp/1p1q1n11/111pb111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                };

                foreach (Tuple<char, int, int, string> tuple in testData)
                {
                    Trace.WriteLine(String.Format("Verifying piece '{0}' inserted at [{1}:{2}] => {3}", tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
                    Assert.IsTrue(String.Compare(FenParser.InsertPiece(expandedFEN, tuple.Item1, tuple.Item2, tuple.Item3), tuple.Item4) == 0);
                }
            }

            /// <summary>
            /// Holds tests for FenParser.RemovePiece()
            /// </summary>
            [TestMethod]
            public void RemovePieceTests()
            {
                //"r3r1k1/p2b1ppp/1p1q1n2/3p4/2P5/3BRN1P/PP1Q1PP1/R5K1 w - - 0 19";
                string expandedFEN = "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19";
                // +---+---+---+---+---+---+---+---+
                // | r |   |   |   | r |   | k |   |
                // +---+---+---+---+---+---+---+---+
                // | p |   |   | b |   | p | p | p |
                // +---+---+---+---+---+---+---+---+
                // |   | p |   | q |   | n |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | p |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   | P |   |   |   |   |   |
                // +---+---+---+---+---+---+---+---+
                // |   |   |   | B | R | N |   | P |
                // +---+---+---+---+---+---+---+---+
                // | P | P |   | Q |   | P | P |   |
                // +---+---+---+---+---+---+---+---+
                // | R |   |   |   |   |   | K |   |
                // +---+---+---+---+---+---+---+---+

                // file, rank, expectedChar, expectedFEN
                Tuple<int, int, char, string>[] testData =
                {
                    new Tuple<int, int, char, string>(1, 1, 'R', "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/111111K1 w - - 0 19"),
                    new Tuple<int, int, char, string>(2, 1, '1', "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                    new Tuple<int, int, char, string>(4, 7, 'b', "r111r1k1/p1111ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                    new Tuple<int, int, char, string>(7, 1, 'K', "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R1111111 w - - 0 19"),
                    new Tuple<int, int, char, string>(1, 4, '1', "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                    new Tuple<int, int, char, string>(8, 8, '1', "r111r1k1/p11b1ppp/1p1q1n11/111p1111/11P11111/111BRN1P/PP1Q1PP1/R11111K1 w - - 0 19"),
                };

                foreach (Tuple<int, int, char, string> tuple in testData)
                {
                    char fenChar;
                    Trace.WriteLine(String.Format("Verifying Remove @ [{0}:{1}] is '{2}', new fen is {3}", tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
                    string newFEN = FenParser.RemovePiece(expandedFEN, tuple.Item1, tuple.Item2, out fenChar);
                    Assert.IsTrue(String.Compare(newFEN, tuple.Item4) == 0);
                    Assert.AreEqual(fenChar, tuple.Item3);
                }
            }

            /// <summary>
            /// Holds tests for FenParser.ApplyMoveToFEN()
            /// </summary>
            [TestMethod]
            public void ApplyMoveToFENTests()
            {
                // This is the method meant to be called by clients for the most
                // part and does take side-effects into account, such as captures,
                // promotions, castlings, updating the move count and active player, etc

                // You should be able to replay a game by applying moves repeatedly
                string initialFEN = ChessBoard.InitialFENPosition;

                // These assume starting from the standard position with white
                // "san move", "expected new FEN"
                Tuple<string, string>[] testData =
                {
                    // Records from a game where stockfish 8 played itself (plus some forced moves to test promotion)
                    new Tuple<string, string>("d2d4", "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq - 0 1"),
                    new Tuple<string, string>("d7d5", "rnbqkbnr/ppp1pppp/8/3p4/3P4/8/PPP1PPPP/RNBQKBNR w KQkq - 0 2"),
                    new Tuple<string, string>("c2c4", "rnbqkbnr/ppp1pppp/8/3p4/2PP4/8/PP2PPPP/RNBQKBNR b KQkq - 0 2"),
                    new Tuple<string, string>("e7e6", "rnbqkbnr/ppp2ppp/4p3/3p4/2PP4/8/PP2PPPP/RNBQKBNR w KQkq - 0 3"),
                    new Tuple<string, string>("g1f3", "rnbqkbnr/ppp2ppp/4p3/3p4/2PP4/5N2/PP2PPPP/RNBQKB1R b KQkq - 1 3"),
                    new Tuple<string, string>("g8f6", "rnbqkb1r/ppp2ppp/4pn2/3p4/2PP4/5N2/PP2PPPP/RNBQKB1R w KQkq - 2 4"),
                    new Tuple<string, string>("b1c3", "rnbqkb1r/ppp2ppp/4pn2/3p4/2PP4/2N2N2/PP2PPPP/R1BQKB1R b KQkq - 3 4"),
                    new Tuple<string, string>("c7c6", "rnbqkb1r/pp3ppp/2p1pn2/3p4/2PP4/2N2N2/PP2PPPP/R1BQKB1R w KQkq - 0 5"),
                    new Tuple<string, string>("e2e3", "rnbqkb1r/pp3ppp/2p1pn2/3p4/2PP4/2N1PN2/PP3PPP/R1BQKB1R b KQkq - 0 5"),
                    new Tuple<string, string>("b8d7", "r1bqkb1r/pp1n1ppp/2p1pn2/3p4/2PP4/2N1PN2/PP3PPP/R1BQKB1R w KQkq - 1 6"),
                    new Tuple<string, string>("f1d3", "r1bqkb1r/pp1n1ppp/2p1pn2/3p4/2PP4/2NBPN2/PP3PPP/R1BQK2R b KQkq - 2 6"),
                    new Tuple<string, string>("d5c4", "r1bqkb1r/pp1n1ppp/2p1pn2/8/2pP4/2NBPN2/PP3PPP/R1BQK2R w KQkq - 0 7"),
                    new Tuple<string, string>("d3c4", "r1bqkb1r/pp1n1ppp/2p1pn2/8/2BP4/2N1PN2/PP3PPP/R1BQK2R b KQkq - 0 7"),
                    new Tuple<string, string>("b7b5", "r1bqkb1r/p2n1ppp/2p1pn2/1p6/2BP4/2N1PN2/PP3PPP/R1BQK2R w KQkq - 0 8"),
                    new Tuple<string, string>("c4e2", "r1bqkb1r/p2n1ppp/2p1pn2/1p6/3P4/2N1PN2/PP2BPPP/R1BQK2R b KQkq - 1 8"),
                    new Tuple<string, string>("c8b7", "r2qkb1r/pb1n1ppp/2p1pn2/1p6/3P4/2N1PN2/PP2BPPP/R1BQK2R w KQkq - 2 9"),
                    new Tuple<string, string>("e1g1", "r2qkb1r/pb1n1ppp/2p1pn2/1p6/3P4/2N1PN2/PP2BPPP/R1BQ1RK1 b kq - 3 9"),
                    new Tuple<string, string>("f8e7", "r2qk2r/pb1nbppp/2p1pn2/1p6/3P4/2N1PN2/PP2BPPP/R1BQ1RK1 w kq - 4 10"),
                    new Tuple<string, string>("a2a3", "r2qk2r/pb1nbppp/2p1pn2/1p6/3P4/P1N1PN2/1P2BPPP/R1BQ1RK1 b kq - 0 10"),
                    new Tuple<string, string>("a7a5", "r2qk2r/1b1nbppp/2p1pn2/pp6/3P4/P1N1PN2/1P2BPPP/R1BQ1RK1 w kq - 0 11"),
                    new Tuple<string, string>("b2b3", "r2qk2r/1b1nbppp/2p1pn2/pp6/3P4/PPN1PN2/4BPPP/R1BQ1RK1 b kq - 0 11"),
                    new Tuple<string, string>("e8g8", "r2q1rk1/1b1nbppp/2p1pn2/pp6/3P4/PPN1PN2/4BPPP/R1BQ1RK1 w - - 1 12"),
                    new Tuple<string, string>("c1b2", "r2q1rk1/1b1nbppp/2p1pn2/pp6/3P4/PPN1PN2/1B2BPPP/R2Q1RK1 b - - 2 12"),
                    new Tuple<string, string>("b5b4", "r2q1rk1/1b1nbppp/2p1pn2/p7/1p1P4/PPN1PN2/1B2BPPP/R2Q1RK1 w - - 0 13"),
                    new Tuple<string, string>("a3b4", "r2q1rk1/1b1nbppp/2p1pn2/p7/1P1P4/1PN1PN2/1B2BPPP/R2Q1RK1 b - - 0 13"),
                    new Tuple<string, string>("a5b4", "r2q1rk1/1b1nbppp/2p1pn2/8/1p1P4/1PN1PN2/1B2BPPP/R2Q1RK1 w - - 0 14"),
                    new Tuple<string, string>("a1a8", "R2q1rk1/1b1nbppp/2p1pn2/8/1p1P4/1PN1PN2/1B2BPPP/3Q1RK1 b - - 0 14"),
                    new Tuple<string, string>("d8a8", "q4rk1/1b1nbppp/2p1pn2/8/1p1P4/1PN1PN2/1B2BPPP/3Q1RK1 w - - 0 15"),
                    new Tuple<string, string>("c3a4", "q4rk1/1b1nbppp/2p1pn2/8/Np1P4/1P2PN2/1B2BPPP/3Q1RK1 b - - 1 15"),
                    new Tuple<string, string>("c6c5", "q4rk1/1b1nbppp/4pn2/2p5/Np1P4/1P2PN2/1B2BPPP/3Q1RK1 w - - 0 16"),
                    new Tuple<string, string>("d4c5", "q4rk1/1b1nbppp/4pn2/2P5/Np6/1P2PN2/1B2BPPP/3Q1RK1 b - - 0 16"),
                    new Tuple<string, string>("d7c5", "q4rk1/1b2bppp/4pn2/2n5/Np6/1P2PN2/1B2BPPP/3Q1RK1 w - - 0 17"),
                    new Tuple<string, string>("a4c5", "q4rk1/1b2bppp/4pn2/2N5/1p6/1P2PN2/1B2BPPP/3Q1RK1 b - - 0 17"),
                    new Tuple<string, string>("e7c5", "q4rk1/1b3ppp/4pn2/2b5/1p6/1P2PN2/1B2BPPP/3Q1RK1 w - - 0 18"),
                    new Tuple<string, string>("d1c2", "q4rk1/1b3ppp/4pn2/2b5/1p6/1P2PN2/1BQ1BPPP/5RK1 b - - 1 18"),
                    new Tuple<string, string>("f8c8", "q1r3k1/1b3ppp/4pn2/2b5/1p6/1P2PN2/1BQ1BPPP/5RK1 w - - 2 19"),
                    new Tuple<string, string>("f1a1", "q1r3k1/1b3ppp/4pn2/2b5/1p6/1P2PN2/1BQ1BPPP/R5K1 b - - 3 19"),
                    new Tuple<string, string>("a8b8", "1qr3k1/1b3ppp/4pn2/2b5/1p6/1P2PN2/1BQ1BPPP/R5K1 w - - 4 20"),
                    new Tuple<string, string>("b2e5", "1qr3k1/1b3ppp/4pn2/2b1B3/1p6/1P2PN2/2Q1BPPP/R5K1 b - - 5 20"),
                    new Tuple<string, string>("c5d6", "1qr3k1/1b3ppp/3bpn2/4B3/1p6/1P2PN2/2Q1BPPP/R5K1 w - - 6 21"),
                    new Tuple<string, string>("c2b2", "1qr3k1/1b3ppp/3bpn2/4B3/1p6/1P2PN2/1Q2BPPP/R5K1 b - - 7 21"),
                    new Tuple<string, string>("d6e5", "1qr3k1/1b3ppp/4pn2/4b3/1p6/1P2PN2/1Q2BPPP/R5K1 w - - 0 22"),
                    new Tuple<string, string>("b2e5", "1qr3k1/1b3ppp/4pn2/4Q3/1p6/1P2PN2/4BPPP/R5K1 b - - 0 22"),
                    new Tuple<string, string>("b8e5", "2r3k1/1b3ppp/4pn2/4q3/1p6/1P2PN2/4BPPP/R5K1 w - - 0 23"),
                    new Tuple<string, string>("f3e5", "2r3k1/1b3ppp/4pn2/4N3/1p6/1P2P3/4BPPP/R5K1 b - - 0 23"),
                    new Tuple<string, string>("c8c3", "6k1/1b3ppp/4pn2/4N3/1p6/1Pr1P3/4BPPP/R5K1 w - - 1 24"),
                    new Tuple<string, string>("e2c4", "6k1/1b3ppp/4pn2/4N3/1pB5/1Pr1P3/5PPP/R5K1 b - - 2 24"),
                    new Tuple<string, string>("g7g5", "6k1/1b3p1p/4pn2/4N1p1/1pB5/1Pr1P3/5PPP/R5K1 w - - 0 25"),
                    new Tuple<string, string>("a1a7", "6k1/Rb3p1p/4pn2/4N1p1/1pB5/1Pr1P3/5PPP/6K1 b - - 1 25"),
                    new Tuple<string, string>("b7d5", "6k1/R4p1p/4pn2/3bN1p1/1pB5/1Pr1P3/5PPP/6K1 w - - 2 26"),
                    new Tuple<string, string>("f2f3", "6k1/R4p1p/4pn2/3bN1p1/1pB5/1Pr1PP2/6PP/6K1 b - - 0 26"),
                    new Tuple<string, string>("d5c4", "6k1/R4p1p/4pn2/4N1p1/1pb5/1Pr1PP2/6PP/6K1 w - - 0 27"),
                    new Tuple<string, string>("b3c4", "6k1/R4p1p/4pn2/4N1p1/1pP5/2r1PP2/6PP/6K1 b - - 0 27"),
                    new Tuple<string, string>("b4b3", "6k1/R4p1p/4pn2/4N1p1/2P5/1pr1PP2/6PP/6K1 w - - 0 28"),
                    new Tuple<string, string>("a7b7", "6k1/1R3p1p/4pn2/4N1p1/2P5/1pr1PP2/6PP/6K1 b - - 1 28"),
                    new Tuple<string, string>("c3c1", "6k1/1R3p1p/4pn2/4N1p1/2P5/1p2PP2/6PP/2r3K1 w - - 2 29"),
                    new Tuple<string, string>("g1f2", "6k1/1R3p1p/4pn2/4N1p1/2P5/1p2PP2/5KPP/2r5 b - - 3 29"),
                    new Tuple<string, string>("b3b2", "6k1/1R3p1p/4pn2/4N1p1/2P5/4PP2/1p3KPP/2r5 w - - 0 30"),
                    new Tuple<string, string>("c4c5", "6k1/1R3p1p/4pn2/2P1N1p1/8/4PP2/1p3KPP/2r5 b - - 0 30"),
                    new Tuple<string, string>("b2b1q", "6k1/1R3p1p/4pn2/2P1N1p1/8/4PP2/5KPP/1qr5 w - - 0 31"),
                    new Tuple<string, string>("b7b1", "6k1/5p1p/4pn2/2P1N1p1/8/4PP2/5KPP/1Rr5 b - - 0 31"),
                    new Tuple<string, string>("c1b1", "6k1/5p1p/4pn2/2P1N1p1/8/4PP2/5KPP/1r6 w - - 0 32"),
                    new Tuple<string, string>("e5d3", "6k1/5p1p/4pn2/2P3p1/8/3NPP2/5KPP/1r6 b - - 1 32"),
                    new Tuple<string, string>("b1d1", "6k1/5p1p/4pn2/2P3p1/8/3NPP2/5KPP/3r4 w - - 2 33"),
                    new Tuple<string, string>("f2e2", "6k1/5p1p/4pn2/2P3p1/8/3NPP2/4K1PP/3r4 b - - 3 33"),
                    new Tuple<string, string>("d1a1", "6k1/5p1p/4pn2/2P3p1/8/3NPP2/4K1PP/r7 w - - 4 34"),
                    new Tuple<string, string>("g2g4", "6k1/5p1p/4pn2/2P3p1/6P1/3NPP2/4K2P/r7 b - - 0 34"),
                    new Tuple<string, string>("a1a2", "6k1/5p1p/4pn2/2P3p1/6P1/3NPP2/r3K2P/8 w - - 1 35"),
                    new Tuple<string, string>("e2e1", "6k1/5p1p/4pn2/2P3p1/6P1/3NPP2/r6P/4K3 b - - 2 35"),
                    new Tuple<string, string>("a2h2", "6k1/5p1p/4pn2/2P3p1/6P1/3NPP2/7r/4K3 w - - 0 36"),
                    new Tuple<string, string>("d3f2", "6k1/5p1p/4pn2/2P3p1/6P1/4PP2/5N1r/4K3 b - - 1 36"),
                    new Tuple<string, string>("f6d5", "6k1/5p1p/4p3/2Pn2p1/6P1/4PP2/5N1r/4K3 w - - 2 37"),
                    new Tuple<string, string>("e3e4", "6k1/5p1p/4p3/2Pn2p1/4P1P1/5P2/5N1r/4K3 b - - 0 37"),
                    new Tuple<string, string>("d5e7", "6k1/4np1p/4p3/2P3p1/4P1P1/5P2/5N1r/4K3 w - - 1 38"),
                };

                string fen = initialFEN;
                foreach (Tuple<string, string> tuple in testData)
                {
                    string newFen = FenParser.ApplyMoveToFEN(fen, tuple.Item1);
                    Trace.WriteLine(String.Format("Verify \"{0}\" + \"{1}\" = \"{2}\" Ret: \"{3}\"", fen, tuple.Item1, tuple.Item2, newFen));
                    Assert.IsTrue(String.Compare(newFen, tuple.Item2) == 0);
                    fen = newFen;
                }

                // White knight in position to capture black kingside rook
                string initialFEN2 = "r1b1kb1r/pp1n1ppp/2p1pnN1/q2p4/2PP4/2N5/PP2PPPP/R1BQKB1R w KQkq - 4 7";
                Tuple<string, string>[] testData2 =
                {
                    new Tuple<string, string>("g6h8", "r1b1kb1N/pp1n1ppp/2p1pn2/q2p4/2PP4/2N5/PP2PPPP/R1BQKB1R b KQq - 0 7"),
                };

                fen = initialFEN2;
                foreach (Tuple<string, string> tuple in testData2)
                {
                    string newFen = FenParser.ApplyMoveToFEN(fen, tuple.Item1);
                    Trace.WriteLine(String.Format("Verify \"{0}\" + \"{1}\" = \"{2}\" Ret: \"{3}\"", fen, tuple.Item1, tuple.Item2, newFen));
                    Assert.IsTrue(String.Compare(newFen, tuple.Item2) == 0);
                    fen = newFen;
                }
            }
        }
    }
}
