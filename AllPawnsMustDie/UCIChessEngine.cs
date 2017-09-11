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
        #region Public Events
        /// <summary>
        /// EventHandler (delegate(s)) that will get the response event
        /// this is only used internally to the class
        /// </summary>
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineResponseReceived;

        /// <summary>
        /// EventHandler (delegate(s)) that will get the verbose event
        /// </summary>
        public event EventHandler<ChessEngineResponseReceivedEventArgs> OnChessEngineVerboseOutputReceived;
        #endregion

        #region Private Structs
        /// <summary>
        /// Wraps an engine command and related information for the worker thread to unpack
        /// </summary>
        private struct CommandExecutionParameters
        {
            /// <summary>
            /// Create a new CommandExecutionParameters object for the worker threaad
            /// </summary>
            /// <param name="sw">StreamWriter for the engine's stdin</param>
            /// <param name="commandString">command to send</param>
            /// <param name="expectedString">expected response or String.Empty if none</param>
            public CommandExecutionParameters(StreamWriter sw, string commandString, string expectedString)
            {
                command = commandString;
                expected = expectedString;
                streamWriter = sw;
            }

            /// <summary>
            /// Gets the StreamWriter for stdin
            /// </summary>
            public StreamWriter StreamWriter { get { return streamWriter; } }

            /// <summary>
            /// Returns the command for execution
            /// </summary>
            public string Command { get { return command; } }

            /// <summary>
            /// Returns the response we expect from the command or String.Empty
            /// </summary>
            public string Expected { get { return expected; } }

            private string command;
            private string expected;
            private StreamWriter streamWriter;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new UCI engine object
        /// </summary>
        public UCIChessEngine()
        {
            bestMove = String.Empty;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~UCIChessEngine()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of disposable objects
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                // Stop listening to stdout of engine process
                engineProcess.OutputDataReceived -= OnDataReceived;
                // Dispose of the process
                engineProcess.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Event handler for process stdout.  This is where we parse responses from the chess engine
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">The string sent to stdout</param>
        public void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            // This level gets all responses from the engine, thinking lines, options, debug, etc
            // However if we get a null event, filter it out since there is nothing we can do
            if (e.Data == null)
            {
                return;
            }
            
            SendVerboseEvent(e.Data); 

            // compare e.Data to the expected string stored in the class
            // Note this requires serialized execution - which is achieved by
            // ThreadPool.QueueUserWorkItem and the logic calling it
            //
            // Really the expected (and whatever else) might go into a queue
            // instead and we can pop the front one off here.  This would allow
            // multiple command/response pairs to be running/queued in order but
            // it's just not needed here yet, but this is intentional and not 
            // an oversight
            if (e.Data.StartsWith(Expected))
            {
                commandResponse = e.Data;

                // If we're asking for a move - then save the response we care about
                // the SAN for the move - it comes right after "bestmove"
                // If no move (e.g. mate) will return 'bestmove (none)'
                if (e.Data.StartsWith(BestMoveResponse))
                {
                    string[] parts = e.Data.Split(' ');
                    bestMove = parts[1];
                }

                SendReceivedEvent(CommandResponse);
            }
        }

        /// <summary>
        /// Invokes subscribers to OnChessEngineVerboseOutputReceived
        /// </summary>
        /// <param name="s">String to send</param>
        public void SendVerboseEvent(string s)
        {
            if (OnChessEngineVerboseOutputReceived != null)
            {
                OnChessEngineVerboseOutputReceived(this, new ChessEngineResponseReceivedEventArgs(s));
            }
        }

        /// <summary>
        /// Invokes subscribers to OnChessEngineResponseReceived
        /// </summary>
        /// <param name="s">String to send</param>
        public void SendReceivedEvent(string s)
        {
            if (OnChessEngineResponseReceived != null)
            {
                OnChessEngineResponseReceived(this, new ChessEngineResponseReceivedEventArgs(s));
            }
        }
        
        /// <summary>
        /// Loads the chess engine process
        /// </summary>
        /// <param name="fullPathToExe">full path to chess engine</param>
        void IChessEngine.LoadEngine(string fullPathToExe)
        {
            // Startup the process, we should only do this once per class instance
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

            // Subscribe to data arriving on the output stream
            engineProcess.OutputDataReceived += OnDataReceived;

            // Start the process up
            if (!engineProcess.Start())
            {
                // Bad path? Invalid exe file? For now just throw
                throw new ArgumentException();
            }

            // Start async read of that output stream.
            engineProcess.BeginOutputReadLine();

            // Now we're inited
            processInited = true;
        }

        /// <summary>
        /// Reset state with the engine
        /// </summary>
        void IChessEngine.Reset()
        {
            // Reset internals
            syncAfterCommand = false;
            command = String.Empty;
            expected = String.Empty;

            // Set a default position with the engine
            ((IChessEngine)this).SendCommandAsync(String.Concat("position fen ", ChessBoard.InitialFENPosition), String.Empty);
        }

        /// <summary>
        /// Send a command to the chess engine
        /// </summary>
        /// <param name="commandString">command to send</param>
        /// <param name="expectedResponse">response we expect to get.  If this is
        /// an empty string, then sync up after the command with the engine</param>
        void IChessEngine.SendCommandAsync(string commandString, string expectedResponse)
        {
            // Queue a work item to run in the background.  This is serialized work,
            // simple to use, and all we need for the background thread for now
            // It's in fact overkill since the work is currently serialized by the
            // calling logic - but that will likely change
            // It, in even more fact, is actually quite broken if we tried to 
            // send more than one of these at a time right now, but that is also
            // commented in the OnDataRecieved callback
            ThreadPool.QueueUserWorkItem(new WaitCallback(CommandCallback),
                    new CommandExecutionParameters(engineProcess.StandardInput, commandString, expectedResponse));
        }

        /// <summary>
        /// Quit the engine
        /// </summary>
        void IChessEngine.Quit()
        {
            // Write directly to the stream. There is no response to this and the
            // engine will exit ASAP, so assume it's gone after this
            if (engineProcess != null && !(Disposed))
            {
                // Unsubscribe from the handler as this will close the process
                //engineProcess.OutputDataReceived -= OnDataReceived;
                engineProcess.StandardInput.WriteLine(UciQuit);
                Dispose();
            }
        }

        /// <summary>
        /// Thread execution Method
        /// </summary>
        /// <param name="state">CommandExecutionParameters object</param>
        public void CommandCallback(object state)
        {
            CommandExecutionParameters cep = (CommandExecutionParameters)state;
            // Save to class fields
            expected = cep.Expected;
            command = cep.Command;
            syncAfterCommand = (expected.Length == 0);
            
            Debug.WriteLine(String.Concat("=>Engine: ", command));
            cep.StreamWriter.WriteLine(command);

            if (syncAfterCommand)
            {
                // Add a sync work item - update the expected - the reason we need
                // this is for commands that have no response, so we're not overwriting
                // any expected here that is meaningful
                // Because of the serialization above, this means we're launching 2 threads
                // (still one per command) behind each other and our callback is going to 
                // fire only once (since there is no output and hence no event for the 1st 
                // command (the whole reason we're sending the 2nd command)expected = ReadyOk;
                ThreadPool.QueueUserWorkItem(new WaitCallback(CommandCallback), 
                    new CommandExecutionParameters(cep.StreamWriter, IsReady, ReadyOk));
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the object has already been disposed
        /// </summary>
        public bool Disposed { get { return disposed; } }

        /// <summary>
        /// Last command response
        /// </summary>
        public string CommandResponse { get { return commandResponse; } }

        /// <summary>
        /// Last "best move" returned by the enine.  Used to get moves for the CPU
        /// </summary>
        public string BestMove { get { return bestMove; } }

        /// <summary>
        /// The expected response for the last command sent (only one that is expecting
        /// a response is sent at a time at the moment.
        /// </summary>
        public string Expected { get { return expected; } }
        #endregion

        #region Public Fields
        public static string BestMoveResponse = "bestmove";
        public static string IsReady = "isready";
        public static string ReadyOk = "readyok";
        public static string Uci = "uci";
        public static string UciOk = "uciok";
        public static string UciNewGame = "position startpos moves";
        public static string UciQuit = "quit";
        #endregion

        #region Private Fields
        private bool disposed = false;
        private bool processInited = false;
        private bool syncAfterCommand = false;
        private string command;
        private string commandResponse;
        private string expected;
        private string bestMove;
        private string fullPathToEngine;
        private Process engineProcess;
        #endregion
    }
}
