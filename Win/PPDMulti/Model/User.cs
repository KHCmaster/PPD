using PPDFramework;
using PPDFramework.Web;
using PPDMultiCommon.Model;
using SharpDX;
using System.IO;

namespace PPDMulti.Model
{
    class User : UserInfo
    {
        private Color4 color;

        private static User systemUser = new User
        {
            Name = "",
            Color = PPDColors.White
        };

        public void UpdateImagePath()
        {
            var bytes = WebManager.Instance.GetAccountImage(accountId);
            if (bytes.Length != 0)
            {
                var tempFilePath = Path.GetTempFileName();
                using (FileStream fs = File.Create(tempFilePath))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }

                ImagePath = tempFilePath;
            }
        }

        public static UserInfo SystemUser
        {
            get
            {
                return systemUser;
            }
        }

        public Color4 Color
        {
            get
            {
                return color;
            }
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged("Color");
                }
            }
        }
    }
}
