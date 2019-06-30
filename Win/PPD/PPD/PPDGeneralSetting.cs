using PPDFramework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PPD
{
    class PPDGeneralSetting : SettingDataBase
    {
        private static PPDGeneralSetting setting = new PPDGeneralSetting();
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
        string password = "i@w8ewgh8faot9aasPIJ(Y8y0ghio";

        private string EncryptString(string sourceString)
        {
            var rijndael = new RijndaelManaged();
            GenerateKeyFromPassword(password, rijndael.KeySize, out byte[] key, rijndael.BlockSize, out byte[] iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            var strBytes = Encoding.UTF8.GetBytes(sourceString);

            using (ICryptoTransform encryptor = rijndael.CreateEncryptor())
            {
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
        }

        private string DecryptString(string sourceString)
        {
            var rijndael = new RijndaelManaged();
            GenerateKeyFromPassword(password, rijndael.KeySize, out byte[] key, rijndael.BlockSize, out byte[] iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            var strBytes = Convert.FromBase64String(sourceString);

            using (ICryptoTransform decryptor = rijndael.CreateDecryptor())
            {
                var decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                return Encoding.UTF8.GetString(decBytes);
            }
        }

        private void GenerateKeyFromPassword(string password, int keySize, out byte[] key, int blockSize, out byte[] iv)
        {
            var salt = Encoding.UTF8.GetBytes("jps9u0sfd8kooASP`OP`JSA)(Yhi43");
            var deriveBytes = new Rfc2898DeriveBytes(password, salt)
            {
                IterationCount = 1000
            };

            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }

        #endregion
    }
}
