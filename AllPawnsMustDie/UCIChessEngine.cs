using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Encapsulates a UCI protocol chess engine
    /// </summary>
    public sealed class UCIChessEngine : IChessEngine, IDisposable
    {
        /// <summary>
        /// Create a new UCI engine object
        /// </summary>
        public UCIChessEngine()
        {
            bestMove = "";
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~UCIChessEngine()
        {
            Dispose();
        }

        private AutoResetEvent readyToSend = new AutoResetEvent(true);

        /// <summary>
        /// EventHandler (delegate(s)) that will get the response event.  For now
        /// this is only used internally to the class
        /// </summary>
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                readyToSend.Dispose();
                engineProcess.OutputDataReceived -= OnDataReceived;
                engineProcess.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        
        /// <summary>
        /// Event handler for process stdout.  This is where we parse responses
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">The string sent to stdout</param>
        public void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            // This level gets all responses from the engine, thinking lines, etc
            Debug.WriteLine(String.Concat("<=Engine: ", e.Data));

            // compare e.Data to the expected string
            if (e.Data.StartsWith(expected))
            {
                commandResponse = e.Data;
                Debug.WriteLine(String.Concat("<=ExResp: ", commandResponse));

                // If we're asking for a move - then save the response we care about
                // the SAN for the move - it comes right after "bestmove"
                // If no move (e.g. mate) will return 'bestmove (none)'
                if (e.Data.StartsWith("bestmove"))
                {
                    string[] parts = e.Data.Split(' ');
                    bestMove = parts[1];
                }

                // raise interface event here if there is a handler
                // This layer is only going to get the expected response at the
                // end of the command (e.g. "bestmove" or "uciready"
                if (OnChessEngineResponseReceived != null)
                {
                    OnChessEngineResponseReceived(this, new ChessEngineResponseReceivedEventArgs(commandResponse));
                }

                // Signal event that we're done processing this command
                readyToSend.Set();
            }
        }

        /// <summary>
        /// Loads the chess engine process
        /// </summary>
        /// <param name="fullPathToExe">full path to chess engine</param>
        void IChessEngine.LoadEngine(string fullPathToExe)
        {
            // Startup the process, we should only do this once
            if (processInited)
            {
                throw new InvalidOperationException();
            }

            fullPathToEngine = fullPathToExe;

            // Set process and startup variables
            // and launch process
            engineProcess = new Process();
            engineProcess.EnableRaisingEvents = true;
            engineProcess.StartInfo.CreateNoWindow = true;
            engineProcess.StartInfo.RedirectStandardOutput = true;
            engineProcess.StartInfo.RedirectStandardInput = true;
            engineProcess.StartInfo.RedirectStandardError = true;
            engineProcess.StartInfo.UseShellExecute = false;
            engineProcess.StartInfo.FileName = fullPathToEngine;
            
            if (!engineProcess.Start())
            {
                // Bad path? For now just throw
                throw new ArgumentException();
            }

            // Subscribe to data arriving on the output stream
            engineProcess.OutputDataReceived += OnDataReceived;
            // Start async read of that output stream.
            engineProcess.BeginOutputReadLine();

            // Now we're inited
            processInited = true;
        }

        /// <summary>
        /// Send a command to the chess engine
        /// </summary>
        /// <param name="commandString">command to send</param>
        /// <param name="expectedResponse">response we expect to get.  If this is
        /// and empty string, then sync up after the command with the engine</param>
        void IChessEngine.SendCommand(string commandString, string expectedResponse)
        {
            // Spin up a thread to send command to exe since this is called on the UI thread
            Thread thread = new Thread(() => CommandExecutionThreadProc(engineProcess.StandardInput, commandString, expectedResponse));
            thread.Start();
            // return ASAP - no waiting or blocking here
        }

        /// <summary>
        /// Quit the engine
        /// </summary>
        void IChessEngine.Quit()
        {
            // Write directly to the stream.There is no response to this and the
            // engine will exit ASAP, so assume it's gone if this worked.
            if (engineProcess != null)
            {
                Debug.WriteLine(String.Concat("=>Engine: ", "quit"));
                // Unsubscribe from the handler as this will close the process
                engineProcess.OutputDataReceived -= OnDataReceived;
                engineProcess.StandardInput.WriteLine("quit");
            }
        }

        /// <summary>
        /// Thread method that sends the command to the external exe via writing
        /// to it's stdin stream
        /// </summary>
        /// <param name="sw">StreamWriter for engines stdin</param>
        public void CommandExecutionThreadProc(StreamWriter sw, string commandString, string expectedString)
        {
            readyToSend.WaitOne(); // Wait on signal (intially signalled)

            syncAfterCommand = (expectedString.Length == 0);
            command = commandString;
            expected = expectedString;

            Debug.WriteLine(String.Concat("=>Engine: ", command));

            sw.WriteLine(command);

            // if SyncAfterCommand == true, then also send IsReady and set 
            // expected to ReadyOk
            if (syncAfterCommand)
            {
                //command = IsReady;
                expected = ReadyOk;
                sw.WriteLine(IsReady);
            }

            // Done, exit thread - wait is elsewhere
        }

        private string commandResponse;

        /// <summary>
        /// Last command response
        /// </summary>
        public string CommandResponse { get { return commandResponse; } }

        private string bestMove;

        /// <summary>
        /// Last "best move" returned by the enine.  Used to get moves for the CPU
        /// </summary>
        public string BestMove { get { return bestMove; } }

        private bool syncAfterCommand = false;
        private string command;
        private string expected;
        private Process engineProcess;
        private bool processInited = false;
        private string fullPathToEngine;

        public static string IsReady = "isready";
        public static string ReadyOk = "readyok";
        public static string Uci = "uci";
        public static string UciOk = "uciok";
        public static string UciNewGame = "ucinewgame";
    }
}
