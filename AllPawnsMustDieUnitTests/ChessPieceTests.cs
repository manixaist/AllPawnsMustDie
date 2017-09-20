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
    /// Holds unit tests related to the ChessPiece class
    /// </summary>
    [TestClass]
    public class ChessPieceTests
    {
        /// <summary>
        /// Holds tests for static IsOnBackRankMethod
        /// </summary>
        [TestClass]
        public class IsOnBackRankMethod
        {
            /// <summary>
            /// Verifies that pieces on a back rank return true when checked
            /// </summary>
            [TestMethod]
            public void ReturnsTrueForValidBackRanks()
            {
                // Test Data: Piece {Color, Class, File}
                Tuple<PieceColor, PieceClass, char>[] testData =
                {
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.Pawn,   'a'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.Bishop, 'b'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.King,   'c'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.Rook,   'd'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.Queen,  'e'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.White, PieceClass.King,   'f'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.Pawn,   'h'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.Bishop, 'e'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.King,   'f'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.Rook,   'e'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.Queen,  'c'),
                    new Tuple<PieceColor, PieceClass, char>(PieceColor.Black, PieceClass.King,   'b'),
                };

                foreach (Tuple<PieceColor, PieceClass, char> tuple in testData)
                {
                    PieceFile file = new PieceFile(tuple.Item3);
                    // The rank should be valid in these tests always
                    // White (8), Black (1) are valid, the other values should not matter
                    int rank = ((tuple.Item1 == PieceColor.White) ? 8 : 1);
                    ChessPiece testPiece = new ChessPiece(tuple.Item1, tuple.Item2, file, rank);
                    Trace.WriteLine(String.Format("Verifying IsOnBackRank() [Rank:{0}, Color: {1}, Class: {2}]",
                        rank, testPiece.Color, testPiece.Job));
                    Assert.IsTrue(ChessPiece.IsOnBackRank(testPiece));
                }
            }

            /// <summary>
            /// Verifies that pieces NOT on a back rank return false when checked
            /// </summary>
            [TestMethod]
            public void ReturnsFalseForInValidBackRanks()
            {
                // Test Data: Piece {Color, Class, File, Rank}
                // Rank should be set to invalid range [2-7] or 1 if White 8 if Black
                Tuple<PieceColor, PieceClass, char, int>[] testData =
                {
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Pawn,   'a', 1),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Bishop, 'b', 2),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.King,   'c', 3),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Rook,   'd', 4),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Queen,  'e', 5),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.King,   'f', 6),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Pawn,   'h', 7),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Bishop, 'e', 8),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.King,   'f', 2),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Rook,   'e', 3),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Queen,  'c', 4),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.King,   'b', 5),
                };

                foreach (Tuple<PieceColor, PieceClass, char, int> tuple in testData)
                {
                    PieceFile file = new PieceFile(tuple.Item3);
                    ChessPiece testPiece = new ChessPiece(tuple.Item1, tuple.Item2, file, tuple.Item4);
                    Trace.WriteLine(String.Format("Verifying !IsOnBackRank() [Rank:{0}, Color: {1}, Class: {2}]",
                        testPiece.Rank, testPiece.Color, testPiece.Job));
                    Assert.IsFalse(ChessPiece.IsOnBackRank(testPiece));
                }
            }
        }

        /// <summary>
        /// Holds tests for the public properties on the ChessPiece class
        /// </summary>
        [TestClass]
        public class PublicProperties
        {
            /// <summary>
            /// Basic tests for the read only properties
            /// </summary>
            [TestMethod]
            public void ReadOnlyPropertiesTests()
            {
                // Test Data: Piece {Color, Class, File, Rank}
                Tuple<PieceColor, PieceClass, char, int>[] testData =
                {
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Pawn,   'f', 8),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Bishop, 'g', 3),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.King,   'c', 1),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Rook,   'd', 5),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.Queen,  'e', 5),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.White, PieceClass.King,   'h', 7),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Pawn,   'a', 6),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Bishop, 'e', 1),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.King,   'c', 8),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Rook,   'd', 2),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.Queen,  'c', 4),
                    new Tuple<PieceColor, PieceClass, char, int>(PieceColor.Black, PieceClass.King,   'e', 3),
                };

                foreach (Tuple<PieceColor, PieceClass, char, int> tuple in testData)
                {
                    PieceFile file = new PieceFile(tuple.Item3);
                    ChessPiece testPiece = new ChessPiece(tuple.Item1, tuple.Item2, file, tuple.Item4);

                    Trace.WriteLine(String.Format("Verifying read-only properties [Color: {0}, Class: {1}, File: {2}, Rank: {3}]",
                        testPiece.Color, testPiece.Job, testPiece.File.ToInt(), testPiece.Rank));

                    // Verify the Read-Only properties
                    Assert.AreEqual(testPiece.Color, tuple.Item1);
                    Assert.AreEqual(testPiece.Job, tuple.Item2);
                    Assert.AreEqual(testPiece.File.ToInt(), new PieceFile(tuple.Item3).ToInt());
                    Assert.AreEqual(testPiece.Rank, tuple.Item4);
                }
            }

            /// <summary>
            /// Basic tests for the read only properties
            /// </summary>
            [TestMethod]
            public void ReadWritePropertiesTests()
            {
                // Deployed, Visible, Highlight
                ChessPiece testPiece = new ChessPiece(PieceColor.White, PieceClass.Pawn, new PieceFile('d'), 1);

                // $TODO - this will need updating when the FEN work sets this positionally (not on home square at creation)
                // For now this is the expected behavior
                Trace.WriteLine("Verifying Deployed is false after creation");
                Assert.IsFalse(testPiece.Deployed);

                Trace.WriteLine("Verifying Visible is true after creation");
                Assert.IsTrue(testPiece.Visible);

                Trace.WriteLine("Verifying Highlight is false after creation");
                Assert.IsFalse(testPiece.Highlight);

                // Now change them
                testPiece.Deployed = true;
                testPiece.Visible = false;
                testPiece.Highlight = true;

                // And re-verify
                Trace.WriteLine("Verifying Deployed is true after update");
                Assert.IsTrue(testPiece.Deployed);

                Trace.WriteLine("Verifying Visible is false after update");
                Assert.IsFalse(testPiece.Visible);

                Trace.WriteLine("Verifying Highlight is true after update");
                Assert.IsTrue(testPiece.Highlight);
            }

            /// <summary>
            /// ChessPiece.Move() tests
            /// </summary>
            [TestMethod]
            public void MoveMethodTests()
            {
                // At the piece level, movement has no side-effects (unlike the board)
                // So this is a simple check if the internal are updated, by
                // checking the public accessors
                ChessPiece testPiece = new ChessPiece(PieceColor.Black, PieceClass.Knight, new PieceFile('f'), 6);

                // Note this move is illegal in chess, it doesn't matter here
                Trace.WriteLine("Moving the piece...");
                testPiece.Move(new PieceFile('a'), 1);

                Trace.WriteLine("Verifying piece location after Move()");
                Assert.AreEqual(testPiece.Rank, 1);
                Assert.AreEqual(testPiece.File.ToInt(), 1); // 'a' is 1

                Trace.WriteLine("Verifying Deployed == true");
                Assert.IsTrue(testPiece.Deployed);
            }

            /// <summary>
            /// ChessPiece.PromoteOnNextMove() tests
            /// </summary>
            [TestMethod]
            public void PromoteOnNextMoveMethodTests()
            {
                // This method is used for promotion, and alters calls to Move()
                // to also update the class.  It does not matter if the move is
                // a legal promotion or not at this layer, the ChessBoard handles that
                ChessPiece testPiece = new ChessPiece(PieceColor.Black, PieceClass.Pawn, new PieceFile('c'), 2);
                Trace.WriteLine("Promoting class to Rook...");
                testPiece.PromoteOnNextMove(PieceClass.Rook);
                testPiece.Move(testPiece.File, 1);

                Trace.WriteLine("Verifying piece location after Move()");
                Assert.AreEqual(testPiece.Rank, 1);
                Assert.AreEqual(testPiece.File.ToInt(), 3); // 'c' is 3

                Trace.WriteLine("Verifying Deployed == true");
                Assert.IsTrue(testPiece.Deployed);

                Trace.WriteLine("Verifying Class was updated to Rook");
                Assert.AreEqual(testPiece.Job, PieceClass.Rook);
            }

            /// <summary>
            /// ChessPiece.TempMove()/Reset() tests
            /// </summary>
            [TestMethod]
            public void TempMoveAndResetMethodTests()
            {
                // TempMove() and its pair ResetTempMove() update only the rank
                // and file of the piece, it ignores promotion and deployment
                // it is used for testing locations on a board without updating
                // other data associated with the piece or board
                ChessPiece testPiece = new ChessPiece(PieceColor.White, PieceClass.Bishop, new PieceFile('e'), 6);
                Trace.WriteLine("Promoting class to Queen (should be ignored on temp move)...");
                testPiece.PromoteOnNextMove(PieceClass.Queen);
                testPiece.TempMove(new PieceFile('h'), 7);

                Trace.WriteLine("Verifying piece location after TempMove()");
                Assert.AreEqual(testPiece.Rank, 7);
                Assert.AreEqual(testPiece.File.ToInt(), 8); // 'h' is 8

                Trace.WriteLine("Verifying Deployed == false");
                Assert.IsFalse(testPiece.Deployed);

                Trace.WriteLine("Verifying Class was not updated to Rook");
                Assert.AreEqual(testPiece.Job, PieceClass.Bishop);

                Trace.WriteLine("Reverting with ResetTempMove()...");
                testPiece.ResetTempMove();

                Trace.WriteLine("Verifying piece location after ResetTempMove()");
                Assert.AreEqual(testPiece.Rank, 6);
                Assert.AreEqual(testPiece.File.ToInt(), 5); // 'e' is 5

                Trace.WriteLine("Verifying Deployed == false");
                Assert.IsFalse(testPiece.Deployed);

                Trace.WriteLine("Verifying Class was not updated to Rook");
                Assert.AreEqual(testPiece.Job, PieceClass.Bishop);
            }

            /// <summary>
            /// ChessPiece.Demote() tests
            /// </summary>
            [TestMethod]
            public void DemoteMethodTests()
            {
                // Demote always sets the class to Pawn (it's the only piece that
                // can be promoted in the first place)
                ChessPiece testPiece = new ChessPiece(PieceColor.White, PieceClass.Queen, new PieceFile('e'), 6);

                Trace.WriteLine("Demoting Queen to Pawn...");
                testPiece.Demote();
                Trace.WriteLine("Verifying new job is peasant...");
                Assert.AreEqual(PieceClass.Pawn, testPiece.Job);

                // Try to demote a King (this is invalid)
                testPiece = new ChessPiece(PieceColor.Black, PieceClass.King, new PieceFile('d'), 3);

                try
                {
                    Trace.WriteLine("Attempting to demote the King...vive la révolution!");
                    testPiece.Demote();
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine("Caught the InvalidOperationException exception, long live the King!");
                }
            }
        }
    }
}
