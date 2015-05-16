using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapp
{
    public abstract class Coop
    {
        protected int _port;
        protected string _password;
        protected bool _listening;
        protected string _nickname;

        public Coop(int port, string pass)
        {
            //use this to get encryption (default will be blank anyways)
            SetPassword(pass);
            _listening = false;
            _port = port;
        }

        public void SetPassword(string pass)
        {
            //TODO: encrypt
            _password = pass;
        }

        public void SetName(string name)
        {
            if (name.Trim().Equals(""))
            {
                Settings temp = Settings.GetInstance();
                if (temp != null)
                {
                    _nickname = temp.UserID;
                }
            }
            else
                _nickname = name;
        }

        public void SetPort(int port)
        {
            _port = port;
        }
    }
}
