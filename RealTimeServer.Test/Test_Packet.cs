using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using Newtonsoft.Json;

namespace RealTimeServer.Test
{
    [TestFixture]
    public class Test_Packet
    {

        int packetHeaderSize;
        Packet packet;
        byte[] buffer;
        [SetUp]
        public void Contructor()
        {
            Packet.Reset();
            packetHeaderSize = 9;
        }

        [Test]
        public void Test_WriteheaderPacketId()
        {
            packet = new Packet(10, false);
            buffer = packet.Stream.ToArray();
            uint packetid = BitConverter.ToUInt32(buffer, 0); //start from first byte
            Assert.That(packetid, Is.EqualTo(0));
        }
        [Test]
        public void Test_WriteheaderPacketIdMultiple()
        {
            packet = new Packet(10, false);
            buffer = packet.Stream.ToArray();
            uint packetid = BitConverter.ToUInt32(buffer, 0); //start from first byte
            Assert.That(packetid, Is.EqualTo(0));

            Packet packet2 = new Packet(10,false);
            byte[] buffer2 = packet2.Stream.ToArray();
            uint packetid2 = BitConverter.ToUInt32(buffer2, 0); //start from first byte
            Assert.That(packetid2, Is.EqualTo(1));
        }
        [Test]
        public void Test_WriteheaderPacketIdNotEqualToPrev()
        {
            packet = new Packet(10, false);
            buffer = packet.Stream.ToArray();
            uint packetid = BitConverter.ToUInt32(buffer, 0); //start from first byte
            Assert.That(packetid, Is.EqualTo(0));

            Packet packet2 = new Packet(10,false);
            byte[] buffer2 = packet2.Stream.ToArray();
            uint packetid2 = BitConverter.ToUInt32(buffer2, 0); //start from first byte
            Assert.That(packetid2, Is.Not.EqualTo(packetid));
        }
        [Test]
        public void Test_WriteHeaderCommand()
        {
            packet = new Packet(10, false);
            buffer = packet.Stream.ToArray();
            byte command = buffer[8];
            Assert.That(command, Is.EqualTo(10));
        }
        [Test]
        public void Test_WriteHeaderCommandReliable()
        {
            packet = new Packet(10, true);
            buffer = packet.Stream.ToArray();
            byte command = buffer[8];
            Assert.That(command, Is.EqualTo(138));
        }
        [Test]
        public void Test_Shortcuts()
        {
            packet = new Packet(125, false);
            Assert.That(packet.Command, Is.EqualTo(125));
            Assert.That(packet.Reliable, Is.EqualTo(false));
            Assert.That(packet.PacketId, Is.EqualTo(0));  //first
            Assert.That(packet.TimeStamp, Is.EqualTo(0));  //not managed
        }
        [Test]
        public void Test_CreatePacketFromByteArray()
        {
            byte[] buffer = new byte[15];
            Buffer.BlockCopy(BitConverter.GetBytes(10),0,buffer,0,sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(100), 0, buffer, 4, sizeof(int));
            buffer[8] = 255; //reliable with command 127

            Packet p = new Packet(buffer,15, new IPEndPoint(IPAddress.Parse("127.0.0.1"),2000));  //simulating a socket.receive()
            Assert.That(p.Buffer.Length, Is.EqualTo(15));
            Assert.That(p.PacketId, Is.EqualTo(10));
            Assert.That(p.TimeStamp, Is.EqualTo(100));
            Assert.That(p.Command, Is.EqualTo(127));
            Assert.That(p.Reliable, Is.EqualTo(true));
        }
        [Test]
        public void Test_IsAck()
        {
            Packet p = Packet.CreateAck(10);
            Assert.That(Packet.IsAck(p), Is.EqualTo(true));
        }
        [Test]
        public void Test_IsAckRedLight()
        {
            Packet p = new Packet(15, true);
            Assert.That(Packet.IsAck(p), Is.EqualTo(false));
        }
        [Test]
        public void Test_PacketGetJoinedNegative()
        {
            Packet p = Packet.GetJoined(uint.MaxValue, "client rejected");
            byte[] data = p.Buffer;
            p.Stream.Seek(packetHeaderSize, System.IO.SeekOrigin.Begin);
            uint id = p.Reader.ReadUInt32();
            string json = p.Reader.ReadString();
            JsonHandler.ErrorMessage message = JsonConvert.DeserializeObject< JsonHandler.ErrorMessage>(json);
            Assert.That(message.message, Is.EqualTo("client rejected"));
            Assert.That(id, Is.EqualTo(uint.MaxValue));
        }
        [Test]
        public void Test_PacketGetJoinedPositive()
        {
            Packet p = Packet.GetJoined(10, "client joined");
            byte[] data = p.Buffer;
            p.Stream.Seek(packetHeaderSize, System.IO.SeekOrigin.Begin);
            uint id = p.Reader.ReadUInt32();
            string json = p.Reader.ReadString();
            JsonHandler.ErrorMessage message = JsonConvert.DeserializeObject<JsonHandler.ErrorMessage>(json);
            Assert.That(message.message, Is.EqualTo("client joined"));
            Assert.That(id, Is.EqualTo(10));
        }
        [Test]
        public void Test_PacketGetKick()
        {
            Packet p = Packet.GetClientKicked(5, "client kicked");
            byte[] data = p.Buffer;
            p.Stream.Seek(packetHeaderSize, System.IO.SeekOrigin.Begin);
            uint id = p.Reader.ReadUInt32();
            string json = p.Reader.ReadString();
            JsonHandler.ErrorMessage message = JsonConvert.DeserializeObject<JsonHandler.ErrorMessage>(json);
            Assert.That(message.message, Is.EqualTo("client kicked"));
            Assert.That(id, Is.EqualTo(5));
        }
    }
}
