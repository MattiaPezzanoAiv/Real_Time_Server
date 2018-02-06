using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;

namespace RealTimeServer.Test
{
    [TestFixture]
    public class Test_Server
    {
        [SetUp]
        public void Constructor()
        {
            Server.Init();
        }

        [Test]
        public void Test_PacketEnqueue()
        {
            Server.EnquePacket(10, new Packet(10, false));
            Assert.That(Server.PacketsCount, Is.EqualTo(1));
        }
        [Test]
        public void Test_PacketEnqueueMultiple()
        {
            Server.EnquePacket(10, new Packet(10, false));
            Server.EnquePacket(9, new Packet(10, false));
            Server.EnquePacket(8, new Packet(10, false));
            Assert.That(Server.PacketsCount, Is.EqualTo(3));
        }
        [Test]
        public void Test_PacketEnqueueSameId()
        {
            Server.EnquePacket(10, new Packet(10, false));
            Server.EnquePacket(10, new Packet(10, false));
            Server.EnquePacket(10, new Packet(10, false));
            Server.EnquePacket(1, new Packet(10, false));
            Assert.That(Server.PacketsCount, Is.EqualTo(4));
        }
        [Test]
        public void Test_PacketDequeue()
        {
            Server.EnquePacket(10, new Packet(10, false));
            Server.EnquePacket(9, new Packet(10, false));
            Server.EnquePacket(8, new Packet(10, false));
            Server.DequeuePackets();
            Assert.That(Server.PacketsCount, Is.EqualTo(0));
        }
        [Test]
        public void Test_receiveNoDataNoException()
        {
            Assert.That(() => Server.Receive(), Throws.Nothing);
        }
        [Test]
        public void Test_IsClientJoinedNameRedLight()
        {
            Assert.That(Server.IsClientJoined("foobar"), Is.EqualTo(false));
        }
        [Test]
        public void Test_IsClientJoinedNameGreenLight()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Assert.That(Server.IsClientJoined("foobar"), Is.EqualTo(true));
        }
        [Test]
        public void Test_IsClientJoinedIDRedLight()
        {
            Assert.That(Server.IsClientJoined(12), Is.EqualTo(false));
        }
        //[Test]
        //public void Test_IsClientJoinedIDGreenLight()
        //{
        //    Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
        //    Assert.That(Server.IsClientJoined(1), Is.EqualTo(true));
        //}
        [Test]
        public void Test_IsClientJoinedEPRedLight()
        {
            Assert.That(Server.IsClientJoined(new IPEndPoint(IPAddress.Any, 2000)), Is.EqualTo(false));
        }
        [Test]
        public void Test_IsClientJoinedEPGreenLight()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Assert.That(Server.IsClientJoined(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)), Is.EqualTo(true));
        }
        [Test]
        public void Test_SendBroadCast()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));

            Server.SendBroadcast(Packet.GetClientKicked(0, "fake reason"));
            Assert.That(Server.PacketsCount, Is.EqualTo(3));
        }
        [Test]
        public void Test_SendBroadCastIgnoredClient()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));

            Server.SendBroadcast(Packet.GetClientKicked(0, "fake reason"), 0); //first client ignored
            Assert.That(Server.PacketsCount, Is.EqualTo(2));
        }
        [Test]
        public void Test_RemoveClientGreenLight()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));

            Server.RemoveClient(0);
            Assert.That(Server.PlayersConnected, Is.EqualTo(2));
        }
        [Test]
        public void Test_RemoveClientRedClient()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));

            Assert.That(() => Server.RemoveClient(1255), Throws.Nothing); //fake n
            Assert.That(Server.PlayersConnected, Is.EqualTo(3));
        }

        //TO DO: ADD TEST ON ADD CLIENT
        #region ADD_CLIENT
        [Test]
        public void Test_AddClientGreenLight()
        {
            Assert.That(Server.PlayersConnected, Is.EqualTo(0));    //empty server
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Assert.That(Server.PlayersConnected, Is.EqualTo(1));    //regular filled server
        }
        [Test]
        public void Test_AddClientSameName()
        {
            Assert.That(Server.PlayersConnected, Is.EqualTo(0));    //empty server
            Client client1 = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Client client2 = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000));

            Server.AddClient(client1);
            Assert.That(() =>Server.AddClient(client2),Throws.Nothing);
            Assert.That(Server.PlayersConnected, Is.EqualTo(1));    
        }
        [Test]
        public void Test_AddClientNull()
        {
            Assert.That(Server.PlayersConnected, Is.EqualTo(0));    //empty server
            Client client1 = null;

            Assert.That(() => Server.AddClient(client1), Throws.Nothing);
            Assert.That(Server.PlayersConnected, Is.EqualTo(0));
        }
        #endregion ADD_CLIENT

        #region GET_CLIENT_BY_ID
        [Test]
        public void Test_GetClientIDGreenLight()
        {
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            IClient toControlClient = Server.GetClient(0);
            Assert.That(client, Is.EqualTo(toControlClient));
        }
        [Test]
        public void Test_GetClientIDAfterRemoving()
        {
            Server.AddClient(new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000)));
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            //prepare client to add after removing
            Client client = new Client("ToCompare", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000));
            Server.RemoveClient(0);
            Server.AddClient(client); //expected id 3

            IClient toControlClient = Server.GetClient(3);
            Assert.That(client, Is.EqualTo(toControlClient));
        }
        [Test]
        public void Test_GetClientIDRedLight()
        {
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            IClient toControlClient = Server.GetClient(4);  //expected does not exist
            Assert.That(toControlClient, Is.Null);
        }
        #endregion GET_CLIENT_BY_ID

        #region GET_CLIENT_BY_NAME
        [Test]
        public void Test_GetClientNameGreenLight()
        {
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            IClient toControlClient = Server.GetClient("foobar");
            Assert.That(client, Is.EqualTo(toControlClient));
        }
        [Test]
        public void Test_GetClientNameRedLight()
        {
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            IClient toControlClient = Server.GetClient("not exist");
            Assert.That(toControlClient, Is.Null);       //this name is not present
        }
        [Test]
        public void Test_GetClientNameEmpty()
        {
            Client client = new Client("foobar", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            Server.AddClient(client);
            Server.AddClient(new Client("foo", new IPEndPoint(IPAddress.Parse("127.0.0.2"), 2000)));
            Server.AddClient(new Client("bar", new IPEndPoint(IPAddress.Parse("127.0.0.3"), 2000)));

            IClient toControlClient = Server.GetClient(string.Empty);
            Assert.That(toControlClient, Is.Null);       //this name is not present
        }
        #endregion GET_CLIENT_BY_NAME

        //TO DO: GET CLIENT BY EP
    }
}
