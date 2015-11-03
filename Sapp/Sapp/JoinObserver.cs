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

        public void Register(Socket client, GamesList clientsGames, string name)
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

        public List<Socket> GetClients()
        {
            List<Socket> listToReturn = new List<Socket>();
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
                newCollectiveGames.SetList(newCollectiveGames.Intersect(client.GetGames(), new GameEqualityComparer()).ToList());

            CoopUtils.CollectivePool = newCollectiveGames;
        }
        public void AddNewGamesToJoinedGames(GamesList games)
        {
            if (CoopUtils.CollectivePool == null)
            {
                CoopUtils.CollectivePool = new GamesList();
                CoopUtils.CollectivePool.SetList(MainWindow.GetAllGames().Intersect(games, new GameEqualityComparer()).ToList());
            }
            else
                CoopUtils.CollectivePool.SetList((CoopUtils.CollectivePool.Intersect(games, new GameEqualityComparer())).ToList());
        }
    }//TODO: write a setlist in gameslist

    public class JoinObserverClient
    {
        Socket _client;
        GamesList _games;
        string _name;

        public JoinObserverClient(Socket client, GamesList games, string name)
        {
            _name = name;
            _client = client;
            _games = games;
        }

        public Socket GetClient()
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
                string thisClient = (_client.RemoteEndPoint as IPEndPoint).Address.ToString();

                return thisClient.Equals(thatClient);
            }
            catch
            {
                return false;
            }
            
        }

    }

    public class GameEqualityComparer : IEqualityComparer<Game>
    {
        public bool Equals(Game game1, Game game2)
        {
            if (game1 == null && game2 == null)
                return true;
            else if ((game1 != null && game2 == null) ||
                    (game1 == null && game2 != null))
                return false;

            return game1.GetAppID() == game2.GetAppID();
        }

        public int GetHashCode(Game obj)
        {
            return -1;
        }
    }
}
