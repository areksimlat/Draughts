using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkerServiceLibrary;

namespace ServerServiceLibrary
{
    public class WorkerInfo
    {
        private string uri;
        private string ip;
        private string port;
        private string hostname;
        private IWorker iworker;

        public WorkerInfo(string uri, string hostname, IWorker worker)
        {
            this.uri = uri;
            this.ip = getIpFromUri(uri);
            this.port = getPortFromUri(uri);
            this.hostname = hostname;
            this.iworker = worker;
        }

        public string getUri()
        {
            return uri;
        }

        public string getHostname()
        {
            return hostname;
        }

        public IWorker getIWorker()
        {
            return iworker;
        }

        public static string getIpFromUri(string uriString)
        {
            int firstIndex = 10;
            int length = uriString.LastIndexOf(':') - firstIndex;

            return uriString.Substring(firstIndex, length);
        }

        public static string getPortFromUri(string uriString)
        {
            int firstIndex = uriString.LastIndexOf(':') + 1;
            int length = uriString.LastIndexOf('/') - firstIndex;

            return uriString.Substring(firstIndex, length);
        }

        public string getIp()
        {
            return ip;
        }

        public string getPort()
        {
            return port;
        }

        public override string ToString()
        {
            return hostname;
        }
    }
}
