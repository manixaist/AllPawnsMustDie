using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
    /// <summary>
    /// ChessGame encapsulates a game "session".  It can either start from scratch
    /// or from a given starting position.  It is responsible for tracking all 
    /// aspects of the game.  A non-exhasutive list: The board (owns the pieces,
    /// and moves, and more), time controls, all UI components like a move history
    /// or captured list, and of course the chess engine.
    /// 
    /// ChessGame is also responsible for routing applicable events from the UI
    /// to the engine, like when a player clicks on a square.
    /// </summary>
    public sealed class ChessGame : IDisposable
    {
        #region Private Enums
        /// <summary>
        /// Possible states for input response
        /// </summary>
        private enum InputState
        {
            WaitingOnPieceSelection,  // Waiting on the 1st click to select piece
            WaitingOnMoveSelection,   // Waiting on the 2nd click to confirm move
            WaitingOnOpponentMove     // Waiting on the opponent to move
        };
        #endregion

        #region Public Events
        /// <summary>
        /// Fired when a self play game ends
        /// </summary>
        public event EventHandler<EventArgs> OnChessGameSelfPlayGameOver;

        /// <summary>
        /// Fired when a normal game ends
        /// </summary>
        public event EventHandler<EventArgs> OnChessGameNormalPlayGameOver;
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new ChessGame object
        /// </summary>
        /// <param name="engineView">IChessBoardView to use</param>
        /// <param name="fullPathToEngine">Full path the chess engine exe</param>
        /// <param name="engineLoader">object to load engine given path</param>
        /// <param name="reduceEngineStrength">true to make the engine weaker</param>
        /// <param name="cultureInfo">CultureInfo for main form</param>
        public ChessGame(IChessBoardView engineView, string fullPathToEngine, 
            IChessEngineProcessLoader engineLoader, bool reduceEngineStrength, CultureInfo cultureInfo)
        {
            // Save the view
            view = engineView;

            // Save culture for this thread
            currentCultureInfo = cultureInfo;
            Thread.CurrentThread.CurrentCulture = currentCultureInfo;
            Thread.CurrentThread.CurrentUICulture = currentCultureInfo;

            ThinkingLocalized = Properties.Resources.Thinking;

            // Create legal move list
            legalMoves = new List<BoardSquare>();
            selectedPiece = null;

            // Create the board, the view, and the engine
            // Create new UCI engine object
            engine = new UCIChessEngine(engineLoader);
            
            // Subscribe to events from the engine (commands and verbose)
            engine.OnChessEngineResponseReceived += ChessEngineResponseReceivedEventHandler;
            engine.OnChessEngineVerboseOutputReceived += ChessEngineVerboseOutputReceivedEventHandler;

            // This will launch the process
            engine.LoadEngine(fullPathToEngine);
            
            // Start UCI
            engine.InitializeUciProtocol();
            
            // Initialize the chess engine with optional parameters
            // Note this is engine dependent and this version only works with stockfish (to my knowledge)
            // Other UCI engines use different options, several a combination of UCI_LimitStrength and UCI_ELO
            // Eventually this will need to be handled in some options dialog that will either
            // be customizable per engine, or provide a standard way to select things like play strength and time
            if (reduceEngineStrength)
            {
                string exeName = Path.GetFileName(fullPathToEngine);
                exeName = exeName.ToLower();
                string gimpedElo = String.Empty;

                //No Options listed (when tested at console)
                //===================
                //SOS 5 Arena
                //Anmon 5.75
                //Hermann28_32
                //Ruffian 105

                if (exeName.StartsWith("stockfish_8"))
                {
                    UciSetOptionCommand command = new UciSetOptionCommand("Skill Level", "0");
                    command.Execute(engine);
                }
                else if (exeName.StartsWith("rybkav2.3.2a"))
                {
                    gimpedElo = "1200";
                }
                else if (exeName.StartsWith("spike1.4"))
                {
                    gimpedElo = "1300";
                }
                //MadChess
                //option name UCI_LimitStrength type check default false
                //option name UCI_Elo type spin default 400 min 400 max 2200
                // Removed because it seems to take forever to return once this is
                // set with the engine.  The same behavior at the console, so 
                // ignore it here

                if (gimpedElo != String.Empty)
                {
                    UciSetOptionCommand reduceEloCommand = new UciSetOptionCommand("UCI_LimitStrength", "true");
                    reduceEloCommand.Execute(engine);

                    UciSetOptionCommand setEloRating = new UciSetOptionCommand("UCI_Elo", gimpedElo);
                    setEloRating.Execute(engine);
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ChessGame()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of disposable objects and unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                engine.OnChessEngineResponseReceived -= ChessEngineResponseReceivedEventHandler;
                engine.OnChessEngineVerboseOutputReceived -= ChessEngineVerboseOutputReceivedEventHandler;
                engine.Dispose();
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="playerColor">Color for the human player</param>
        /// <param name="engineThinkTimeInMs">Time engine is allowed for a move</param>
        public void NewGame(PieceColor playerColor, int engineThinkTimeInMs)
        {
            // Basic setup
            CommonInit(playerColor, engineThinkTimeInMs);

            // Initialize the board
            board.NewGame();

            // Initialize the engine
            engine.ResetBoard();

            // Start the game if the computer goes first
            if (playerColor != PieceColor.White)
            {
                GetBestMoveAsync();
            }
        }

        /// <summary>
        /// Start requesting moves from the engine for the board.  This will alternate
        /// and apply moves for both sides, not just the player color (human)
        /// </summary>
        public void StartEngineSelfPlay()
        {
            selfPlay = true;
            UpdateEnginePosition();
        }

        /// <summary>
        /// Stop self play
        /// </summary>
        public void StopEngineSelfPlay()
        {
            selfPlay = false;
        }

        /// <summary>
        /// Ask the engine to evaluate the current board for the best move.  This
        /// is an asynchronous call.  ChessEngineResponseReceivedEventHandler will
        /// fired when the command is processed
        /// </summary>
        public void GetBestMoveAsync()
        {
            UciGoCommand command = new UciGoCommand(thinkTime.ToString());
            command.Execute(engine);
        }

        /// <summary>
        /// Create a new position based on a FEN
        /// </summary>
        /// <param name="playerColor">Color for the human player</param>
        /// <param name="fenNotation">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        /// <param name="engineThinkTimeInMs">Time engine is allowed for a move</param>
        public void NewPosition(PieceColor playerColor, string fenNotation, int engineThinkTimeInMs)
        {
            // Basic setup
            CommonInit(playerColor, engineThinkTimeInMs);

            // Initialize board
            board.NewPosition(fenNotation);

            // Initialize the engine with a new position
            UpdateEnginePosition();
        }

        /// <summary>
        /// User clicked somewhere on the client area for ChessGame
        /// </summary>
        /// <param name="x">x coordinate relative to top-left</param>
        /// <param name="y">y coordinate relative to top-left</param>
        public void ProcessClick(int x, int y)
        {
            InputState inputState = GetInputState();

            if (selfPlay || (inputState == InputState.WaitingOnOpponentMove))
            {
                return;
            }

            
            // Only clicks on the board mean anything right now, so get that rect
            Rectangle boardViewRect = view.BoardRect;

            // If the point sent to us is inside that rect, then deal with it
            // otherwise just ignore it
            if (!boardViewRect.Contains(x, y))
            {
                return;
            }

            if (inputState == InputState.WaitingOnPieceSelection)
            {
                OnWaitingForPieceSelection(x, y);
            }
            else if (inputState == InputState.WaitingOnMoveSelection)
            {
                OnWaitingForMoveSelection(x, y);
            }

            // Redraw the form
            view.Invalidate();
        }

        /// <summary>
        /// Tell the view to render itself
        /// </summary>
        /// <param name="g">Graphics object for the form</param>
        public void Render(Graphics g)
        {
            view?.Render(g);
        }

        /// <summary>
        /// Quit
        /// </summary>
        public void Quit()
        {
            // Close the engine (external process)
            engine.QuitEngine();
        }

        /// <summary>
        /// Undo the last move made if applicable
        /// </summary>
        public void UndoLastMove()
        {
            // Do nothing if waiting on the engine...or self play
            if ((selfPlay == false) && (GetInputState() != InputState.WaitingOnOpponentMove) &&
                (board.Moves.Count() > 1))
            {
                // Actually need the last 2 moves (first undo the last opponent move, then
                // undo the last player move...
                board.RevertLastMove();
                board.RevertLastMove();
            }
        }

        /// <summary>
        /// Should only be called after a game over event, no draws right now in normal
        /// play so find the mated player, or possibly the stalemate.
        /// </summary>
        /// <returns>GameResult value</returns>
        public GameResult GetWinner()
        {
            bool whiteStalemate = false;
            bool whiteMated = PlayerMated(PieceColor.White, out whiteStalemate);

            bool blackStalemate = false;
            bool blackMated = PlayerMated(PieceColor.Black, out blackStalemate);

            bool activeMated = (ActivePlayer == PieceColor.White) ? whiteMated : blackMated;
            bool opponentMated = (ActivePlayer == PieceColor.White) ? blackMated : whiteMated;
            bool activeStalemate = (ActivePlayer == PieceColor.White) ? whiteStalemate : blackStalemate;

            //Drawn - neither player has legal moves and neither is in check
            if (!opponentMated && activeMated && activeStalemate)
            {
                return GameResult.Draw;
            }

            // Stalemate is stalemate at this point
            if (whiteStalemate)
            {
                return GameResult.Stalemate;
            }

            // Still going....needed to check agains engines that behave questionably
            // in drawn and or mated positions
            if (!opponentMated && !activeMated)
            {
                return GameResult.Contested;
            }

            // we must have an actual winner
            return whiteMated ? GameResult.BlackWins : GameResult.WhiteWins;
        }

        /// <summary>
        /// Returns current FEN for the board
        /// </summary>
        /// <returns>current FEN for the board</returns>
        public string GetCurrentFEN()
        {
            return board.CurrentFEN;
        }

        /// <summary>
        /// Converts a class to its character.  This does not need to differentiate
        /// case for color like FEN does, it's just needed to update the engine.  
        /// The starting position or FEN for the engine determines the active player
        /// </summary>
        /// <param name="job">Job to convert</param>
        /// <returns></returns>
        public static char PieceClassToPromotionChar(PieceClass job)
        {
            if (job == PieceClass.Queen)
                return 'q';
            if (job == PieceClass.Rook)
                return 'r';
            if (job == PieceClass.Knight)
                return 'n';
            if (job == PieceClass.Bishop)
                return 'b';
            // King and Pawn are missing from the normal list, as this is only
            // used in promotion, and you cannot promote to yourself or to the king
            throw new ArgumentOutOfRangeException();
        }
        #endregion

        #region Private Methods
        private void CommonInit(PieceColor humanColor, int engineThinkTimeInMs)
        {
            playerColor = humanColor; // save the player color
            thinkTime = engineThinkTimeInMs;

            // Create the board and view
            board = new ChessBoard();

            // Set the data for the view
            view.ViewData = board;

            // Set the Offset for the view
            view.Offset = new Point(25, 75);

            // Create and initialize the board and view
            view.ViewData = board;

            // Set orientation for black players
            if (playerColor == PieceColor.Black)
            {
                board.Orientation = BoardOrientation.BlackOnBottom;
            }

            // Override the unicode drawing with bmp images
            view.SetBitmapImages(new Bitmap(Properties.Resources.chesspieces), new Size(64, 64));
        }

        /// <summary>
        /// Returns the current input state
        /// </summary>
        /// <returns></returns>
        private InputState GetInputState()
        {
            if ((playerColor != ActivePlayer) || selfPlay)
            {
                return InputState.WaitingOnOpponentMove;
            }

            if (selectedPiece == null)
            {
                return InputState.WaitingOnPieceSelection;
            }
            return InputState.WaitingOnMoveSelection;
        }

        /// <summary>
        /// Process the input when waiting for the current player to select a piece
        /// </summary>
        /// <param name="x">x coordinate in form</param>
        /// <param name="y">y coordinate in form</param>
        private void OnWaitingForPieceSelection(int x, int y)
        {
            ChessPiece foundPiece = ((IChessBoardView)view).GetPiece(x, y);
            if ((foundPiece != null) && (foundPiece.Color == PlayerColor))
            {
                if (foundPiece.Color == PlayerColor)
                {
                    legalMoves = LegalChessMovesGenerator.GetLegalMoves(foundPiece, board);
                    ((IChessBoardView)view).HighlightSquares(ref legalMoves);
                    ((IChessBoardView)view).HighlightSquare(foundPiece.File, foundPiece.Rank);
                    selectedPiece = foundPiece;
                }
            }
        }

        /// <summary>
        /// Process the input when waiting for the current player to select a move
        /// </summary>
        /// <param name="x">x coordinate in form</param>
        /// <param name="y">y coordinate in form</param>
        private void OnWaitingForMoveSelection(int x, int y)
        {
            BoardSquare square = ((IChessBoardView)view).GetSquare(x, y);
            foreach (BoardSquare move in legalMoves)
            {
                if (move == square)
                {
                    // Done - this is the move
                    MoveInformation moveInfo = new MoveInformation(
                        new BoardSquare(selectedPiece.File, selectedPiece.Rank), move, selectedPiece.Deployed, board.CurrentFEN);

                    moveInfo.Color = selectedPiece.Color;
                    moveInfo.CastlingRights = board.ActivePlayerCastlingRights;

                    Debug.WriteLine("Valid Move Detected: [{0},{1}]=>[{2},{3}]",
                        selectedPiece.File, selectedPiece.Rank, move.File, move.Rank);
                    
                    // Need to detect promotion and launch dialog for it...
                    bool isPawnMovingToBackRank = (selectedPiece.Color == PieceColor.White) ? (moveInfo.End.Rank == 8) : (moveInfo.End.Rank == 1);
                    if ((selectedPiece.Job == PieceClass.Pawn) && isPawnMovingToBackRank)
                    {
                        PieceClass promotionJob = view.ChoosePromotionJob();
                        board.PromotePiece(moveInfo.Start.File, moveInfo.Start.Rank, moveInfo.End.File, moveInfo.End.Rank, promotionJob, ref moveInfo);
                    }
                    
                    // Always returns true now
                    board.MovePiece(ref moveInfo);
                    view.Invalidate();

                    Debug.WriteLine(String.Format("Fullmoves: {0}", board.FullMoveCount));
                    Debug.WriteLine(String.Format("Halfmoves: {0}", board.HalfMoveCount));
                    Debug.WriteLine(String.Format("WhCastle: {0}", board.WhiteCastlingRights.ToString()));
                    Debug.WriteLine(String.Format("BlCastle: {0}", board.BlackCastlingRights.ToString()));

                    // Update the position with the engine
                    UpdateEnginePosition();
                    break;
                }
            }

            // Either way this gets cleared
            legalMoves.Clear();
            ((IChessBoardView)view).ClearHiglightedSquares();
            selectedPiece = null;
        }

        /// <summary>
        /// Invoked when the chess engine has responded with a move to apply to the 
        /// local board
        /// </summary>
        private void OnEngineBestMoveResponse()
        {
            thinkingIndex = 0;  // reset index counter for simple progress text

            // Get the best move from the engine
            string bestMove = engine.BestMove;
            if ((String.Compare(bestMove, "(none)") == 0) || // Stockfish (and converted ones)
                (String.Compare(bestMove, "a1a1") == 0)   || // Rybka
                (board.HalfMoveCount >= HalfMovesUntilDraw)) // Propably spinning on self play or just a draw
            {
                if (board.HalfMoveCount >= HalfMovesUntilDraw)
                {
                    Debug.WriteLine("Draw by 50 moves rule...");
                }
                GameOverHandler();
            }
            else if (GetInputState() == InputState.WaitingOnOpponentMove)
            {
                // Extract the board location from the move string
                PieceFile startFile = new PieceFile(bestMove[0]);
                int startRank = Convert.ToInt16(bestMove[1]) - Convert.ToInt16('0');
                PieceFile destFile = new PieceFile(bestMove[2]);
                int destRank = Convert.ToInt16(bestMove[3]) - Convert.ToInt16('0');

                ChessPiece foundPiece = board.FindPieceAt(startFile, startRank);
                MoveInformation moveInfo = new MoveInformation(
                        new BoardSquare(startFile, startRank),
                        new BoardSquare(destFile, destRank),
                        foundPiece.Deployed, board.CurrentFEN);

                moveInfo.Color = foundPiece.Color;
                moveInfo.CastlingRights = board.ActivePlayerCastlingRights;

                // When coming from the engine, we get the promotion detection for free
                if (bestMove.Length == 5)
                {
                    // Applied on the next move
                    PieceClass promotionJob = ChessBoard.PieceClassFromFen(bestMove[4]);
                    board.PromotePiece(startFile, startRank, destFile, destRank, promotionJob, ref moveInfo);
                }

                // Move the piece on the board, and add it to the official moves list
                board.MovePiece(ref moveInfo);
                
                // trigger a redraw
                view.Invalidate();

                // Apply the move the engine just gave us with the engine (update it's own move)
                UpdateEnginePosition();
            }
        }

        /// <summary>
        /// Calls the valid handler for game over depending on state
        /// </summary>
        private void GameOverHandler()
        {
            FenParser.DebugFENPosition(board.CurrentFEN);

            // Full verbose output
            string[] lines = FenParser.FENToStrings(board.CurrentFEN);
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
            }

            if (selfPlay)
            {
                if (OnChessGameSelfPlayGameOver != null)
                {
                    OnChessGameSelfPlayGameOver(this, null);
                }
            }
            else
            {
                if (OnChessGameNormalPlayGameOver != null)
                {
                    OnChessGameNormalPlayGameOver(this, null);
                }
            }
        }

        /// <summary>
        /// SelfPlay response handler
        /// </summary>
        /// <param name="response">response from the chess engine</param>
        private void OnEngineSelfPlayResponseHandler(string response)
        {
            // The code is the same right now again...but may change
            OnEngineNormalPlayResponseHandler(response);
        }

        /// <summary>
        /// Handler for normal play.  If updating the position for the human player
        /// get the next computer move.  Otherwise, it the computer is responding
        /// with its move, apply it to the local board
        /// </summary>
        /// <param name="response"></param>
        private void OnEngineNormalPlayResponseHandler(string response)
        {
            Debug.WriteLine(String.Concat("Response: ", response));

            // If this is true, it means we're updating our position with the engine
            // (e.g. syncing up after a move was applied locally to our object)
            if (updatingPosition)
            {
                // Unfortunately - some engines are wonky when it comes to drawn or
                // mated positions, so just check first before
                // However, this pretty much negates the "bestmove" handling for the
                // end game - so engines that behaved badly there otherwise will
                // also be "fixed" by this - it slows the handling down, but not
                // really noticeably even in fast self play.
                updatingPosition = false;
                GameResult result = GetWinner();
                if (result == GameResult.Contested)
                {
                    // Now trigger getting the next move from the engine and exit
                    GetBestMoveAsync();
                }
                else
                {
                    Debug.WriteLine(String.Format("GameOver Detected: {0} - stop updating engine.", result.ToString()));
                    GameOverHandler();
                }
            }
            else if (response.StartsWith("bestmove"))
            {
                OnEngineBestMoveResponse();
                Debug.WriteLine(String.Format("Fullmoves: {0}", board.FullMoveCount));
                Debug.WriteLine(String.Format("Halfmoves: {0}", board.HalfMoveCount));
                Debug.WriteLine(String.Format("WhCastle: {0}", board.WhiteCastlingRights.ToString()));
                Debug.WriteLine(String.Format("BlCastle: {0}", board.BlackCastlingRights.ToString()));
            }
        }

        /// <summary>
        /// EventHandler for the chess engine.  This handler is invoked (after
        /// subscription) when the engine has finished processing a command
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Contains the final response string</param>
        private void ChessEngineResponseReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
        {
            if (selfPlay)
            {
                // Likely common code, but it's working so leave it for now
                OnEngineSelfPlayResponseHandler(e.Response);
            }
            else
            {
                OnEngineNormalPlayResponseHandler(e.Response);
            }
        }

        /// <summary>
        /// EventHandler for the chess engine.This handler is invoked(after
        /// subscription) when the engine has received any output from the engine
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">string response from the chess engine</param>
        private void ChessEngineVerboseOutputReceivedEventHandler(object sender, ChessEngineResponseReceivedEventArgs e)
        {
            // Removing the thinking lines to declutter the debug outbput
            if (!e.Response.StartsWith("info"))
            {
                Debug.WriteLine(String.Concat("<=Engine: ", e.Response));
            }

            // Build the progress text bar
            StringBuilder sb = new StringBuilder(": [");
            sb.Insert(0, ThinkingLocalized);
            sb.Append('\u25AB', 75);
            sb.Append(']');
            // Replace the current spinning index to the marker character
            sb.Replace('\u25AB', '\u25AA', 11, thinkingIndex);
            board.ThinkingText = sb.ToString();
            
            // Wrap the progress counter index.  It moves between the '[' and ']' chars
            if (thinkingIndex < 75)
            {
                thinkingIndex++;
            }
            else
            {
                thinkingIndex = 0;
            }
        }

        /// <summary>
        /// Update the position with the chess engine.  Once a move is applied with the
        /// board, then engine needs to know, so it can analyze the next move whether
        /// this came from a player or the engine iteself
        /// </summary>
        private void UpdateEnginePosition()
        {
            // The board constantly updates its FEN, so send the FEN to the engine
            // Some engines have a limited input buffer, so games with many many
            // moves will eventually exceed the buffer length and then it breaks.
            // This method ensures we'll fit in any reasonable buffer lenghth, but
            // puts the burden on the board to keep it constantly updated
            // See FenParser class for those details
            updatingPosition = true;
            UciPositionCommand command = new UciPositionCommand(board.CurrentFEN);
            command.Execute(engine);
        }
        
        /// <summary>
        /// Checks if a player is mated (no legal moves)
        /// </summary>
        /// <param name="color">Color of player to check</param>
        /// <param name="positionIsStaleMate">Set to true on stalemate</param>
        /// <returns>True if there are no legal moves for the player color</returns>
        private bool PlayerMated(PieceColor color, out bool positionIsStaleMate)
        {
            positionIsStaleMate = false;
            bool result = true;
            List<ChessPiece> pieces = (color == PieceColor.White) ? board.WhitePieces : board.BlackPieces;
            foreach (ChessPiece piece in pieces)
            {
                if (!piece.Captured && LegalChessMovesGenerator.GetLegalMoves(piece, board).Count() > 0)
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                // Verify the King is in check, if not, stalemate
                ChessPiece king = board.GetKing(color);
                if (!LegalChessMovesGenerator.IsSquareInCheck(board, king.File, king.Rank, color))
                {
                    positionIsStaleMate = true;
                }
            }
            return result;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The ClientSize that ChessGame would like.  Some extra room for now.
        /// </summary>
        public static Size RequestedSize
        {
            get
            {
                return new Size(ChessBoardView.BoardSizeInPixels + 50 + ChessBoardView.MoveHistoryWidthInPixels + 25,
                    ChessBoardView.BoardSizeInPixels + 100);
            }
        }

        /// <summary>
        /// Color for the human player
        /// </summary>
        public PieceColor PlayerColor
        {
            get { return playerColor; }
        }

        /// <summary>
        /// Color for the active player.
        /// </summary>
        public PieceColor ActivePlayer
        {
            get { return board.ActivePlayer; }
        }
        #endregion

        #region Private Fields
        private bool disposed = false;
        private bool selfPlay = false;
        private string thinking = String.Empty;
        private int thinkingIndex = 0;
        private bool updatingPosition = false;
        private PieceColor playerColor;
        private ChessBoard board;
        private IChessBoardView view;
        private UCIChessEngine engine;
        private ChessPiece selectedPiece;
        private List<BoardSquare> legalMoves;
        private static int HalfMovesUntilDraw = 50;
        private int thinkTime = 250;
        private CultureInfo currentCultureInfo;
        private readonly string ThinkingLocalized;
        #endregion
    }
}