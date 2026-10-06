using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace PPDMultiCommon.Web
{
    public class WebManager
    {
        // テストでは localhost のテスト用のサーバーにする（readonly にすると、テストから変えられない）
        private static string baseUrl = @"http://projectdxxx.me" + "/multi-api";

        private string token;
        private bool isRoomExist;

        internal bool CreateRoom(RoomInfo roomInfo)
        {
            try
            {
                var url = String.Format("{0}/createroom?username={1}&roomname={2}&passwordhash={3}&language={4}&port={5}", baseUrl, roomInfo.UserName,
                    HttpUtility.UrlEncode(roomInfo.RoomName), roomInfo.PasswordHash, roomInfo.Language, roomInfo.Port);
                var req = CreateRequest(url);
                using var res = req.GetResponse();
                using XmlReader reader = XmlReader.Create(res.GetResponseStream());
                while (reader.Read())
                {
                    if (reader.IsStartElement("Root"))
                    {
                        token = reader.GetAttribute("Token");
                        isRoomExist = true;
                        break;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal HttpStatusCode UpdateRoomInfo(int playerCount, string scoreHash)
        {
            if (!isRoomExist)
            {
                return HttpStatusCode.OK;
            }

            var url = String.Format("{0}/updateroominfo?token={1}&playercount={2}&scorehash={3}", baseUrl, token, playerCount, scoreHash);
            // 成功以外のステータスコードも、例外にせずそのまま返す
            return SyncHttpClient.GetStatusCode(url);
        }

        internal void DeleteRoom()
        {
            if (!isRoomExist)
            {
                return;
            }

            try
            {
                var url = String.Format("{0}/deleteroom?token={1}", baseUrl, token);
                var req = CreateRequest(url);
                using (var res = req.GetResponse())
                {
                }

                isRoomExist = false;
            }
            catch
            {

            }
        }

        public static RoomInfo[] GetRoomList()
        {
            var url = String.Format("{0}/getroomlist", baseUrl);

            var ret = new List<RoomInfo>();
            try
            {
                var req = CreateRequest(url);
                using var res = req.GetResponse();
                using XmlReader reader = XmlReader.Create(res.GetResponseStream());
                while (reader.Read())
                {
                    if (reader.IsStartElement("Room"))
                    {
                        ret.Add(new RoomInfo(
                            reader.GetAttribute("UserName"),
                            reader.GetAttribute("RoomName"),
                            reader.GetAttribute("PasswordHash"),
                            int.Parse(reader.GetAttribute("Port")),
                            reader.GetAttribute("Language"),
                            reader.GetAttribute("IP"),
                            int.Parse(reader.GetAttribute("PlayerCount"))
                        ));
                    }
                }
            }
            catch
            {
            }

            return [.. ret];
        }

        public static PPDScoreInfo[] GetScores()
        {
            try
            {
                var list = new List<PPDScoreInfo>();
                var request = CreateRequest(String.Format("{0}/api/all-score-list", PPDFrameworkCore.Web.WebManager.BaseUrl));
                using (var response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Score"))
                        {
                            string title = reader.GetAttribute("Title"),
                                easy = reader.GetAttribute("Easy"),
                                normal = reader.GetAttribute("Normal"),
                                hard = reader.GetAttribute("Hard"),
                                extreme = reader.GetAttribute("Extreme"),
                                hash = reader.GetAttribute("Hash"),
                                movieUrl = reader.GetAttribute("MovieUrl");
                            list.Add(new PPDScoreInfo
                            {
                                EasyHash = easy,
                                ExtremeHash = extreme,
                                HardHash = hard,
                                NormalHash = normal,
                                Title = title,
                                Hash = hash,
                                MovieUrl = movieUrl
                            });
                        }
                    }
                }
                return [.. list];
            }
            catch
            {
                return [];
            }
        }

        private static string GetString(string url)
        {
            var req = CreateRequest(url);
            using var response = req.GetResponse();
            using StreamReader reader = new(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        private static Request CreateRequest(string url)
        {
            return new Request(url);
        }

        /// <summary>
        /// 送る前のリクエストです（HttpWebRequest の代わり）。
        /// </summary>
        private sealed class Request(string url)
        {
            private readonly string url = url;

            public HttpResponse GetResponse()
            {
                return SyncHttpClient.Get(url);
            }
        }

        public static string GetPasswordHash(string password)
        {
            string val = "a83" + password + "asdo9";
            var array = Encoding.UTF8.GetBytes(val);

            var hash = SHA256.HashData(array);

            var ret = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                ret.AppendFormat("{0:X2}", hash[i]);
            }

            return ret.ToString();
        }
    }
}
