using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore
{
    public class MoveFinder
    {

        private class MoveOptions
        {
            public Board CurrentBoard { get; set; }
            public Position CurrentPos { get; set; }
            public List<MoveOptions> AllMoveOptPos { get; set; }

            public MoveOptions(Board currBoard, Position fromPos)
            {
                CurrentBoard = currBoard;
                CurrentPos = fromPos;
                AllMoveOptPos = new List<MoveOptions>();
            }

            public void Add(MoveOptions moveOpt)
            {
                AllMoveOptPos.Add(moveOpt);
            }
        }

        private class MoveOptionsContext
        {
            public bool IsVisited { get; set; }
            public MoveOptions CurrMoveOpt { get; set; }

            public MoveOptionsContext(MoveOptions moveOptions)
            {
                CurrMoveOpt = moveOptions;
                IsVisited = false;
            }
        }

        
        public MoveFinder()
        {

        }

        private List<Position> getEmptyPlaces(Board board, Direction direction, Position fromPosition, int maxOffset)
        {
            List<Position> emptyPlaces = new List<Position>();
            Position currPos = new Position(fromPosition);
            int currMoveOffset = 0;

            while (currMoveOffset < maxOffset)
            {
                currPos.move(direction);

                if (!currPos.isInRange(0, board.Size))
                    break;

                if (Board.isEmpty(board.getPiece(currPos)))
                {
                    emptyPlaces.Add(new Position(currPos));
                }
                else
                {
                    break;
                }

                currMoveOffset++;
            }

            return emptyPlaces;
        }

        private List<Position> getEmptyPlaces(Board board, Position fromPos, Position toPos)
        {
            List<Position> emptyPlaces = new List<Position>();
            Direction direction = Direction.getDirection(fromPos, toPos);
            Position currPos = new Position(fromPos);

            while (!currPos.Equals(toPos))
            {
                currPos.move(direction);

                if (Board.isEmpty(board.getPiece(currPos)))
                {
                    emptyPlaces.Add(new Position(currPos));
                }
            }

            return emptyPlaces;
        }

        private List<Position> findCapture(Board board, Direction direction, Position fromPosition, int piece)
        {
            List<Position> foundCapture = new List<Position>();

            Position currPos = new Position(fromPosition);
            bool pieceIsDame = Board.isDame(piece);
            int maxMoveOffset = pieceIsDame ? board.Size : 1;
            int currMoveOffset = 0;

            while (currMoveOffset < maxMoveOffset)
            {
                currPos.move(direction);

                if (!currPos.isInRange(0, board.Size))
                    break;

                if (!Board.isEmpty(board.getPiece(currPos)))
                {
                    if (!Board.matchPieceColor(board.getPiece(currPos), piece))
                    {
                        currPos.move(direction);

                        if (currPos.isInRange(0, board.Size) &&
                                Board.isEmpty(board.getPiece(currPos)))
                        {
                            foundCapture.Add(new Position(currPos));

                            if (pieceIsDame)
                            {
                                foundCapture.AddRange(getEmptyPlaces(board, direction, currPos, board.Size));
                            }

                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                currMoveOffset++;
            }

            return foundCapture;
        }

        private List<Position> findCaptures(Board board, Position fromPosition, int piece)
        {
            List<Position> foundCaptures = new List<Position>();

            foreach (Direction direction in Direction.getAll())
            {
                List<Position> capturePos = findCapture(board, direction, fromPosition, piece);

                if (capturePos != null)
                {
                    foundCaptures.AddRange(capturePos);
                }
            }

            return foundCaptures;
        }

        private List<Position> findMove(Board board, Direction direction, Position fromPosition, int piece)
        {
            int maxMoveOffset = Board.isDame(piece) ? board.Size : 1;

            return getEmptyPlaces(board, direction, fromPosition, maxMoveOffset);
        }

        private Direction[] getAllowDirection(Board board, int piece)
        {
            if (Board.isDame(piece))
            {
                return Direction.getAll();
            }

            if (Board.matchPieceColor(board.PlayerColor, piece))
            {
                return new Direction[] { Direction.UP_LEFT, Direction.UP_RIGHT };
            }
            else
            {
                return new Direction[] { Direction.DOWN_LEFT, Direction.DOWN_RIGHT };
            } 
        }

        private MoveScenarios getAllMoveScenarios(Board board, Position basePosition)
        {
            List<List<Position>> foundMoves = new List<List<Position>>();
            int piece = board.getPiece(basePosition);
            
            foreach (Direction direction in getAllowDirection(board, piece))
            {
                List<Position> movesPos = findMove(board, direction, basePosition, piece);

                foreach (Position currMove in movesPos)
                {
                    List<Position> singleMove = new List<Position>();
                    singleMove.Add(currMove);

                    foundMoves.Add(singleMove);
                }
            }

            return new MoveScenarios(basePosition, foundMoves, false);
        }

        private MoveScenarios getAllCaptureScenarios(Board board, Position basePosition)
        {
            int piece = board.getPiece(basePosition);
            Stack<MoveOptionsContext> stack = new Stack<MoveOptionsContext>();
            List<List<Position>> allScenarios = new List<List<Position>>();
            List<Position> currScenario = new List<Position>();
            MoveOptionsContext currContext;
            MoveOptions currMoveOpt;

            MoveOptions rootMoveOpt = new MoveOptions(new Board(board), basePosition);

            stack.Push(new MoveOptionsContext(rootMoveOpt));

            while (stack.Count > 0)
            {
                currContext = stack.Pop();
                currMoveOpt = currContext.CurrMoveOpt;

                if (!currContext.IsVisited)
                {
                    if (!currMoveOpt.Equals(rootMoveOpt))
                        currScenario.Add(currMoveOpt.CurrentPos);

                    currContext.IsVisited = true;
                    stack.Push(currContext);

                    List<Position> foundCapture = findCaptures(currMoveOpt.CurrentBoard, currMoveOpt.CurrentPos, piece);
                    
                    foreach (Position currCapturePos in foundCapture)
                    {
                        Board newBoardState = new Board(currMoveOpt.CurrentBoard);
                        MoveController.execMove(newBoardState, currMoveOpt.CurrentPos, currCapturePos);

                        MoveOptions captureMoveOpt = new MoveOptions(newBoardState, currCapturePos);
                        currMoveOpt.Add(captureMoveOpt);

                        stack.Push(new MoveOptionsContext(captureMoveOpt));
                    }
                }
                else
                {
                    if (currMoveOpt.AllMoveOptPos.Count == 0 && currScenario.Count > 0)
                    {
                        List<Position> leafPath = new List<Position>();
                        leafPath.AddRange(currScenario);

                        allScenarios.Add(leafPath);
                    }

                    if (currScenario.Count > 0)
                    {
                        currScenario.RemoveAt(currScenario.Count - 1);
                    }
                }
            }

            if (allScenarios.Count == 0)
                return null;

            return new MoveScenarios(basePosition, allScenarios, true);
        }

        public MoveScenarios getMoveScenarios(Board board, Position basePosition)
        {
            MoveScenarios captureScenarios = getAllCaptureScenarios(board, basePosition);

            if (captureScenarios != null)
            {
                return captureScenarios;
            }

            return getAllMoveScenarios(board, basePosition);
        }

        public List<MoveScenarios> getAllowMoveScenarios(Board board, int color)
        {
            List<MoveScenarios> allMoveScenarios = new List<MoveScenarios>();
            List<MoveScenarios> allCaptureScenarios = new List<MoveScenarios>();

            List<Position> piecePositions = board.getPiecePositions(color);

            foreach (Position currPosition in piecePositions)
            {
                MoveScenarios captureScenarios = getAllCaptureScenarios(board, currPosition);

                if (captureScenarios != null)
                {
                    allCaptureScenarios.Add(captureScenarios);
                }
                else
                {
                    MoveScenarios moveScenarios = getAllMoveScenarios(board, currPosition);

                    if (moveScenarios.Count() > 0)
                        allMoveScenarios.Add(moveScenarios);
                }
            }

            if (allCaptureScenarios.Count > 0)
            {
                return allCaptureScenarios;
            }

            return allMoveScenarios;
        }
    }
}
