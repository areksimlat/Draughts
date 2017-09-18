using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore
{
    public class MoveController
    {
        private MoveScenarios allMoveScenarios;
        private MoveScenarios currMoveScenarios;
        private MoveFinder moveFinder;
        private int currMoveIndex;


        public MoveController()
        {
            allMoveScenarios = null;
            currMoveScenarios = null;
            currMoveIndex = 0;

            moveFinder = new MoveFinder();
        }

        public List<MoveScenarios> getAllowPlayerMoves(Board board)
        {
            if (currMoveScenarios == null)
                return moveFinder.getAllowMoveScenarios(board, board.PlayerColor);

            List<MoveScenarios> list = new List<MoveScenarios>();
            list.Add(currMoveScenarios);

            return list;
        }

        public bool isAllowMove(Board board, Position movePos)
        {
            if (currMoveScenarios == null)
            {
                if (Board.isEmpty(board.getPiece(movePos)))
                    return false;

                List<MoveScenarios> allowScenarios = moveFinder.getAllowMoveScenarios(board, board.PlayerColor);

                foreach (MoveScenarios currScenario in allowScenarios)
                {
                    if (currScenario.getFromPosition().Equals(movePos))
                    {
                        return true;
                    }
                }

                return false;
            }

            MoveScenarios matchScenario;

            if (currMoveScenarios.isCapture())
            {
                matchScenario = currMoveScenarios.getMatchScenarios(movePos, currMoveIndex);
            }
            else
            {
                matchScenario = currMoveScenarios.getMatchScenarios(movePos);
            }

            return matchScenario.Count() > 0;
        }

        public bool isActiveMove(Position movePos)
        {
            if (currMoveScenarios == null)
                return false;

            return currMoveScenarios.Contains(movePos, currMoveIndex);
        }

        public Position getAllMovesInfo(Board board)
        {
            List<MoveScenarios> allowScenarios = moveFinder.getAllowMoveScenarios(board, board.PlayerColor);
            int countScenario = 0;

            if (allowScenarios != null && allowScenarios.Count > 0)
            {
                foreach (MoveScenarios currScenario in allowScenarios)
                {
                    countScenario += currScenario.Count();
                }

                if (allowScenarios[0].isCapture())
                {
                    return new Position(0, countScenario);
                }
                else
                {
                    return new Position(countScenario, 0);
                }
            }

            return new Position(0, 0);
        }

        public Position getCurrMovesInfo()
        {
            if (currMoveScenarios != null)
            {
                if (currMoveScenarios.isCapture())
                {
                    return new Position(0, currMoveScenarios.Count());
                }
                else
                {
                    return new Position(currMoveScenarios.Count(), 0);
                }
            }

            return null;
        }

        /*
         * Return true if move is end
         */
        public bool execPlayerMove(Board board, Position movePos)
        {
            if (currMoveScenarios == null)
            {
                allMoveScenarios = moveFinder.getMoveScenarios(board, movePos);
                currMoveScenarios = allMoveScenarios;
                currMoveIndex = 0;
            }
            else
            {
                currMoveScenarios = currMoveScenarios.getMatchScenarios(movePos, currMoveIndex);
                currMoveIndex++;

                if (currMoveScenarios.Count() == 1 && currMoveScenarios.getScenario(0).Count == currMoveIndex)
                {
                    execScenario(board, currMoveScenarios, 0);

                    currMoveScenarios = null;

                    return true;
                }                
            }
            
            return false;
        }

        public static void execScenario(Board board, MoveScenarios scenarios, int scenarioIndex)
        {
            List<Position> moves = new List<Position>();
            moves.Add(scenarios.getFromPosition());
            moves.AddRange(scenarios.getScenario(scenarioIndex));

            Position lastPosition = moves[moves.Count - 1];

            board.move(scenarios.getFromPosition(), lastPosition);

            if (board.isTurnToDame(lastPosition))
                board.turnToDame(lastPosition);

            Direction direction;
            Position currPos;

            for (int i = 0; i < moves.Count - 1; i++)
            {
                direction = Direction.getDirection(moves[i], moves[i + 1]);
                currPos = new Position(moves[i]);

                currPos.move(direction);

                while (!currPos.Equals(moves[i + 1]))
                {
                    if (!Board.isEmpty(board.getPiece(currPos)))
                    {
                        board.removePiece(currPos);
                    }

                    currPos.move(direction);
                }
            }

            board.switchCurrentColor();
        }

        public static void execMove(Board board, Position fromPos, Position toPos)
        {
            Position currPos = new Position(fromPos);
            Direction direction = Direction.getDirection(fromPos, toPos);

            currPos.move(direction);

            while (!currPos.Equals(toPos))
            {
                if (!Board.isEmpty(board.getPiece(currPos)))
                {
                    board.removePiece(currPos);
                }

                currPos.move(direction);
            }
        }

        public void deletePlayerMove(Position toMovePos)
        {
            if (currMoveScenarios == null)
                return;

            if (toMovePos.Equals(currMoveScenarios.getFromPosition()))
            {
                currMoveScenarios = null;
                currMoveIndex = 0;
            }
            else
            {
                int i = 0;

                foreach (Position p in currMoveScenarios.getScenario(0))
                {
                    if (!p.Equals(toMovePos))
                        break;

                    i++;
                }

                currMoveIndex = currMoveIndex - i - 1;

                if (currMoveIndex == -1)
                {
                    currMoveScenarios = allMoveScenarios;
                    currMoveIndex = 0;
                }
                else
                {
                    Position currPos = currMoveScenarios.getScenario(0)[currMoveIndex];
                    currMoveScenarios = allMoveScenarios.getMatchScenarios(currPos, currMoveIndex);
                }
            }
        }
    }
}
