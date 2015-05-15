using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sapp
{
    public class FriendsList
    {
        private string _friendsJoinedList;
        private static FriendsList instance = new FriendsList();

        private FriendsList()
        {

        }

        public void ClearList()
        {
            _friendsJoinedList = "";
        }

        public static FriendsList GetInstance()
        {
            return instance;
        }

        public string GetList()
        {
            return _friendsJoinedList;
        }

        public void SetList(string list)
        {
            _friendsJoinedList = list + "\n";
        }

        public void AddFriend(string friend)
        {
            if (_friendsJoinedList != null)
                _friendsJoinedList += friend + "\n";
        }

        public void RemoveFriend(string friend)
        {
            if (_friendsJoinedList != null)
                _friendsJoinedList = _friendsJoinedList.Substring(0, _friendsJoinedList.IndexOf(friend)) + //append strings together, exclude name leaving
                    _friendsJoinedList.Substring(_friendsJoinedList.IndexOf(friend) + friend.Length + 1);//+1 for \n
        }

    }
}
