using System;

namespace PPDUpdater
{
    /// <summary>
    /// 行う処理です（引数の -d でダウンロードだけ、-i でインストールだけ）。
    /// </summary>
    [Flags]
    public enum WorkState
    {
        None = 0,
        DownLoad = 1,
        Install = 2,
        DownloadAndInstall = 3
    }
}
