using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameCore
{
    [DataContract]
    public class Board
    {
        public static readonly byte EMPTY = 0;
        public static readonly byte DAME = 1;
        public static readonly byte WHITE = 2;
        public static readonly byte BLACK = 4;

        [DataMember]
        public byte Size { get; private set; }

        [DataMember]
        public byte PlayerPiecesCount { get; private set; }

        [DataMember]
        public byte EnemyPiecesCount { get; private set; }

        [DataMember]
        public byte PlayerDameCount { get; private set; }

        [DataMember]
        public byte EnemyDameCount { get; private set; }

        [DataMember]
        public byte PlayerColor { get; private set; }

        [DataMember]
        public byte EnemyColor { get; private set; }

        [DataMember]
        public byte CurrentColor { get; private set; }

        [DataMember]
        public int[] Pieces { get; private set; }



        public Board(bool international, int playerColor)
        {
            if (international)
            {
                Size = 10;
                PlayerPiecesCount = 20;
                EnemyPiecesCount = 20;
            }
            else
            {
                Size = 8;
                PlayerPiecesCount = 12;
                EnemyPiecesCount = 12;
            }

            if (playerColor == WHITE)
            {
                this.PlayerColor = Board.WHITE;
                this.EnemyColor = Board.BLACK;
            }
            else
            {
                this.PlayerColor = Board.BLACK;
                this.EnemyColor = Board.WHITE;
            }
            
            this.PlayerDameCount = 0;
            this.EnemyDameCount = 0;
            this.CurrentColor = Board.WHITE;

            this.Pieces = new int[Size];
        }

        public Board(Board board)
        {
            this.Size = board.Size;
            this.PlayerPiecesCount = board.PlayerPiecesCount;
            this.EnemyPiecesCount = board.EnemyPiecesCount;
            this.PlayerColor = board.PlayerColor;
            this.EnemyColor = board.EnemyColor;
            this.PlayerDameCount = board.PlayerDameCount;
            this.EnemyDameCount = board.EnemyDameCount;
            this.CurrentColor = board.CurrentColor;
            this.Pieces = new int[board.Size];

            Array.Copy(board.Pieces, this.Pieces, board.Size);
        }

        public void initPieces()
        {
            Position pos = new Position();

            int rows = PlayerPiecesCount / (Size / 2);
            bool currIsBlack = false;
            int currColor;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (currIsBlack)
                    {
                        pos.set(row, col);
                        currColor = EnemyColor;
                    }
                    else
                    {
                        pos.set(Size - row - 1, col);
                        currColor = PlayerColor;  
                    }

                    setPiece(pos, currColor);
                    currIsBlack = !currIsBlack;
                }
                currIsBlack = !currIsBlack;
            }
        }

        public int getPiece(Position pos)
        {
            return (Pieces[pos.Row] >> (pos.Col + pos.Col + pos.Col)) & 7;
        }

        public void setPiece(Position pos, int piece)
        {
            Pieces[pos.Row] |= (piece << (pos.Col + pos.Col + pos.Col));
        }

        private void clearPiece(Position pos)
        {
            Pieces[pos.Row] &= ~(7 << (pos.Col + pos.Col + pos.Col));
        }

        public void removePiece(Position pos)
        {
            int piece = getPiece(pos);

            if (Board.matchPieceColor(piece, PlayerColor))
            {
                if (Board.isDame(piece))
                {
                    PlayerDameCount--;
                }
                else
                {
                    PlayerPiecesCount--;
                }
            }
            else
            {
                if (Board.isDame(piece))
                {
                    EnemyDameCount--;
                }
                else
                {
                    EnemyPiecesCount--;
                }
            }

            clearPiece(pos);
        }

        public static bool isEmpty(int piece)
        {
            return (piece | Board.EMPTY) == 0;
        }

        public static bool isWhite(int piece)
        {
            return (piece & Board.WHITE) != 0;
        }

        public static bool isBlack(int piece)
        {
            return (piece & Board.BLACK) != 0;
        }

        public static bool isDame(int piece)
        {
            return (piece & Board.DAME) != 0;
        }

        private void setDame(Position pos)
        {
            Pieces[pos.Row] |= (Board.DAME << (pos.Col + pos.Col + pos.Col));
        }

        public void turnToDame(Position pos)
        {
            setDame(pos);

            if (Board.matchPieceColor(getPiece(pos), PlayerColor))
            {
                PlayerDameCount++;
                PlayerPiecesCount--;
            }
            else
            {
                EnemyDameCount++;
                EnemyPiecesCount--;
            }
        }

        public bool isTurnToDame(Position piecePos)
        {
            int piece = getPiece(piecePos);

            if (isDame(piece))
                return false;

            if (Board.matchPieceColor(piece, PlayerColor))
            {
                return piecePos.Row == 0;
            }
            else
            {
                return piecePos.Row == Size - 1;
            }
        }

        public void switchCurrentColor()
        {
            CurrentColor = (CurrentColor == Board.WHITE) ? Board.BLACK : Board.WHITE;
        }

        public void setCurrentColor(int color)
        {
            CurrentColor = (byte) color;
        }

        public void move(Position fromPos, Position toPos)
        {
            setPiece(toPos, getPiece(fromPos));
            clearPiece(fromPos);
        }

        public List<Position> getPiecePositions(int pieceColor)
        {
            List<Position> piecePositions = new List<Position>();

            int currPiece;
            int pieceCount = 0;
            int allPiece;

            if (Board.matchPieceColor(pieceColor, PlayerColor))
            {
                allPiece = PlayerPiecesCount + PlayerDameCount;
            }
            else
            {
                allPiece = EnemyPiecesCount + EnemyDameCount;
            }

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Position currPos = new Position(row, col);
                    currPiece = getPiece(currPos);

                    if (!Board.isEmpty(currPiece))
                    {
                        if (Board.matchPieceColor(currPiece, pieceColor))
                        {
                            piecePositions.Add(currPos);
                            pieceCount++;

                            if (pieceCount == allPiece)
                                return piecePositions;
                        }
                    }
                }
            }

            return piecePositions;
        }

        public static bool matchPieceColor(int piece1, int piece2)
        {
            return ((piece1 & 6) == (piece2 & 6));
        }

        public bool arePieces()
        {
            return (PlayerPiecesCount + PlayerDameCount > 0) && 
                (EnemyPiecesCount + EnemyDameCount > 0);
        }
    }
}
