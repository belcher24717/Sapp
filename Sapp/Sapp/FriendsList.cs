using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sapp
{
    public class FriendsList
    {
        private TextBlock _friendsJoinedList;
        private Dispatcher _listUpdater;
        private static FriendsList instance = new FriendsList();

        private FriendsList()
        {

        }

        public void ClearList()
        {
            _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(()=>_friendsJoinedList.Text = ""));
        }

        public static FriendsList GetInstance()
        {
            return instance;
        }

        public void SetList(ref TextBlock list)
        {
            _friendsJoinedList = list;
            _listUpdater = list.Dispatcher;
        }

        public void AddFriend(string friend)
        {
            if (_friendsJoinedList != null)
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(()=> _friendsJoinedList.Text += friend + "\n"));
        }

        public void RemoveFriend(string friend)
        {
            if (_friendsJoinedList != null)
                _listUpdater.BeginInvoke(DispatcherPriority.Normal, (Action)(()=>_friendsJoinedList.Text = _friendsJoinedList.Text.Substring(0, _friendsJoinedList.Text.IndexOf(friend)) + //append strings together, exclude name leaving
                    _friendsJoinedList.Text.Substring(_friendsJoinedList.Text.IndexOf(friend) + friend.Length + 1)));//+1 for \n
        }

    }
}
