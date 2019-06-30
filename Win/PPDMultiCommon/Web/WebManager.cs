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
        private static readonly string baseUrl = @"http://projectdxxx.me" + "/multi-api";

        private string token;
        private bool isRoomExist;

        internal bool CreateRoom(RoomInfo roomInfo)
        {
            try
            {
                var url = String.Format("{0}/createroom?username={1}&roomname={2}&passwordhash={3}&language={4}&port={5}", baseUrl, roomInfo.UserName,
                    HttpUtility.UrlEncode(roomInfo.RoomName), roomInfo.PasswordHash, roomInfo.Language, roomInfo.Port);
                var req = CreateRequest(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = XmlReader.Create(res.GetResponseStream()))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                token = reader.GetAttribute("Token");
                                isRoomExist = true;
                                break;
                            }
                        }
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

            try
            {
                var url = String.Format("{0}/updateroominfo?token={1}&playercount={2}&scorehash={3}", baseUrl, token, playerCount, scoreHash);

                var req = CreateRequest(url);
                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    return res.StatusCode;
                }
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                return response.StatusCode;
            }
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
                using (WebResponse res = req.GetResponse())
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
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = XmlReader.Create(res.GetResponseStream()))
                    {
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
                }
            }
            catch
            {
            }

            return ret.ToArray();
        }

        public static PPDScoreInfo[] GetScores()
        {
            try
            {
                var list = new List<PPDScoreInfo>();
                var request = CreateRequest(String.Format("{0}/api/all-score-list", PPDFrameworkCore.Web.WebManager.BaseUrl));
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
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
                return list.ToArray();
            }
            catch
            {
                return new PPDScoreInfo[0];
            }
        }

        private static string GetString(string url)
        {
            var req = CreateRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            return req;
        }

        public static string GetPasswordHash(string password)
        {
            var sha = new SHA256Managed();

            string val = "a83" + password + "asdo9";
            var array = Encoding.UTF8.GetBytes(val);

            var hash = sha.ComputeHash(array);

            var ret = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                ret.AppendFormat("{0:X2}", hash[i]);
            }

            return ret.ToString();
        }
    }
}
