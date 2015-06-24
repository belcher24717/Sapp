using DataOverNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sapp
{
    public class JoinObserver
    {
        private List<JoinObserverClient> _clients;
        public JoinObserver()
        {
            _clients = new List<JoinObserverClient>();
        }

        public int GetNumberInLobby()
        {
            return _clients.Count;
        }

        public void Unregister(string clientAddress)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].Equals(clientAddress))
                {
                    _clients[i].GetClient().Close();

                    FriendsList.GetInstance().RemoveFriend(_clients[i].GetName()); 
                    _clients.RemoveAt(i);
                    CoopHost.GetInstance().UpdateJoinedFriends();
                    break;
                }
            }
        }

        public void Register(TcpClient client, GamesList clientsGames, string name)
        {
            JoinObserverClient newClient = new JoinObserverClient(client, clientsGames, name);
            _clients.Add(newClient);

            FriendsList.GetInstance().AddFriend(newClient.GetName());
            CoopHost.GetInstance().UpdateJoinedFriends();
        }

        public void StopHosting()
        {
            while(_clients.Count > 0)
            {
                _clients[0].GetClient().Close();
                _clients.RemoveAt(0);
            }
            
        }

        public List<TcpClient> GetClients()
        {
            List<TcpClient> listToReturn = new List<TcpClient>();
            foreach (JoinObserverClient joc in _clients)
            {
                listToReturn.Add(joc.GetClient());
            }

            return listToReturn;
        }

        public void RefreshCollectiveGames()
        {
            CoopUtils.CollectivePool = null;
            if (_clients.Count < 1)
                return;

            //Get all the hosts games
            GamesList newCollectiveGames = MainWindow.GetAllGames();

            foreach (JoinObserverClient client in _clients)
                newCollectiveGames = (GamesList)newCollectiveGames.Union(client.GetGames());

            CoopUtils.CollectivePool = newCollectiveGames;
        }
        public void AddNewGamesToJoinedGames(GamesList games)
        {
            if (CoopUtils.CollectivePool == null) 
                CoopUtils.CollectivePool = (GamesList)MainWindow.GetAllGames().Union(games);
            else
                CoopUtils.CollectivePool = (GamesList)CoopUtils.CollectivePool.Union(games);
        }
    }

    public class JoinObserverClient
    {
        TcpClient _client;
        GamesList _games;
        string _name;

        public JoinObserverClient(TcpClient client, GamesList games, string name)
        {
            _name = name;
            _client = client;
            _games = games;
        }

        public TcpClient GetClient()
        {
            return _client;
        }

        public string GetName()
        {
            return _name;
        }

        public GamesList GetGames()
        {
            return _games;
        }

        public override bool Equals(object obj)
        {
            try
            {
                string thatClient = (string)obj;
                string thisClient = IPAddress.Parse(((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString()).ToString();

                return thisClient.Equals(thatClient);
            }
            catch
            {
                return false;
            }
            
        }

    }
}
