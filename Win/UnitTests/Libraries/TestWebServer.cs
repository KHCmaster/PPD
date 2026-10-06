using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace UnitTests.Libraries
{
    /// <summary>
    /// テスト用の HTTP サーバーです。localhost（127.0.0.1）の空いているポートで待ち受け、決めた応答を返します。
    /// </summary>
    /// <remarks>
    /// localhost だけで待ち受けるので、ファイアウォールの確認は出ない（HttpListener と違い、URL の予約もいらない）。
    /// 1 つの接続で 1 つの要求だけを受け、応答したら閉じる。
    /// </remarks>
    sealed class TestWebServer : IDisposable
    {
        /// <summary>
        /// 受け取った要求です。
        /// </summary>
        public sealed record Request(string Method, string Path, NameValueCollection Query, NameValueCollection Form);

        /// <summary>
        /// 返す応答です。
        /// </summary>
        public sealed record Response(HttpStatusCode Status, byte[] Body);

        readonly TcpListener listener = new(IPAddress.Loopback, 0);
        readonly Thread thread;
        readonly Dictionary<string, Func<Request, Response>> routes = [];

        /// <summary>
        /// 受け取った要求です（順に）。
        /// </summary>
        public ConcurrentQueue<Request> Requests { get; } = new();

        public string BaseUrl { get; }

        public TestWebServer()
        {
            listener.Start();
            BaseUrl = "http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port;
            thread = new Thread(Serve) { IsBackground = true };
            thread.Start();
        }

        /// <summary>
        /// パス（「/api/login」など。クエリは含めない）への応答を決めます。決めていないパスは 404 です。
        /// </summary>
        public TestWebServer On(string path, Func<Request, Response> handler)
        {
            lock (routes)
            {
                routes[path] = handler;
            }
            return this;
        }

        public TestWebServer OnXml(string path, string xml) => On(path, _ => Xml(xml));

        public static Response Xml(string xml) => new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(xml));

        public static Response Status(HttpStatusCode status) => new(status, []);

        private void Serve()
        {
            while (true)
            {
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (Exception ex) when (ex is SocketException or ObjectDisposedException or InvalidOperationException)
                {
                    return;
                }
                using (client)
                {
                    try
                    {
                        Handle(client.GetStream());
                    }
                    catch (IOException)
                    {
                    }
                }
            }
        }

        private void Handle(NetworkStream stream)
        {
            // 見出し（空の行まで）を読む
            var header = new List<byte>();
            while (header.Count < 4 || !(header[^4] == '\r' && header[^3] == '\n' && header[^2] == '\r' && header[^1] == '\n'))
            {
                var b = stream.ReadByte();
                if (b < 0)
                {
                    return;
                }
                header.Add((byte)b);
            }
            var lines = Encoding.ASCII.GetString([.. header]).Split("\r\n");
            var requestLine = lines[0].Split(' ');
            var contentLength = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    contentLength = int.Parse(line["Content-Length:".Length..].Trim());
                }
            }
            var body = new byte[contentLength];
            stream.ReadExactly(body);

            var target = requestLine[1];
            var queryIndex = target.IndexOf('?');
            var path = queryIndex >= 0 ? target[..queryIndex] : target;
            var request = new Request(requestLine[0], path,
                HttpUtility.ParseQueryString(queryIndex >= 0 ? target[(queryIndex + 1)..] : ""),
                HttpUtility.ParseQueryString(Encoding.ASCII.GetString(body)));
            Requests.Enqueue(request);

            Func<Request, Response> handler;
            lock (routes)
            {
                routes.TryGetValue(path, out handler);
            }
            var response = handler?.Invoke(request) ?? Status(HttpStatusCode.NotFound);
            var head = string.Format("HTTP/1.1 {0} {1}\r\nContent-Type: text/xml; charset=utf-8\r\nContent-Length: {2}\r\nConnection: close\r\n\r\n",
                (int)response.Status, response.Status, response.Body.Length);
            stream.Write(Encoding.ASCII.GetBytes(head));
            stream.Write(response.Body);
        }

        public void Dispose()
        {
            listener.Stop();
            thread.Join(TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// テストの間だけ、PPD の Web の接続先（BaseUrl）をテスト用のサーバーにします。終わったら、接続先とログインの状態を元に戻します。
    /// </summary>
    /// <remarks>
    /// 本物のサーバーには接続しない。デバッグのビルドは今のフォルダーの credentials（認証情報）を読むので、一時フォルダーを今のフォルダーにする。
    /// </remarks>
    sealed class WebTestScope : IDisposable
    {
        static readonly FieldInfo baseUrlField = typeof(PPDFrameworkCore.Web.WebManager).GetField("baseUrl", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo accountField = typeof(PPDFramework.Web.WebManager).GetField("currentAccount", BindingFlags.Static | BindingFlags.NonPublic);

        readonly object savedBaseUrl = baseUrlField.GetValue(null);
        readonly object savedAccount = accountField.GetValue(null);
        readonly TempFolder folder = new(setCurrentDirectory: true);

        public TestWebServer Server { get; } = new();

        public PPDFramework.Web.WebManager Web => PPDFramework.Web.WebManager.Instance;

        public WebTestScope()
        {
            baseUrlField.SetValue(null, Server.BaseUrl);
        }

        public void Dispose()
        {
            baseUrlField.SetValue(null, savedBaseUrl);
            accountField.SetValue(null, savedAccount);
            Server.Dispose();
            folder.Dispose();
        }
    }
}
