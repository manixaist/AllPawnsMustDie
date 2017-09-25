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
    /// Holds unit tests related to the MoveInformation struct
    /// </summary>
    [TestClass]
    public class MoveInformationUnitTests
    {
        /// <summary>
        /// MoveInformation.ToString() tests
        /// </summary>
        [TestClass]
        public class ToStringMethodTests
        {
            /// <summary>
            /// Basic verification tests
            /// </summary>
            [TestMethod]
            public void BasicVerificationTests()
            {
                // Tuple tops out at 7 items so we're pushing it
                // <startSquare, endSquare, Deployed, Color, Promotion Class, ExpectedToString>
                Tuple<CB.BoardSquare, CB.BoardSquare, bool, PieceColor, PieceClass, string>[] testData =
                {
                    // PieceClass.EnPassantTarget is invalid, so use when not promoting
                    new Tuple<CB.BoardSquare, CB.BoardSquare, bool, PieceColor, PieceClass, string>
                        (new CB.BoardSquare(new PieceFile(5), 2), new CB.BoardSquare(new PieceFile(5), 4),
                        false, PieceColor.White, PieceClass.EnPassantTarget, "e2e4"),
                    new Tuple<CB.BoardSquare, CB.BoardSquare, bool, PieceColor, PieceClass, string>
                        (new CB.BoardSquare(new PieceFile(4), 4), new CB.BoardSquare(new PieceFile(4), 5),
                        false, PieceColor.Black, PieceClass.EnPassantTarget, "d4d5"),
                    new Tuple<CB.BoardSquare, CB.BoardSquare, bool, PieceColor, PieceClass, string>
                        (new CB.BoardSquare(new PieceFile(5), 7), new CB.BoardSquare(new PieceFile(5), 8),
                        true, PieceColor.White, PieceClass.Queen, "e7e8q"),
                };

                // Verify each entry in the testData array
                foreach (Tuple<CB.BoardSquare, CB.BoardSquare, bool, PieceColor, PieceClass, string> tuple in testData)
                {
                    Trace.WriteLine(String.Format("Constucting MoveInformation[{0}:{1}->{2}:{3}] Color: {4} Deployed: {5} Promotion: {6}",
                        tuple.Item1.File.ToString(), tuple.Item1.Rank, tuple.Item2.File.ToString(), tuple.Item2.Rank,
                        tuple.Item4.ToString(), tuple.Item3.ToString(), tuple.Item5.ToString()));
                    CB.MoveInformation testMove = new CB.MoveInformation(tuple.Item1, tuple.Item2, tuple.Item3, ChessBoard.InitialFENPosition);
                    testMove.Color = tuple.Item4;
                    if (tuple.Item5 != PieceClass.EnPassantTarget)
                    {
                        testMove.PromotionJob = tuple.Item5;
                    }

                    Trace.WriteLine(String.Format("Verifying ToString() == \"{0}\"", tuple.Item6));
                    Assert.AreEqual(0, String.Compare(testMove.ToString(), tuple.Item6));
                }
            }
        }

        /// <summary>
        /// Holds test related to the public Properties (accessors)
        /// </summary>
        [TestClass]
        public class PropertiesTests
        {
            /// <summary>
            /// Basic tests for the public properties
            /// </summary>
            [TestMethod]
            public void BasicPropertiesTests()
            {
                CB.BoardSquare startSquare = new CB.BoardSquare(new PieceFile('b'), 6);
                CB.BoardSquare endSquare = new CB.BoardSquare(new PieceFile('e'), 3);
                bool previouslyDeployed = true;

                Trace.WriteLine(String.Format("Constucting default MoveInformation[{0}:{1}->{2}:{3}] Deployed: {4}",
                        startSquare.File.ToString(), startSquare.Rank, endSquare.File.ToString(), endSquare.Rank, previouslyDeployed));
                CB.MoveInformation testMove = new CB.MoveInformation(startSquare, endSquare, previouslyDeployed, ChessBoard.InitialFENPosition);

                // First verify the basic properties set on creation
                Trace.WriteLine(String.Format("Verifying start and end squares..."));
                Assert.AreEqual(testMove.Start, startSquare);
                Trace.WriteLine(String.Format("Verifying FirstMove..."));
                Assert.IsFalse(testMove.FirstMove);

                // Now set some extraneous properties
                Trace.WriteLine(String.Format("Verifying Color set/get..."));
                PieceColor testColor = PieceColor.Black;
                testMove.Color = testColor;
                Assert.AreEqual(testMove.Color, testColor);

                // CastlingRights
                Trace.WriteLine(String.Format("Verifying CastlingRights set/get..."));
                BoardSide testCastlingRights = BoardSide.Queen;
                testMove.CastlingRights = testCastlingRights;
                Assert.AreEqual(testMove.CastlingRights, testCastlingRights);

                // PromotionClass
                Trace.WriteLine(String.Format("Verifying PromotionJob set/get..."));
                PieceClass testPromotionClass = PieceClass.Knight;
                Assert.IsFalse(testMove.IsPromotion);
                testMove.PromotionJob = testPromotionClass;
                Assert.AreEqual(testMove.PromotionJob, testPromotionClass);
                Assert.IsTrue(testMove.IsPromotion);

                // CapturedPiece and CastlingRook are mutually exclusive Properties
                // neither can be checked without first being set or an exception is
                // thrown. Also, if one is set and you check the other, an exception
                // is thrown
                // IsCapture and IsCastle can be used to check if either is set
                // without the exception
                Trace.WriteLine(String.Format("Verifying IsCapture and IsCastle initially false..."));
                Assert.IsFalse(testMove.IsCapture);
                Assert.IsFalse(testMove.IsCastle);

                Trace.WriteLine(String.Format("Set a capture piece and verify IsCapture and CapturedPiece..."));
                ChessPiece testPiece = new ChessPiece(PieceColor.White, PieceClass.Pawn, new PieceFile('c'), 2);
                testMove.CapturedPiece = testPiece;
                Assert.IsTrue(testMove.IsCapture);
                Assert.AreEqual(testMove.CapturedPiece, testPiece);

                try
                {
                    Trace.WriteLine(String.Format("Attempting to get the CastlingRook (invalid)..."));
                    ChessPiece rook = testMove.CastlingRook;
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine(String.Format("Caught InvalidOperationException..."));
                }

                // Recreate the test move to clear the captured piece
                testMove = new CB.MoveInformation(startSquare, endSquare, previouslyDeployed, ChessBoard.InitialFENPosition);

                Trace.WriteLine(String.Format("Set a castling rook and verify IsCastle and CsatlingRook..."));
                ChessPiece testRook = new ChessPiece(PieceColor.White, PieceClass.Rook, new PieceFile('a'), 1);
                testMove.CastlingRook = testRook;
                Assert.IsTrue(testMove.IsCastle);
                Assert.AreEqual(testMove.CastlingRook, testRook);

                try
                {
                    Trace.WriteLine(String.Format("Attempting to get the CapturedPiece (invalid)..."));
                    ChessPiece capture = testMove.CapturedPiece;
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine(String.Format("Caught InvalidOperationException..."));
                }

                // Setting a non-Rook to a castling rook will throw
                testMove = new CB.MoveInformation(startSquare, endSquare, previouslyDeployed, ChessBoard.InitialFENPosition);

                try
                {
                    testMove.CastlingRook = testPiece;
                }
                catch (ArgumentException)
                {
                    Trace.WriteLine(String.Format("Caught ArgumentException..."));
                }
            }
        }
    }
}
