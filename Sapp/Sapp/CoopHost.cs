using DataOverNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sapp
{
    public class CoopHost : Coop
    {
        //private cant remember, but save the joiners info to send them messages
        private JoinObserver _clientsRegistered;
        private static CoopHost _instance = null;

        public const int MAX_ALLOWED_IN_LOBBY = 10; //+1 which is the host for a total of 8

        private CoopHost() : base(7780, "")
        {

        }

        public bool IsHosting()
        {
            return _listening;
        }

        public static CoopHost GetInstance()
        {
            if (_instance == null)
                _instance = new CoopHost();

            return _instance;
        }

        public void StartHost()
        {
            if (CoopUtils.HostListening)
                return;

            Task.Factory.StartNew(() =>
            {
                CoopHostThread();
            });
        }

        public void StopHost()
        {
            SendMessageToClients(CoopUtils.DISCONNECT, -1);
            Thread.Sleep(100);
            _clientsRegistered.StopHosting();
            SetListening(false);
        }

        public void Launch(Int64 appID)
        {
            SendMessageToClients(CoopUtils.LAUNCH, appID);
        }

        public void SendMessageToClients(string action, Int64 appID)
        {
            if (!_listening || _clientsRegistered == null)
                return;

            DataContainer message = new DataContainer();
            message.RequestedAction = action;
            message.AppID = appID;

            foreach (TcpClient reciever in _clientsRegistered.GetClients())
            {
                CoopUtils.SendMessage(message, reciever);
            }
        }

        public void SendMessageToClients(DataContainer message)
        {
            if (!_listening || _clientsRegistered == null)
                return;

            foreach (TcpClient reciever in _clientsRegistered.GetClients())
            {
                CoopUtils.SendMessage(message, reciever);
            }
        }

        public void CoopHostThread()
        {
            if (_listening)
            {
                return;
            }

            SetListening(true);
            TcpListener listener = new TcpListener(IPAddress.Any, _port);

            try
            {
                listener.Start();
            }
            catch
            {
                //Failed because port in use
                //TODO: Make an error window
                SetListening(false);
                return;
            }

            _clientsRegistered = new JoinObserver();
            TcpClient clientJoining;
            FriendsList.GetInstance().AddFriend(_nickname);

            while (_listening)
            {
                Thread.Sleep(500);
                clientJoining = null;
                if (listener.Pending())
                {

                    clientJoining = listener.AcceptTcpClient();
                    string temp = IPAddress.Parse(((IPEndPoint)clientJoining.Client.RemoteEndPoint).Address.ToString()).ToString();

                    DataContainer message = CoopUtils.ProcessMessage(clientJoining, 10 * 1000);

                    if (message == null)
                        continue;

                    DataContainer reply = new DataContainer();

                    //wrong password, continue looping
                    if (message.RequestedAction.Equals(CoopUtils.DISCONNECT))
                    {
                        _clientsRegistered.Unregister(temp);
                        _clientsRegistered.RefreshCollectiveGames();
                        continue;
                    }

                    if (!message.Password.Equals(_password))
                    {
                        reply.PasswordOK = false;
                        CoopUtils.SendMessage(reply, clientJoining);
                        clientJoining.Close();
                        clientJoining = null;
                        continue;
                    }

                    else if (message.RequestedAction.Equals(CoopUtils.REGISTER))
                    {
                        //-1 to include the host
                        if (_clientsRegistered.GetNumberInLobby() >= MAX_ALLOWED_IN_LOBBY - 1)
                        {
                            reply.RequestedAction = CoopUtils.LOBBY_FULL;
                            CoopUtils.SendMessage(reply, clientJoining);
                        }
                        else
                        {
                            reply.PasswordOK = true;

                            //need to send this message first so that the Joined person knows the password is good
                            CoopUtils.SendMessage(reply, clientJoining);
                            _clientsRegistered.Register(clientJoining, (GamesList)message.Games, message.Name);
                            _clientsRegistered.AddNewGamesToJoinedGames((GamesList)message.Games);
                        }
                        
                        //CoopUtils.SendMessage(reply, clientJoining);
                        
                    }

                }
            }//end while listening

            listener.Stop();
            SetListening(false);
            FriendsList.GetInstance().ClearList();
        }

        public void UpdateJoinedFriends()
        {
            DataContainer message = new DataContainer();
            message.RequestedAction = CoopUtils.UPDATE;
            message.Name = FriendsList.GetInstance().GetLobbyList();

            SendMessageToClients(message);
        }

        public void SetListening(bool val)
        {
            CoopUtils.HostListening = val;
            _listening = val;
        }

    }
}
