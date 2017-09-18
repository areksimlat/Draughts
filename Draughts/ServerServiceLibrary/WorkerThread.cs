using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WorkerServiceLibrary;

namespace ServerServiceLibrary
{
    public class WorkerThread
    {
        private Thread thread;
        private IWorker iworker;
        private Semaphore semaphore;
        private ManualResetEvent stopEvent;
        private Stack<MoveContext> inStack;
        private Stack<MoveContext> outStack;
        private CountdownTimer countdownTimer;
        private int maxColor;
        private int maxDepth;
        private int secTimeout;

        public WorkerThread(IWorker iworker, Semaphore semaphore, ManualResetEvent stopEvent)
        {
            this.iworker = iworker;
            this.semaphore = semaphore;
            this.stopEvent = stopEvent;
            countdownTimer = new CountdownTimer();
            thread = new Thread(doWork);
        }

        public void setInStack(Stack<MoveContext> inStack)
        {
            this.inStack = inStack;
        }

        public void setOutStack(Stack<MoveContext> outStack)
        {
            this.outStack = outStack;
        }

        public void setMaxColor(int maxColor)
        {
            this.maxColor = maxColor;
        }

        public void setMaxDepth(int maxDepth)
        {
            this.maxDepth = maxDepth;
        }

        public void setSecTimeout(int secTimeout)
        {
            this.secTimeout = secTimeout;
        }

        public void Start()
        {
            thread.Start();
        }

        public void Abort()
        {
            thread.Abort();
        }

        private void doWork()
        {
            MoveContext context = null;

            while (true)
            {
                semaphore.WaitOne();

                countdownTimer.Start(secTimeout);

                while (true)
                {
                    lock (inStack)
                    {
                        if (inStack.Count == 0)
                            break;

                        context = inStack.Pop();
                    }

                    context.BestValue = iworker.FindBestValue(context, maxDepth, maxColor, countdownTimer.getRemainTime());
                    context.IsVisited = true;
                    
                    lock (outStack)
                    {
                        outStack.Push(context);
                    }

                    if (countdownTimer.isTimeout())
                    {
                        break;
                    }
                }

                countdownTimer.Stop();
                stopEvent.Set();
            }
        }
    }
}
