using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace RealTimeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Init();
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));

            Server.Init();
            Server.AddClient(new Client("foobar2", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo2", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar2", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foobar3", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            bool b = Server.IsClientJoined(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));


            Log.SLog.AutomaticHeader = true;
            Log.SLog.WriteOnConsole = true;
            Log.SLog.WriteOnFile = true;
            Log.SLog.Write("first log.");
            Console.ReadLine();
        }
    }
}
