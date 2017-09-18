using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameCore
{
    public class MiniMax
    {
        public static int LEVEL_EASY = 7;
        public static int LEVEL_MEDIUM = 9;
        public static int LEVEL_HARD = 11;

        private static readonly int POINTS_FOR_PIECE = 5;
        private static readonly int POINTS_FOR_DAME = 20;
        private static readonly int POINTS_FOR_EDGE_AREA_1 = 1;
        private static readonly int POINTS_FOR_EDGE_AREA_2 = 3;
        private static readonly int POINTS_FOR_EDGE_AREA_3 = 4;

        private MoveController moveController;
        private MoveFinder moveFinder;
        private int maxDepth;


        public MiniMax(int maxDepth)
        {
            this.maxDepth = maxDepth;
            moveController = new MoveController();
            moveFinder = new MoveFinder();
        }


        private int getEdgeAreaPoints(Board boardState, Position currPosition)
        {
            if (currPosition.Col == 0 || currPosition.Col == boardState.Size - 1 ||
                    currPosition.Row == 0 || currPosition.Row == boardState.Size - 1)
                return POINTS_FOR_EDGE_AREA_1;

            if (currPosition.Col == 1 || currPosition.Col == boardState.Size - 2 ||
                    currPosition.Row == 1 || currPosition.Row == boardState.Size - 2)
                return POINTS_FOR_EDGE_AREA_2;

            return POINTS_FOR_EDGE_AREA_3;
        }

        private int getBoardStateWeight(Board boardState, int maxColor)
        {
            Position currPosition;
            int currPiece, currPoints;
            int maxPoints = 0;
            int minPoints = 0;

            for (int row = 0; row < boardState.Size; row++)
            {
                for (int col = 0; col < boardState.Size; col++)
                {
                    currPosition = new Position(row, col);
                    currPiece = boardState.getPiece(currPosition);

                    if (!Board.isEmpty(currPiece))
                    {
                        currPoints = getEdgeAreaPoints(boardState, currPosition);

                        if (Board.matchPieceColor(currPiece, maxColor))
                        {
                            maxPoints += currPoints;
                        }
                        else
                        {
                            minPoints += currPoints;
                        }
                    }
                }
            }

            if (Board.matchPieceColor(boardState.PlayerColor, maxColor))
            {
                maxPoints += (boardState.PlayerPiecesCount * POINTS_FOR_PIECE) +
                    (boardState.PlayerDameCount * POINTS_FOR_DAME);

                minPoints += (boardState.EnemyPiecesCount * POINTS_FOR_PIECE) +
                    (boardState.EnemyDameCount * POINTS_FOR_DAME);
            }
            else
            {
                maxPoints += (boardState.EnemyPiecesCount * POINTS_FOR_PIECE) +
                    (boardState.EnemyDameCount * POINTS_FOR_DAME);

                minPoints += (boardState.PlayerPiecesCount * POINTS_FOR_PIECE) +
                    (boardState.PlayerDameCount * POINTS_FOR_DAME);
            }

            return maxPoints - minPoints;
        }


        public int resolve(Stack<MoveContext> stack, int maxColor, int currMaxDepth, int secTimeout)
        {
            CountdownTimer countdownTimer = new CountdownTimer();

            if (secTimeout > 0)
            {
                countdownTimer.Start(secTimeout);
            }

            MoveContext currContext;
            int moveWeight;

            while (stack.Count > 0)
            {
                currContext = stack.Pop();

                if (currContext.CurrDepth < currMaxDepth)
                {
                    if (currContext.IsVisited)
                    {
                        if (currContext.ChildsContext.Count > 0 &&
                            !currContext.cutOffPossible())
                        {
                            stack.Push(currContext);
                            stack.Push(currContext.ChildsContext.Pop());
                        }
                        else
                        {
                            if (currContext.ParentContext.IsMax)
                            {
                                currContext.setParentMax();
                            }
                            else
                            {
                                currContext.setParentMin();
                            }
                        }
                    }
                    else if (!countdownTimer.isTimeout())
                    {
                        currContext.addChildBoardStates(moveFinder);
                        stack.Push(currContext);
                    }
                    else
                    {
                        moveWeight = getBoardStateWeight(currContext.BoardState, maxColor);
                        currContext.setParentValue(moveWeight);
                    }
                }
                else
                {
                    moveWeight = getBoardStateWeight(currContext.BoardState, maxColor);
                    currContext.setParentValue(moveWeight);
                }
            }

            countdownTimer.Stop();

            return countdownTimer.getRemainTime();
        }

        public int findBestMoveValue(MoveContext context, int maxColor, int secTimeout)
        {
            context.addChildBoardStates(moveFinder);

            Stack<MoveContext> stack = new Stack<MoveContext>();

            while (context.ChildsContext.Count > 0)
            {
                stack.Push(context.ChildsContext.Pop());
            }

            return resolve(stack, maxColor, maxDepth, secTimeout);
        }


        public MoveScenarios getBestMove(Board currBoard, int secTimeout)
        {
            int maxColor = currBoard.CurrentColor;
            MoveContext rootContext = new MoveContext(currBoard, null, null, 0, null, true);

            findBestMoveValue(rootContext, maxColor, secTimeout);

            if (rootContext.BestValue != int.MinValue)
                return new MoveScenarios(rootContext.CurrPosition, rootContext.CurrMoveScenario);

            return null;
        }


        public int generateMoveContext(MoveContext context, int minCount, List<MoveContext> list)
        {
            List<MoveContext> tmpList = new List<MoveContext>();
            MoveContext currContext = null;

            if (context.CurrDepth >= maxDepth || !context.BoardState.arePieces())
                return 0;

            bool isCapture = context.addChildBoardStates(moveFinder);

            int childsCount = context.ChildsContext.Count;
            int addedContext = childsCount;

            if (childsCount == 0)
                return 0;

            while (context.ChildsContext.Count > 0)
                list.Add(context.ChildsContext.Pop());

            if (isCapture)
                return -childsCount;

            if (childsCount >= minCount)
                return childsCount;

            while (true)
            {
                for (int i = 0; i < addedContext; i++)
                {
                    currContext = list[list.Count - i - 1];
                    currContext.addChildBoardStates(moveFinder);
                    childsCount += currContext.ChildsContext.Count;

                    while (currContext.ChildsContext.Count > 0)
                        tmpList.Add(currContext.ChildsContext.Pop());
                }

                list.AddRange(tmpList);
                tmpList.Clear();

                if (childsCount == 0 || childsCount >= minCount || currContext.CurrDepth >= maxDepth)
                    return childsCount;

                addedContext = childsCount;
                childsCount = 0;
            }
        }



        public void findBestMoveValueParallel(MoveContext context, int maxColor, int secTimeout, int maxThreads)
        {
            int tCount = maxThreads;

            List<MoveContext> list = new List<MoveContext>();
            Stack<MoveContext> mainStack = new Stack<MoveContext>();
            Stack<MoveContext> threadsStack = new Stack<MoveContext>();

            int childs = generateMoveContext(context, tCount, list);

            if (childs < 0)
            {
                foreach (MoveContext mc in list)
                    mainStack.Push(mc);

                resolve(mainStack, maxColor, 2, -1);
            }
            else if (childs > 0)
            {
                if (childs < tCount)
                    tCount = childs;

                int i = 0;

                for (; i < list.Count - childs; i++)
                    mainStack.Push(list[i]);

                for (; i < list.Count; i++)
                    threadsStack.Push(list[i]);

                Task[] tasks = new Task[tCount];

                for (i = 0; i < tCount; i++)
                {
                    ThreadContext tContext = new ThreadContext(threadsStack, mainStack, maxColor, secTimeout - 15, maxDepth);

                    tasks[i] = new Task(() => { tContext.findBestMoveValueWithEvent(); });
                    tasks[i].Start();
                }

                Task.WaitAll(tasks);

                resolve(mainStack, maxColor, maxDepth, -1);
            }
        }
    }

    public class ThreadContext
    {
        private Stack<MoveContext> inStack;
        private Stack<MoveContext> outStack;
        private int maxColor;
        private int secTimeout;
        private MiniMax minimax;

        public ThreadContext(Stack<MoveContext> inStack, Stack<MoveContext> outStack, int maxColor,
            int secTimeout, int maxDepth)
        {
            this.inStack = inStack;
            this.outStack = outStack;
            this.maxColor = maxColor;
            this.secTimeout = secTimeout;
            minimax = new MiniMax(maxDepth);
        }

        public void findBestMoveValueWithEvent()
        {
            MoveContext currContext;
            int remainTime = secTimeout;

            while (true)
            {
                lock (inStack)
                {
                    if (inStack.Count == 0)
                        break;

                    currContext = inStack.Pop();
                }

                remainTime = minimax.findBestMoveValue(currContext, maxColor, remainTime);

                lock (outStack)
                {
                    outStack.Push(currContext);
                }

                if (remainTime <= 0)
                    break;
            }
        }
    }
}
