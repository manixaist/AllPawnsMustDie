using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Wrapper for the chess engine.  This is here to allow for the expansion of
    /// other protocols, but none are planned at the moment. It's provides far
    /// more logic than just the chese engine though (which really at its heart
    /// just evaluates a given position)
    /// </summary>
    class ChessEngineWrapper : IDisposable
    {
        #region Public Methods
        /// <summary>
        /// Create a new engine wrapper object
        /// </summary>
        /// <param name="fullPathToEngine">Path to the chess engine exe.  E.g. stockfish.exe</param>
        public ChessEngineWrapper(string fullPathToEngine)
        {
            // Create new UCI engine object
            UCIChessEngine uciEngine = new UCIChessEngine();
            engine = (IChessEngine)uciEngine;

            // This will launch the process
            engine.LoadEngine(fullPathToEngine);
            engine.SendCommandAsync(UCIChessEngine.Uci, UCIChessEngine.UciOk);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ChessEngineWrapper()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of underlying engine
        /// </summary>
        public void Dispose()
        {
            // Don't have to track this being called since the target will
            // and it's basically a pass through
            ((UCIChessEngine)engine).Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new game
        /// </summary>
        public void NewGame()
        {
            engine.Reset();
        }

        /// <summary>
        /// Set a new position with the engine
        /// </summary>
        /// <param name="fenString">FEN for the new position</param>
        public void NewPosition(string fenString)
        {
            engine.SendCommandAsync(String.Concat("position fen ", fenString), "");
        }

        /// <summary>
        /// Sets a new position based on a list of moves.  This is the history
        /// of the game, plus the latest move (e.g. the string gets longer as you
        /// play)
        /// </summary>
        /// <param name="san">Send a move in standard algebraic notation</param>
        public void SendMove(string san)
        {
            engine.SendCommandAsync(String.Concat("position startpos moves ", san), "");
        }

        /// <summary>
        /// Quit the chess engine
        /// </summary>
        public void Quit()
        {
            engine.Quit();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the best move from the engine.  This should only be called
        /// after the engine has responded (which will also have the move) so this
        /// is really a cache of that last best move
        /// </summary>
        public string BestMove
        {
            get { return ((UCIChessEngine)engine).BestMove; }
        }

        /// <summary>
        /// Returns the current underlying IChessEngine interface
        /// </summary>
        public IChessEngine Engine
        {
            get { return engine; }
        }
        #endregion

        #region Private Fields
        private IChessEngine engine;
        #endregion
    }
}
