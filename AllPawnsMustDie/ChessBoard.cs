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
        #region Public Structs
        /// <summary>
        /// Wraps a board location
        /// </summary>
        public struct BoardSquare
        {
            /// <summary>
            /// Checks if a square is valid.  You cannot instantiate an invalid
            /// PieceFile object, so passing that form is meaningless, though
            /// the rank could be bad
            /// </summary>
            /// <param name="file">PieceFile.ToInt() most likely [1-8]</param>
            /// <param name="rank">[1-8]</param>
            /// <returns>true if the square is valid (located on the board)</returns>
            public static bool IsValid(int file, int rank)
            {
                return (((file <= 8) && (file > 0)) && 
                        ((rank <= 8) && (rank > 0)));
            }

            /// <summary>
            /// Save the file, rank for the location
            /// </summary>
            /// <param name="file">[a-h]</param>
            /// <param name="rank">[1-8]</param>
            public BoardSquare(PieceFile file, int rank)
            {
                pieceFile = file;
                pieceRank = rank;
            }

            /// <summary>
            /// Override for equality tests
            /// </summary>
            /// <param name="obj">object testing</param>
            /// <returns>true if obj is the same as this instance</returns>
            public override bool Equals(System.Object obj)
            {
                return (obj is BoardSquare) && (this == (BoardSquare)obj); ;
            }

            /// <summary>
            /// Override for equality tests
            /// </summary>
            /// <returns>hashcode for the object</returns>
            public override int GetHashCode()
            {
                return pieceFile.GetHashCode() | pieceRank;
            }
            
            /// <summary>
            /// Override for equality tests
            /// </summary>
            /// <param name="p1">object1</param>
            /// <param name="p2">object2</param>
            /// <returns>true if the value of object1 and object2 are the same</returns>
            public static bool operator ==(BoardSquare p1, BoardSquare p2)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(p1, p2))
                {
                    return true;
                }
                return ((p1.File == p2.File) && (p1.Rank == p2.Rank));
            }

            /// <summary>
            /// Override for equality tests
            /// </summary>
            /// <param name="p1">Object1</param>
            /// <param name="p2">Onject2</param>
            /// <returns>True if the values of Object1 and Object2 are NOT the same</returns>
            public static bool operator !=(BoardSquare p1, BoardSquare p2)
            {
                return !(p1 == p2);
            }

            /// <summary>
            /// Returns the File for the square
            /// </summary>
            public PieceFile File { get { return pieceFile; } }

            /// <summary>
            /// Returns the Rank for the square
            /// </summary>
            public int Rank { get { return pieceRank; } }

            private PieceFile pieceFile;
            private int pieceRank;
        }

        /// <summary>
        /// Holds more detailed information on a single move for the GUI.
        /// </summary>
        public struct MoveInformation
        {
            /// <summary>
            /// Create a new MoveInformation struct and fill in the minumum
            /// required information
            /// </summary>
            /// <param name="start">starting square</param>
            /// <param name="end">ending square</param>
            /// <param name="deployed">true if piece has ever moved, prior to this move</param>
            public MoveInformation(BoardSquare start, BoardSquare end, bool deployed)
            {
                ancillaryPiece = null;
                promotionClass = PieceClass.King; // Invalid promotion job
                startSquare = start;
                endSquare = end;
                firstMove = !deployed;
                color = PieceColor.White; // assume white
                castlingRights = BoardSide.None;
                isCapture = false;
                isCastle = false;
            }

            /// <summary>
            /// Return the move in a SAN friendly format
            /// </summary>
            /// <returns>SAN engine-friendly format e.g. e2e4 or d7d8q, etc</returns>
            public override string ToString()
            {
                string result = String.Concat(startSquare.File.ToString(), startSquare.Rank.ToString(), 
                    endSquare.File.ToString(), endSquare.Rank.ToString());
                if (IsPromotion)
                {
                    result = String.Concat(result, ChessGame.PieceClassToPromotionChar(promotionClass));
                }
                return result;
            }

            /// <summary>
            /// If not null, the piece that was captured on this move.
            /// </summary>
            public ChessPiece CapturedPiece
            {
                get
                {
                    if (!isCapture) { throw new InvalidOperationException(); }
                    return ancillaryPiece;
                }
                set
                {
                    if (value != null) // ignore nulls
                    {
                        if (isCastle) { throw new InvalidOperationException(); }
                        isCapture = true;
                        ancillaryPiece = value;
                    }
                }
            }

            /// <summary>
            /// If not null, the Rook associated with the castle
            /// </summary>
            public ChessPiece CastlingRook
            {
                get
                {
                    if (!isCastle) { throw new InvalidOperationException(); }
                    return ancillaryPiece;
                }
                set
                {
                    if (value != null) // ignore nulls
                    {
                        if (isCapture) { throw new InvalidOperationException(); }
                        if (value.Job != PieceClass.Rook) { throw new ArgumentException(); }
                        isCastle = true;
                        ancillaryPiece = value;
                    }
                }
            }

            /// <summary>
            /// Property for the promotion job
            /// </summary>
            public PieceClass PromotionJob
            {
                get { return promotionClass; }
                set { promotionClass = value; }
            }

            /// <summary>
            /// Color of the move
            /// </summary>
            public PieceColor Color
            {
                get { return color; }
                set { color = value; }
            }

            /// <summary>
            /// Castling rights prior to this move
            /// </summary>
            public BoardSide CastlingRights
            {
                get { return castlingRights; }
                set { castlingRights = value; }
            }

            /// <summary>Returns the start square for the move</summary>
            public BoardSquare Start { get { return startSquare; } }

            /// <summary>Returns the ending square for the move</summary>
            public BoardSquare End { get { return endSquare; } }

            /// <summary>True if this is the piece's first move</summary>
            public bool FirstMove { get { return firstMove; } }
            
            /// <summary>True if the move is a promotion move</summary>
            public bool IsPromotion { get { return (promotionClass != PieceClass.King); } }

            /// <summary>true if move was a capture</summary>
            public bool IsCapture { get { return isCapture; } }

            /// <summary>true if move was a castle</summary>
            public bool IsCastle { get { return isCastle; } }

            BoardSquare startSquare;
            BoardSquare endSquare;
            ChessPiece ancillaryPiece;
            PieceClass promotionClass;
            PieceColor color;
            BoardSide castlingRights;
            bool firstMove;
            bool isCapture;
            bool isCastle;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new chess board
        /// </summary>
        public ChessBoard()
        {
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
            CreateAndPlacePieces(fen);
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
        /// Calculates the FEN for the board as is and returns it
        /// </summary>
        /// <returns>FEN for the board</returns>
        public string CurrentPositionAsFEN()
        {
            return "TODO";
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
                    enemyPiece.Visible)                 // and it's not already captured
                {
                    enemyPiece.Visible      = false;        // Stop drawing it (capture)
                    moveInfo.CapturedPiece  = enemyPiece;   // Record the capture
                    break;                                  // exit the search loop
                }
            }

            // Update castling rights - moving the king ruins all castling forever
            if ((playerPiece.Job == PieceClass.King) && (moveInfo.FirstMove))
            {
                ActivePlayerCastlingRights = BoardSide.None;
            }
            // moving the rook removes the possibility on that side
            else if ((playerPiece.Job == PieceClass.Rook) && (moveInfo.FirstMove))
            {
                if (moveInfo.Start.File.ToInt() == 8)
                {
                    ActivePlayerCastlingRights &= ~BoardSide.King;
                }
                else if (moveInfo.Start.File.ToInt() == 1)
                {
                    ActivePlayerCastlingRights &= ~BoardSide.Queen;
                }
            }
            // Capturing the opponent's rook removes castling rights for them on that side
            else if ((moveInfo.IsCapture) && (moveInfo.CapturedPiece.Job == PieceClass.Rook))
            {
                if (moveInfo.Start.File.ToInt() == 8)
                {
                    OpponentPlayerCastlingRights &= ~BoardSide.King;
                }
                else if (moveInfo.Start.File.ToInt() == 1)
                {
                    OpponentPlayerCastlingRights &= ~BoardSide.Queen;
                }
            }

            // If it's Black's turn, then we just finished a full move
            if (activePlayer == PieceColor.Black)
            {
                fullMoveCount++;    // Increment our fullmove counter
            }

            // Update the Halfmove tracker.  This is used in the "draw by 50" rule
            // and tracks the number of halfmoves (single moves by either side)
            // since the capture of any piece, or the advancement of any pawn
            // The rationale is if neither of those things has happened in 50 moves
            // then no progress is being made and the game is drawn and the players
            // are too stubborn to admit it.
            halfMoveCount++;
            if ((moveInfo.IsCapture) || (playerPiece.Job == PieceClass.Pawn))
            {
                halfMoveCount = 0;
            }

            // Flip players
            activePlayer = (activePlayer == PieceColor.White) ? PieceColor.Black : PieceColor.White;

            // Reset this target flag for all pieces except the pawn, which handles it in HandleEnPassant
            if (playerPiece.Job != PieceClass.Pawn)
            {
                enPassantValid = false;
            }

            // save the last capture state for external callers
            lastMoveWasCapture = moveInfo.IsCapture;

            Moves.Add(moveInfo);
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
                lastMove.CapturedPiece.Visible = true;
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
            activePlayer = (activePlayer == PieceColor.White) ? PieceColor.Black : PieceColor.White;
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

            FenParser parser = new FenParser(fenString);

            activePlayer = parser.ActivePlayer;
            whiteCastlingRights =  parser.WhiteCastlingRights;
            blackCastlingRights = parser.BlackCastlingRights;
            enPassantValid = parser.IsEnPassantTargetValid;
            if (parser.IsEnPassantTargetValid)
            {
                enPassantTarget = parser.EnPassantTarget;
            }
            fullMoveCount = parser.FullMoves;
            halfMoveCount = parser.HalfMoves;
            
            foreach (ChessPiece fenPiece in parser.Pieces)
            {
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
            return (piece.Visible && (piece.Rank == rank) && (piece.File == file));
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
            enPassantValid = false;
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
                enPassantVictim.Visible = false;
            }
            else
            {
                // Check if we need to set the target en-passant square
                if (Math.Abs(startRank - targetRank) == 2)
                {
                    // Target is one space behind the pawn that just jumped 2 spaces
                    targetRank += (ActivePlayer == PieceColor.White) ? -1 : 1;
                    enPassantTarget = new BoardSquare(targetFile, targetRank);
                    enPassantValid = true;
                }
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
        public static String InitialFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";

        // Some interesting test positions for legal moves, etc
        //public static String InitialFENPosition = "r3kb1r/pp1npppp/3p4/2P5/4q3/8/P4P1P/3QK2R w KQkq - 0 0";
        //public static String InitialFENPosition = "3k1r2/8/4N3/2N5/8/8/3PP3/r2RK3 w KQkq - 0 0";
        //public static String InitialFENPosition = "r1b1kb1r/pppp1p1p/1q3np1/2n1p3/1BQ3N1/3N1B2/PPP3PP/R3K2R w KQkq - 0 0";
        //public static String InitialFENPosition = "2b1k2r/pppp1p1p/3r1np1/b1n1p3/2Q1q1N1/2B1NB2/PPP3PP/R3K2R w KQkq - 0 0";
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
        #endregion
    }
}
