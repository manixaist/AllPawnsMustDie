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
        /// <param name="clientForm">Windows Form the game will draw to</param>
        /// <param name="fullPathToEngine">Full path the chess engine exe</param>
        /// <param name="reduceEngineStrength">true to make the engine weaker</param>
        /// <param name="cultureInfo">CultureInfo for main form</param>
        public ChessGame(Form clientForm, string fullPathToEngine, bool reduceEngineStrength, CultureInfo cultureInfo)
        {
            // Save the form
            form = clientForm;

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
            UCIChessEngine uciEngine = new UCIChessEngine();
            engine = (IChessEngine)uciEngine;

            // Subscribe to events from the engine (commands and verbose)
            engine.OnChessEngineResponseReceived += ChessEngineResponseReceivedEventHandler;
            engine.OnChessEngineVerboseOutputReceived += ChessEngineVerboseOutputReceivedEventHandler;

            // This will launch the process
            engine.LoadEngine(fullPathToEngine);
            engine.SendCommandAsync(UCIChessEngine.Uci, UCIChessEngine.UciOk);

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
                    engine.SendCommandAsync("setoption name Skill Level value 0", String.Empty);
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
                    engine.SendCommandAsync("setoption name UCI_LimitStrength value true", String.Empty);
                    engine.SendCommandAsync(String.Format("setoption name UCI_Elo value {0}", gimpedElo), String.Empty);
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
                ((IDisposable)view).Dispose();
                ((UCIChessEngine)engine).Dispose();
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
            engine.Reset();

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
            string moveCommand = String.Format("{0} {1}", MoveCommand, thinkTime);
            engine.SendCommandAsync(moveCommand, UCIChessEngine.BestMoveResponse);
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

            IChessBoardView viewInterface = ((IChessBoardView)view);

            // Only clicks on the board mean anything right now, so get that rect
            Rectangle boardViewRect = viewInterface.BoardRect;

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
            form.Invalidate();
        }

        /// <summary>
        /// Tell the view to render itself
        /// </summary>
        /// <param name="g">Graphics object for the form</param>
        public void Render(Graphics g)
        {
            if (view != null)
            {
                ((IChessBoardView)view).Render(g);
            }
        }

        /// <summary>
        /// Quit
        /// </summary>
        public void Quit()
        {
            // Close the engine (external process)
            engine.Quit();
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
            view = new ChessBoardView(form);

            // Set the data for the view
            ((IChessBoardView)view).ViewData = board;

            // Set the Offset for the view
            ((IChessBoardView)view).Offset = new Point(25, 75);

            // Create and initialize the board and view
            ((IChessBoardView)view).ViewData = board;

            // Set orientation for black players
            if (playerColor == PieceColor.Black)
            {
                board.Orientation = BoardOrientation.BlackOnBottom;
            }

            // Override the unicode drawing with bmp images
            ((IChessBoardView)view).SetBitmapImages(new Bitmap(Properties.Resources.chesspieces), new Size(64, 64));
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
                    legalMoves = GetLegalMoves(foundPiece, board);
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
                        PromotionDialog pd = new PromotionDialog();
                        pd.ShowDialog(form);
                        board.PromotePiece(moveInfo.Start.File, moveInfo.Start.Rank, moveInfo.End.File, moveInfo.End.Rank, pd.PromotionJob, ref moveInfo);
                    }
                    
                    // Always returns true now
                    board.MovePiece(ref moveInfo);
                    form.Invalidate();

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
            string bestMove = ((UCIChessEngine)engine).BestMove;
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
                form.Invalidate();

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

            // Get the control name w're using to output verbose for now (it's a label)
            Control verboseControl = form.Controls[APMD_Form.EngineUpdateControlName];

            // Build the progress text bar
            StringBuilder sb = new StringBuilder(": [");
            sb.Insert(0, ThinkingLocalized);
            sb.Append('\u25AB', 75);
            sb.Append(']');
            // Replace the current spinning index to the marker character
            sb.Replace('\u25AB', '\u25AA', 11, thinkingIndex);
            thinking = sb.ToString();

            // Wrap the progress counter index.  It moves between the '[' and ']' chars
            if (thinkingIndex < 75)
            {
                thinkingIndex++;
            }
            else
            {
                thinkingIndex = 0;
            }

            // Check if we're on the UI thread, the answer is almostly certainly no
            if (verboseControl.InvokeRequired)
            {
                // Invoke is synchronous - this will block this thread
                verboseControl.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread now, so this is safe
                    verboseControl.Text = thinking;
                });
            }
            else
            {
                // If for some reason we are on the UI thread, then we can just update it
                verboseControl.Text = thinking;
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
            string position = String.Format("position fen {0}", board.CurrentFEN);
            engine.SendCommandAsync(position, "");
        }

        /// <summary>
        /// Helper to check if a king of a given color is in check on a given square
        /// </summary>
        /// <param name="board">ChessBoard to check against</param>
        /// <param name="file">ChessFile to check against</param>
        /// <param name="rank">Rank to check against</param>
        /// <param name="kingColor">Color of king to check against</param>
        /// <returns>true if the king would be in check on this square</returns>
        private static bool IsSquareInCheck(ChessBoard board, PieceFile file, int rank, PieceColor kingColor)
        {
            bool result = false;
            List<ChessPiece> opponentPieces = (kingColor == PieceColor.White) ? board.BlackPieces : board.WhitePieces;
            foreach (ChessPiece piece in opponentPieces)
            {
                if (CanPieceTargetSquare(board, piece, file, rank))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a given piece can target a specified square.  It is not required
        /// that the square be occupied by an enemy piece, just potentially reachable
        /// </summary>
        /// <param name="board">ChessBoard to check against</param>
        /// <param name="piece">ChessPiece to check</param>
        /// <param name="targetFile">file to target</param>
        /// <param name="targetRank">rank to target</param>
        /// <returns>true if the piece can target the given square</returns>
        private static bool CanPieceTargetSquare(ChessBoard board, ChessPiece piece, PieceFile targetFile, int targetRank)
        {
            bool result = false;

            if (piece.Captured == true)
            {
                return false;
            }

            BoardSquare targetSquare = new BoardSquare(targetFile, targetRank);
            List<BoardSquare> moves = new List<BoardSquare>();
            switch (piece.Job)
            {
                case PieceClass.Pawn:
                    // Can't reuse the normal helper as it requires the space to be occupied
                    // also w/o en-passant and double moves, this can be simpler
                    int pawnTargetRank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;
                    if (targetRank == pawnTargetRank)
                    {
                        if (((piece.File.ToInt() - 1) == targetFile.ToInt()) ||
                            ((piece.File.ToInt() + 1) == targetFile.ToInt()))
                        {
                            result = true;
                        }
                    }
                    break;
                case PieceClass.Knight:
                    moves = GetLegalMoves_Knight(piece, board);
                    break;
                case PieceClass.Bishop:
                    moves = GetLegalMoves_Bishop(piece, board);
                    break;
                case PieceClass.Rook:
                    moves = GetLegalMoves_Rook(piece, board);
                    break;
                case PieceClass.Queen:
                    moves = GetLegalMoves_Queen(piece, board);
                    break;
                case PieceClass.King:
                    // don't recurse into the normal call, also alternate method to examine
                    // These are pairs of offsets (-1, 0), (-1, 1),...etc so there are twice
                    // as many of these as squares to check
                    int[] offsets = new int[] { -1, 0, -1, 1, -1, -1, 1, 0, 1, 1, 1, -1, 0, 1, 0, -1 };
                    for (int index = 0; index < offsets.Length / 2; index++)
                    {
                        int fileOffset = offsets[index * 2];
                        int rankOffset = offsets[(index * 2) + 1];
                        // Test the validity of the square offset
                        if (BoardSquare.IsValid(piece.File.ToInt() + fileOffset, piece.Rank + rankOffset))
                        {
                            BoardSquare testSquare =
                                new BoardSquare(new PieceFile(piece.File.ToInt() + fileOffset), piece.Rank + rankOffset);
                            if (testSquare == targetSquare)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // King is special above
            if (piece.Job != PieceClass.King)
            {
                // Check moves for the target square
                foreach (BoardSquare square in moves)
                {
                    if (square == targetSquare)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a square is either empty, or has an opponent piece in it
        /// It also performs bounds checking
        /// </summary>
        /// <param name="col">int based col (can be out of bounds)</param>
        /// <param name="row">int based row (can be out of bounds)</param>
        /// <param name="board">ChessBoard to check</param>
        /// <param name="playerColor">PieceColor of the player</param>
        /// <param name="occupied">set to true if an opponent piece is also there</param>
        /// <returns>true if the square is empty or contains an opponent piece</returns>
        private static bool SquareIsFreeOrContainsOpponent(int col, int row, ChessBoard board, PieceColor playerColor, out bool occupied)
        {
            bool result = false;
            occupied = false;
            if (BoardSquare.IsValid(col, row))
            {
                // Get the piece at the square if any
                ChessPiece tPiece = board.FindPieceAt(new PieceFile(col), row);
                // No piece...
                if (tPiece == null)
                {
                    result = true;
                    occupied = false;
                }
                else // ...or opponent piece
                {
                    PieceColor opponentColor = (playerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (tPiece.Color == opponentColor)
                    {
                        result = true;
                        occupied = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Helper to return all legal moves for the given piece.
        /// </summary>
        /// <param name="piece">piece to check, assumed to be valid</param>
        /// <param name="board">board to check agains, also assumed valid</param>
        /// <returns>List of squares the piece can legally move to</returns>
        private static List<BoardSquare> GetLegalMoves(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> preCheckMoves = new List<BoardSquare>();

            // Get a list of legal moves ignoring check violations
            switch (piece.Job)
            {
                case PieceClass.Pawn:
                    preCheckMoves = GetLegalMoves_Pawn(piece, board);
                    break;
                case PieceClass.Knight:
                    preCheckMoves = GetLegalMoves_Knight(piece, board);
                    break;
                case PieceClass.Bishop:
                    preCheckMoves = GetLegalMoves_Bishop(piece, board);
                    break;
                case PieceClass.Rook:
                    preCheckMoves = GetLegalMoves_Rook(piece, board);
                    break;
                case PieceClass.Queen:
                    preCheckMoves = GetLegalMoves_Queen(piece, board);
                    break;
                case PieceClass.King:
                    preCheckMoves = GetLegalMoves_King(piece, board);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Eliminate any move found that would place the King in check
            List<BoardSquare> legalMoves = new List<BoardSquare>();
            ChessPiece playerKing = board.GetKing(piece.Color);
            foreach (BoardSquare square in preCheckMoves)
            {
                // Move the piece for the check - also remove any capture for the test
                ChessPiece tempCapture = board.FindPieceAt(square.File, square.Rank);
                piece.TempMove(square.File, square.Rank);
                if (tempCapture != null)
                {
                    tempCapture.Captured = true;
                }                

                if (!IsSquareInCheck(board, playerKing.File, playerKing.Rank, playerKing.Color))
                {
                    legalMoves.Add(square);
                }

                if (tempCapture != null)
                {
                    tempCapture.Captured = false;
                }

                // reset the piece (this bypasses the ChessBoard class)
                piece.ResetTempMove();
            }
            return legalMoves;
        }

        // Helper delegate to check targets (lambdas in each specific method)
        private delegate void CheckPieceTargets(ChessPiece p, int fileOffset,
            int rankOffset, ChessBoard board, List<BoardSquare> validMoves);

        /// <summary>
        /// Checks for valid moves along a linear path.  The path starts at the piece
        /// and moves outward in one of 8 direction until either a piece is reached
        /// or the edge of the board
        /// </summary>
        private static CheckPieceTargets CheckLinearTargets = (p, fileDelta, rankDelta, b, m) =>
        {
            int startCol = p.File.ToInt();
            int startRow = p.Rank;
            int endCol = startCol + fileDelta;
            int endRow = startRow + rankDelta;

            // As long as we're still on the board...
            while (BoardSquare.IsValid(endCol, endRow))
            {
                bool occupied;
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                    if (occupied) // Must be an opponent, so this move is valid
                    {
                        break;  // stop though, no more can be valid along this path
                    }
                }
                else
                {
                    break;
                }
                endCol += fileDelta; // advance along our 'slope'
                endRow += rankDelta;
            }
        };

        /// <summary>
        /// Returns a list of valid squares a pawn can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_Pawn(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Pawns can move one space forward on any move provided the square is empty
            * and 2 squares if it's the first move the pawn has ever made.
            * A pawn may move diagonally if the square is occupied by an opponent piece (capture)
            * or if the space behind the diagonal is occuped by an opponent pawn that
            * just moved 2 spaces (en-passant)
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |  C = Capture only
            * +---+---+---+---+---+---+---+---+  T = Move or capture
            * |   |   |   | M |   |   |   |   |  M = Move Only
            * +---+---+---+---+---+---+---+---+
            * |   |   | C | M | C |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   | P |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            * |   |   |   |   |   |   |   |   |
            * +---+---+---+---+---+---+---+---+
            */
            // One rank "forward" which depends on your color
            int rank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;
            if (null == board.FindPieceAt(piece.File, rank))
            {
                moves.Add(new BoardSquare(piece.File, rank));

                // The 2nd move is only valid of the 1st one was (i.e. you can't move through
                // another piece on your first pawn move)
                if (piece.Deployed == false)
                {
                    rank += (piece.Color == PieceColor.White) ? 1 : -1;
                    if (null == board.FindPieceAt(piece.File, rank))
                    {
                        moves.Add(new BoardSquare(piece.File, rank));
                    }
                }
            }

            // Get the en-passant target if it exists, most of the time it will not
            // it only exists the move after an enemy pawn has jumped 2 spaces on
            // its initial move.
            BoardSquare enPassantTarget;
            bool enPassantValid = board.GetEnPassantTarget(out enPassantTarget);

            // Targets will ALWAYS be 1 rank away (enPassant target is behind piece captured)
            rank = (piece.Color == PieceColor.White) ? piece.Rank + 1 : piece.Rank - 1;

            // Lambda helper
            CheckPieceTargets checkPawnTargets = (p, fileOffset, rankOffset, b, m) =>
            {
                int newFileIndex = p.File.ToInt() + fileOffset;

                // Can't have diagonal targets on the back rank, or if we're up
                // against the edge we want to check towards
                if (!ChessPiece.IsOnBackRank(piece) && (newFileIndex > 0) && (newFileIndex <= 8))
                {
                    PieceFile tFile = new PieceFile(p.File.ToInt() + fileOffset);
                    BoardSquare targetSquare = new BoardSquare(tFile, rank);
                    ChessPiece tPiece = b.FindPieceAt(tFile, rank);

                    // Either there is a piece of the opposing color on this square
                    // or the sqaure is a valid enpassant target
                    if (((tPiece != null) && (tPiece.Color != p.Color)) ||
                        ((targetSquare == enPassantTarget) && enPassantValid))
                    {
                        m.Add(new BoardSquare(tFile, rank));
                    }
                }
            };

            // There are 2 possible Files (L,R or Kingside, Queenside, etc)
            // Diagonal left (lower file) PieceFile.ToInt() is 0-based since the 
            // drawing code used it first...so adjust by 1 here
            checkPawnTargets(piece, -1, 0, board, moves);
            checkPawnTargets(piece, 1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a knight can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_Knight(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Knights are the only piece that can jump over other pieces, and move
             * in an L-shape (2:1) or (1:2) with a maximum of 8 valid squares.
             * Because they can jump, the only requirement is the target square
             * be empty or contain an opposing piece (and lie within the boundaries
             * of the board)
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+
             * |   | T |   |   |   | T |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | N |   |   |   |   |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | T |   |   |   | T |   |   |     | N |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |     |   |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     |   | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Deltas should be 1 or -1 to indicate direction
            // Checks one quadrant
            CheckPieceTargets checkKnightTargets = (p, fileDelta, rankDelta, b, m) =>
            {
                // Verify targets are reachable (not off edge)
                int startCol = p.File.ToInt();
                int startRow = p.Rank;

                // #1
                int endCol = startCol + (fileDelta * 1);
                int endRow = startRow + (rankDelta * 2);
                bool occupied; // ignored here
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }

                // #2
                endCol = startCol + (fileDelta * 2);
                endRow = startRow + (rankDelta * 1);
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }
            };

            // Check each quadrant
            checkKnightTargets(piece, 1, 1, board, moves);
            checkKnightTargets(piece, 1, -1, board, moves);
            checkKnightTargets(piece, -1, 1, board, moves);
            checkKnightTargets(piece, -1, -1, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a bishop can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_Bishop(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Bishops are long operators that move diagonaly only.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture.  Just as in the case
             * of the knight, the total number of moves reduces as you near the
             * edge, and further more if you approach the corner.
             * 
             * +---+---+---+---+---+---+---+---+
             * | T |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   | T |   |   |   | P |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  q = enemy queen (capture/block))
             * |   |   | T |   | T |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | B |   |   |   |   |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T |   | T |   |   |   |     | B |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | q |   |   |   | T |   |   |     |   | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   | T |   |     |   |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check each linear direction (away from piece as center)
            CheckLinearTargets(piece, 1, 1, board, moves);
            CheckLinearTargets(piece, 1, -1, board, moves);
            CheckLinearTargets(piece, -1, 1, board, moves);
            CheckLinearTargets(piece, -1, -1, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a rook can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_Rook(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Rooks are long operators that move horizontaly only.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture. 
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+
             * |   |   |   | q |   |   |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  q = enemy queen (capture/block))
             * |   |   |   | T |   |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   | P | T | R | T | T | T | T |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | R | T | T | T | T | T |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | T |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   | T |   |   |   |   |     | T |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check each linear direction (away from piece as center)
            // Just like a bishop, but the slope is different
            CheckLinearTargets(piece, 0, 1, board, moves);
            CheckLinearTargets(piece, 0, -1, board, moves);
            CheckLinearTargets(piece, 1, 0, board, moves);
            CheckLinearTargets(piece, -1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a queen can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_Queen(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* Queens are long operators that combine the bishop and rook.  They cannot
             * move over or through a friendly piece, but may take the spot of
             * an opposing piece along its path as a capture. 
             * 
             * +---+---+---+---+---+---+---+---+
             * | T |   |   |   |   |   | T |   |
             * +---+---+---+---+---+---+---+---+
             * |   | T |   | r |   | T |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  r = enemy rook (capture/block))
             * |   |   | T | T | T |   |   |   |  P = friendly pawn (block)
             * +---+---+---+---+---+---+---+---+
             * |   | P | T | Q | T | T | T | T |     Moves reduced on edges of board
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T | T | T |   |   |   |     | Q | T | T | T | T | T |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   | T |   | T |   | T |   |   |     | T | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * | T |   |   | T |   |   | T |   |     | T |   | T |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Check all 8 linear paths away from the Queen
            // "Bishopy" paths (diagonal)
            CheckLinearTargets(piece, 1, 1, board, moves);
            CheckLinearTargets(piece, 1, -1, board, moves);
            CheckLinearTargets(piece, -1, 1, board, moves);
            CheckLinearTargets(piece, -1, -1, board, moves);
            // "Rooky" paths (horz/vert)
            CheckLinearTargets(piece, 0, 1, board, moves);
            CheckLinearTargets(piece, 0, -1, board, moves);
            CheckLinearTargets(piece, 1, 0, board, moves);
            CheckLinearTargets(piece, -1, 0, board, moves);
            return moves;
        }

        /// <summary>
        /// Returns a list of valid squares a king can move to.  The list can be empty
        /// </summary>
        /// <param name="piece">ChessPiece to examine</param>
        /// <param name="board">ChessBoard the piece exists within</param>
        /// <returns>List of valid squares, or empty list</returns>
        private static List<BoardSquare> GetLegalMoves_King(ChessPiece piece, ChessBoard board)
        {
            List<BoardSquare> moves = new List<BoardSquare>();
            /* The King is special.  He can move one square in any direction 
             * (provided it is on the board) so long as the square is empty or
             * it has an opponent piece on it.  However, the King can never
             * move into check, even if the square or capture would be legal
             * otherwise, so it requires some extra checking.
             * 
             * Further complicating things, if the king is castling, he cannot
             * move THROUGH check either (basically check those squares as if
             * they were final destinations)
             * 
             * +---+---+---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+  r = enemy rook (block)
             * |   |   |   |   |   | p |   |   |  T = Move or capture
             * +---+---+---+---+---+---+---+---+  p = enemy pawn (block)
             * |   |   | T | T | X |   | b |   |  b = enemy bishop (block)
             * +---+---+---+---+---+---+---+---+  X = illegal move
             * |   |   | T | K | T |   |   |   |     
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   | T | T | X |   |   |   |     | K | T |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     | X | X |   |   |   | r |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             * |   |   |   |   |   |   |   |   |     |   |   |   |   |   |   |
             * +---+---+---+---+---+---+---+---+     +---+---+---+---+---+---+
             */

            // Cannot castle if in check
            if (!IsSquareInCheck(board, piece.File, piece.Rank, piece.Color))
            {
                /* Castling may only be done if the king has never moved, the rook involved has never moved, 
                 * the squares between the king and the rook involved are unoccupied, the king is not in check, 
                 * and the king does not cross over or end on a square in which it would be in check.
                 *
                 * The ChessBoard will keep track of the castling rights when various pieces move, but it
                 * won't constantly update the legality of the move
                 */

                // Build a list of squares to check
                BoardSide castlingRights = (piece.Color == PieceColor.White) ? board.WhiteCastlingRights : board.BlackCastlingRights;
                BoardSide[] sidesToCheck = new BoardSide[2] { BoardSide.King, BoardSide.Queen };
                foreach (BoardSide sideToCheck in sidesToCheck)
                {
                    // Backrank depends on color
                    int kingRank = (piece.Color == PieceColor.White) ? 1 : 8;
                    BoardSquare[] squares = null;

                    // First check if we still have the right, if not, no need to persue it
                    if (castlingRights.HasFlag(sideToCheck))
                    {
                        squares = new BoardSquare[2];
                        // The target Files depend on the side of the board we're checking
                        // put the final target in [0]
                        if (sideToCheck == BoardSide.King)
                        {
                            squares[0] = new BoardSquare(new PieceFile(7), kingRank);
                            squares[1] = new BoardSquare(new PieceFile(6), kingRank);
                        }
                        else // Queenside
                        {
                            squares[0] = new BoardSquare(new PieceFile(3), kingRank);
                            squares[1] = new BoardSquare(new PieceFile(4), kingRank);
                        }
                    }

                    // There should be 2 and only 2 from above if we found potential targets
                    if (squares != null)
                    {
                        // must be empty and not in check - empty is faster so verify it first
                        if ((board.FindPieceAt(squares[0].File, squares[0].Rank) == null) &&
                            (board.FindPieceAt(squares[1].File, squares[1].Rank) == null))
                        {
                            // Now make sure neither square is in check
                            if (!IsSquareInCheck(board, squares[0].File, squares[0].Rank, piece.Color) &&
                                !IsSquareInCheck(board, squares[1].File, squares[1].Rank, piece.Color))
                            {
                                // King can still castle to this side, add the move option
                                moves.Add(squares[0]);
                            }
                        }
                    }
                }

            }
            // Check each of the 8 squares around the king.  If it's free or has
            // an enemy piece, then check if it's targetable by the opponent
            // (moving into check)  If not, then add it to the list
            CheckPieceTargets checkKingTargets = (p, fileDelta, rankDelta, b, m) =>
            {
                // Verify targets are reachable (not off edge)
                int startCol = p.File.ToInt();
                int startRow = p.Rank;

                int endCol = startCol + (fileDelta);
                int endRow = startRow + (rankDelta);
                bool occupied; // ignored here
                if (SquareIsFreeOrContainsOpponent(endCol, endRow, b, p.Color, out occupied))
                {
                    m.Add(new BoardSquare(new PieceFile(endCol), endRow));
                }
            };

            // Check all 8 squares around the king
            checkKingTargets(piece, 0, 1, board, moves);
            checkKingTargets(piece, 0, -1, board, moves);
            checkKingTargets(piece, 1, 0, board, moves);
            checkKingTargets(piece, -1, 0, board, moves);
            checkKingTargets(piece, 1, 1, board, moves);
            checkKingTargets(piece, -1, -1, board, moves);
            checkKingTargets(piece, 1, -1, board, moves);
            checkKingTargets(piece, -1, 1, board, moves);
        
            // Check violations are handled by the common caller for regulatr moves
            return moves;
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
                if (!piece.Captured && GetLegalMoves(piece, board).Count() > 0)
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                // Verify the King is in check, if not, stalemate
                ChessPiece king = board.GetKing(color);
                if (!IsSquareInCheck(board, king.File, king.Rank, color))
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
        private ChessBoardView view;
        private IChessEngine engine;
        private Form form;
        private ChessPiece selectedPiece;
        private List<BoardSquare> legalMoves;
        private static int HalfMovesUntilDraw = 50;
        private int thinkTime = 250;
        private static string MoveCommand = "go movetime";
        private CultureInfo currentCultureInfo;
        private readonly string ThinkingLocalized;
        #endregion
    }
}