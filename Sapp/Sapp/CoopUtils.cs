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
using System.Xml.Serialization;
using System.Xml;

namespace Sapp
{
    public static class CoopUtils
    {
        public static string DISCONNECT = "DISCONNECT";
        public static string PRE_REGISTER = "PRE_REGISTER";
        public static string LOBBY_FULL = "LOBBY_FULL";
        public static string UPDATE = "UPDATE";
        public static string UPDATE_GAME_POOL = "UPDATE_GAME_POOL";
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
            message.Games = (object)games;
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
            catch (Exception e)
            {
                Logger.Log("NETWORK ERROR: " + e.Message, true);
                //DataContainer disconnect = new DataContainer();
               // disconnect.RequestedAction = CoopUtils.DISCONNECT;
                //return disconnect;
            }

            DataContainer dataFromClient = null;

            try
            {
                XmlSerializer xmlS = new XmlSerializer(typeof(DataContainer));
                XmlTextWriter xmlTW = new XmlTextWriter(stream, Encoding.UTF8);

                dataFromClient = (DataContainer)xmlS.Deserialize(stream);
                //BinaryFormatter formatter = new BinaryFormatter();
                //dataFromClient = (DataContainer)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Logger.Log("NETWORK ERROR: " + e.Message, true);
                return null;
            }

            return dataFromClient;
        }

        public static void SendMessage(DataContainer message, TcpClient client)
        {
            
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                String strXML = null;
                XmlSerializer xmlS = new XmlSerializer(typeof(DataContainer));
                XmlTextWriter xmlTW = new XmlTextWriter(stream, Encoding.UTF8);

                xmlS.Serialize(xmlTW, message);
                stream = (MemoryStream)xmlTW.BaseStream;

                //return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                formatter.Serialize(stream, message);
            }
            catch (Exception e)
            {
                Logger.Log("NETWORK ERROR: " + e.Message, true);
            }

            byte[] bytes = stream.ToArray();
            byte[] requestLen = BitConverter.GetBytes((Int32)bytes.Length);

            try
            {
                client.GetStream().Write(requestLen, 0, 4);
                client.GetStream().Write(bytes, 0, bytes.Length);
                client.GetStream().Flush();
            }
            catch (Exception e)
            {
                Logger.Log("NETWORK ERROR: " + e.Message, true);
            }

            stream.Position = 0;
        }




        
    }
}
