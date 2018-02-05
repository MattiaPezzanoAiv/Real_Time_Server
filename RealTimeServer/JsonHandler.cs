using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeServer
{
    public class JsonHandler
    {
        public class ErrorMessage
        {
            public string message;
        }
        

        public class Join
        {
            public string name;
        }
        public class Joined
        {
            public uint clientId;
            public string reason; //only for negative success
        }
        public class Leave
        {
            public uint clientId;
        }
    }
}
