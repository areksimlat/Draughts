using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WorkerServiceLibrary
{
    public class Worker : IWorker
    {
        public static int MAX_THREADS = 1;

        public string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }

        public int FindBestValue(MoveContext moveContext, int maxDepth, int maxColor, int secTimeout)
        {
            if (MAX_THREADS == 1)
            {
                new MiniMax(maxDepth).findBestMoveValue(moveContext, maxColor, secTimeout);
            }
            else
            {
                new MiniMax(maxDepth).findBestMoveValueParallel(moveContext, maxColor, secTimeout, MAX_THREADS);
            }

            return moveContext.BestValue;
        }
    }
}
