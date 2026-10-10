using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace PPDFrameworkCore
{
    /// <summary>
    /// 同期で HTTP 通信をするクライアントです（旧形式の HttpWebRequest の代わり）。
    /// HttpWebRequest と同じく、Cookie は保持せず、タイムアウトはレスポンスのヘッダーを受け取るまでの時間です。
    /// </summary>
    public static class SyncHttpClient
    {
        /// <summary>
        /// 既定のタイムアウトです（HttpWebRequest と同じ 100 秒）。
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

        private static readonly HttpClient client = CreateClient(null);

        /// <summary>
        /// GET を送ります。成功以外のステータスコードのときは HttpRequestException を投げます。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <param name="credentials">認証情報（null なら使わない）。</param>
        /// <returns></returns>
        public static HttpResponse Get(string url, TimeSpan? timeout = null, ICredentials credentials = null)
        {
            return Send(new HttpRequestMessage(HttpMethod.Get, url), timeout, credentials, true);
        }

        /// <summary>
        /// POST を送ります。成功以外のステータスコードのときは HttpRequestException を投げます。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">送るデータ。</param>
        /// <param name="contentType">Content-Type。</param>
        /// <param name="timeout"></param>
        /// <param name="credentials">認証情報（null なら使わない）。</param>
        /// <returns></returns>
        public static HttpResponse Post(string url, byte[] content, string contentType, TimeSpan? timeout = null, ICredentials credentials = null)
        {
            var body = new ByteArrayContent(content);
            body.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            return Send(new HttpRequestMessage(HttpMethod.Post, url) { Content = body }, timeout, credentials, true);
        }

        /// <summary>
        /// GET を送り、ステータスコードを返します。成功以外のステータスコードでも例外にしません。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static HttpStatusCode GetStatusCode(string url, TimeSpan? timeout = null)
        {
            using var response = Send(new HttpRequestMessage(HttpMethod.Get, url), timeout, null, false);
            return response.StatusCode;
        }

        /// <summary>
        /// GET を送り、本文を文字列で返します。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string GetString(string url, TimeSpan? timeout = null)
        {
            using var response = Get(url, timeout);
            using var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        private static HttpResponse Send(HttpRequestMessage request, TimeSpan? timeout, ICredentials credentials, bool ensureSuccess)
        {
            // 認証情報を使うとき（デバッグ用）だけ、専用のクライアントを作る
            var ownedClient = credentials != null ? CreateClient(credentials) : null;
            try
            {
                HttpResponseMessage message;
                using (request)
                using (var cancellation = new CancellationTokenSource(timeout ?? DefaultTimeout))
                {
                    message = (ownedClient ?? client).Send(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
                }
                if (ensureSuccess && !message.IsSuccessStatusCode)
                {
                    var statusCode = message.StatusCode;
                    message.Dispose();
                    throw new HttpRequestException(String.Format("Response status code does not indicate success: {0}", (int)statusCode), null, statusCode);
                }
                return new HttpResponse(message, ownedClient);
            }
            catch
            {
                ownedClient?.Dispose();
                throw;
            }
        }

        private static HttpClient CreateClient(ICredentials credentials)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false
            };
            if (credentials != null)
            {
                handler.Credentials = credentials;
            }
            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }
    }

    /// <summary>
    /// <see cref="SyncHttpClient"/> のレスポンスです。
    /// </summary>
    public sealed class HttpResponse : IDisposable
    {
        private readonly HttpResponseMessage message;
        private readonly HttpClient ownedClient;
        private Stream stream;

        internal HttpResponse(HttpResponseMessage message, HttpClient ownedClient)
        {
            this.message = message;
            this.ownedClient = ownedClient;
        }

        /// <summary>
        /// ステータスコードを取得します。
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return message.StatusCode; }
        }

        /// <summary>
        /// 本文のストリームを取得します。
        /// </summary>
        /// <returns></returns>
        public Stream GetResponseStream()
        {
            return stream ??= message.Content.ReadAsStream();
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        public void Dispose()
        {
            stream?.Dispose();
            message.Dispose();
            ownedClient?.Dispose();
        }
    }
}
