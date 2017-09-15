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
    sealed class FenParser
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
            activePlayer = chessBoard.ActivePlayer;
            enPassantValid = chessBoard.GetEnPassantTarget(out enPassantTarget);
            whiteCastlingRights = chessBoard.WhiteCastlingRights;
            blackCastlingRights = chessBoard.BlackCastlingRights;
            fenInput = null;

            // TODO - convert all this into a string
            // ...
        }

        #endregion

        #region Public Properties
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
