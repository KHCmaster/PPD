using System;
using System.Collections.Generic;
using System.Threading;

namespace PPDFramework.Chars
{
    /// <summary>
    /// 文字のテクスチャを、メインスレッドとは別のスレッドで作ります。
    /// </summary>
    /// <remarks>
    /// メインスレッドで新しい文字が必要になったとき、その場で描くとフレームが重くなる（1 文字あたり 0.数 ms）。
    /// 大きさだけをその場で求めて（文字列の配置は変わらない）、描く処理はここで行い、できた文字から表示される。
    /// </remarks>
    sealed class CharTextureWorker : IDisposable
    {
        readonly Queue<CharCacheInfo> queue = new();
        readonly Thread thread;
        bool stopped;

        public CharTextureWorker()
        {
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "CharTextureWorker",
            };
            thread.Start();
        }

        public void Enqueue(CharCacheInfo info)
        {
            lock (queue)
            {
                queue.Enqueue(info);
                Monitor.Pulse(queue);
            }
        }

        private void Run()
        {
            while (true)
            {
                CharCacheInfo info;
                lock (queue)
                {
                    while (queue.Count == 0 && !stopped)
                    {
                        Monitor.Wait(queue);
                    }
                    if (stopped)
                    {
                        return;
                    }
                    info = queue.Dequeue();
                }
                try
                {
                    info.CreateTextureOnWorker();
                }
                catch
                {
                    // 作れなかった文字は表示されないが、ゲームは続ける
                }
            }
        }

        public void Dispose()
        {
            lock (queue)
            {
                stopped = true;
                queue.Clear();
                Monitor.Pulse(queue);
            }
            // 描いている途中の文字が終わるのを少し待つ（終わらなくてもバックグラウンドのスレッドなので、終了は妨げない）
            thread.Join(1000);
        }
    }
}
