using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /*  
     *  This file implements a Command pattern for the UCIChessEngine object.
     *  
     *  This allows the invoker/caller (ChessGame in most cases) to not care 
     *  about any underyling implementating.  That knowledge is held in the 
     *  concrete versions of the abstract command defined below.
     *  
     *  ChessGame is also the client in this case, the containing object
     *  for ChessGame is the UI form and it has no direct contact with the
     *  commands (just the public methods, which might invoke a command or not)
     *  
     *   (Client)         (Invoker)          (base command - abstract)
     *  +-----------+   +-----------+        +-----------------------+
     *  | ChessGame |   | ChessGame |        | UciChessEngineCommand |
     *  +-----------+   +-----------+<>----->+-----------------------+ 
     *  |           |   |           |        |                       |
     *  +-----------+   +-----------+        +-----------------------+
     *     |                                 | +Execute()            |
     *   | |                                 +-----------------------+
           |
     *   | |      (Receiver)                 (concreate command object)
     *     |   +--------------------+        +-----------------------+
     *   | +-->| IChessEngine       |        | UciInitCommand (e.g.) |
     *         +--------------------+        +-----------------------+
     *   |     | +SendCommandAsync()|<-------| -state (IChessEngine) |
     *         +--------------------+        +-----------------------+
     *   |                                   | +Execute()            |
     *                                       +-----------------------+
     *   |                                        ^
     *                                            |
     *   +- - - - - - - - - - - - - - - - - - - - +
     *  
     */

    /// <summary>
    /// Base command object for the UciEngine, all commands derive from this
    /// </summary>
    public abstract class UciChessEngineCommand
    {
        /// <summary>
        /// Derived classes put their concrete implemenations of each command here
        /// </summary>
        /// <param name="engine">engine object</param>
        public abstract void Execute(IChessEngine engine);
    }

    /// <summary>
    /// This command will initialize the UCI protocol.  It should be issued
    /// once at the start of the process initialization before other commands
    /// are sent (with the exception of UciLoadEngineCommand which loads the
    /// actual process)
    /// </summary>
    public class UciInitCommand : UciChessEngineCommand
    {
        /// <summary>UciInitCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.SendCommandAsync(UCIChessEngine.Uci, UCIChessEngine.UciOk);
        }
    }

    /// <summary>
    /// Wraps a "new game" with the engine and resets the board to its initial
    /// state
    /// </summary>
    public class UciNewGameCommand : UciChessEngineCommand
    {
        /// <summary>UciNewGameCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.SendCommandAsync(UCIChessEngine.UciNewGame, String.Empty);
        }
    }

    /// <summary>
    /// Tells the engine to set a new position with the board based on a FEN
    /// </summary>
    public class UciPositionCommand : UciChessEngineCommand
    {
        /// <summary>
        /// Initialize the UciPositionCommand object with the FEN
        /// </summary>
        /// <param name="positionAsFEN">FEN to set with engine</param>
        public UciPositionCommand(string positionAsFEN)
        {
            fen = String.Format("position fen {0}", positionAsFEN);
        }

        /// <summary>UciPositionCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.SendCommandAsync(fen, String.Empty);
        }

        private readonly string fen;
    }

    /// <summary>
    /// Start analyzing the current position for the best move.  Only take
    /// moveTimeInMilliseconds to make up your mind.
    /// </summary>
    public class UciGoCommand : UciChessEngineCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveTimeInMilliseconds"></param>
        public UciGoCommand(string moveTimeInMilliseconds)
        {
            moveCommand = String.Format("go movetime {0}", moveTimeInMilliseconds);
        }

        /// <summary>UciGoCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.SendCommandAsync(moveCommand, UCIChessEngine.BestMoveResponse);
        }

        private readonly string moveCommand;
    }

    /// <summary>
    /// This command is really meant for internal use by the engine and should not
    /// be called by others.  The caller needs the underlying process input 
    /// stream to use the command, and only the engine has it.
    /// </summary>
    public class UciIsReadyCommand : UciChessEngineCommand
    {

        /// <summary>
        /// This command is special and done in line with another async
        /// command, so it writes directly to the process stream
        /// </summary>
        /// <param name="engineInputStream">stream to write to</param>
        public UciIsReadyCommand(ref StreamWriter engineInputStream)
        {
            stream = engineInputStream;
        }

        /// <summary>UciIsReadyCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            stream.WriteLine(UCIChessEngine.IsReady);
        }

        private readonly StreamWriter stream;
    }

    /// <summary>
    /// Wraps a 'setoption' UCI command with required name and optional value
    /// field(s)
    /// </summary>
    public class UciSetOptionCommand : UciChessEngineCommand
    {
        /// <summary>
        /// Initialize the UciSetOptionCommand with the option name and optional
        /// option value
        /// </summary>
        /// <param name="optionName">Name of the option, e.g. UCI_Elo</param>
        /// <param name="optionValue">Associated value if needed, e.g. "1300" or "true"</param>
        public UciSetOptionCommand(string optionName, string optionValue = null)
        {
            string optionFormat = "setoption name {0}";
            if (optionValue != null)
            {
                // Having a value is common, but not required
                optionFormat = String.Concat(optionFormat, " value {1}");
            }
            optionCommand = String.Format(optionFormat, (optionValue == null) ? optionName : optionName, optionValue);
        }

        /// <summary>UciSetOptionCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.SendCommandAsync(optionCommand, String.Empty);
        }

        private readonly string optionCommand;
    }

    /// <summary>
    /// Not a UCI protocol command, but the client issued command to load the 
    /// process that implements the UCI protocol for use
    /// </summary>
    public class UciLoadEngineCommand : UciChessEngineCommand
    {
        /// <summary>
        /// Initialize the UciLoadEngineCommand with the path to the engine exe
        /// </summary>
        /// <param name="pathToExe">full path to engine exe (includes exe)</param>
        public UciLoadEngineCommand(string pathToExe)
        {
            fullPathToExe = pathToExe;
        }

        /// <summary>UciLoadEngineCommand implementation</summary>
        /// <param name="engine">engine object</param>
        public override void Execute(IChessEngine engine)
        {
            engine.LoadEngineProcess(fullPathToExe);
        }

        private readonly string fullPathToExe;
    }
}
