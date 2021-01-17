using System;

namespace Template
{
    [Serializable]
    public class AccountTemplate
    {
        private string userName;
        private string password;
        public string UserName
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }
    }
}
