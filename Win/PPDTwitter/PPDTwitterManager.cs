using CoreTweet;
using System;
using System.IO;

namespace PPDTwitter
{
    public class PPDTwitterManager
    {
        OAuth.OAuthSession session;
        Tokens tokens;

        public PPDTwitterManager(string consumerKey, string consumerSecret)
        {
            session = OAuth.Authorize(consumerKey, consumerSecret);
        }

        public PPDTwitterManager(string accessToken, string accessTokenSecret, string consumerKey, string consumerSecret)
        {
            session = OAuth.Authorize(consumerKey, consumerSecret);
            tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        public Uri GetAuthorizationUri()
        {
            return session.AuthorizeUri;
        }

        public bool GetTokens(string verifier, out string accessToken, out string accessTokenSecret)
        {
            accessToken = null;
            accessTokenSecret = null;
            try
            {
                tokens = session.GetTokens(verifier);
                accessToken = tokens.AccessToken;
                accessTokenSecret = tokens.AccessTokenSecret;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Tweet(string status)
        {
            try
            {
                var statusResponse = tokens.Statuses.Update(status);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TweetWithMedia(string status, Stream stream)
        {
            try
            {
                var mediaResponse = tokens.Media.Upload(stream);
                var statusResponse = tokens.Statuses.Update(new
                {
#pragma warning disable RECS0069 // Redundant explicit property name
                    status = status,
#pragma warning restore RECS0069 // Redundant explicit property name
                    media_ids = mediaResponse
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
