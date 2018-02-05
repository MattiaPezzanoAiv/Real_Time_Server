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
            Assert.That(Server.IsClientJoined(new IPEndPoint(IPAddress.Any,2000)), Is.EqualTo(false));
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
            
            Server.SendBroadcast(Packet.GetClientKicked(0, "fake reason"),0); //first client ignored
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

            Assert.That(()=>Server.RemoveClient(1255),Throws.Nothing); //fake n
            Assert.That(Server.PlayersConnected,Is.EqualTo(3));
        }
    }
}
