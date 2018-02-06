using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using RealTimeServer.Config;
using System.Diagnostics;

namespace RealTimeServer
{
    public static partial class Server
    {
        #region DELTATIME
        public static float DeltaTime
        {
            get
            {
                return lastTime;
            }
        }
        private static float lastTime;
        public static int TimeStamp { get; private set; }

        private static Stopwatch watch;
        #endregion DELTA_TIME
        #region TIMER
        private static float sendTime;
        private static float sendCD;
        #endregion TIMER

        private static Dictionary<uint, IClient> connectedClients;
        public static int PlayersConnected { get { return connectedClients.Count; } }

        private static Socket socket;
        private static EndPoint myEndpoint;

        private static Dictionary<uint, Queue<Packet>> toSend;  //uint = player id
        /// <summary>
        /// Return the numbers of packet in queue 
        /// </summary>
        public static int PacketsCount
        {
            get
            {
                int i = 0;
                foreach (var item in toSend)
                {
                    i += item.Value.Count;
                }
                return i;
            }
        }

        /// <summary>
        /// Initialize the server context, reading datas from configuration
        /// </summary>
        public static void Init()
        {
            Configuration.UpdateConfiguration("D:/UNITY/ExternalProjectsRandom/Real_Time_Server/RealTimeServer.Test/Assets/config_fullparam.txt");
            Client.Reset();

            //internal
            toSend = new Dictionary<uint, Queue<Packet>>();
            connectedClients = new Dictionary<uint, IClient>();


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //configuration
            myEndpoint = new IPEndPoint(IPAddress.Parse(Configuration.BindIpAddress), Configuration.BindPort);
            socket.Blocking = Configuration.Blocking;


            watch = new Stopwatch();
            watch.Start();
            sendTime = Configuration.SendRate;
            sendCD = sendTime;
        }
        /// <summary>
        /// Bind a socket on a local ip address
        /// </summary>
        public static void Bind()
        {
            socket.Bind(myEndpoint);
        }

        /// <summary>
        /// Called one time per frame
        /// </summary>
        public static void Update()
        {
            UpdateClients();



            Receive();


            //delta time
            lastTime = watch.Elapsed.TotalSeconds > 0 ? (float)watch.Elapsed.TotalSeconds : 0f;
            TimeStamp += (int)(DeltaTime * 1000);
            watch.Reset();
            watch.Start();
        }

        #region PACKET_QUEUE
        /// <summary>
        /// Put the packet in a queue automatically managed from server
        /// </summary>
        /// <param name="playerId" type="uint"></param>
        /// <param name="packet" type="Packet"></param>
        public static void EnquePacket(uint playerId, Packet packet)
        {
            if (!toSend.ContainsKey(playerId))
                toSend.Add(playerId, new Queue<Packet>());
            toSend[playerId].Enqueue(packet);
        }
        /// <summary>
        /// Send a packet instantly to specific client, this call bypass the queue system
        /// </summary>
        /// <param name="playerId" type="uint"></param>
        /// <param name="packet" type="Packet"></param>
        public static void SendPacketInstantly(uint playerId, Packet packet)
        {
            //socket.SendTo(packet.Buffer,)//endpoint from players dic
        }
        public static void SendPacketInstantly(EndPoint ep, Packet packet)
        {
            socket.SendTo(packet.Buffer, ep);
        }
        /// <summary>
        /// Send a packet to all connected clients
        /// </summary>
        /// <param name="packet" type="Packet"></param>
        /// <param name="ignoredClient" type="uint">the id assigned to a client will be ignored, if not specified, all clients will be considered</param>
        public static void SendBroadcast(Packet packet, uint ignoredClient = uint.MaxValue)
        {
            foreach (var client in connectedClients)
            {
                if (ignoredClient == client.Key)
                    continue;
                EnquePacket(client.Key, packet);
            }
        }
        /// <summary>
        /// Send all queued packets to the specified client, it's automatically called at specific rate from configuration
        /// </summary>
        public static void DequeuePackets()
        {
            foreach (var item in toSend)
            {
                while (item.Value.Count > 0)
                {
                    Packet p = item.Value.Dequeue();
                    //socket.SendTo(p.buffer,)//endpoint from players dic
                }
            }
        }
        #endregion PACKET_QUEUE

        #region RECEIVE
        public static void Receive()
        {
            byte[] receiveBuffer = new byte[Configuration.ReceiveMaxBufferSize];
            if (socket.Available <= 0)      //MUST BE REPLACED
                return;

            EndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000);
            int amount = socket.ReceiveFrom(receiveBuffer, ref ep);
            if (amount > Configuration.ReceiveMaxBufferSize) //not able to parse this datas
                return;

            //check this ep is a joined player? if not return  (mark ep as not reliable?)
            if (!IsClientJoined(ep))
                return;

            Packet receivedPacket = new Packet(receiveBuffer, amount, ep);
            if (Packet.IsAck(receivedPacket))
            {
                //EnquePacket(, Packet.CreateAck(receivedPacket.PacketId));// get id from joined players list
                return;
            }
            //read command and parse

            byte command = receivedPacket.Command;
            //MUST BE REFACTORED
            switch (command)
            {
                case 1:                             //join
                    ParseJoin(receivedPacket);
                    break;
                case 5:                             //leave
                    ParseLeave(receivedPacket);
                    break;
                case 10:                            //update
                    ParseUpdate(receivedPacket);
                    break;
            }

        }
        #endregion RECEIVE


        //TO DO: REMOVE CLIENT ID,NAME AND EP
        #region CONNECTED_CLIENTS_MANAGEMENT
        public static void ReconciliationClientMovements()
        {
            //server gets at the specific rate a single update packet 
            //this packet update the movements with simulation
            //at specific rate send to client the correction of the position


            //implement parser for client action like shot, punch ecc
        }
        public static void UpdateClients()
        {
            foreach (var client in connectedClients)
            {
                if (client.Value.RefuseTimes > Configuration.NOfRefusePerClient)
                {
                    //kick client
                    Packet packet = Packet.GetClientKicked(client.Key, string.Format("Client {0} kicked for eccessive latency", client.Value.Name));
                    SendBroadcast(packet); //all clients needs to know about kick
                    RemoveClient(client.Key);
                }
            }
        }
        public static void AddClient(IClient client)
        {
            //same id is impossible to happen
            //same end point is accepted

            if (client == null) return;
            if (IsClientJoined(client.Name)) return;    //client with same name is not accepted
            connectedClients.Add(client.ClientId, client);
        }
        public static void RemoveClient(uint clientId)
        {
            if (IsClientJoined(clientId))
                connectedClients.Remove(clientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id" type="uint">id to find from client pool</param>
        /// <returns>returns true if there is a client joined with specific id</returns>
        public static bool IsClientJoined(uint id)
        {
            return connectedClients.ContainsKey(id);
        }
        /// <summary>
        /// Get specific client from connected client list by id
        /// </summary>
        /// <param name="id" type="uint"></param>
        /// <returns>Return null if client is not joined</returns>
        public static IClient GetClient(uint id)
        {
            foreach (var client in connectedClients)
            {
                if (client.Key == id)
                    return client.Value;                    //return client
            }
            return null;                                    //client does not joined            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name" type="string">the name to find</param>
        /// <returns>returns true if there is a client joined with specific name</returns>
        public static bool IsClientJoined(string name)
        {
            foreach (var client in connectedClients)
                if (client.Value.Name == name)
                    return true;
            return false;
        }
        /// <summary>
        /// Get specific client from connected client list by name
        /// </summary>
        /// <param name="name" type="string"></param>
        /// <returns>Return null if client is not joined</returns>
        public static IClient GetClient(string name)
        {
            foreach (var client in connectedClients.Values)
            {
                if (client.Name == name)
                    return client;                          //return client
            }
            return null;                                    //client does not joined       
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep" type="EndPoint">the endpoint to find</param>
        /// <returns>returns true if there is a client joined with specific end point</returns>
        public static bool IsClientJoined(EndPoint ep)
        {
            IPEndPoint _ep = (IPEndPoint)ep;
            foreach (var client in connectedClients)
            {
                IPEndPoint ip = (IPEndPoint)client.Value.EP;
                if (ip.Address.ToString() == _ep.Address.ToString() && ip.Port == _ep.Port)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Get specific client from connected client list by end point
        /// </summary>
        /// <param name="ep" type="EndPoint"></param>
        /// <returns>Return null if client is not joined</returns>
        public static IClient GetClient(EndPoint ep)
        {
            IPEndPoint _ep = (IPEndPoint)ep;                        //save reference to wanted ep as IpEndPoint
            foreach (var client in connectedClients.Values)
            {
                IPEndPoint ip = (IPEndPoint)client.EP;              //save reference to current client ep as IpEndPoint
                if (ip.Address.ToString() == _ep.Address.ToString() && ip.Port == _ep.Port) //compare this end points
                    return client;
            }
            return null;                                            //no one client is joined with this ep
        }
        #endregion CONNECTED_CLIENTS_MANAGEMENT

        

    }
}
