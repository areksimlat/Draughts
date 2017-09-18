using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace GameCore
{
    public class Game
    {
        private Board board;
        private MoveController moveCtrl;
        private bool areAllowMoves;
        private MiniMax miniMax;
        private int maxDepth;

        private CountdownTimer countdownTimer;
        private int moveTimeInSec;

        public Game(GameSettings gameSettings)
        {
            this.maxDepth = gameSettings.Depth;
            areAllowMoves = true;

            board = new Board(gameSettings.International, gameSettings.PieceColor);
            moveCtrl = new MoveController();
            miniMax = new MiniMax(maxDepth);

            this.moveTimeInSec = gameSettings.MoveTimeInSec;

            countdownTimer = new CountdownTimer();
        }

        public void init()
        {
            board.initPieces();
        }

        public int getMoveTime()
        {
            return moveTimeInSec;
        }

        public CountdownTimer getCountdownTimer()
        {
            return countdownTimer;
        }

        public int getBoardSize()
        {
            return board.Size;
        }

        public Board getBoard()
        {
            return board;
        }

        public int getPiece(Position pos)
        {
            return board.getPiece(pos);
        }

        public int getMaxDepth()
        {
            return maxDepth;
        }

        public int getWhitePiecesCount()
        {
            if (board.PlayerColor == Board.WHITE)
                return board.PlayerPiecesCount + board.PlayerDameCount;

            return board.EnemyPiecesCount + board.EnemyDameCount;
        }

        public int getBlackPiecesCount()
        {
            if (board.PlayerColor == Board.BLACK)
                return board.PlayerPiecesCount + board.PlayerDameCount;

            return board.EnemyPiecesCount + board.EnemyDameCount;
        }

        public Position getAllMovesInfo()
        {
            return moveCtrl.getAllMovesInfo(board);
        }

        public Position getCurrMovesInfo()
        {
            return moveCtrl.getCurrMovesInfo();
        }

        public bool isPlayerTurn()
        {
            return board.CurrentColor == board.PlayerColor;
        }

        public bool isEndGame()
        {
            return board.PlayerPiecesCount + board.PlayerDameCount == 0 ||
                    board.EnemyPiecesCount + board.EnemyDameCount == 0 ||
                        getAllowPlayerMoves().Count == 0 ||
                            !areAllowMoves;
        }

        public bool isPlayerWin()
        {
            if (board.PlayerPiecesCount + board.PlayerDameCount > board.EnemyPiecesCount + board.EnemyDameCount)
                return true;

            return false;
        }

        public bool isFieldUsed(Position fieldPos)
        {
            if (board.PlayerColor != board.CurrentColor)
                return false;

            return moveCtrl.isActiveMove(fieldPos);
        }

        public bool isFieldAllow(Position fieldPos)
        {
            if (board.PlayerColor != board.CurrentColor) 
                return false;

            return moveCtrl.isAllowMove(board, fieldPos);
        }

        public List<MoveScenarios> getAllowPlayerMoves()
        {
            return moveCtrl.getAllowPlayerMoves(board);
        }

        public List<Position> getPlayerPiecesPosition()
        {
            return board.getPiecePositions(board.PlayerColor);
        }

        public bool addMove(Position movePos)
        {
            if (board.PlayerColor != board.CurrentColor)
                return false;

            areAllowMoves = true;

            return moveCtrl.execPlayerMove(board, movePos);
        }

        public void deleteMoves(Position movePos)
        {
            moveCtrl.deletePlayerMove(movePos);
        }

        public void execMove(MoveScenarios bestScenario)
        {
            if (bestScenario != null)
            {
                MoveController.execScenario(board, bestScenario, 0);
            }
            else
            {
                areAllowMoves = false;
            }
        }

        public void execRandomMove()
        {
            List<MoveScenarios> moves = getAllowPlayerMoves();
            MoveScenarios moveScenario = moves[new Random().Next(0, moves.Count)];
            execMove(moveScenario);
        }
    }
}
