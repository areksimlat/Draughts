using GameCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WorkerServiceLibrary;

namespace ServerServiceLibrary
{
    public class ServerService : IServerService
    {
        public MoveScenarios GetBestMove(Board board, int maxDepth, int secTimeout)
        {
            MiniMax miniMax = new MiniMax(maxDepth);
            WorkerManager workerManager = WorkerManager.getInstance();

            if (workerManager.getWorkersCount() == 0)
            {
                return miniMax.getBestMove(board, secTimeout);
            }

            int maxColor = board.CurrentColor;
            MoveContext rootContext = new MoveContext(board, null, null, 0, null, true);            

            Stack<MoveContext> mainStack = new Stack<MoveContext>();
            Stack<MoveContext> childsStack = new Stack<MoveContext>();
            List<MoveContext> contextList = new List<MoveContext>();

            int generatedChilds = miniMax.generateMoveContext(rootContext, workerManager.getWorkersCount(), contextList);
        
            if (generatedChilds == 0)
            {
                return null;
            }
            else if (generatedChilds == 1)
            {
                rootContext = contextList[0];
                miniMax.resolve(contextList[0].ChildsContext, maxColor, maxDepth, secTimeout);
            }
            else if (generatedChilds <= -1)
            {
                foreach (MoveContext mc in contextList)
                    mainStack.Push(mc);

                miniMax.resolve(mainStack, maxColor, 2, -1);
            }
            else
            {
                int i = 0;

                for (; i < contextList.Count - generatedChilds; i++)
                {
                    mainStack.Push(contextList[i]);
                }

                for (; i < contextList.Count; i++)
                {
                    childsStack.Push(contextList[i]);
                }

                workerManager.setParameters(childsStack, mainStack, maxDepth, maxColor, secTimeout - 15);
                workerManager.runWorkers();
                workerManager.waitForWorkers();

                childsStack.Clear();

                miniMax.resolve(mainStack, maxColor, maxDepth, -1);
            }

            if (rootContext.BestValue != int.MinValue)
            {
                return new MoveScenarios(rootContext.CurrPosition, rootContext.CurrMoveScenario);
            }

            return new MiniMax(2).getBestMove(board, -1);
        }
    }
}
