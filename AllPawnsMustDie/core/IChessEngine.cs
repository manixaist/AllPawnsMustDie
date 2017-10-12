using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    #region EventArgs
    /// <summary>
    /// EventArgs for the engine response (external process response)
    /// </summary>
    public class ChessEngineResponseReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Basically just wraps a string.  Pass that string in here on creation
        /// </summary>
        /// <param name="data">engine response</param>
        public ChessEngineResponseReceivedEventArgs(string data)
        {
            response = data;
        }

        /// <summary>
        /// Get the response string
        /// </summary>
        public string Response {get { return response; } }

        private readonly string response;
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Generic interface for the chess engine (external process).  At a bare
    /// minumum, we need to be able to send a command and wait for a response
    /// </summary>
    public interface IChessEngine
    {
        /// <summary>
        /// Event fired when the engine has finished with a given command
        /// </summary>
        event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;

        /// <summary>
        /// Event fired when the engine has receieved any output
        /// </summary>
        event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineVerboseOutputReceived;

        /// <summary>
        /// Loads the chess engine process
        /// </summary>
        /// <param name="fullPathToExe">full path to chess engine</param>
        void LoadEngineProcess(string fullPathToExe);

        /// <summary>
        /// Resets the engine, only valid after a succesful load
        /// </summary>
        void Reset();

        /// <summary>
        /// Send a command to the chess engine, does not block, fires events above
        /// </summary>
        /// <param name="commandString">command to send</param>
        /// <param name="expectedResponse">response we expect to get.  If this is
        /// and empty string, then sync up after the command with the engine</param>
        void SendCommandAsync(string commandString, string expectedResponse);

        /// <summary>
        /// Quit the engine
        /// </summary>
        void Quit();
    }
    #endregion
}
