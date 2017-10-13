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
            /// Returns the need for a sync after the command (no response)
            /// </summary>
            public bool NeedsIsReadySync { get { return (expected == String.Empty); } }

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
        public UCIChessEngine(IChessEngineProcessLoader engineLoader)
        {
            engineProcessLoader = engineLoader;
            initialFEN = null;
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
                engineProcess.OnDataReceived -= this.OnDataReceived;
                // Send the UCI quit if we haven't
                ((IChessEngine)this).Quit();

                // Dispose of the process
                ((IDisposable)engineProcess).Dispose();
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
            SendVerboseEvent(data); // let the verbose handler get the empty lines

            if (data == String.Empty)
            {
                return; // don't bother, nothing to do
            }

            // Spike 1.1
            if (String.Compare(data, "Error: Fatal no best move") == 0)
            {
                // Convert to something approaching a real protocol response
                data = "bestmove (none)";
                SendVerboseEvent(String.Format("Fixed formatting to \"{0}\"", data)); 
            }
            // MadChess
            else if (String.Compare(data, "bestmove Null") == 0)
            {
                data = "bestmove (none)";
                SendVerboseEvent(String.Format("Fixed formatting to \"{0}\"", data));
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
            
            if (foundCommand)
            {
                // First check if we need to wait on a ReadyOK response, this happens
                // when there is no expected response when queued
                if (cep.NeedsIsReadySync)
                {
                    if (String.Compare(data, ReadyOk) == 0)
                    {
                        dequeue = true;         // This dequeues the original command
                        commandResponse = data; // nothing was actually queued for the "IsReady" write
                    }
                }
                else if (data.StartsWith(cep.Expected))
                {
                    dequeue = true;         // Mark command for dequeue and
                    commandResponse = data; // save the response

                    // If we're asking for a move - then save the response we care about
                    // the SAN for the move - it comes right after "bestmove"
                    // If no move (e.g. mate) will return 'bestmove (none)'
                    if (data.StartsWith(BestMoveResponse))
                    {
                        string[] parts = data.Split(' ');
                        bestMove = parts[1];
                    }
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
        /// Loads the chess engine process.  It's fine to call this directly,
        /// but there is a command for it 'UciLoadEngineCommand' for consistency
        /// with the other commands.  That command ultimately invokes this method
        /// </summary>
        /// <param name="fullPathToExe">full path to chess engine</param>
        void IChessEngine.LoadEngineProcess(string fullPathToExe)
        {
            // Startup the process, we should only do this once per class instance
            if (processInited)
            {
                throw new InvalidOperationException();
            }

            // The IChessEngineProcessLoader implementation will do the actual load
            // but we need to cache the path to the exe for it (and so we have it)
            fullPathToEngine = fullPathToExe;

            // Create the worker thread the first time, at the start of this thread
            // execution is the actual loading...finally but on a non-UI thread.
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
            UciPositionCommand command = new UciPositionCommand(fen);
            command.Execute(this);
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
            // The loader and path are injected, so the UCIChessEngine object doesn't
            // need to care how the interface was created/obtained/loaded from disk
            engineProcess = engineProcessLoader.Start(fullPathToEngine, OnDataReceived);

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

                    if ((String.Compare(cep.Command, dummyCommand) != 0) && (engineProcess.InputStream != null))
                    {
                        // Found something - write it to the process, 

                        Debug.WriteLine(String.Concat("=>Engine: ", cep.Command));
                        engineProcess.InputStream.WriteLine(cep.Command);

                        // If this is true, the above command has no response, and we'd wait here forever
                        if (cep.NeedsIsReadySync)
                        {
                            StreamWriter writer = engineProcess.InputStream;
                            UciIsReadyCommand isReadyCommand = new UciIsReadyCommand(ref writer);
                            isReadyCommand.Execute(this);
                        }

                        // Wait on the exit event as well, so we can bail as fast as needed
                        AutoResetEvent[] commandEvents = { uciCommandFinished, shutdownCommandQueue };
                        int commandWait = WaitHandle.WaitAny(commandEvents);
                        if (commandWait == 1)
                        {
                            exitThread = true;
                        }
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
            if (engineProcess.InputStream != null && !(Disposed))
            {
                engineProcess.InputStream.WriteLine(UciQuit);
            }
        }

        /// <summary>
        /// Start the UCI session with the engine.  This is required before all
        /// other commands will function
        /// </summary>
        public void InitializeUciProtocol()
        {
            UciInitCommand command = new UciInitCommand();
            command.Execute(this);
        }

        /// <summary>
        /// Calls the underlying Quit implementation
        /// </summary>
        public void QuitEngine()
        {
            ((IChessEngine)this).Quit();
        }

        /// <summary>
        /// Sets a new position with the chess engine
        /// </summary>
        /// <param name="positionAsFEN">FEN for the new position</param>
        public void SetPosition(string positionAsFEN)
        {
            UciPositionCommand command = new UciPositionCommand(positionAsFEN);
            command.Execute(this);
        }

        /// <summary>
        /// Wraps the internal reset state.  This will either reset to a standard
        /// starting position, or the initial position provided
        /// </summary>
        public void ResetBoard()
        {
            ((IChessEngine)this).Reset();
        }

        /// <summary>
        /// Start analyzing the current position for the best move
        /// </summary>
        /// <param name="timeInMilliseconds">Time in milliseconds to think at
        /// a maximum as a string, e.g. "2000" is 2 seconds</param>
        public void GetMoveForCurrentPosition(string timeInMilliseconds)
        {
            UciGoCommand command = new UciGoCommand(timeInMilliseconds);
            command.Execute(this);
        }

        /// <summary>
        /// Sets a given option name with a value (if provided)
        /// </summary>
        /// <param name="optionName">engine dependent option name</param>
        /// <param name="optionValue">option dependent value</param>
        public void SetOption(string optionName, string optionValue = null)
        {
            UciSetOptionCommand command = new UciSetOptionCommand(optionName, optionValue);
            command.Execute(this);
        }

        /// <summary>
        /// Loads the engine process
        /// </summary>
        /// <param name="pathToEngine">Path to the engine process</param>
        public void LoadEngine(string pathToEngine)
        {
            UciLoadEngineCommand command = new UciLoadEngineCommand(pathToEngine);
            command.Execute(this);
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
        private IChessEngineProcess engineProcess;
        private IChessEngineProcessLoader engineProcessLoader;
        private Queue<CommandExecutionParameters> commandQueue;
        private AutoResetEvent newCommandAddedToQueue;
        private AutoResetEvent shutdownCommandQueue;
        private AutoResetEvent uciCommandFinished;
        private object queue_lock;
        private Thread queueThread;
        #endregion
    }
}
