using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class User
    {
        public User(string _email, string _userName, string _positionName, int _money)
        {
            Email = _email;
            UserName = _userName;
            PositionName = _positionName;
            Money = _money;
        }

        public string Email { get; set; }
        public string UserName { get; set; }
        public string PositionName { get; set; }
        public int Money { get; set; }

        bool IsNewPlayer(string _email)
        {
            return true;
        }

        void UpdateUserPositionId(string id)
        {

        }

    }
}
