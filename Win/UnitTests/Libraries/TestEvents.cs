using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 別のスレッドから届く通知を、届いた順に待ちます（通信のテスト用）。
    /// </summary>
    sealed class TestEvents
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        readonly BlockingCollection<string> items = [];

        /// <summary>
        /// 待っている間に受け取ったものです（失敗のメッセージ用）。
        /// </summary>
        public List<string> All { get; } = [];

        public void Add(string item) => items.Add(item);

        public void WaitFor(string expected)
        {
            while (items.TryTake(out var item, Timeout))
            {
                All.Add(item);
                if (item == expected)
                {
                    return;
                }
            }
            throw new TimeoutException(expected + " が届きません。届いたもの: " + string.Join(", ", All));
        }

        /// <summary>
        /// localhost の空いているポートです。
        /// </summary>
        public static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
