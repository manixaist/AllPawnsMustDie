using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllPawnsMustDie;

namespace AllPawnsMustDieUnitTests
{
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
            CommandString = String.Empty;
            ExpectedResponse = String.Empty;
        }

        #pragma warning disable CS0067
        /// <summary>Required for interface but not used in mock</summary>
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;
        /// <summary>Required for interface but not used in mock</summary>
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
            // Save last pair
            CommandString = commandString;
            ExpectedResponse = expectedResponse;

            // Add to total list
            commandResponsePairs.Add(new Tuple<string, string>(commandString, expectedResponse));
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

        /// <summary>List of command/responses received</summary>
        public List<Tuple<string, string>> commandResponsePairs = new List<Tuple<string, string>>();
    }
}
