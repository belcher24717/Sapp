using DataOverNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Sapp
{
    public class CoopHost : Coop
    {
        //private cant remember, but save the joiners info to send them messages
        private JoinObserver _clientsRegistered;
        private static CoopHost _instance = null;
        Action _blanketUpdate;
        private bool _gamePoolChanged;

        public const int MAX_ALLOWED_IN_LOBBY = 8; //+1 which is the host for a total of 8

        private CoopHost() : base(7780, "")
        {
            _gamePoolChanged = false;
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

        public void SetBlanketUpdateMethod(Action method)
        {
            _blanketUpdate = method;
        }

        public void StartHost()
        {
            if (CoopUtils.HostListening)
                return;

            Thread host = new Thread(new ThreadStart(CoopHostThread));
            host.SetApartmentState(ApartmentState.STA);
            host.Start();

            /*Task.Factory.StartNew(() =>
            {
                CoopHostThread();
            });*/
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

            SendMessageToClients(message);
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

            Logger.Log("HOST: hosting", true);

            while (_listening)
            {
                Thread.Sleep(100);
                clientJoining = null;
                if (listener.Pending())
                {
                    clientJoining = listener.AcceptTcpClient();
                    string clientIP = IPAddress.Parse(((IPEndPoint)clientJoining.Client.RemoteEndPoint).Address.ToString()).ToString();

                    DataContainer message = CoopUtils.ProcessMessage(clientJoining, 10 * 1000);

                    if (message == null)
                        continue;

                    DataContainer reply = new DataContainer();

                    //wrong password, continue looping
                    if (message.RequestedAction.Equals(CoopUtils.DISCONNECT))
                    {
                        DisconnectClient(clientIP);
                    }
                    else if (message.RequestedAction.Equals(CoopUtils.PRE_REGISTER))
                    {
                        //-1 to include the host
                        if (_clientsRegistered.GetNumberInLobby() >= MAX_ALLOWED_IN_LOBBY - 1)
                        {
                            reply.PasswordOK = false;
                            reply.RequestedAction = CoopUtils.LOBBY_FULL;
                            CoopUtils.SendMessage(reply, clientJoining);
                        }
                        else
                        {
                            reply.RequestedAction = CoopUtils.FINALIZE_REGISTER;//tell the joiner to finalize registering
                            reply.PasswordOK = message.Password.Equals(_password);

                            if (CoopUtils.CollectivePool == null)
                                reply.Games = MainWindow.GetAllGames().GetIDList();
                            else
                                reply.Games = CoopUtils.CollectivePool.GetIDList();

                            CoopUtils.SendMessage(reply, clientJoining);

                            if (reply.PasswordOK == false)
                            {
                                clientJoining.Close();
                                clientJoining = null;
                            }
                        }
                    }
                    else if(message.RequestedAction.Equals(CoopUtils.FINALIZE_REGISTER))
                    {
                        Int64[] gameIDs = GamesList.ConvertIDList((string)message.Games);
                        _clientsRegistered.Register(clientJoining, gameIDs, message.Name);
                        _clientsRegistered.AddNewGamesToJoinedGames(gameIDs);
                        Logger.Log("HOST: Client " + message.Name + " joined lobby." , true);
                        RunBlanketUpdate();
                    }
                }

                if (_gamePoolChanged)
                {
                    UpdateClientsGamePool();
                    _gamePoolChanged = false;
                }

            }//end while listening

            listener.Stop();
            SetListening(false);
            FriendsList.GetInstance().ClearList();
            CoopUtils.CollectivePool = null;
        }

        private void DisconnectClient(string ip)
        {
            _clientsRegistered.Unregister(ip);
            _clientsRegistered.RefreshCollectiveGames();
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

        public void UpdateClientsGamePool()
        {
            DataContainer message = new DataContainer();
            message.RequestedAction = CoopUtils.UPDATE_GAME_POOL;
            message.Games = (string)MainWindow.GetGamePool().GetIDList();

            SendMessageToClients(message);
        }

        public void UpdateGamePool()
        {
            _gamePoolChanged = true;
        }

        private void RunBlanketUpdate()
        {
            if (_blanketUpdate != null)
                _blanketUpdate.Invoke();
        }
    }
}
