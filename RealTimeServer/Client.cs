using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace RealTimeServer
{
    public class Client : IClient
    {
        private static uint clientId;

        public Client(string name, EndPoint ep)
        {
            this.ClientId = clientId++;
            this.Name = name;
            this.EP = ep;
        }

        public uint ClientId
        {
            get; set;
        }

        public EndPoint EP
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }

        public int RefuseTimes
        {
            get;set;
        }


        public static void Reset()
        {
            clientId = 0;
        }
        
    }
}
