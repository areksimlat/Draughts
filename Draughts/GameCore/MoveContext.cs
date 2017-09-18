using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameCore
{
    [DataContract]
    public class MoveContext
    {
        [DataMember]
        public Board BoardState { get; set; }

        [DataMember]
        public Position CurrPosition { get; set; }

        [DataMember]
        public List<Position> CurrMoveScenario { get; set; }

        [DataMember]
        public int CurrDepth { get; set; }

        [DataMember]
        public bool IsVisited { get; set; }

        [DataMember]
        public int BestValue { get; set; }

        [DataMember]
        public MoveContext ParentContext { get; set; }

        [DataMember]
        public Stack<MoveContext> ChildsContext { get; set; }

        [DataMember]
        public bool IsMax { get; private set; }



        public MoveContext(Board boardState, Position currPosition, List<Position> currMoveScenario,
            int currDepth, MoveContext parentContext, bool isMax)
        {
            BoardState = boardState;
            CurrPosition = currPosition;
            CurrMoveScenario = currMoveScenario;
            CurrDepth = currDepth;
            IsVisited = false;
            ParentContext = parentContext;
            ChildsContext = new Stack<MoveContext>();
            IsMax = isMax;
            BestValue = IsMax ? int.MinValue : int.MaxValue;
        }

        public void setParentValue(int value)
        {
            lock (ParentContext)
            {
                if (ParentContext.IsMax)
                {
                    if (value > ParentContext.BestValue)
                        ParentContext.BestValue = value;
                }
                else
                {
                    if (value < ParentContext.BestValue)
                        ParentContext.BestValue = value;
                }
            }
        }

        public void setParentMax()
        {
            lock (ParentContext)
            {
                if (ParentContext.BestValue < BestValue)
                {
                    ParentContext.BestValue = BestValue;

                    if (ParentContext.CurrDepth == 0)
                    {
                        ParentContext.CurrPosition = CurrPosition;
                        ParentContext.CurrMoveScenario = CurrMoveScenario;
                    }
                }
            }
        }

        public void setParentMin()
        {
            lock (ParentContext)
            {
                if (ParentContext.BestValue > BestValue)
                {
                    ParentContext.BestValue = BestValue;

                    if (ParentContext.CurrDepth == 0)
                    {
                        ParentContext.CurrPosition = CurrPosition;
                        ParentContext.CurrMoveScenario = CurrMoveScenario;
                    }
                }
            }
        }

        public bool addChildBoardStates(MoveFinder moveFinder)
        {
            List<MoveScenarios> allScenarios = moveFinder.getAllowMoveScenarios(BoardState, BoardState.CurrentColor);

            foreach (MoveScenarios currScenario in allScenarios)
            {
                for (int i = 0; i < currScenario.Count(); i++)
                {
                    Board newBoardState = new Board(BoardState);
                    MoveController.execScenario(newBoardState, currScenario, i);

                    ChildsContext.Push(
                        new MoveContext(
                            newBoardState,
                            currScenario.getFromPosition(),
                            currScenario.getScenario(i),
                            CurrDepth + 1,
                            this,
                            !IsMax
                            )
                        );
                }
            }

            IsVisited = true;

            if (allScenarios.Count > 0)
                return allScenarios[0].isCapture();

            return false;
        }

        public bool cutOffPossible()
        {
            lock (ParentContext)
            {
                if (ParentContext == null ||
                    ParentContext.BestValue == int.MinValue || ParentContext.BestValue == int.MaxValue ||
                        BestValue == int.MinValue || BestValue == int.MaxValue)
                    return false;

                if (!BoardState.arePieces())
                    return true;

                if (ParentContext.IsMax)
                    return BestValue < ParentContext.BestValue;
                else
                    return BestValue > ParentContext.BestValue;
            }
        }
    }
}
