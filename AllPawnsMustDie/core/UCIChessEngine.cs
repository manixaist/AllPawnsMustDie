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
            /// <param name="commandString">command to send</param>
            /// <param name="expectedString">expected response or String. Empty if none</param>
            public CommandExecutionParameters(string commandString, string expectedString)
            {
                command = commandString;
                expected = expectedString;
            }

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
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new UCI engine object
        /// </summary>
        public UCIChessEngine()
        {
            initialFEN = null;
            needIsReadySync = false;
            queueThread = null;
            bestMove = String.Empty;
            queue_lock = new object();
            uciCommandFinished = new AutoResetEvent(false);
            newCommandAddedToQueue = new AutoResetEvent(false);
            shutdownCommandQueue = new AutoResetEvent(false);
            commandQueue = new Queue<CommandExecutionParameters>();
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

                // Ask thread to close
                shutdownCommandQueue.Set();
                if (!queueThread.Join(500))
                {
                    // Tell thread to close
                    queueThread.Abort();
                }
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

            string data = e.Data;

            // Spike 1.1
            if (String.Compare(data, "Error: Fatal no best move") == 0)
            {
                // Convert to something approaching a real protocol response
                data = "bestmove (none)";
            }

            // Set to an empty one since it's a value type...but we only care
            // if we actually find one.
            string dummyCommand = "{OnDataReceived}";
            CommandExecutionParameters cep = new CommandExecutionParameters("", "");

            // Don't need a lock here, just need to know if it's > 0, and only this
            // method Dequeues
            if (commandQueue.Count() > 0)
            {
                cep = commandQueue.Peek();
            }
            bool foundCommand = (String.Compare(dummyCommand, cep.Command) != 0);
            bool dequeue = false;
            SendVerboseEvent(data); 

            // First check if we need to wait on a ReadyOK response
            if (foundCommand && (String.Compare(data, ReadyOk) == 0) && (needIsReadySync))
            {
                dequeue = true;
                commandResponse = data;
                needIsReadySync = false; 
            }
            else if (foundCommand && data.StartsWith(cep.Expected))
            {
                dequeue = true;
                commandResponse = data;
                
                // If we're asking for a move - then save the response we care about
                // the SAN for the move - it comes right after "bestmove"
                // If no move (e.g. mate) will return 'bestmove (none)'
                if (data.StartsWith(BestMoveResponse))
                {
                    string[] parts = data.Split(' ');
                    bestMove = parts[1];
                }
            }

            // Remove item if processed and notify listeners
            if (dequeue) 
            {
                lock (queue_lock)
                {
                    commandQueue.Dequeue();
                }

                SendReceivedEvent(CommandResponse);
                uciCommandFinished.Set();
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

            // Create the worker thread the first time
            if (queueThread == null)
            {
                queueThread = new Thread(() => CommandQueueExecutionMethod());
                queueThread.Start();
            }
        }

        /// <summary>
        /// Reset state with the engine
        /// </summary>
        void IChessEngine.Reset()
        {
            string fen = (initialFEN != null) ? initialFEN : ChessBoard.InitialFENPosition;
            // Set a default position with the engine
            ((IChessEngine)this).SendCommandAsync(UCIChessEngine.UciNewGame, String.Empty);
            ((IChessEngine)this).SendCommandAsync(String.Format("position fen {0}", fen), String.Empty);
        }

        /// <summary>
        /// Set an initial position with the engine
        /// </summary>
        /// <param name="fen">fen to store</param>
        void IChessEngine.SetInitialPosition(string fen)
        {
            initialFEN = fen;
            ((IChessEngine)this).Reset();
        }

        /// <summary>
        /// Send a command to the chess engine.  Commands are serialized on another thread
        /// </summary>
        /// <param name="commandString">command to send</param>
        /// <param name="expectedResponse">response we expect to get.  If this is
        /// an empty string, then sync up after the command with the engine</param>
        void IChessEngine.SendCommandAsync(string commandString, string expectedResponse)
        {
            // Under lock - update the queue of commands to execute and signal
            // our running worker thread (or create it the first time)
            lock (queue_lock)
            {
                CommandExecutionParameters cep = 
                    new CommandExecutionParameters(commandString, expectedResponse);
                Debug.WriteLine(String.Format("Enqueueing command pair (\"{0}\", \"{1}\")", commandString, expectedResponse));
                commandQueue.Enqueue(cep);
                // Signal the worker thread
                newCommandAddedToQueue.Set();
            }
        }

        /// <summary>
        /// Worker Thread Method responsible for processing commands to the chess engine.
        /// When first launched, the thread will attempt to launch the engine process, 
        /// then enter a wait state until work is sent (added to the queue).
        /// 
        /// When signalled for work, the thread will take the next queueded item (if any)
        /// and send the command to the engine, and wait for the response triggered in 
        /// the stdout callback OnDataReceieved.  This will repeat so long as items
        /// are in the queue.
        /// 
        /// If a command has no response (e.g. 'position' uci command) a pair of
        /// "IsReady"/"ReadyOk" command/response is sent to sync up.
        /// 
        /// This thread exists for the lifetime of the owning ChessGame object
        /// </summary>
        public void CommandQueueExecutionMethod()
        {
            // Load the process
            LoadEngineProcess();

            AutoResetEvent[] workerEvents = { newCommandAddedToQueue, shutdownCommandQueue };
            int waitResult = WaitHandle.WaitTimeout;
            bool exitThread = false;
            bool moreWork = false;

            while (!exitThread)
            {
                lock (queue_lock)
                {
                    moreWork = (commandQueue.Count() > 0);
                }

                if (!moreWork)
                {
                    waitResult = WaitHandle.WaitAny(workerEvents);
                }
                else
                {
                    waitResult = 0; // Process more items
                }

                if (waitResult == 1) // shutdownCommandQueue
                {
                    exitThread = true;
                }
                else
                {
                    // Set to an empty one since it's a value type...but we only care
                    // if we actually find one.
                    string dummyCommand = "{CommandQueueExecutionMethod}";
                    CommandExecutionParameters cep = new CommandExecutionParameters(dummyCommand, "");
                    
                    // There should be new work to do
                    lock (queue_lock)
                    {
                        if (commandQueue.Count() > 0)
                        {
                            // Do not remove the item yet - wait until it's processed
                            cep = commandQueue.Peek();
                        }
                    }

                    if ((String.Compare(cep.Command, dummyCommand) != 0) && (engineProcess.StandardInput != null))
                    {
                        // Found something - write it to the process, but first check if we're
                        // going to get a response to wait on
                        needIsReadySync = (String.Compare(cep.Expected, String.Empty) == 0);

                        Debug.WriteLine(String.Concat("=>Engine: ", cep.Command));
                        engineProcess.StandardInput.WriteLine(cep.Command);

                        // If this is true, the above command has no response, and we'd wait here forever
                        if (needIsReadySync)
                        {
                            // So instead, send it an "IsReady" and wait on the reply
                            engineProcess.StandardInput.WriteLine(IsReady);
                        }

                        // Wait for the response
                        uciCommandFinished.WaitOne();
                    }
                }
            }
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
        #endregion

        #region Private Methods
        private void LoadEngineProcess()
        {
            if (!processInited)
            {
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
        #endregion

        #region Public Fields
        /// <summary>UCI "bestmove" command</summary>
        public static string BestMoveResponse = "bestmove";
        
        /// <summary>UCI "isready" response</summary>
        public static string IsReady = "isready";
        
        /// <summary>UCI "readyok" command</summary>
        public static string ReadyOk = "readyok";
        
        /// <summary>UCI "uci" command</summary>
        public static string Uci = "uci";
        
        /// <summary>UCI "uciok" response</summary>
        public static string UciOk = "uciok";
        
        /// <summary>UCI "new game" command</summary>
        public static string UciNewGame = "ucinewgame";

        /// <summary>UCI "quit" command</summary>
        public static string UciQuit = "quit";
        #endregion

        #region Private Fields
        private bool disposed = false;
        private bool processInited = false;
        private string commandResponse;
        private string bestMove;
        private string fullPathToEngine;
        private string initialFEN;
        private Process engineProcess;
        private Queue<CommandExecutionParameters> commandQueue;
        private AutoResetEvent newCommandAddedToQueue;
        private AutoResetEvent shutdownCommandQueue;
        private AutoResetEvent uciCommandFinished;
        private object queue_lock;
        private Thread queueThread;
        private bool needIsReadySync;
        #endregion
    }
}
