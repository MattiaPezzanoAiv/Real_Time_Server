using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace RealTimeServer
{
    /// <summary>
    /// An abstraction for a packet structure
    /// </summary>
    public class Packet
    {
        private static uint packetId;

        private BinaryReader reader;
        private BinaryWriter writer;
        private MemoryStream stream;

        public BinaryReader Reader { get { return reader; } }
        public BinaryWriter Writer { get { return writer; } }
        public MemoryStream Stream { get { return stream; } }

        /// <summary>
        /// Create a packet with empty payload, header will be writed automatically
        /// </summary>
        /// <param name="command" type="byte"></param>
        /// <param name="reliable" type="bool"></param>
        public Packet(byte command, bool reliable)
        {
            stream = new MemoryStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            //set up shortcuts
            PacketId = packetId;
            TimeStamp = Server.TimeStamp;
            Command = command;
            Reliable = reliable;
            Buffer = stream.ToArray();

            //write header on buffer
            WriteHeader(command, reliable);
        }

        /// <summary>
        /// NB: Create a packet from an existing byte[] that respect this protocol
        /// </summary>
        /// <param name="buffer" type="byte[]"></param>
        /// <param name="dataReceived" type="int">the amount of data effectively received</param>
        public Packet(byte[] buffer, int dataReceived, EndPoint source)
        {
            this.Buffer = new byte[dataReceived];
            System.Buffer.BlockCopy(buffer, 0, this.Buffer, 0, dataReceived); //fully payload
            this.PacketId = BitConverter.ToUInt32(buffer, 0);
            this.TimeStamp = BitConverter.ToInt32(buffer, 4);
            this.Reliable = Utility.IsBitSet(buffer[8], 7);
            this.Command = Utility.SetBitOnByte(buffer[8], 7, false);
            this.LocalTimeStamp = (int)(Server.TimeStamp * 1000);
            this.SourceEp = source;
        }


        /// <summary>
        /// Is a shortcut to write the header of a packet into this packet (time stamp not managed)
        /// </summary>
        /// <param name="command" type="byte">the command relative to the protocol</param>
        /// <param name="reliable" type="bool">mark this packet as reliable</param>
        private void WriteHeader(byte command, bool reliable)
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(packetId++);
            writer.Write(0); //TIME STAMP (uint)
            command = Utility.SetBitOnByte(command, 7, reliable);
            writer.Write(command);
        }


        #region SHORTCUTS
        public uint PacketId { get; private set; }
        /// <summary>
        /// Sender time stamp
        /// </summary>
        public int TimeStamp { get; private set; }
        /// <summary>
        /// Local time stamp
        /// </summary>
        public int LocalTimeStamp { get; private set; }
        public byte Command { get; private set; }
        public bool Reliable { get; private set; }
        public byte[] Buffer { get; private set; }
        public EndPoint SourceEp { get; private set; }
        #endregion SHORTCUTS


        /// <summary>
        /// A shortcut for creating an ack packet 
        /// </summary>
        /// <param name="packetId">packet id to acknowledge</param>
        /// <returns></returns>
        public static Packet CreateAck(uint packetId)
        {
            Packet packet = new Packet(0, false);
            packet.writer.Write(packetId);
            return packet;
        }
        public static bool IsAck(Packet packet)
        {
            return (packet.Command == 0) ? true : false;
        }
        public static void Reset()
        {
            packetId = 0;
        }
        /// <summary>
        /// A shortcut that create a JOINED packet (only for client that made the request)
        /// </summary>
        /// <param name="clientId" type="uint">the id assigned to this client, if is uint.maxvalue the client request will be rejected</param>
        /// <param name="jsonReason" type="string">The reason of join or reject</param>
        /// <returns></returns>
        public static Packet GetJoined(uint clientId, string jsonReason)
        {
            Packet packet = new Packet(2, true);
            packet.Writer.Write(clientId);

            JsonHandler.ErrorMessage message = new JsonHandler.ErrorMessage();
            message.message = jsonReason;
            string reason = JsonConvert.SerializeObject(message);

            packet.Writer.Write(reason);
            return packet;
        }
        /// <summary>
        /// This is the response message to other clients, excluding the request sender
        /// </summary>
        /// <param name="clientId" type="uint"></param>
        /// <param name="jsonReason" type="string"></param>
        /// <returns></returns>
        public static Packet GetClientJoined(uint clientId, string jsonReason)
        {
            Packet packet = new Packet(3, true);
            packet.Writer.Write(clientId);
            //to add other stuffs
            JsonHandler.ErrorMessage message = new JsonHandler.ErrorMessage();
            message.message = jsonReason;
            string reason = JsonConvert.SerializeObject(message);

            packet.Writer.Write(reason);
            return packet;
        }
        public static Packet GetClientKicked(uint clientId, string jsonReason)
        {
            Packet packet = new Packet(4, true);
            packet.Writer.Write(clientId);
            JsonHandler.ErrorMessage message = new JsonHandler.ErrorMessage();
            message.message = jsonReason;
            string reason = JsonConvert.SerializeObject(message);

            packet.Writer.Write(reason);
            return packet;
        }
    }
}
