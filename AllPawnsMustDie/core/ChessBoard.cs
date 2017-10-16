using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Encapsulates a chess board.  The board owns the pieces and the history
    /// </summary>
    public class ChessBoard
    {
        #region Public Methods
        /// <summary>
        /// Create a new chess board
        /// </summary>
        public ChessBoard()
        {
            initialFEN = InitialFENPosition;
            currentFEN = initialFEN;
            enPassantValid = false;
            NewGame();
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void NewGame()
        {
            CreateAndPlacePieces(InitialFENPosition);
        }

        /// <summary>
        /// Start a new game at a specified starting position
        /// </summary>
        /// <param name="fen">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        public void NewPosition(string fen)
        {
            initialFEN = fen;
            currentFEN = fen;
            CreateAndPlacePieces(initialFEN);
        }

        /// <summary>
        /// Returns opposite color e.g. white->black black->white
        /// </summary>
        /// <param name="color">color to oppose</param>
        /// <returns>Opposite of 'color'</returns>
        public static PieceColor OppositeColor(PieceColor color)
        {
            return (color == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        }

        /// <summary>
        /// Determine piece class based on the fen character
        /// </summary>
        /// <param name="fenChar">FEN defined char for a piece</param>
        /// <returns>PieceClass for fen, e.g. King for 'k' or 'K'</returns>
        public static PieceClass PieceClassFromFen(Char fenChar)
        {
            PieceClass outClass;
            Char inputChar = Char.ToLower(fenChar);
            switch (inputChar)
            {
                case 'k':
                    outClass = PieceClass.King;
                    break;
                case 'q':
                    outClass = PieceClass.Queen;
                    break;
                case 'r':
                    outClass = PieceClass.Rook;
                    break;
                case 'b':
                    outClass = PieceClass.Bishop;
                    break;
                case 'n':
                    outClass = PieceClass.Knight;
                    break;
                case 'p':
                    outClass = PieceClass.Pawn;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return outClass;
        }

        /// <summary>
        /// Helper to return a FEN char for a job, color matters
        /// </summary>
        /// <param name="pieceClass">Class to convert</param>
        /// <param name="color">PieceColor Black is lowercase</param>
        /// <returns></returns>
        public static char FenCharFromPieceClass(PieceClass pieceClass, PieceColor color)
        {
            char result; 
            switch (pieceClass)
            {
                case PieceClass.King:
                    result = 'k';
                    break;
                case PieceClass.Queen:
                    result = 'q';
                    break;
                case PieceClass.Rook:
                    result = 'r';
                    break;
                case PieceClass.Bishop:
                    result = 'b';
                    break;
                case PieceClass.Knight:
                    result = 'n';
                    break;
                case PieceClass.Pawn:
                    result = 'p';
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (color == PieceColor.White)
            {
                result = Char.ToUpper(result);
            }
            return result;
        }

        /// <summary>
        /// Move a piece from startFile:startRank -> targetFile:targetRank.  Because
        /// self-play is the only mode enabled right now, these moves are always
        /// going to be considered valid, since they came from the chess engine
        /// (and we will assume it is correct).  In the future, this will likely
        /// remain, and validation of the legallity for the player can be handled
        /// above this call
        /// </summary>
        /// <param name="moveInfo">detailed move information struct</param>
        public void MovePiece(ref MoveInformation moveInfo)
        {
            // Get the player piece at the starting location
            // Piece should never be null if chess logic is sound
            PieceFile startFile = moveInfo.Start.File;
            int startRank = moveInfo.Start.Rank;
            PieceFile targetFile = moveInfo.End.File;
            int targetRank = moveInfo.End.Rank;

            ChessPiece playerPiece = FindPieceAt(startFile, startRank);
            if (playerPiece.Color != activePlayer)
            {
                // This also should not be able to happen with correct game logic
                throw new InvalidOperationException();
            }

            // Get each side's pieces
            List<ChessPiece> playerPieces = ActivePlayerPieces;
            List<ChessPiece> opponentPieces = OpponentPlayerPieces;

            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // PRE-MOVE CHECKS

            // We have to detect castling.  It does not come across the wire as O-O or O-O-O
            // but rather as a regular move like e1g1.  Separate the detection from the move
            // of the rook
            bool isCastling = IsCastling(playerPiece, targetFile);

            // We also need to check for an en-passant capture if the pieces is a pawn
            if (playerPiece.Job == PieceClass.Pawn)
            {
                moveInfo.CapturedPiece = HandleEnPassant(startFile, startRank, targetFile, targetRank);
            }

            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // RAW MOVE(S)
            playerPiece.Move(targetFile, targetRank);
            if (isCastling)
            {
                PerformCastle(targetFile, ref moveInfo); // Also move the rook if needed
            }

            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // POST-MOVE CHECKS/UPDATES

            // For normal captures, just do a quick iteration of the opponent pieces
            // there are only 16 of these total in normal chess
            foreach (ChessPiece enemyPiece in opponentPieces)
            {
                if ((enemyPiece.Rank == targetRank) &&  // Enemy piece is located in
                    (enemyPiece.File == targetFile) &&  // the square we just moved to
                    !enemyPiece.Captured)               // and it's not already captured
                {
                    enemyPiece.Captured     = true;         // Stop drawing it (capture)
                    moveInfo.CapturedPiece  = enemyPiece;   // Record the capture
                    break;                                  // exit the search loop
                }
            }

            // save the last capture state for external callers
            lastMoveWasCapture = moveInfo.IsCapture;

            Moves.Add(moveInfo);

            // Update our FEN
            currentFEN = FenParser.ApplyMoveToFEN(currentFEN, moveInfo.ToString());
            enPassantValid = FenParser.ExtractEnPassantTarget(currentFEN, out enPassantTarget);

            FenParser.ExtractCastlingRights(CurrentFEN, ref whiteCastlingRights, ref blackCastlingRights);
            FenParser.ExtractMoveCounts(CurrentFEN, ref halfMoveCount, ref fullMoveCount);

            // Flip players - easy to just do here rather than parse the FEN again
            activePlayer = OppositeColor(activePlayer);
        }

        /// <summary>
        /// Returns the current en-passant target square (if any)
        /// </summary>
        /// <param name="target">BoardSquare that is the en-passant target</param>
        /// <returns>true if target exists, in which case 'target' contains the square.
        /// false if there is no target, and the contents of 'target' are invalid</returns>
        public bool GetEnPassantTarget(out BoardSquare target)
        {
            target = enPassantTarget;
            return enPassantValid;
        }

        /// <summary>
        /// Checks if a given player is allowed to castle
        /// </summary>
        /// <param name="playerColor">Player to check</param>
        /// <param name="side">Side of the board to validate.</param>
        /// <returns>True if the player can castle on the given side</returns>
        public bool CanPlayerCastle(PieceColor playerColor, BoardSide side)
        {
            BoardSide playerSide = (playerColor == PieceColor.White) ? whiteCastlingRights : blackCastlingRights;
            return playerSide.HasFlag(side);
        }

        /// <summary>
        /// When a pawn has made it to the back rank, it can be promoted.  This method
        /// will mark a piece as needing promotion on the next move.  We don't want to
        /// change the job until it has moved to keep inline with the rest of the logic
        /// </summary>
        /// <param name="startFile"></param>
        /// <param name="startRank"></param>
        /// <param name="targetFile"></param>
        /// <param name="targetRank"></param>
        /// <param name="promotionClass"></param>
        /// <param name="moveInfo">Detailed move info</param>
        public void PromotePiece(PieceFile startFile, int startRank, PieceFile targetFile, int targetRank, PieceClass promotionClass, ref MoveInformation moveInfo)
        {
            ChessPiece piece = FindPieceAt(startFile, startRank);
            int validRank = (piece.Color == PieceColor.White) ? 8 : 1;
            if (targetRank != validRank)
            { 
                throw new ArgumentOutOfRangeException();
            }
            
            // Find the pawn and mark it
            if (piece.Job != PieceClass.Pawn)
            {
                // Logic check
                throw new InvalidOperationException();
            }
            moveInfo.PromotionJob = promotionClass;
            piece.PromoteOnNextMove(promotionClass);
        }

        /// <summary>
        /// Returns a ChessPiece at a given file:rank.  It's very possible there
        /// is no piece and if so returns null
        /// </summary>
        /// <param name="file">ChessFile to check against</param>
        /// <param name="rank">Rank to check against</param>
        /// <returns>ChessPiece object at the given file:rank or null if not found</returns>
        public ChessPiece FindPieceAt(PieceFile file, int rank)
        {
            ChessPiece result = null;
            // Check each piece in the list (32 max)
            List<ChessPiece> pieces = AllPieces;
            foreach (ChessPiece piece in pieces)
            {
                // Should only ever be 1 in the list at any given location visible
                if (IsPieceAtLocation(piece, file, rank))
                {
                    result = piece; // Again we can stop on the 1st hit
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the King of the specified color
        /// should be in an active game
        /// </summary>
        /// <param name="kingColor"></param>
        /// <returns></returns>
        public ChessPiece GetKing(PieceColor kingColor)
        {
            List<ChessPiece> pieces = (kingColor == PieceColor.White) ? WhitePieces : BlackPieces;
            foreach (ChessPiece piece in pieces)
            {
                if (piece.Job == PieceClass.King)
                {
                    return piece;
                }
            }
            return null;
        }

        /// <summary>
        /// Reverts last move applied to the board
        /// </summary>
        public void RevertLastMove()
        {
            MoveInformation lastMove = Moves.Last();
            Moves.RemoveAt(Moves.Count()-1);

            ChessPiece lastPieceMoved = FindPieceAt(lastMove.End.File, lastMove.End.Rank);
            lastPieceMoved.Move(lastMove.Start.File, lastMove.Start.Rank);
            if (lastMove.FirstMove)
            {
                lastPieceMoved.Deployed = false; // Reset first move
            }

            // Undo captures
            lastMoveWasCapture = false;
            if (lastMove.IsCapture)
            {
                lastMove.CapturedPiece.Captured = false;
                lastMoveWasCapture = true;
            }

            if (lastMove.IsPromotion)
            {
                lastPieceMoved.Demote();
            }

            // Undo a castling move
            if (lastMove.IsCastle)
            {
                // Move the rook back as well
                ChessPiece rook = lastMove.CastlingRook;
                PieceFile castleTargetFile = lastMove.End.File;
                int rookRank = (rook.Color == PieceColor.White) ? 1 : 8;
                if (castleTargetFile.ToInt() == 7) // g->h
                {
                    rook.Move(new PieceFile('h'), rookRank);
                }
                else if (castleTargetFile.ToInt() == 3) // c->a
                {
                    rook.Move(new PieceFile('a'), rookRank);
                }
                else
                {
                    // It has to be one of the above if logic is correct
                    throw new IndexOutOfRangeException(); 
                }
                rook.Deployed = false;
            }

            // Reset castling rights
            ActivePlayerCastlingRights = lastMove.CastlingRights;

            // Flip players
            activePlayer = OppositeColor(activePlayer);

            // Set last FEN
            currentFEN = lastMove.PreviousFEN;
        }

        /// <summary>
        /// Returns the home rank of a pawn
        /// </summary>
        /// <param name="pawnColor">color matters</param>
        /// <returns>2 for white 7 for black</returns>
        public static int PawnHomeRank(PieceColor pawnColor)
        {
            return pawnColor == PieceColor.White ? 2 : 7;
        }

        /// <summary>
        /// Returns the home rank of a rook
        /// </summary>
        /// <param name="rookColor">Color matters</param>
        /// <returns>1 for white and 8 for black</returns>
        public static int RookHomeRank(PieceColor rookColor)
        {
            return rookColor == PieceColor.White ? 1 : 8;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reset internal fields
        /// </summary>
        private void Reset()
        {
            moveHistory = new List<MoveInformation>();
            whitePieces = new List<ChessPiece>();
            blackPieces = new List<ChessPiece>();
            activePlayer = PieceColor.White;
            whiteCastlingRights = BoardSide.King | BoardSide.Queen;
            blackCastlingRights = BoardSide.King | BoardSide.Queen;
        }
        
        /// <summary>
        /// Sets up the board using the given FEN string.  It's possible not all
        /// pieces are present.
        /// </summary>
        /// <param name="fen">FEN string that describes the position 
        /// (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)</param>
        private void CreateAndPlacePieces(string fen)
        {
            Reset();

            // String starts on the back rank on the A file then moves Left->Right
            // Top->Bottom
            string fenString = fen.Trim();

            activePlayer = FenParser.ExtractActivePlayer(fenString);
            FenParser.ExtractCastlingRights(fenString, ref whiteCastlingRights, ref blackCastlingRights);
            enPassantValid = FenParser.ExtractEnPassantTarget(fenString, out enPassantTarget);
            FenParser.ExtractMoveCounts(fenString, ref halfMoveCount, ref fullMoveCount);

            List<ChessPiece> pieces = FenParser.ExtractPieces(fenString);
            foreach (ChessPiece fenPiece in pieces)
            {
                // For pawns and rooks and kings, deployment matters, check if they're on their home
                // rank/square and if not, set it to true
                if (fenPiece.Job == PieceClass.Pawn)
                {
                    if (PawnHomeRank(fenPiece.Color) != fenPiece.Rank)
                    {
                        fenPiece.Deployed = true;
                    }
                }
                else if (fenPiece.Job == PieceClass.Rook)
                {
                    if (((fenPiece.File.ToInt() != 1) && (fenPiece.File.ToInt() != 8)) ||
                        (fenPiece.Rank != RookHomeRank(fenPiece.Color)))
                    {
                        fenPiece.Deployed = true;
                    }
                }
                else if (fenPiece.Job == PieceClass.King)
                {
                    // Both colors should be on the E file and their home rank
                    int homeRank = (fenPiece.Color == PieceColor.White) ? 1 : 8;
                    if ((fenPiece.Rank != homeRank) || (fenPiece.File != new PieceFile('e')))
                    {
                        fenPiece.Deployed = true;
                    }
                }
                
                if (fenPiece.Color == PieceColor.White)
                {
                    whitePieces.Add(fenPiece);
                }
                else
                {
                    blackPieces.Add(fenPiece);
                }
            }
        }

        /// <summary>
        /// Returns true if the given ChessPiece is located at the given file:rank
        /// and that piece is not captured (must be visible)
        /// </summary>
        /// <param name="piece">ChessPiece object to check</param>
        /// <param name="file">ChessFile to check against</param>
        /// <param name="rank">Rank to check against</param>
        /// <returns>true if piece is found</returns>
        private bool IsPieceAtLocation(ChessPiece piece, PieceFile file, int rank)
        {
            return (!piece.Captured && (piece.Rank == rank) && (piece.File == file));
        }

        /// <summary>
        /// Returns true if any ChessPiece is located at the given file:rank
        /// </summary>
        /// <param name="file">ChessFile to check against</param>
        /// <param name="rank">Rank to check against</param>
        /// <returns>true if any piece is found</returns>
        private bool IsAnyPieceAtLocation(PieceFile file, int rank)
        {
            bool result = false;

            // Check each piece in the list (32 max)
            List<ChessPiece> pieces = AllPieces;
            foreach (ChessPiece piece in pieces)
            {
                // Only 1 piece in the list should ever return true for this
                // if the chess logic is sound (non-visible or captured pieces
                // will still be in the list though) so we can stop on the 1st hit
                if (IsPieceAtLocation(piece, file, rank))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// For pawn moves, this check if the move was "en-passant" (in-passing) and if
        /// so, it will remove the captured piece.  It is assumed the move being
        /// checked is for a pawn
        /// </summary>
        /// <param name="startFile">ChessFile the piece originated from</param>
        /// <param name="startRank">Rank the piece originated from </param>
        /// <param name="targetFile">ChessFile the piece is moving to</param>
        /// <param name="targetRank">Rank the piece is moving to</param>
        /// <returns>captured piece or null</returns>
        private ChessPiece HandleEnPassant(PieceFile startFile, int startRank, PieceFile targetFile, int targetRank)
        {
            ChessPiece enPassantVictim = null;
            if ((startFile != targetFile) &&                      // Diagonal move
                (!IsAnyPieceAtLocation(targetFile, targetRank)))  // There is some piece behind us
            {
                int enPassantTargetRank = startRank;  // Our potential target rank

                // This should never return a null object if the chess logic around it is sound
                // as a diagonal move (already detected) is not possible otherwise for a pawn
                // that did not make a capture (already detected)
                enPassantVictim = FindPieceAt(targetFile, enPassantTargetRank);

                // Capture the piece
                enPassantVictim.Captured = true;
            }
            return enPassantVictim;
        }

        /// <summary>
        /// Returns true if the current piece is trying to castle.  It must be 
        /// a king that moved in the correct manner (2 squares horizontally)
        /// </summary>
        /// <param name="piece">ChessPiece to check</param>
        /// <param name="targetFile">ChessFile the piece is moving to</param>
        /// <returns>True if the move is a castling move</returns>
        private bool IsCastling(ChessPiece piece, PieceFile targetFile)
        {
            bool result = false;
            int deltaSquares = Math.Abs(piece.File.ToInt() - targetFile.ToInt());
            if ((piece.Job == PieceClass.King) && (deltaSquares == 2))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Moves the rook for a castle move of a King.  At this point the castling
        /// has (or should have) been detected.  This is called to bring the rook
        /// along for the ride when the king is moved. See IsCastling for detection
        /// </summary>
        /// <param name="targetFile">ChessFile the King is moving to</param>
        /// <param name="moveInfo">Extended move information</param>
        private void PerformCastle(PieceFile targetFile, ref MoveInformation moveInfo)
        {
            // Find the corresponding Rook and move it too
            int rookRank = (activePlayer == PieceColor.White) ? 1 : 8;
            PieceFile gTargetFile = new PieceFile('g');
            PieceFile cTargetFile = new PieceFile('c');
            PieceFile rookStartFile;
            PieceFile rookTargetFile;

            // These are the only 2 legal files to move a king when castling
            // and they are the same for both players (only the rank differs above)
            if (targetFile == gTargetFile)
            {
                rookStartFile = new PieceFile('h');
                rookTargetFile = new PieceFile('f');
            }
            else if (targetFile == cTargetFile)
            {
                rookStartFile = new PieceFile('a');
                rookTargetFile = new PieceFile('d');
            }
            else
            {
                // If the above chess logic was sound, this should not happen.
                throw new ArgumentOutOfRangeException();
            }

            // Get the rook (which should exist if logic is sound)
            ChessPiece castleRook = FindPieceAt(rookStartFile, rookRank);

            // Move it
            castleRook.Move(rookTargetFile, rookRank);

            // Save the castling info
            moveInfo.CastlingRook = castleRook;

            // Remove all castling rights for the active player
            if (activePlayer == PieceColor.White)
            {
                whiteCastlingRights = BoardSide.None;
            }
            else
            {
                blackCastlingRights = BoardSide.None;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the last recorded move was a capturing move
        /// </summary>
        public bool LastMoveWasCapture { get { return lastMoveWasCapture; } }

        /// <summary>
        /// Orientation of the board (who is on bottom?)
        /// </summary>
        public BoardOrientation Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        /// <summary>
        /// List of moves in SAN (standard algebraic notation)
        /// </summary>
        public List<MoveInformation> Moves { get { return moveHistory; } }

        /// <summary>
        /// Set of active white pieces
        /// </summary>
        public List<ChessPiece> WhitePieces { get { return whitePieces; } }

        /// <summary>
        /// Set of active black pieces
        /// </summary>
        public List<ChessPiece> BlackPieces { get { return blackPieces; } }

        /// <summary>
        /// Current player's turn
        /// </summary>
        public PieceColor ActivePlayer { get { return activePlayer; } }

        /// <summary>
        /// Half moves since a pawn advance or capture
        /// </summary>
        public int HalfMoveCount { get { return halfMoveCount; } }

        /// <summary>
        /// Total full moves applied to the board
        /// </summary>
        public int FullMoveCount { get { return fullMoveCount; } }

        /// <summary>
        /// Returns white's current castling rights
        /// </summary>
        public BoardSide WhiteCastlingRights {  get { return whiteCastlingRights; } }

        /// <summary>
        /// Returns black's current castling rights
        /// </summary>
        public BoardSide BlackCastlingRights { get { return blackCastlingRights; } }

        /// <summary>
        /// Returns castling rights for active player
        /// </summary>
        public BoardSide ActivePlayerCastlingRights
        {
            get
            {
                if (activePlayer == PieceColor.White)
                {
                    return whiteCastlingRights;
                }
                else
                {
                    return blackCastlingRights;
                }
            }

            set
            {
                if (activePlayer == PieceColor.White)
                {
                    whiteCastlingRights = value; 
                }
                else
                {
                    blackCastlingRights = value;
                }
            }
        }
        
        /// <summary>
        /// Returns castling rights for opponent player
        /// </summary>
        public BoardSide OpponentPlayerCastlingRights
        {
            get
            {
                if (activePlayer == PieceColor.White)
                {
                    return blackCastlingRights;
                }
                else
                {
                    return whiteCastlingRights;
                }
            }

            set
            {
                if (activePlayer == PieceColor.White)
                {
                    blackCastlingRights = value;
                }
                else
                {
                    whiteCastlingRights = value;
                }
            }
        }

        /// <summary>
        /// Returns initial position
        /// </summary>
        public string InitialFEN { get { return initialFEN; } }

        /// <summary>
        /// Returns the current board FEN (updated every move)
        /// </summary>
        public string CurrentFEN { get { return currentFEN; } }

        /// <summary>
        /// Sets the data for the text during engine thinking time
        /// </summary>
        public string ThinkingText { get { return thinkingText;} set { thinkingText = value;} }
        #endregion

        #region Private Properties
        /// <summary>
        /// returns a list of all pieces for the board
        /// </summary>
        private List<ChessPiece> AllPieces
        {
            get
            {
                List<ChessPiece> pieces = new List<ChessPiece>();
                pieces.AddRange(WhitePieces);
                pieces.AddRange(BlackPieces);
                return pieces;
            }
        }

        /// <summary>
        /// Returns a list of pieces for the active player (flips on move)
        /// </summary>
        private List<ChessPiece> ActivePlayerPieces
        {
            get
            {
                if (activePlayer == PieceColor.White)
                {
                    return WhitePieces;
                }
                else
                {
                    return BlackPieces;
                }
            }
        }

        /// <summary>
        /// Returns a list of pieces for the opponent player (flips on move)
        /// </summary>
        private List<ChessPiece> OpponentPlayerPieces
        {
            get
            {
                if (activePlayer == PieceColor.White)
                {
                    return BlackPieces;
                }
                else
                {
                    return WhitePieces;
                }
            }
        }
        #endregion

        #region Public Fields
        /// <summary>
        /// FEN for the standard starting position
        /// </summary>
        public static String InitialFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        #endregion

        #region Private Fields
        private bool lastMoveWasCapture = false;
        private int halfMoveCount;
        private int fullMoveCount;
        private BoardOrientation orientation;
        private PieceColor activePlayer;
        private BoardSide whiteCastlingRights;
        private BoardSide blackCastlingRights;
        private List<MoveInformation> moveHistory;
        private List<ChessPiece> whitePieces;
        private List<ChessPiece> blackPieces;
        private bool enPassantValid;
        private BoardSquare enPassantTarget;
        private string initialFEN;
        private string currentFEN;
        private string thinkingText;
        #endregion
    }
}
