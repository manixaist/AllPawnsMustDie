using System;
using System.IO;
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
    /// Holds tests for the various UciChessEngineCommand objects
    /// </summary>
    [TestClass]
    public class UciChessEngineCommandTests
    {
        /// <summary>
        /// Holds tests for the UciNewGameCommand object
        /// </summary>
        [TestClass]
        public class UciNewGameCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'new game' command
                UciNewGameCommand command = new UciNewGameCommand();
                // Execute against the mock
                command.Execute(engine);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciNewGameCommand sends correct command to the engine..."));
                Assert.AreEqual(engine.CommandString, UCIChessEngine.UciNewGame);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);
            }
        }

        /// <summary>
        /// Holds tests for the UciInitCommand object
        /// </summary>
        [TestClass]
        public class UciInitCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'init uci' command
                UciInitCommand command = new UciInitCommand();
                // Execute against the mock
                command.Execute(engine);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciInitCommand sends correct command to the engine..."));
                Assert.AreEqual(engine.CommandString, UCIChessEngine.Uci);
                Assert.AreEqual(engine.ExpectedResponse, UCIChessEngine.UciOk);
            }
        }

        /// <summary>
        /// Holds tests for the UciPositionCommand object
        /// </summary>
        [TestClass]
        public class UciPositionCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'position' command
                UciPositionCommand command = new UciPositionCommand(ChessBoard.InitialFENPosition);
                // Execute against the mock
                command.Execute(engine);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciPositionCommand sends correct command to the engine..."));
                string expected = String.Format("position fen {0}", ChessBoard.InitialFENPosition);
                Assert.AreEqual(engine.CommandString, expected);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);
            }
        }

        /// <summary>
        /// Holds tests for the UciGoCommand object
        /// </summary>
        [TestClass]
        public class UciGoCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                string moveTime = "543210";
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'UciGoCommand' command
                UciGoCommand command = new UciGoCommand(moveTime);
                // Execute against the mock
                command.Execute(engine);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciGoCommand sends correct command to the engine..."));
                string expected = String.Format("go movetime {0}", moveTime);
                Assert.AreEqual(engine.CommandString, expected);
                Assert.AreEqual(engine.ExpectedResponse, UCIChessEngine.BestMoveResponse);
            }
        }

        /// <summary>
        /// Holds tests for the UciIsReadyCommand object
        /// </summary>
        [TestClass]
        public class UciIsReadyCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Mock stream in memory
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);

                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'isready' command
                UciIsReadyCommand command = new UciIsReadyCommand(ref writer);
                // Execute against the mock
                command.Execute(engine);
                // Flush the writer so the data shows up in the underlying stream
                writer.Flush();

                // Now get that string back from the stream
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                string streamString = reader.ReadLine();

                // Verify commands sent - in this case the command is written
                // directly to the stream - so verify it gets there
                // The engine does not record the expected in this case so it
                // should remain empty in the mock
                Trace.WriteLine(String.Format("Verifying UciIsReadyCommand sends correct command to the engine..."));
                Assert.AreEqual(streamString, UCIChessEngine.IsReady);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);
            }
        }

        /// <summary>
        /// Holds tests for the UciSetOptionCommand object
        /// </summary>
        [TestClass]
        public class UciSetOptionCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();
                // Create the 'set option' command
                string mockOptionName = "OptionName";
                string mockOptionValue = "OptionValue";

                UciSetOptionCommand command = new UciSetOptionCommand(mockOptionName, mockOptionValue);
                // Execute against the mock
                command.Execute(engine);

                // The command should either be 
                // A) setoption name {name} value {value}
                // B) setoption name {name}

                string expected = String.Format("setoption name {0} value {1}", mockOptionName, mockOptionValue);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciSetOptionCommand sends correct command to the engine..."));
                Assert.AreEqual(engine.CommandString, expected);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);

                command = new UciSetOptionCommand(mockOptionName);
                // Execute against the mock
                command.Execute(engine);
                expected = String.Format("setoption name {0}", mockOptionName);
                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciSetOptionCommand sends correct command to the engine..."));
                Assert.AreEqual(engine.CommandString, expected);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);
            }
        }

        /// <summary>
        /// Holds tests for the UciLoadEngineCommand object
        /// </summary>
        [TestClass]
        public class UciLoadEngineCommandTests
        {
            /// <summary>
            /// Execute the command and verify it send the correct data to the engine
            /// </summary>
            [TestMethod]
            public void ExecuteTest()
            {
                // Create the mock engine
                MockChessEngine engine = new MockChessEngine();

                string mockPath = @"x:\foo\bar\mymockengine.exe";
                UciLoadEngineCommand command = new UciLoadEngineCommand(mockPath);
                // Execute against the mock
                command.Execute(engine);

                // Verify commands sent
                Trace.WriteLine(String.Format("Verifying UciLoadEngineCommand sends correct command to the engine..."));
                // IChessEngine.LoadEngineProces() records this in the mock, and
                // we're testing that layer correctly gets the path passed in here
                Assert.AreEqual(engine.ExePath, mockPath);
                Assert.AreEqual(engine.ExpectedResponse, String.Empty);
            }
        }
    }

    /// <summary>
    /// Test implementation to record the last commands send and the expected response
    /// </summary>
    internal class MockChessEngine : IChessEngine
    {
        /// <summary>
        /// Initialize the mock object (clear the strings)
        /// </summary>
        public MockChessEngine()
        {
            CommandString = "";
            ExpectedResponse = "";
        }

        // Nothing is done with these, but they are required by the interface
        // Disable the warning that they are never used or assigned to since
        // it is 100% intentional
        #pragma warning disable CS0067
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineVerboseOutputReceived;
        #pragma warning restore CS0067

        /// <summary>
        /// Record the path
        /// </summary>
        /// <param name="fullPathToExe">path send to us from the command object</param>
        void IChessEngine.LoadEngineProcess(string fullPathToExe)
        {
            ExePath = fullPathToExe;
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        void IChessEngine.Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Record the command sent, and the expected resonse byu whatever
        /// command object invoked it
        /// </summary>
        /// <param name="commandString">command to send to UCI</param>
        /// <param name="expectedResponse">response if any expected</param>
        void IChessEngine.SendCommandAsync(string commandString, string expectedResponse)
        {
            CommandString = commandString;
            ExpectedResponse = expectedResponse;
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        void IChessEngine.Quit()
        {
            throw new NotImplementedException();
        }

        /// <summary>Command last sent to mock</summary>
        public string CommandString;
        
        /// <summary>Expected response for last command sent to mock</summary>
        public string ExpectedResponse;

        /// <summary>Path sent to LoadEngine</summary>
        public string ExePath;
    }
}