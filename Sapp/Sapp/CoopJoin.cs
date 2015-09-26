using DataOverNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sapp
{
    public class CoopJoin : Coop
    {
        private string _ipJoining;
        private GamesList _myGames = null;
        private TcpClient _host;
        private DataGrid _gamePool;
        private Dispatcher _gamePoolUpdater; 
        private DataGrid _removedPool;
        private Dispatcher _removedPoolUpdater;

        private bool _justDisconnected;
        private bool _allowLaunch;

        private static CoopJoin _instance = null;

        private CoopJoin() : base(7780, "")
        {
            _allowLaunch = true;
        }

        public static CoopJoin GetInstance()
        {
            if (_instance == null)
                _instance = new CoopJoin();

            return (CoopJoin)_instance;
        }

        public bool IsJoined()
        {
            return _listening;
        }

        public void SetGamesList(GamesList list)
        {
            _myGames = list;
        }

        public void SetGamePools(ref DataGrid gamePool, ref DataGrid removedPool)
        {
            _gamePool = gamePool;
            _removedPool = removedPool;

            _gamePoolUpdater = gamePool.Dispatcher;
            _removedPoolUpdater = removedPool.Dispatcher;
        }

        public bool SetIpJoining(string ip)
        {
            if (ip.Equals("localhost"))
            {
                _ipJoining = ip;
                return true;
            }

            bool realIP = true;

            string[] test = ip.Split('.');
            if (test.Length == 4)
                foreach (string s in test)
                    try
                    {
                        int.Parse(s);
                    }
                    catch { realIP = false; }
            else
                realIP = false;

            _ipJoining = ip;

            return realIP;
        }

        public void Join()
        {
            if (_listening || _myGames == null)
                return;

            Task.Factory.StartNew(() =>
            {
                CoopJoinThread();
            });
        }

        public void CoopJoinThread()
        {
            if (_listening)
                return;

            _justDisconnected = false;
            SetListening(true);
            if (!TryOpenHost())
                goto StopListening;

            DataContainer message = CoopUtils.ConstructMessage(CoopUtils.PRE_REGISTER, _password, null);
            CoopUtils.SendMessage(message, _host);
            DataContainer passwordOK = CoopUtils.ProcessMessage(_host, 10 * 1000);

            if (passwordOK == null)
            {
                //no response
                DisplayMessage msg = new DisplayMessage("Join Notification", "", System.Windows.Forms.MessageBoxButtons.OK);
                goto StopListening;
            }
            if (passwordOK.PasswordOK != true)
            {
                if (passwordOK.RequestedAction.Equals(CoopUtils.LOBBY_FULL))
                {
                    //lobby is full
                    DisplayMessage msg = new DisplayMessage("Join Notification", "", System.Windows.Forms.MessageBoxButtons.OK);
                }
                else
                {
                    //password was wrong
                    DisplayMessage msg = new DisplayMessage("Join Notification", "", System.Windows.Forms.MessageBoxButtons.OK);
                }
                goto StopListening;
            }
            //just being clear
            else if (passwordOK.PasswordOK == true)
            {
                if (passwordOK.RequestedAction.Equals(CoopUtils.FINALIZE_REGISTER))
                {
                    if (passwordOK.Games == null)
                    {
                        //something went wrong!
                        DisplayMessage msg = new DisplayMessage("Join Notification", "", System.Windows.Forms.MessageBoxButtons.OK);
                        goto StopListening;
                    }
                    List<Game> testSimilarGames = _myGames.Intersect(  (GamesList)passwordOK.Games, new GameEqualityComparer()  ).ToList();
                    if (testSimilarGames.Count == 0)
                    {
                        //No similar games
                        DisplayMessage msg = new DisplayMessage("Join Notification", "", System.Windows.Forms.MessageBoxButtons.OK);
                        goto StopListening;
                    }
                    else
                    {
                        TryOpenHost();//TODO: stop if this fails?

                        message = CoopUtils.ConstructMessage(CoopUtils.FINALIZE_REGISTER, _password, _myGames);
                        message.Name = _nickname;
                        CoopUtils.SendMessage(message, _host);//TODO:what if this doenst get sent? (times out)
                    }
                }
            }

            while (_listening)
            {
                Thread.Sleep(250);

                if (_host == null || !_host.Connected)
                    goto StopListening;

                DataContainer hostMessage = CoopUtils.ProcessMessage(_host, 10 * 1000);

                if(hostMessage == null)
                    continue;

                if (hostMessage.RequestedAction.Equals(CoopUtils.DISCONNECT))
                    SetListening(false);

                else if (hostMessage.RequestedAction.Equals(CoopUtils.UPDATE))
                {
                    if (hostMessage.Name != null)
                        FriendsList.GetInstance().SetFriends(hostMessage.Name);
                }

                else if (hostMessage.RequestedAction.Equals(CoopUtils.UPDATE_GAME_POOL))
                {
                    if (hostMessage.Games == null)
                        continue;

                    UpdateGamePool((GamesList)hostMessage.Games);
                }

                else if (hostMessage.RequestedAction.Equals(CoopUtils.LAUNCH))
                {
                    //TODO: show error message?
                    Game gameToLaunch = _myGames.GetGame(hostMessage.AppID);
                    if (gameToLaunch != null && LaunchAllowed())
                    {
                        gameToLaunch.Launch();
                        //ToggleAllowLaunch(false);
                    }
                }
            }

            StopListening:
                SetListening(false);
                CloseHost();
                FriendsList.GetInstance().ClearList();
                _justDisconnected = true;
        }

        public void GamesRelinked()
        {
            _justDisconnected = false;
        }

        public bool JustDisconnected()
        {
            return _justDisconnected;
        }

        private void UpdateGamePool(GamesList games)
        {
            if (_removedPoolUpdater == null || _removedPool == null || _gamePool == null || _gamePoolUpdater == null)
                return;

            GamesList gamePool = new GamesList();
            GamesList removedPool = new GamesList();
            //TODO Test with reference to GamesList instead. 
            removedPool.AddList((GamesList)_removedPool.ItemsSource);
            removedPool.AddList((GamesList)_gamePool.ItemsSource);
            gamePool.AddList(games);
            removedPool.RemoveList(games);

            _gamePoolUpdater.Invoke(DispatcherPriority.Normal, (Action)(() => _gamePool.ItemsSource = gamePool));
            _removedPoolUpdater.Invoke(DispatcherPriority.Normal, (Action)(() => _removedPool.ItemsSource = removedPool));

            _gamePoolUpdater.Invoke(DispatcherPriority.Normal, (Action)(() => _gamePool.Items.Refresh()));
            _removedPoolUpdater.Invoke(DispatcherPriority.Normal, (Action)(() => _removedPool.Items.Refresh()));
        }

        public void Disconnect()
        {
            FriendsList.GetInstance().ClearList();

            if (!_listening || _host == null)
            {
                SetListening(false);
                return;
            }

            try
            {
                //refresh the connection socket so the message will send
                if (_host.Connected)
                {
                    CloseHost();
                    if (TryOpenHost())
                    {
                        //dont need password to disconnect
                        DataContainer message = CoopUtils.ConstructMessage(CoopUtils.DISCONNECT, "", null);
                        CoopUtils.SendMessage(message, _host);
                        Thread.Sleep(100);
                        CloseHost();
                    }
                }
            }
            catch
            {}

            SetListening(false);
        }

        private void CloseHost()
        {
            if (_host != null)
                _host.Close();
            _host = null;
        }

        private bool TryOpenHost()
        {
            try
            {
                _host = new TcpClient(_ipJoining, _port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetListening(bool val)
        {
            CoopUtils.JoinListening = val;
            _listening = val;
        }

        public void ToggleAllowLaunch(bool changeTo)
        {
            _allowLaunch = changeTo;
        }

        public bool LaunchAllowed()
        {
            return _allowLaunch;
        }
    }
}
