using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
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
    /// Holds tests for the ChessGame object
    /// </summary>
    [TestClass]
    public class ChessGameUnitTests
    {
        /// <summary>
        /// Holds tests for the ChessGame CTOR
        /// </summary>
        [TestClass]
        public class ChessGameConstructorTests
        {
            /// <summary>
            /// Basic creation and init test.
            /// </summary>
            [TestMethod]
            public void BasicInitTest()
            {
                MockEngineProcessLoader mockLoader = new MockEngineProcessLoader();
                MockChessBoardView mockView = new MockChessBoardView();
                string mockPath = @"x:\Foo\Bar\MyEngine.exe";

                // More or less base tests the mocks, just make sure this doesn't
                // crash/compiles
                Trace.WriteLine("Creating ChessGame object with mocks injected");
                ChessGame chessGame = new ChessGame(
                    mockView,   // View
                    mockPath,   // Path
                    mockLoader, // Loader
                    false,      // Reduce ELO
                    Thread.CurrentThread.CurrentUICulture);

                chessGame.NewGame(PieceColor.White, 5000);
            }
        }
    }
}
