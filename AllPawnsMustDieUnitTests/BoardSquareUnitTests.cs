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
    /// Holds unit tests for the BoardSquare struct
    /// </summary>
    [TestClass]
    public class BoardSquareUnitTests
    {
        /// <summary>
        /// Checks a BoardSquare validity versus an expected validity
        /// </summary>
        /// <param name="file">file in int form [1-8] is valid</param>
        /// <param name="rank">rank [1-8] is valid</param>
        /// <param name="expected">true if we expect the file, rank to be valid
        /// false otherwise</param>
        private delegate void BoardSquareChecker(int file, int rank, bool expected);

        [TestClass]
        public class IsValidMethodTests
        {
            [TestMethod]
            public void BasicTests()
            {
                BoardSquareChecker callback = (int file, int rank, bool expected) =>
                {
                    Trace.WriteLine(String.Format("Verifying BoardSquare[{0}:{1}] is {2}", file, rank, expected));
                    Assert.IsTrue(BoardSquare.IsValid(file, rank) == expected);
                };

                // ChessBoard.BoardSquare.IsValid is a static helper that takes
                // ints rather than PieceFiles as those objects must be valid.  
                // IsValid helps with offsets to locations that may be invalid
                // (and thus you can't construct a PieceFile)
                // There are only 64 valid squares, check them all
                for (int fileIndex = 1; fileIndex <= 8; fileIndex++)
                {
                    for (int rankIndex = 1; rankIndex <= 8; rankIndex++)
                    {
                        callback(fileIndex, rankIndex, true);
                    }
                }

                // Now try some invalid ones
                callback(0, 0, false);
                callback(-1, -20, false);
                callback(-1, 8, false);
                callback(1, 9, false);
                callback(9, 8, false);
            }
        }

        /// <summary>
        /// Holds test for the public properties
        /// </summary>
        [TestClass]
        public class PropertiesTests
        {
            /// <summary>
            /// Perform some basic tests on the public properties
            /// </summary>
            [TestMethod]
            public void BasicTests()
            {
                // ChessBoard.BoardSquare encapsulates a File:Rank pair
                // Most of the unit level testing is verifying correctness of Properties
                // and since Equality tests are overloaded, testing those work
                // as expected
                PieceFile testFile = new PieceFile('c');
                int testRank = 4;

                Trace.WriteLine(String.Format("Creating BoardSquare[{0}:{1}]", testFile.ToString(), testRank));
                BoardSquare testSquare = new BoardSquare(testFile, testRank);

                //testSquare.File
                Assert.AreEqual(testSquare.File, testFile);
                Assert.IsTrue(testSquare.File == testFile);

                // Various equality checks
                Assert.AreEqual(testSquare.Rank, testRank);
                Assert.IsTrue(testSquare.Rank == testRank);
                Assert.IsTrue(testFile.Equals(testSquare.File));
            }
        }
    }
}
