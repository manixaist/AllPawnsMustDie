using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// EventArgs for the engine response (external process response)
    /// </summary>
    public class ChessEngineResponseReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Basically just wraps a string.  Pass that string in here on creation
        /// </summary>
        /// <param name="data"></param>
        public ChessEngineResponseReceivedEventArgs(string data)
        {
            response = data;
        }

        private readonly string response;

        /// <summary>
        /// Get the response string
        /// </summary>
        public string Response
        {
            get { return response; }
        }
    }

    /// <summary>
    /// Generic interface for the chess engine (external process).  At a bare
    /// minumum, we need to be able to send a command and wait for a response
    /// </summary>
    interface IChessEngine
    {
        /// <summary>
        /// Event fired when the engine has finished with a given command
        /// </summary>
        event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;

        /// <summary>
        /// Loads the chess engine process
        /// </summary>
        /// <param name="fullPathToExe">full path to chess engine</param>
        void LoadEngine(string fullPathToExe);

        /// <summary>
        /// Send a command to the chess engine
        /// </summary>
        /// <param name="commandString">command to send</param>
        /// <param name="expectedResponse">response we expect to get.  If this is
        /// and empty string, then sync up after the command with the engine</param>
        void SendCommand(string commandString, string expectedResponse);

        /// <summary>
        /// Quit the engine
        /// </summary>
        void Quit();
    }
}
