using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace RealTimeServer
{
    public static partial class Server
    {
        public static void ParseJoin(Packet packet)
        {
            packet.Stream.Seek(9, SeekOrigin.Begin);
            string json = packet.Reader.ReadString();
            JsonHandler.Join join = JsonConvert.DeserializeObject<JsonHandler.Join>(json);
            //check if alredy joined
            Packet p;
            if (IsClientJoined(join.name))
            {
                //send negatibve joined
                p = Packet.GetJoined(uint.MaxValue, "A client with same name alredy joined");
                SendPacketInstantly(packet.SourceEp, p);
                return;
            }

            //add to players list
            IClient client = new Client(join.name, packet.SourceEp);
            AddClient(client);
            //SEND POSITIVE JOINED
            p = Packet.GetJoined(client.ClientId, "Welcome");
            EnquePacket(client.ClientId, p);

            //alert all joined clients that a new player incoming
            foreach (var c in connectedClients)
            {
                if (c.Key == client.ClientId)
                    continue;

                Packet _p = Packet.GetClientJoined(client.ClientId,string.Format("Player {0} join your match!",client.Name));
                EnquePacket(c.Key, _p);
            }
        }

        public static void ParseLeave(Packet packet)
        {
            packet.Stream.Seek(9, SeekOrigin.Begin);
            string json = packet.Reader.ReadString();
            JsonHandler.Leave leave = JsonConvert.DeserializeObject<JsonHandler.Leave>(json);

            if (IsClientJoined(leave.clientId))
            {
                //send negative joined
                Packet p = Packet.GetClientKicked(uint.MaxValue, string.Format("Client {0} was kicked",leave.clientId));
                SendBroadcast(p);
                return;
            }
        }
    }
}
