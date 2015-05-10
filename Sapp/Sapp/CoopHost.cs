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
            _listening = false;

            SendMessageToClients("DISCONNECT", -1);
            Thread.Sleep(100);
            _clientsRegistered.StopHosting();
        }

        public void Launch(int appID)
        {
            SendMessageToClients("LAUNCH", appID);
        }

        public void SendMessageToClients(string action, int appID)
        {
            if (!_listening || _clientsRegistered == null)
                return;

            DataContainer launchMessage = new DataContainer();
            launchMessage.RequestedAction = action;
            launchMessage.AppID = appID;

            foreach (TcpClient reciever in _clientsRegistered.GetClients())
            {
                CoopUtils.SendMessage(launchMessage, reciever);
            }
        }

        public void CoopHostThread()
        {
            if (_listening)
            {
                return;
            }

            _listening = true;

            TcpListener listener = new TcpListener(IPAddress.Any, _port);

            try
            {
                listener.Start();
            }
            catch
            {
                //Failed because port in use
                //TODO: Make an error window
                return;
            }

            _clientsRegistered = new JoinObserver();
            TcpClient clientJoining;

            while (_listening)
            {
                Thread.Sleep(500);
                if (listener.Pending())
                {

                    clientJoining = listener.AcceptTcpClient();
                    string temp = IPAddress.Parse(((IPEndPoint)clientJoining.Client.RemoteEndPoint).Address.ToString()).ToString();

                    DataContainer message = CoopUtils.ProcessMessage(clientJoining, 10 * 1000);
                    DataContainer reply = new DataContainer();

                    //wrong password, continue looping
                    if (message.RequestedAction.Equals("UNREGISTER"))
                    {
                        _clientsRegistered.Unregister(temp);
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

                    reply.PasswordOK = true;
                    _clientsRegistered.Register(clientJoining, (GamesList)message.Games, message.Name);

                    CoopUtils.SendMessage(reply, clientJoining);

                }
            }
        }
    }
}
