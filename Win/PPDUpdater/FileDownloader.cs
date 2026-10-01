using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PPDUpdater
{
    /// <summary>
    /// ファイルをダウンロードします（旧形式の WebClient.DownloadFileAsync の代わり）。
    /// WebClient と同じく、進捗と完了のイベントは DownloadFileAsync を呼んだスレッドの SynchronizationContext で発生します。
    /// </summary>
    sealed class FileDownloader
    {
        private static readonly HttpClient client = new(new HttpClientHandler { UseCookies = false })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        /// <summary>
        /// 進捗（0～100）が変わったときに発生します。
        /// </summary>
        public event Action<int> ProgressChanged;

        /// <summary>
        /// 完了したときに発生します。引数は失敗したときの例外で、成功したときは null です。
        /// </summary>
        public event Action<Exception> Completed;

        /// <summary>
        /// ダウンロードを始めます。
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="filePath">保存先。</param>
        public void DownloadFileAsync(Uri uri, string filePath)
        {
            var context = SynchronizationContext.Current;
            Task.Run(() => Download(uri, filePath, context));
        }

        private async Task Download(Uri uri, string filePath, SynchronizationContext context)
        {
            Exception error = null;
            try
            {
                using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var total = response.Content.Headers.ContentLength ?? -1;
                    using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var destination = File.Create(filePath))
                    {
                        var buffer = new byte[81920];
                        long received = 0;
                        var lastPercentage = -1;
                        int read;
                        while ((read = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                        {
                            await destination.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                            received += read;
                            var percentage = total > 0 ? (int)(received * 100 / total) : 0;
                            if (percentage != lastPercentage)
                            {
                                lastPercentage = percentage;
                                Raise(context, () => ProgressChanged?.Invoke(percentage));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
            }
            Raise(context, () => Completed?.Invoke(error));
        }

        private static void Raise(SynchronizationContext context, Action action)
        {
            if (context != null)
            {
                context.Post(_ => action(), null);
            }
            else
            {
                action();
            }
        }
    }
}