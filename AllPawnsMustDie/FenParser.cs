using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Helper class to parse and extract information from a FEN string
    /// </summary>
    public sealed class FenParser
    {
        #region Public Methods
        /// <summary>
        /// Construct and parse the given string
        /// </summary>
        /// <param name="fenInputString">FEN input</param>
        public FenParser(string fenInputString)
        {
            pieces = new List<ChessPiece>();
            activePlayer = PieceColor.White;
            enPassantValid = false;
            whiteCastlingRights = BoardSide.None;
            blackCastlingRights = BoardSide.None;
            fenInput = fenInputString;
            fenOutput = fenInput; // No difference here
            Parse();
        }

        /// <summary>
        /// Construct a FEN string given an input ChessBoard
        /// </summary>
        /// <param name="chessBoard"></param>
        public FenParser(ChessBoard chessBoard)
        {
            pieces = chessBoard.WhitePieces;
            pieces.AddRange(chessBoard.BlackPieces);
            // remove any non-visible pieces from the board data
            pieces.RemoveAll(p => { return !p.Visible; });
            activePlayer = chessBoard.ActivePlayer;
            enPassantValid = chessBoard.GetEnPassantTarget(out enPassantTarget);
            whiteCastlingRights = chessBoard.WhiteCastlingRights;
            blackCastlingRights = chessBoard.BlackCastlingRights;
            fullMoves = chessBoard.FullMoveCount;
            halfMoves = chessBoard.HalfMoveCount;
            fenInput = null;

            CalculateFEN();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Active player specified in the FEN
        /// </summary>
        public PieceColor ActivePlayer { get { return activePlayer; } }

        /// <summary>
        /// FEN string from a board construction, or just the echoed input
        /// </summary>
        public string FEN { get { return fenOutput; } }

        /// <summary>
        /// Returns the pieces specified in the FEN
        /// </summary>
        public List<ChessPiece> Pieces { get { return pieces; } }

        /// <summary>
        /// Castling rights if any for white
        /// </summary>
        public BoardSide WhiteCastlingRights { get { return whiteCastlingRights; } }

        /// <summary>
        /// Castling rights if any for black
        /// </summary>
        public BoardSide BlackCastlingRights { get { return blackCastlingRights; } }

        /// <summary>
        /// If true, the EnPassantTarget should be looked at
        /// </summary>
        public bool IsEnPassantTargetValid { get { return enPassantValid; } }

        /// <summary>
        /// Target for EnPassant, if IsEnPassantTargetValid is true
        /// </summary>
        public ChessBoard.BoardSquare EnPassantTarget {  get { return enPassantTarget; } }

        /// <summary>
        /// Number of half moves, or moves since any capture or pawn advance
        /// </summary>
        public int HalfMoves { get { return halfMoves; } }

        /// <summary>
        /// Number of total full moves so far (each player has moved is 1 full turn)
        /// </summary>
        public int FullMoves { get { return fullMoves; } }
        #endregion

        #region Private Methods
        /// <summary>
        /// Parse the input string.
        /// </summary>
        private void Parse()
        {
            // Tokens: 6 total if valid
            // {pieces} {active player} {CastlingRights} {EnPassantTarget} {halfmoves} {fullmoves}
            // The tokens in the FEN string are seratated by a space
            string[] tokens = fenInput.Split(' ');

            // For the FEN to be valid, it needs to contain all 6 tokens in a recognizable format
            if (tokens.Length != 6)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Token 0 is the piece data
            CreatePieces(tokens[0]);

            // Token 1 is the active player
            if (String.Compare(tokens[1], "w") == 0)
            {
                activePlayer = PieceColor.White;
            }
            else if (String.Compare(tokens[1], "b") == 0)
            {
                activePlayer = PieceColor.Black;
            }
            else
            {
                // something invalid
                throw new ArgumentOutOfRangeException();
            }

            // Token 2 is the CastlingRights
            ParseCastlingRights(tokens[2]);

            // Token 3 is the EnPassant target or '-'
            ParseEnPassantTarget(tokens[3]);

            // Token 4 is the number of half moves
            halfMoves = Convert.ToInt16(tokens[4]);

            // Token 5 is the number of full moves
            fullMoves = Convert.ToInt16(tokens[5]);
        }

        /// <summary>
        /// Convert state into a FEN string
        /// </summary>
        private void CalculateFEN()
        {
            string fenPieces = TokenizePieces();

            // Castling rights
            string castlingRights = "";
            string whcastlingRights = TokenizeCastlingRights(WhiteCastlingRights, PieceColor.White);
            string blcastlingRights = TokenizeCastlingRights(BlackCastlingRights, PieceColor.Black);

            bool noWhiteCastlingRights = (String.Compare(whcastlingRights, "-") == 0);
            bool noBlackCastlingRights = (String.Compare(blcastlingRights, "-") == 0);

            if (noWhiteCastlingRights && noBlackCastlingRights)
            {
                castlingRights = "-";
            }
            else if (noWhiteCastlingRights)
            {
                castlingRights = blcastlingRights;
            }
            else if (noBlackCastlingRights)
            {
                castlingRights = whcastlingRights;
            }
            else
            {
                castlingRights = String.Concat(whcastlingRights, blcastlingRights);
            }

            // En-Passant target
            string enPassantOut = "-";
            if (enPassantValid)
            {
                enPassantOut = String.Format("{0}{1}", EnPassantTarget.File.ToString(), EnPassantTarget.Rank);
            }

            // Build final output
            fenOutput = String.Format("{0} {1} {2} {3} {4} {5}", fenPieces, (ActivePlayer == PieceColor.White) ? "w" : "b", castlingRights, enPassantOut, HalfMoves, FullMoves);
        }

        /// <summary>
        /// Builds a FEN string for the pieces data only (the first section of the FEN)
        /// </summary>
        /// <returns></returns>
        private string TokenizePieces()
        {
            // Sort the pieces in the order FEN wants
            // Place them highest rank and lowest file first
            // the pieces should read top->bottom, left->right
            // as the white player faces the board
            List<ChessPiece> chessPieces = Pieces;
            chessPieces.Sort((pieceA, pieceB) =>
            {
                int result = 0;
                // Prioritize Rank
                if (pieceA.Rank > pieceB.Rank)
                {
                    result = -1;
                }
                else if (pieceA.Rank < pieceB.Rank)
                {
                    result = 1;
                }
                else if (pieceA.Rank == pieceB.Rank)
                {
                    // Rank being equal - try the file
                    if (pieceA.File.ToInt() > pieceB.File.ToInt())
                    {
                        result = 1;
                    }
                    else if (pieceA.File.ToInt() < pieceB.File.ToInt())
                    {
                        result = -1;
                    }
                    else
                    {
                        // Both must be equal (and it shouldn't happen on a chessboard)
                        throw new InvalidOperationException();
                    }
                }
                return result;
            });

            string fenPieces = String.Empty;
            int pieceIndex = 0;
            int spaceCounter = 0;

            // Loop through all 64 squares back rank first as white is facing
            // and insert either blank space data (RLE counter) or the piece char
            // every spot is either empty, or the next piece in the sorted list
            // now that it is sorted in the same order
            for (int rankIndex = 8; rankIndex >= 1; rankIndex--)
            {
                for (int fileIndex = 1; fileIndex <= 8; fileIndex++)
                {
                    // Pieces remain, and the piece exists at this square
                    if ((pieceIndex < chessPieces.Count()) &&
                        (chessPieces[pieceIndex].File.ToInt() == fileIndex) &&
                        (chessPieces[pieceIndex].Rank == rankIndex))
                    {
                        if (spaceCounter > 0)
                        {
                            // Write out current empty space count
                            fenPieces = String.Concat(fenPieces, Convert.ToString(spaceCounter));
                            spaceCounter = 0;
                        }

                        // Append the piece character
                        fenPieces = String.Concat(fenPieces, ChessBoard.FenCharFromPieceClass(chessPieces[pieceIndex].Job, chessPieces[pieceIndex].Color));
                        pieceIndex++;
                    }
                    else
                    {
                        //Empty space, count it, no writing until we have the final total
                        spaceCounter++;
                    }
                }

                // Hit the end of a rank - if the counter is >0 it means we have blank spaces to record
                if (spaceCounter > 0)
                {
                    // Write out current empty space count
                    fenPieces = String.Concat(fenPieces, Convert.ToString(spaceCounter));
                    spaceCounter = 0;
                }
                
                // End of rank marker
                fenPieces = String.Concat(fenPieces, "/");
            }
            // Trim last '/' and return
            return fenPieces.Remove(fenPieces.Length - 1, 1);
        }

        /// <summary>
        /// Converts Boardside castling rights to a FEN string
        /// </summary>
        /// <param name="castlingRights">rights to check</param>
        /// <param name="color">Affects casing of string, white is uppercase</param>
        /// <returns>kq, k, q, or - (or uppercase versions)</returns>
        private string TokenizeCastlingRights(BoardSide castlingRights, PieceColor color)
        {
            string result = String.Empty;
            if (castlingRights.HasFlag(BoardSide.King) & castlingRights.HasFlag(BoardSide.Queen))
            {
                result = String.Concat(result, "kq");
            }
            else if (WhiteCastlingRights.HasFlag(BoardSide.King))
            {
                result = String.Concat(result, "k");
            }
            else if (WhiteCastlingRights.HasFlag(BoardSide.Queen))
            {
                result = String.Concat(result, "q");
            }
            else
            {
                result = "-";
            }

            if (color == PieceColor.White)
            {
                result = result.ToUpper();
            }

            return result;
        }

        /// <summary>
        /// Parse the enpassant portion of the fen
        /// </summary>
        /// <param name="fenToken">'-' or square e.g. e5</param>
        private void ParseEnPassantTarget(string fenToken)
        {
            // '-' or a square like e5
            if (String.Compare(fenToken, "-") == 0)
            {
                enPassantValid = false;
            }
            else
            {
                enPassantValid = true;
                enPassantTarget = new ChessBoard.BoardSquare(new PieceFile(fenToken[0]), Convert.ToInt16(fenToken[1]) - Convert.ToUInt16('0'));
            }
        }

        /// <summary>
        /// Parse the castling rights portion of the fen
        /// </summary>
        /// <param name="fenToken">KQkq or '-'</param>
        private void ParseCastlingRights(string fenToken)
        {
            // Inited to None, so skip unless something specified
            if (String.Compare(fenToken, "-") != 0)
            {
                int index = 0;
                while (index < fenToken.Length)
                {
                    switch (fenToken[index])
                    {
                        case 'K':
                            whiteCastlingRights |= BoardSide.King;
                            break;
                        case 'Q':
                            whiteCastlingRights |= BoardSide.Queen;
                            break;
                        case 'k':
                            blackCastlingRights |= BoardSide.King;
                            break;
                        case 'q':
                            blackCastlingRights |= BoardSide.Queen;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Creates pieces described in the FEN
        /// </summary>
        private void CreatePieces(string fenToken)
        {
            string fenString = fenToken;

            int currentRank = 8; // 8 - back rank for Black
            int currentFile = 1; // A

            int index = 0;
            while (index < fenString.Length)
            {
                char fenChar = fenString[index++];

                if (Char.IsLetter(fenChar))
                {
                    PieceColor color = PieceColor.White;
                    // New piece
                    if (Char.IsLower(fenChar))
                    {
                        // Black
                        color = PieceColor.Black;
                    }

                    ChessPiece newPiece = new ChessPiece(color, ChessBoard.PieceClassFromFen(fenChar), new PieceFile(currentFile), currentRank);

                    // Need to update deployed for pawns not on their home ranks
                    if (newPiece.Job == PieceClass.Pawn)
                    {
                        int homeRank = (newPiece.Color == PieceColor.White) ? 2 : 7;
                        if (newPiece.Rank != homeRank)
                        {
                            newPiece.Deployed = true;
                        }
                    }

                    pieces.Add(newPiece);
                    currentFile++;
                }
                else if (Char.IsDigit(fenChar))
                {
                    // advance File the amount of the spaces
                    currentFile += (Convert.ToUInt16(fenChar) - Convert.ToUInt16('0'));
                }
                else if (fenChar == '/')
                {
                    // decrement Rank
                    currentRank--;
                    // reset File
                    currentFile = 1;
                }
            }
        }
        #endregion

        #region Private Fields
        private List<ChessPiece> pieces;
        private PieceColor activePlayer;
        private BoardSide whiteCastlingRights;
        private BoardSide blackCastlingRights;
        private bool enPassantValid;
        private ChessBoard.BoardSquare enPassantTarget;
        private int halfMoves;
        private int fullMoves;
        private string fenInput;
        private string fenOutput;
        #endregion
    }
}
