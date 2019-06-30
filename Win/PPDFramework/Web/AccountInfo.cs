using System;

namespace PPDFramework.Web
{
    class AccountInfo
    {
        public AccountInfo()
        {
            IsValid = false;
            var random = new Random();
            UserName = AccountId = "Guest" + random.Next(0, ushort.MaxValue);
        }

        public AccountInfo(string accountId, string userName, string token, int ppdy)
        {
            AccountId = accountId;
            UserName = userName;
            Token = token;
            IsValid = true;
            PPDY = ppdy;
        }

        public string AccountId
        {
            get;
            internal set;
        }

        public string UserName
        {
            get;
            internal set;
        }

        public string Token
        {
            get;
            internal set;
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public int PPDY
        {
            get;
            internal set;
        }
    }
}
