using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sapp
{
    public class FriendsList
    {
        private string _friendsListString;
        private TextBlock _friendsJoinedList;
        private Dispatcher _listUpdater;

        private Label _numInLobby;
        private Dispatcher _labelUpdater;

        private static FriendsList instance = new FriendsList();

        private FriendsList()
        {

        }

        public void ClearList()
        {
            _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsJoinedList.Text = ""));
            _labelUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _numInLobby.Content = ""));
        }

        public static FriendsList GetInstance()
        {
            return instance;
        }

        public void SetList(ref TextBlock list, ref Label numInLobby)
        {
            _friendsJoinedList = list;
            _listUpdater = list.Dispatcher;

            _numInLobby = numInLobby;
            _labelUpdater = numInLobby.Dispatcher;
        }

        public void SetFriends(string wholeList)
        {
            if (_friendsJoinedList != null)
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsJoinedList.Text = wholeList));

            UpdateLobbyCount();
        }

        public void AddFriend(string friend)
        {
            if (_friendsJoinedList != null)
            {
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsJoinedList.Text += friend + "\n"));
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsListString = _friendsJoinedList.Text));
            }

            UpdateLobbyCount();
        }

        public void RemoveFriend(string friend)
        {
            if (_friendsJoinedList != null)
            {
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsJoinedList.Text = _friendsJoinedList.Text.Substring(0, _friendsJoinedList.Text.IndexOf(friend)) + //append strings together, exclude name leaving
                    _friendsJoinedList.Text.Substring(_friendsJoinedList.Text.IndexOf(friend) + friend.Length + 1)));//+1 for \n
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _friendsListString = _friendsJoinedList.Text));
            }

            UpdateLobbyCount();
        }

        public string GetLobbyList()
        {       
            return _friendsListString;
        }

        private void UpdateLobbyCount()
        {
            if (_numInLobby != null)
                _labelUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _numInLobby.Content = "(" + (_friendsJoinedList.Text.Split('\n').Length - 1) + "/13)"));
        }
    }
}
