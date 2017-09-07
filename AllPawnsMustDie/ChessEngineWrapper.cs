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

            engine.SendCommand(UCIChessEngine.IsReady, UCIChessEngine.ReadyOk);
            engine.SendCommand(UCIChessEngine.Uci, UCIChessEngine.UciOk);
        }

        ~ChessEngineWrapper()
        {
            Dispose();
        }

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
            engine.SendCommand(UCIChessEngine.UciNewGame, "");
        }

        /// <summary>
        /// Set a new position with the engine
        /// </summary>
        /// <param name="fenString"></param>
        public void NewPosition(string fenString)
        {
            engine.SendCommand(String.Concat("position fen ", fenString), "");
        }

        /// <summary>
        /// Sets a new position based on a list of moves.  This is the history
        /// of the game, plus the latest move (e.g. the string gets longer as you
        /// play)
        /// </summary>
        /// <param name="san"></param>
        public void SendMove(string san)
        {
            engine.SendCommand(String.Concat("position startpos moves ", san), "");
        }

        /// <summary>
        /// Gets the best move from the engine.  This should only be called
        /// after the engine has responded (which will also have the move) so this
        /// is really a cache of that last best move
        /// </summary>
        /// <param name="bestMove"></param>
        public void GetBestMove(out string bestMove)
        {
            bestMove = ((UCIChessEngine)engine).BestMove;
        }

        /// <summary>
        /// Quit the chess engine
        /// </summary>
        public void Quit()
        {
            engine.Quit();
        }

        private IChessEngine engine;
        public IChessEngine Engine
        {
            get { return engine; }
        }
    }
}
