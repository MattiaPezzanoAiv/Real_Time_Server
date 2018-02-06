using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using RealTimeServer;
using RealTimeServer.AutorityServer;

namespace RealTimeServer.Test
{
    [TestFixture]
    public class Test_PacketParser
    {
        Packet packet;
        [SetUp]
        public void Constructor()
        {
            Server.Init();

            ServerJsonHandler.Join join = new ServerJsonHandler.Join();
            join.name = "foobar";
            string json = JsonConvert.SerializeObject(join);
            packet = new Packet(1, true,Server.TimeStamp); //this is a join packet
            packet.Writer.Write(json);
            Type packetType = typeof(Packet);
            PropertyInfo prop = packetType.GetProperty("SourceEp");
            if (prop == null) throw new Exception("PROP NOT FOUND");

            prop.SetValue(packet, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
        }

        [Test]
        public void Test_ParseJoin()
        {
            Assert.That(() => Server.ParseJoin(packet), Throws.Nothing);
            //if client joined no queued packet
        }
        [Test]
        public void Test_ParseJoinAlredyJoined()
        {
            IClient client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.ParseJoin(packet);

            Assert.That(Server.PacketsCount, Is.EqualTo(0));
            //if client joined no queued packet
        }
        [Test]
        public void Test_ParseJoinGreenLight()
        {
            Server.ParseJoin(packet);
            Assert.That(Server.PlayersConnected, Is.EqualTo(1));
        }
        [Test]
        public void Test_ParseJoinOtherClientsAllerted()        //this check alredy if the client with same ip join the game
        {
            Server.AddClient(new Client("fake", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.4"), 2000)));

            Server.ParseJoin(packet);
            Assert.That(Server.PacketsCount, Is.EqualTo(Server.PlayersConnected));
        }
    }
}
