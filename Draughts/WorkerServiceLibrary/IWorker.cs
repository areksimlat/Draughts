using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WorkerServiceLibrary
{
    [ServiceContract]
    public interface IWorker
    {
        [OperationContract]
        string GetHostName();

        [OperationContract]
        int FindBestValue(MoveContext moveContext, int maxDepth, int maxColor, int secTimeout);
    }
}
