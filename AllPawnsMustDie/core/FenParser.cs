using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Stateles FEN Parser Helper Methods
    /// </summary>
    public sealed class FenParser
    {
        #region Public Static Methods
        /// <summary>
        /// Given a FEN, return a list of ChessPiece objects descibed by the FEN
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <returns>ChessPieces contained in the FEN description</returns>
        public static List<ChessPiece> ExtractPieces(string fen)
        {
            string[] fenTokens = TokenizeFEN(fen);
            // The first part describes the pieces...
            string fenString = fenTokens[0];
            // Create a new list for the pieces we'll find
            List<ChessPiece> pieces = new List<ChessPiece>();

            int currentRank = 8; // 8 - back rank for Black
            int currentFile = 1; // A
            int index = 0;

            // Token can be 1-8 characters long per rank plus separators, so not long
            while (index < fenString.Length)
            {
                char fenChar = fenString[index++]; // Save the current char

                if (Char.IsLetter(fenChar))
                {
                    // Uppercase is white, lower black
                    PieceColor color = Char.IsUpper(fenChar) ? PieceColor.White : PieceColor.Black;

                    // Create the new ChessPiece object at the given location
                    ChessPiece newPiece = new ChessPiece(color, 
                        ChessBoard.PieceClassFromFen(fenChar), new PieceFile(currentFile), currentRank);

                    // Need to update deployed for pawns not on their home ranks
                    if (newPiece.Job == PieceClass.Pawn)
                    {
                        int homeRank = (newPiece.Color == PieceColor.White) ? 2 : 7;
                        if (newPiece.Rank != homeRank)
                        {
                            newPiece.Deployed = true;
                        }
                    }

                    pieces.Add(newPiece); // Add it to our total list of pieces
                    currentFile++;
                }
                else if (Char.IsDigit(fenChar))
                {
                    // Digits represent empty squares and are RLE (run length encoded) like a bitmap
                    // advance File the amount of the spaces specified
                    currentFile += (Convert.ToUInt16(fenChar) - Convert.ToUInt16('0'));
                }
                else if (fenChar == '/')
                {
                    // The '/' denotes the end of a rank on the chess board
                    // decrement Rank and reset the File (like a newline)
                    currentRank--;
                    currentFile = 1;
                }
            }

            // Return all of the pieces found
            return pieces;
        }

        /// <summary>
        /// Extracts the active player from the FEN
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <returns>PieceColor of the active player (white or black)</returns>
        public static PieceColor ExtractActivePlayer(string fen)
        {
            string[] fenTokens = TokenizeFEN(fen);
            return (String.Compare(fenTokens[1], "w") == 0) ? PieceColor.White : PieceColor.Black;
        }

        /// <summary>
        /// Extracts just the enpassant target if any from the FEN
        /// </summary>
        /// <param name="fen">FEN string to parse</param>
        /// <param name="enPassantSquare">BoardSquare with the target if return is true</param>
        /// <returns>If true, enPassantSquare holds the target, otherwise it will be a1(never valid)</returns>
        public static bool ExtractEnPassantTarget(string fen, out BoardSquare enPassantSquare)
        {
            string[] fenTokens = TokenizeFEN(fen);
            bool result = false;
            string enpassant = fenTokens[3];
            enPassantSquare = new BoardSquare(new PieceFile('a'), 1);

            // '-' or a square like e5
            if (String.Compare(enpassant, "-") != 0)
            {
                result = true;
                enPassantSquare = new BoardSquare(new PieceFile(enpassant[0]), 
                    Convert.ToInt16(enpassant[1]) - Convert.ToUInt16('0'));
            }
            return result;
        }

        /// <summary>
        /// Extracts the castling rights from a given FEN
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <param name="whiteCastlingRights">White player's castling rights in enum form</param>
        /// <param name="blackCastlingRights">Black player's castling rights in enum form</param>
        public static void ExtractCastlingRights(string fen, ref BoardSide whiteCastlingRights, ref BoardSide blackCastlingRights)
        {
            string[] fenTokens = TokenizeFEN(fen);
            ParseCastlingRights(fenTokens[2], ref whiteCastlingRights, ref blackCastlingRights);
        }

        /// <summary>
        /// Extract the halfmove and fullmove counts from the given FEN
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <param name="halfMoves">On return will hold halfmoves from the FEN</param>
        /// <param name="fullMoves">On return will hold fullmoves from the FEN</param>
        public static void ExtractMoveCounts(string fen, ref int halfMoves, ref int fullMoves)
        {
            string[] fenTokens = TokenizeFEN(fen);
            halfMoves = Convert.ToInt16(fenTokens[4]);
            fullMoves = Convert.ToInt16(fenTokens[5]);
        }
        
        /// <summary>
        /// Produces a new FEN given a FEN and a move string
        /// </summary>
        /// <param name="fen">Starting FEN</param>
        /// <param name="sanMove">move e.g. e2e4 or d7c8q</param>
        /// <returns>Updated FEN for new position</returns>
        public static string ApplyMoveToFEN(string fen, string sanMove)
        {
            // Extract the original pieces of the FEN
            string[] fenTokens = TokenizeFEN(fen);

            // Extract start and end squares and promotion info
            BoardSquare startSquare = new BoardSquare(
                new PieceFile(sanMove[0]), (Convert.ToInt16(sanMove[1]) - Convert.ToInt16('0')));
            BoardSquare endSquare = new BoardSquare(
                new PieceFile(sanMove[2]), (Convert.ToInt16(sanMove[3]) - Convert.ToInt16('0')));
            bool isPromotion = (sanMove.Length == 5);

            PieceColor activePlayer = ExtractActivePlayer(fen);

            // token[2] is castling rights
            BoardSide whiteCastlingRights = BoardSide.None;
            BoardSide blackCastlingRights = BoardSide.None;
            FenParser.ParseCastlingRights(fenTokens[2], ref whiteCastlingRights, ref blackCastlingRights);

            // Get the current move counts
            int halfMoves = 0;
            int fullMoves = 0;
            ExtractMoveCounts(fen, ref halfMoves, ref fullMoves);

            // Needed for move count updates
            bool isCapture = false;
            bool isPawnMove = false;
            bool isNewEnPassantNeeded = false;

            // Expand the start and target ranks (one or both if different)
            fen = ExpandRank(fen, startSquare.Rank);
            if (startSquare.Rank != endSquare.Rank)
            {
                fen = ExpandRank(fen, endSquare.Rank);
            }

            // Get the piece moving
            char fenPiece = PieceAtBoardPosition(fen, startSquare.File.ToInt(), startSquare.Rank);
            PieceColor pieceColor = char.IsUpper(fenPiece) ? PieceColor.White : PieceColor.Black;
            if (isPromotion)
            {
                isPawnMove = true;
                fenPiece = (activePlayer == PieceColor.White) ?  char.ToUpper(sanMove[4]) : sanMove[4]; // Update the piece type
            }

            // target piece if any
            char fenTargetPiece = PieceAtBoardPosition(fen, endSquare.File.ToInt(), endSquare.Rank);

            // Common lambda for castling updates - used on rook moves and captures
            UpdateCastlingRightsOnEqualRank UpdateCastlingRightsIfNeeded = (rankA, rankB, targetSquare, color) =>
            {
                if (rankA == rankB)
                {
                    // A File?
                    if (targetSquare.File.ToInt() == 1)
                    {
                        if (color == PieceColor.White)
                        {
                            whiteCastlingRights &= ~BoardSide.Queen;
                        }
                        else
                        {
                            blackCastlingRights &= ~BoardSide.Queen;
                        }
                    }
                    // H File?
                    else if (targetSquare.File.ToInt() == 8)
                    {
                        if (color == PieceColor.White)
                        {
                            whiteCastlingRights &= ~BoardSide.King;
                        }
                        else
                        {
                            blackCastlingRights &= ~BoardSide.King;
                        }
                    }
                }
            };

            // generic capture
            // en-passant caught under pawn moves
            if (fenTargetPiece != '1')
            {
                isCapture = true;

                // If captured piece is enemy ROOK, that will potentially remove 
                // rights on the opponent's side (if at home) - you can't capture
                // the king, so no need to check there
                PieceColor opponentColor = ChessBoard.OppositeColor(activePlayer);
                UpdateCastlingRightsIfNeeded(ChessBoard.RookHomeRank(opponentColor), endSquare.Rank, endSquare, opponentColor);
            }

            // Check the type of piece moving
            if (Char.ToUpper(fenPiece) == 'P') // PAWN
            {
                isPawnMove = true; // resets halfmoves

                // en-passant only matters in pawn moves
                if (String.Compare(fenTokens[3], "-") != 0)
                {
                    // There is an en-passant square
                    BoardSquare enPassantSquare = new BoardSquare(
                        new PieceFile(fenTokens[3][0]), (Convert.ToInt16(fenTokens[3][1]) - Convert.ToInt16('0')));

                    // If the en-passant target is the move target, this is a capture
                    if (enPassantSquare == endSquare)
                    {
                        // en-passant capture - must also expand the rank 'behind' the target and
                        // remove that pawn - mark as capture
                        int captureRank = (pieceColor == PieceColor.White) ? endSquare.Rank - 1 : endSquare.Rank + 1;
                        char captured;
                        fen = FenParser.RemovePiece(fen, endSquare.File.ToInt(), captureRank, out captured);
                        if (captured == '1') { throw new InvalidOperationException(); }
                        isCapture = true;
                    }
                }
                else if (Math.Abs(endSquare.Rank - startSquare.Rank) == 2)
                {
                    // If there is an enemy pawn on either side of the endSquare,
                    // then we need to create an enpassant target
                    // rank is already expanded
                    char neighbor = '1';

                    if (endSquare.File.ToInt() > 1)
                    {
                        neighbor = PieceAtBoardPosition(fen, endSquare.File.ToInt() - 1, endSquare.Rank);
                        if (char.ToUpper(neighbor) == 'P' && (ColorFromFen(neighbor) != activePlayer))
                        {
                            isNewEnPassantNeeded = true;
                        }
                    }

                    if (endSquare.File.ToInt() < 8)
                    {
                        neighbor = PieceAtBoardPosition(fen, endSquare.File.ToInt() + 1, endSquare.Rank);
                        if (char.ToUpper(neighbor) == 'P' && (ColorFromFen(neighbor) != activePlayer))
                        {
                            isNewEnPassantNeeded = true;
                        }
                    }
                }
            }

            if (Char.ToUpper(fenPiece) == 'K') // KING
            {
                // Check if this is a castling move - only time king moves 2 squares
                if (Math.Abs(endSquare.File.ToInt() - startSquare.File.ToInt()) == 2)
                {
                    // Move the associated rook... already expanded the rank
                    int rookFileStart = endSquare.File.ToInt() == 7 ? 8 : 1;
                    int rookFileEnd = rookFileStart == 1 ? 4 : 6;

                    char rook;
                    fen = FenParser.RemovePiece(fen, rookFileStart, endSquare.Rank, out rook);
                    fen = FenParser.InsertPiece(fen, rook, rookFileEnd, endSquare.Rank);
                }

                // Moving the king removes all rights on all sides
                if (activePlayer == PieceColor.White)
                {
                    whiteCastlingRights = BoardSide.None;
                }
                else
                {
                    blackCastlingRights = BoardSide.None;
                }
            }
            if (Char.ToUpper(fenPiece) == 'R') // ROOK
            {
                // Check if at home position at start
                int homeRank = (pieceColor == PieceColor.White) ? 1 : 8;
                UpdateCastlingRightsIfNeeded(homeRank, startSquare.Rank, startSquare, activePlayer);
            }

            // Remove piece
            char fenChar;
            fen = RemovePiece(fen, startSquare.File.ToInt(), startSquare.Rank, out fenChar);
            if ((fenPiece != fenChar) && !isPromotion)
            {
                throw new InvalidOperationException();
            }

            // Place piece (it might not be the same type (promotions) - TODO
            fen = InsertPiece(fen, fenPiece, endSquare.File.ToInt(), endSquare.Rank);

            // Collapse the rows we touched
            fen = CollapseRank(fen, startSquare.Rank);
            if (startSquare.Rank != endSquare.Rank)
            {
                fen = CollapseRank(fen, endSquare.Rank);
            }

            // Re-tokenize
            fenTokens = TokenizeFEN(fen);

            // castling
            string wcr = TokenizeCastlingRights(whiteCastlingRights, PieceColor.White);
            string bcr = TokenizeCastlingRights(blackCastlingRights, PieceColor.Black);
            if ((whiteCastlingRights == BoardSide.None) && (blackCastlingRights == BoardSide.None))
            {
                fenTokens[2] = "-";
            }
            else
            {
                fenTokens[2] = String.Empty;
                if (whiteCastlingRights != BoardSide.None)
                {
                    fenTokens[2] = wcr;
                }

                if (blackCastlingRights != BoardSide.None)
                {
                    fenTokens[2] = String.Concat(fenTokens[2], bcr);
                }
            }
            
            // Update the other pieces of the FEN
            // active player
            fenTokens[1] = (activePlayer == PieceColor.White) ? "b" : "w";

            // Did we create a new en-passant target?
            if (isNewEnPassantNeeded)
            {
                // Target is behind pawn
                int enpassantTargetRank = (activePlayer == PieceColor.White) ? endSquare.Rank - 1 : endSquare.Rank + 1;
                BoardSquare ept = new BoardSquare(endSquare.File, enpassantTargetRank);
                fenTokens[3] = ept.ToString();
            }
            else
            {
                fenTokens[3] = "-";
            }

            // half moves
            halfMoves++;
            if (isCapture || isPawnMove)
            {
                halfMoves = 0;
            }
            fenTokens[4] = halfMoves.ToString();

            // full moves
            if (activePlayer == PieceColor.Black)
            {
                fullMoves++;
            }
            fenTokens[5] = fullMoves.ToString();

            fen = string.Join(" ", fenTokens);
            return fen;
        }

        /// <summary>
        /// Expands a given rank inside a FEN string to 8 characters.  Any numeric
        /// counts are expanded to all 1s.  E.g. /8/ => /11111111/
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <param name="rank">Rank to expand</param>
        /// <returns>Updated FEN</returns>
        public static string ExpandRank(string fen, int rank)
        {
            // First break FEN into its 6 parts
            string[] fenTokens = TokenizeFEN(fen);
            // Now split that first part into ranks
            string[] rankTokens = fenTokens[0].Split('/');
            // Now get the rank we care about (back rank [8] is first)
            string sourceRankString = rankTokens[8 - rank];
            
            StringBuilder sb = new StringBuilder();
            foreach (char ch in sourceRankString)
            {
                if (char.IsWhiteSpace(ch))
                {
                    // Should only happen on rank 1
                    if (rank != 1) { throw new InvalidOperationException(); }
                    break;
                }

                // Expand numbers
                if ((ch >= '1') && (ch <= '8'))
                {
                    int count = Convert.ToInt16(ch) - Convert.ToInt16('0');
                    sb.Append('1', count);
                }
                else // copy
                {
                    sb.Append(ch);
                }
            }

            // Piece the FEN back together
            rankTokens[8 - rank] = sb.ToString();
            fenTokens[0] = string.Join("/", rankTokens);
            fen = string.Join(" ", fenTokens);
            return fen;
        }

        /// <summary>
        /// Opposite of ExpandRank, it will take adjacent 1s and combine them
        /// e.g. /111P11PP/ => /3P2PP/
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <param name="rank">Rank to collapse</param>
        /// <returns>Updated FEN</returns>
        public static string CollapseRank(string fen, int rank)
        {
            // First break FEN into its 6 parts
            string[] fenTokens = TokenizeFEN(fen);
            // Now split that first part into ranks
            string[] rankTokens = fenTokens[0].Split('/');
            // Now get the rank we care about (back rank [8] is first)
            string sourceRankString = rankTokens[8 - rank];

            char[] sourceChars = sourceRankString.ToCharArray();
            StringBuilder sb = new StringBuilder();

            int count = 0;
            int countMax = 8;
            for (int index = 0; (index < sourceChars.Length) && (countMax > 0); index++)
            {
                if (char.IsDigit(sourceChars[index]))
                {
                    count++;
                }
                else if (count > 0)
                {
                    // Write out the count total
                    sb.Append( Convert.ToChar(count + '0'));
                    countMax -= count;
                    count = 0;
                    // also write out the non-digit
                    sb.Append(sourceChars[index]);
                    countMax--;
                }
                else
                {
                    // just a char, copy it
                    sb.Append(sourceChars[index]);
                    countMax--;
                }
            }

            if ((count == countMax) && (count != 0)) // ends in all spaces, append final count
            {
                sb.Append(Convert.ToChar(count + '0'));
            }
            
            // Piece the FEN back together
            rankTokens[8 - rank] = sb.ToString();
            fenTokens[0] = string.Join("/", rankTokens);
            fen = string.Join(" ", fenTokens);
            return fen;
        }

        /// <summary>
        /// Insert a piece into the FEN string at the specified location
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <param name="fenChar">FEN character to insert e.g. 'P' or 'q'</param>
        /// <param name="file">File to insert [1-8]==[a-h]</param>
        /// <param name="rank">Rank to insert [1-8]</param>
        /// <returns>Updated FEN string</returns>
        /// <remarks>Assumes target rank is already expaded</remarks>
        public static string InsertPiece(string fen, char fenChar, int file, int rank)
        {
            // Assumes rank is exanded
            if ((file < 1) || (file > 8) || (rank < 1) || (rank > 8))
            {
                throw new ArgumentOutOfRangeException();
            }
            // ranks are highest first in FEN, but strings are parsed left to right
            string[] rankTokens = fen.Split('/');
            string sourceRankString = rankTokens[8 - rank];

            char[] copy = sourceRankString.ToCharArray();
            copy[file - 1] = fenChar;
            rankTokens[8 - rank] = new string(copy);
            fen = string.Join("/", rankTokens);
            return fen;
        }

        /// <summary>
        /// Removes a piece at a given location on the board
        /// </summary>
        /// <param name="fen">FEN string input</param>
        /// <param name="file">[1-8] => [a-h]</param>
        /// <param name="rank">[1-8]</param>
        /// <param name="fenChar">piece removed(e.g. 'q' or 'Q')</param>
        /// <returns>Updated FEN</returns>
        /// <remarks>Assumes target rank is already expaded</remarks>
        public static string RemovePiece(string fen, int file, int rank, out char fenChar)
        {
            fenChar = PieceAtBoardPosition(fen, file, rank);

            // Now replace that piece with a '1'
            string[] rankTokens = fen.Split('/');
            string sourceRankString = rankTokens[8 - rank];
            char[] sourceChars = sourceRankString.ToCharArray();
            sourceChars[file-1] = '1';
            rankTokens[8 - rank] = new string(sourceChars);
            fen = string.Join("/", rankTokens);
            return fen;
        }

        /// <summary>
        /// Returns the FEN character for the piece at a given location.  This
        /// method assumes the proper rank has already been expanded
        /// </summary>
        /// <param name="fen">FEN string input</param>
        /// <param name="file">[1-8] => [a-h]</param>
        /// <param name="rank">[1-8]</param>
        /// <returns>The FEN char for the piece or '1' if no piece</returns>
        /// <remarks>Assumes target rank is already expaded</remarks>
        public static char PieceAtBoardPosition(string fen, int file, int rank)
        {
            // Assumes rank is exanded
            if ((file < 1) || (file > 8) || (rank < 1) || (rank > 8))
            {
                throw new ArgumentOutOfRangeException();
            }
            // ranks are highest first in FEN, but strings are parsed left to right
            string[] rankTokens = fen.Split('/');
            string sourceRankString = rankTokens[8 - rank];
            return sourceRankString[file-1];
        }

        /// <summary>
        /// Color check (a case check for FEN, white is UPPERCASE, black lowercase)
        /// </summary>
        /// <param name="fenPiece">'p' 'P' 'q', 'Q', etc</param>
        /// <returns>White or Black</returns>
        public static PieceColor ColorFromFen(char fenPiece)
        {
            return char.IsUpper(fenPiece) ? PieceColor.White : PieceColor.Black;
        }

        /// <summary>
        /// Convert state FEN to a text board a la StockFish's "d" command.
        /// </summary>
        /// <param name="fen">A valid FEN string</param>
        public static string[] FENToStrings(string fen)
        {
            // Helper to mimic stockfish's 'd' command for an arbitrary FEN
            // This is an empty board - it gets filled in with FEN pieces in
            // the appropriate squares (a1 on bottom-left - White position)
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            // *|   |   |   |   |   |   |   |   |
            // *+---+---+---+---+---+---+---+---+
            string boardEdge = "+---+---+---+---+---+---+---+---+\r\n";
            string boardEnd = "|";
            string boardSquaresFormat = " {0} |";
            string[] fenTokens = TokenizeFEN(fen);
            string boardPieces = fenTokens[0];

            // Split out each of the 8 rows in the first token
            string[] boardPiecesByRow = boardPieces.Split('/');

            List<string> output = new List<string>();

            // Draw the top edge
            output.Add(boardEdge);

            // Iterate on each row and print out either empty squares
            // or pieces, dictated by the FEN
            foreach (string boardRow in boardPiecesByRow)
            {
                StringBuilder sb = new StringBuilder(boardEnd);
                for (int charIndex = 0; charIndex < boardRow.Length; charIndex++)
                {
                    if (char.IsDigit(boardRow[charIndex]))
                    {
                        int emptySquareCount = (Convert.ToUInt16(boardRow[charIndex]) - Convert.ToUInt16('0'));
                        for (int i = 0; i < emptySquareCount; i++)
                        {
                            sb.AppendFormat(boardSquaresFormat, ' ');
                        }
                    }
                    else
                    {
                        sb.AppendFormat(boardSquaresFormat, boardRow[charIndex]);
                    }
                }
                output.Add(sb.Append("\r\n").ToString());
                output.Add(boardEdge);
            }

            return output.ToArray();
        }

#if DEBUG
        /// <summary>
        /// DEBUG only method to dump the state of the board given a FEN
        /// a la StockFish's "d" command.
        /// </summary>
        /// <param name="fen">A valid FEN string</param>
        public static void DebugFENPosition(string fen)
        {
            string[] lines = FENToStrings(fen);
            foreach (string line in lines)
            {
                Debug.Write(line);
            }
        }
#else
        /// <summary>
        /// DEBUG only method to dump the state of the board given a FEN
        /// a la StockFish's "d" command. Does nothing in Release
        /// </summary>
        /// <param name="fen">A valid FEN string</param>
        public static void DebugFENPosition(string fen)
        {
            // No OP
        }
#endif

        #endregion

        #region Private Static Methods
        /// <summary>
        /// Converts BoardSide enum castling rights into a FEN compliant string
        /// </summary>
        /// <param name="castlingRights">Rights to convert</param>
        /// <param name="color">Color (for casing)</param>
        /// <returns>FEN compliant string for the BoardSide, e.g. "KQ"</returns>
        private static string TokenizeCastlingRights(BoardSide castlingRights, PieceColor color)
        {
            string result = String.Empty;
            if (castlingRights.HasFlag(BoardSide.King) & castlingRights.HasFlag(BoardSide.Queen))
            {
                result = String.Concat(result, "kq");
            }
            else if (castlingRights.HasFlag(BoardSide.King))
            {
                result = String.Concat(result, "k");
            }
            else if (castlingRights.HasFlag(BoardSide.Queen))
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
        /// Parses a FEN castling rights token and converts to BoardSide enums
        /// for each player
        /// </summary>
        /// <param name="fenToken">KQkq, -, or some combination of the first, e.g. Kq</param>
        /// <param name="whiteCastlingRights">On return holds the castling rights for white player</param>
        /// <param name="blackCastlingRights">On return holds the castling rights for black player</param>
        private static void ParseCastlingRights(string fenToken, ref BoardSide whiteCastlingRights, ref BoardSide blackCastlingRights)
        {
            whiteCastlingRights = BoardSide.None;
            blackCastlingRights = BoardSide.None;

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
                            throw new ArgumentOutOfRangeException("fenToken", "Only valid choices are 'K' 'Q' 'k' or 'q'");
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Splits FEN into its 6 main pieces
        /// </summary>
        /// <param name="fen">FEN input</param>
        /// <returns>6 pieces of the FEN as a string array</returns>
        private static string[] TokenizeFEN(string fen)
        {
            // Tokens: 6 total if valid
            // {pieces} {active player} {CastlingRights} {EnPassantTarget} {halfmoves} {fullmoves}
            // The tokens in the FEN string are seratated by a space
            string[] tokens = fen.Split(' ');

            // For the FEN to be valid, it needs to contain all 6 tokens in a recognizable format
            if (tokens.Length != 6)
            {
                throw new ArgumentOutOfRangeException("fen", "Valid FEN strings contain 6 parts.");
            }
            return tokens;
        }

        private delegate void UpdateCastlingRightsOnEqualRank(int rankA, int rankB, BoardSquare targetSquare, PieceColor color);
#endregion
    }
}
