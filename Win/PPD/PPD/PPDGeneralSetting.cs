using PPDFramework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PPD
{
    class PPDGeneralSetting : SettingDataBase
    {
        private static readonly PPDGeneralSetting setting = new();
        public override string Name
        {
            get { return "PPDGeneral.setting"; }
        }

        protected override void OnInitialize()
        {
            IsFirstExecution = true;
        }

        public static PPDGeneralSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public bool IsFirstExecution
        {
            get
            {
                return this["FirstExecution"] == "1";
            }
            set
            {
                this["FirstExecution"] = value ? "1" : "0";
            }
        }

        public MoviePanel.MovieLoopType MovieLoopType
        {
            get
            {
                if (!int.TryParse(this["MovieSecLoop"], out int val))
                {
                    val = 0;
                }
                return (MoviePanel.MovieLoopType)val;
            }
            set
            {
                this["MovieSecLoop"] = ((int)value).ToString();
            }
        }

        public string Username
        {
            get
            {
                return this["PPD_Username"];
            }
            set
            {
                this["PPD_Username"] = value;
            }
        }

        public string Password
        {
            get
            {
                return Restore(this["PPD_Password"]);
            }
            set
            {
                this["PPD_Password"] = Save(value);
            }
        }

        private string Restore(string str)
        {
            return DecryptString(str);
        }

        private string Save(string str)
        {
            return EncryptString(str);
        }


        #region Encrypt & Decrypt
        readonly string password = "i@w8ewgh8faot9aasPIJ(Y8y0ghio";

        private string EncryptString(string sourceString)
        {
            var strBytes = Encoding.UTF8.GetBytes(sourceString);

            using var aes = CreateAes();
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            try
            {
                var encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                return Convert.ToBase64String(encBytes);
            }
            catch
            {
                return "";
            }
        }

        private string DecryptString(string sourceString)
        {
            var strBytes = Convert.FromBase64String(sourceString);

            using var aes = CreateAes();
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            var decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
            return Encoding.UTF8.GetString(decBytes);
        }

        /// <summary>
        /// 保存済みの設定を読めるように、従来（RijndaelManaged + Rfc2898DeriveBytes）と同じ鍵・IV・方式の AES を作ります。
        /// （鍵 256 ビット、ブロック 128 ビット、CBC、PKCS7。鍵と IV は PBKDF2-SHA1 1000 回で導出した 48 バイトを順に使う）
        /// </summary>
        private Aes CreateAes()
        {
            var salt = Encoding.UTF8.GetBytes("jps9u0sfd8kooASP`OP`JSA)(Yhi43");
            var aes = Aes.Create();
            var keyLength = aes.KeySize / 8;
            var ivLength = aes.BlockSize / 8;
            var bytes = Rfc2898DeriveBytes.Pbkdf2(password, salt, 1000, HashAlgorithmName.SHA1, keyLength + ivLength);
            aes.Key = bytes.AsSpan(0, keyLength).ToArray();
            aes.IV = bytes.AsSpan(keyLength, ivLength).ToArray();
            return aes;
        }

        #endregion
    }
}
