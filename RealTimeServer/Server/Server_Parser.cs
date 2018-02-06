using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace RealTimeServer.AutorityServer
{
    public static partial class Server
    {
        public static void ParseJoin(Packet packet)
        {
            packet.Stream.Seek(9, SeekOrigin.Begin);
            string json = packet.Reader.ReadString();
            ServerJsonHandler.Join join = JsonConvert.DeserializeObject<ServerJsonHandler.Join>(json);
            Packet p;
            if (IsClientJoined(join.name))              //is client alredy joined
            {
                p = Packet.GetJoined(uint.MaxValue, "A client with same name alredy joined",TimeStamp);   
                SendPacketInstantly(packet.SourceEp, p);                    //cant enqueue this packet cuz not joined
                return;
            }
           
            if(IsClientJoined(packet.SourceEp))
            {
                p = Packet.GetJoined(uint.MaxValue, "A client with same IP alredy joined", TimeStamp);
                SendPacketInstantly(packet.SourceEp, p);                    //cant enqueue this packet cuz not joined
                return;
            }


            //add to players list
            IClient client = new Client(join.name, packet.SourceEp);
            AddClient(client);
            p = Packet.GetJoined(client.ClientId, "Welcome", TimeStamp);               //positive join
            EnquePacket(client.ClientId, p);

            foreach (var c in connectedClients)                             //alert all joined clients that a new player incoming
            {
                if (c.Key == client.ClientId)
                    continue;

                Packet _p = Packet.GetClientJoined(client.ClientId,string.Format("Player {0} join your match!",client.Name), TimeStamp);
                EnquePacket(c.Key, _p);
            }
        }

        public static void ParseLeave(Packet packet)
        {
            packet.Stream.Seek(9, SeekOrigin.Begin);
            string json = packet.Reader.ReadString();
            ServerJsonHandler.Leave leave = JsonConvert.DeserializeObject<ServerJsonHandler.Leave>(json);

            if (IsClientJoined(leave.clientId))
            {
                //send negative joined
                Packet p = Packet.GetClientKicked(leave.clientId, string.Format("Client {0} was kicked",leave.clientId), TimeStamp);
                SendBroadcast(p);
                return;
            }
        }

        public static void ParseUpdate(Packet packet)
        {

        }

    }
}
