using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using WorkerServiceLibrary;

namespace ServerServiceLibrary
{
    public class WorkerManager
    {
        private static WorkerManager instance = null;

        private List<WorkerInfo> workersInfo = null;
        private WorkerThread[] workersThread = null;
        private ManualResetEvent[] workersStopEvent = null;
        private Semaphore semaphore = null;


        private WorkerManager()
        {
            workersInfo = new List<WorkerInfo>();
        }

        public static WorkerManager getInstance()
        {
            if (instance == null)
            {
                instance = new WorkerManager();
            }

            return instance;
        }

        public int getWorkersCount()
        {
            return workersInfo.Count;
        }

        public List<string> searchWorkers()
        {
            List<string> foundUri = new List<string>();

            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            FindCriteria findCriteria = new FindCriteria(typeof(IWorker));
            findCriteria.Duration = TimeSpan.FromSeconds(2);

            FindResponse findResponse = discoveryClient.Find(findCriteria);

            foreach (EndpointDiscoveryMetadata edm in findResponse.Endpoints)
            {
                foundUri.Add(edm.Address.Uri.ToString());
            }

            return foundUri;
        }

        public void connectToWorkers(List<string> workersUri)
        {
            workersInfo = new List<WorkerInfo>();

            foreach (string uri in workersUri)
            {
                try
                {
                    Uri baseAddress = new Uri(uri);
                    EndpointAddress address = new EndpointAddress(baseAddress);
                    NetTcpBinding binding = new NetTcpBinding("UnsecureNetTcpBinding");

                    ChannelFactory<WorkerServiceLibrary.IWorker> factory
                        = new ChannelFactory<WorkerServiceLibrary.IWorker>(binding, address);

                    IWorker worker = factory.CreateChannel();

                    ((IContextChannel)worker).OperationTimeout = TimeSpan.MaxValue;

                    try
                    {
                        string hostname = worker.GetHostName();

                        workersInfo.Add(new WorkerInfo(uri, hostname, worker));
                    }
                    catch (Exception ex)
                    {
                        //
                    }
                }
                catch (Exception ex)
                {

                }
            }

            createWorkersThread();
        }

        public List<WorkerInfo> getWorkersInfo()
        {
            return workersInfo;
        }

        private void createWorkersThread()
        {
            if (workersInfo.Count > 0)
            {
                int workersCount = workersInfo.Count;

                semaphore = new Semaphore(0, workersCount);
                workersStopEvent = new ManualResetEvent[workersCount];
                workersThread = new WorkerThread[workersCount];

                for (int i = 0; i < workersCount; i++)
                {
                    workersStopEvent[i] = new ManualResetEvent(false);
                    workersThread[i] = new WorkerThread(workersInfo[i].getIWorker(), semaphore, workersStopEvent[i]);
                    workersThread[i].Start();
                }
            }
        }

        public void setParameters(Stack<MoveContext> inStack, Stack<MoveContext> outStack,
            int maxDepth, int maxColor, int secTimeout)
        {
            foreach (WorkerThread wThread in workersThread)
            {
                wThread.setInStack(inStack);
                wThread.setOutStack(outStack);
                wThread.setMaxColor(maxColor);
                wThread.setMaxDepth(maxDepth);
                wThread.setSecTimeout(secTimeout);
            }
        }

        public void runWorkers()
        {
            semaphore.Release(workersInfo.Count);
        }

        public void abortWorkers()
        {
            if (workersThread != null)
            {
                foreach (WorkerThread wThread in workersThread)
                {
                    wThread.Abort();
                }
            }
        }

        public void waitForWorkers()
        {
            foreach (ManualResetEvent mre in workersStopEvent)
            {
                mre.WaitOne();
                mre.Reset();
            }
        }
    }
}
