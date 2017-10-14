using System;
using System.IO;
using System.Diagnostics;
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
    /// Holds tests for the UCIChessEngine object
    /// </summary>
    [TestClass]
    public class UCIChessEngineUnitTests
    {
        /// <summary>
        /// Holds functional tests for the UCIChessEngine object
        /// </summary>
        [TestClass]
        public class FunctionalTests
        {
            /// <summary>
            /// The purpose of this test is to exercise the basic framework for the
            /// IChessEngine.  We should be able to pass our own implementations
            /// of both the loader and the process proving they have been decoupled
            /// from the UCIChessEngine object.
            /// 
            /// We can then issue commands via the public methods on the engine, 
            /// and we should get the responses from the engine via its event
            /// </summary>
            [TestMethod]
            public void BasicFunctionalTest()
            {
                // This will create a MockEngineProcess when invoked by the UCIChessEngine
                Trace.WriteLine("Creating Mock Loader...");
                MockEngineProcessLoader mockLoader = new MockEngineProcessLoader();
                
                // prevents the test from exiting until all events are accounted for
                testDone = new AutoResetEvent(false);

                // Create a real engine object using the mock loader (and mock process by extension)
                Trace.WriteLine("Creating Real Engine passing Mock Loader...");
                UCIChessEngine engine = new UCIChessEngine(mockLoader);
                engine.OnChessEngineResponseReceived += ChessEngineResponseReceivedEventHandler;
                engine.OnChessEngineVerboseOutputReceived += ChessEngineVerboseResponseReceivedEventHandler;

                // This will force the loader to get invoked, it's required to init the engine
                // but the path is ingored by the mock loader, so pass anything
                Trace.WriteLine("Telling the engine to load...");
                engine.LoadEngine("x:\\foo\\bar\\myeng.exe");

                // At this point, the UCIChessEngine should think it's good to go.
                // We can issue commands as normal here, and the mock process will
                // respond to UCIChessEngine in a way to finish the cycle (e.g.
                // if it gets 'isready' it will respond with 'readyok'.  The actual
                // moves don't matter at all, just the response.  There are no
                // chess game rules applied here (we're below ChessGame)

                // Responses go to ChessEngineResponseReceivedEventHandler below
                Trace.WriteLine("Starting UCI protocol...");
                engine.InitializeUciProtocol();
                Trace.WriteLine("Setting option...");
                engine.SetOption("foo", "bar");
                Trace.WriteLine("Setting option...");
                engine.SetOption("foo1", "bar1");
                Trace.WriteLine("Resetting board...");
                engine.ResetBoard();
                Trace.WriteLine("Getting move...");
                engine.GetMoveForCurrentPosition("1000");
                Trace.WriteLine("Setting position...");
                engine.SetPosition("foobar");
                Trace.WriteLine("Getting move...");
                engine.GetMoveForCurrentPosition("1000");
                Trace.WriteLine("Setting position...");
                engine.SetPosition("foobar");
                Trace.WriteLine("Getting move...");
                engine.GetMoveForCurrentPosition("1000");
                Trace.WriteLine("Setting position...");
                engine.SetPosition("foobar");
                Trace.WriteLine("Getting move...");
                engine.GetMoveForCurrentPosition("1000");
                Trace.WriteLine("Setting position...");
                engine.SetPosition("foobar");
                Trace.WriteLine("Waiting for last response...");
                Assert.IsTrue(testDone.WaitOne(30000));
                // There are no extraneous responses in the mock (like info)
                // so these should match in count.
                Assert.IsTrue(verboseCount == commandCount);
            }

            /// <summary>
            /// Handler for the UCIChessEngine command responses
            /// </summary>
            /// <param name="sender">ignored</param>
            /// <param name="e">ignored</param>
            private void ChessEngineResponseReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
            {
                Trace.WriteLine(String.Format("Engine RESP: {0}", e.Response));
                if (++commandCount >= 12) // Needs to match the number of commands sent
                {
                    Trace.WriteLine("Last response...setting exit event");
                    testDone.Set();
                }
            }

            /// <summary>
            /// Handler for the UCIChessEngine verbose responses
            /// </summary>
            /// <param name="sender">ignored</param>
            /// <param name="e">ignored</param>
            private void ChessEngineVerboseResponseReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
            {
                ++verboseCount;
            }

            private int verboseCount = 0;
            private int commandCount = 0;
            private AutoResetEvent testDone;
        }
    }
}
