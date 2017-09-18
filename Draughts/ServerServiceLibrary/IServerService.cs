using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ServerServiceLibrary
{
   
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        MoveScenarios GetBestMove(Board board, int maxDepth, int secTimeout);
    }

}
