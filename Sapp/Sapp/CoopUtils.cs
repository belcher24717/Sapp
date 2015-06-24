using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataOverNetwork;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace Sapp
{
    public static class CoopUtils
    {
        public static string DISCONNECT = "DISCONNECT";
        public static string PRE_REGISTER = "REGISTER";
        public static string LOBBY_FULL = "LOBBY_FULL";
        public static string UPDATE = "UPDATE";
        public static string LAUNCH = "LAUNCH";
        public static string FINALIZE_REGISTER = "FINALIZE_REGISTER";

        public static bool HostListening = false;
        public static bool JoinListening = false;
        public static GamesList CollectivePool = null;
        //seperate this stuff out, most should be in CoopJoin or CoopHost

        //encryption

        public static DataContainer ConstructMessage(string action, string password, GamesList games)
        {
            DataContainer message = new DataContainer();
            message.Password = password;
            message.Games = games;
            message.RequestedAction = action;

            //CoopUtils.SendMessage(message, _host);
            return message;
        }

        public static DataContainer ProcessMessage(TcpClient client, int timeoutInMili)
        {

            client.ReceiveTimeout = timeoutInMili;
            
            MemoryStream stream = null;
            try
            {
                //get the length of the message
                byte[] readMsgLen = new byte[4];
                client.GetStream().Read(readMsgLen, 0, 4);
                int dataLen = BitConverter.ToInt32(readMsgLen, 0);

                //get DataContainer
                byte[] readMsgData = new byte[dataLen];
                client.GetStream().Read(readMsgData, 0, dataLen);

                //create everything to deserialize the data
                stream = new MemoryStream(readMsgData);
            }
            catch
            {
                //LOG
                //DataContainer disconnect = new DataContainer();
               // disconnect.RequestedAction = CoopUtils.DISCONNECT;
                //return disconnect;
            }

            DataContainer dataFromClient = null;

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                dataFromClient = (DataContainer)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                //LOG
                return null;
            }

            return dataFromClient;
        }

        public static void SendMessage(DataContainer message, TcpClient client)
        {
            
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);

            byte[] bytes = stream.ToArray();
            byte[] requestLen = BitConverter.GetBytes((Int32)bytes.Length);

            try
            {
                client.GetStream().Write(requestLen, 0, 4);
                client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                //Console.WriteLine("ERR: Failed to send data");
                //Console.WriteLine(e.StackTrace);
            }

            stream.Position = 0;
        }




        
    }
}
