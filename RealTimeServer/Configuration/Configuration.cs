using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using System.Reflection;

namespace RealTimeServer.Config
{
    /// <summary>
    /// Class Configuration: a static interface for server configuration
    /// </summary>
    public static class Configuration
    {
        //ADD CONFIGURATION FOR PROTOCOL
        #region PACKET
        private static int headerSize;
        /// <summary>
        /// numbers of bytes that compose the packet header
        /// </summary>
        public static int HeaderSize { get { return headerSize; } }


        #endregion PACKET
        #region SOCKET
        private static string bindIpAddress;
        public static string BindIpAddress { get { return bindIpAddress; } }

        private static int bindPort;
        public static int BindPort { get { return bindPort; } }

        private static bool blocking;
        /// <summary>
        /// Is this server blocking
        /// </summary>
        public static bool Blocking { get { return blocking; } }

        private static float sendRate;
        public static float SendRate { get { return sendRate; } }

        private static int receiveMaxBufferSize;
        public static int ReceiveMaxBufferSize { get { return receiveMaxBufferSize; } }
        #endregion SOCKET
        #region LOG
        private static bool logOn;
        public static bool LogOn { get { return logOn; } }
        private static string logPath;
        public static string LogPath { get { return logPath; } }
        #endregion LOG

        #region CLIENTS_MANAGEMENT
        private static int nOfRefusePerClient;
        public static int NOfRefusePerClient { get { return nOfRefusePerClient; } }

        #endregion CLIENTS_MANAGEMENT
        /// <summary>
        /// Read a default configuration file and update all parameters from default path of config file. If not exist throw an exception
        /// </summary>
        /// <returns>return true if configuration update gone ok, false in opposite case</returns>
        public static bool UpdateConfiguration()
        {
            return false;
        }

        /// <summary>
        /// Read a custom configuration file, if file path is wrong, simply ignore this call
        /// </summary>
        /// <param name="path" type="string">the custom path of config file</param>
        /// <returns>return true if configuration update gone ok, false in opposite case</returns>
        public static bool UpdateConfiguration(string path)
        {
            if (!File.Exists(path))
                return false;
            using (StreamReader reader = new StreamReader(path))
            {
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("//")) //is a comment line
                        continue;
                    if (line.Contains("//")) //remove comments from teh string
                    {
                        int index = line.IndexOf('/');
                        line = line.Remove(index);
                    }

                    string[] keyVal = line.Split(':');
                    if (keyVal.Length != 2)  //line not correctly formatted 
                        return false;
                    AssignValue(keyVal[0], keyVal[1]);        
                }
            }
            return true;
        }

        private static void AssignValue(string key, string value)
        {
            switch(key)
            {
                case "headerSize":
                    headerSize = int.Parse(value);
                    break;
                case "bindPort":
                    bindPort = int.Parse(value);
                    break;
                case "bindIpAddress":
                    bindIpAddress = value;
                    break;
                case "blocking":
                    blocking = bool.Parse(value);
                    break;
                case "sendRate":
                    sendRate = float.Parse(value);
                    break;
                case "receiveMaxBufferSize":
                    receiveMaxBufferSize = int.Parse(value);
                    break;
                case "nOfRefusePerClient":
                    nOfRefusePerClient = int.Parse(value);
                    break;
                case "logOn":
                    logOn = bool.Parse(value);
                    break;
                case "logPath":
                    logPath = value;
                    break;

            }
        }
    }
}
