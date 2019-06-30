using PPDMultiCommon.Model;

namespace PPDMultiServer.Model
{
    class UserResult
    {
        public PPDMultiCommon.Model.Result Result
        {
            get;
            set;
        }

        public UserInfo User
        {
            get;
            set;
        }
    }
}
