using PPDFrameworkCore;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// ゲームホストインターフェース
    /// </summary>
    public interface IGameHost : ITimerManager
    {
        /// <summary>
        /// 動画を取得する
        /// </summary>
        /// <returns></returns>
        IMovie GetMovie(SongInformation songInformation);
        /// <summary>
        /// テキストボックスの有効無効を切り替えます
        /// </summary>
        bool TextBoxEnabled { get; set; }
        /// <summary>
        /// テキストボックスのテキストを取得、設定します
        /// </summary>
        string TextBoxText { get; set; }
        /// <summary>
        /// テキストボックスの位置を取得、設定します
        /// </summary>
        Vector2 TextBoxLocation { get; set; }
        /// <summary>
        /// テキストボックスの選択範囲を取得、設定します
        /// </summary>
        TextBoxSelection TextBoxSelection { get; set; }
        /// <summary>
        /// テキストボックスのキャレットの位置を取得、設定します
        /// </summary>
        int TextBoxCaretIndex { get; set; }
        /// <summary>
        /// テキストボックスのフォントサイズを取得、設定します
        /// </summary>
        int TextBoxFontSize { get; set; }
        /// <summary>
        /// エンターキーでテキストボックスが閉じられたか
        /// </summary>
        bool TextBoxEnterClosed { get; }
        /// <summary>
        /// IME入力が開始された
        /// </summary>
        event EventHandler IMEStarted;
        /// <summary>
        /// テキストボックスの有効状態が変更された
        /// </summary>
        event EventHandler TextBoxEnabledChanged;
        /// <summary>
        /// 終了する
        /// </summary>
        void Exit();
        /// <summary>
        /// 終了をキャンセルする
        /// </summary>
        void CancelExit();
        /// <summary>
        /// ホームに行く
        /// </summary>
        void GoHome();
        /// <summary>
        /// 終了しようとしているか
        /// </summary>
        bool IsCloseRequired { get; }
        /// <summary>
        /// ホームに行けるかどうか（今、ホームでないか）
        /// </summary>
        bool CanGoHome { get; }
        /// <summary>
        /// ウィンドウがアクティブかどうか
        /// </summary>
        bool IsWindowActive { get; }

        /// <summary>
        /// ウィンドウハンドル。
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// ゲームタイマー。
        /// </summary>
        GameTimer GameTimer { get; }

        /// <summary>
        /// FPS。
        /// </summary>
        float FPS { get; }

        /// <summary>
        /// 最後の更新でかかった時間。
        /// </summary>
        float LastUpdateTime { get; }

        /// <summary>
        /// 最後の描画でかかった時間。
        /// </summary>
        float LastDrawTime { get; }

        /// <summary>
        /// クリッピングを設定します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void SetClipping(int x, int y, int width, int height);

        /// <summary>
        /// クリッピング設定を解除します。
        /// </summary>
        void RestoreClipping();

        /// <summary>
        /// 情報表示をします。
        /// </summary>
        /// <param name="text">情報。</param>
        void AddNotify(string text);

        /// <summary>
        /// スクリーンショットを保存します。
        /// </summary>
        /// <param name="filePath">ファイルパス。</param>
        /// <param name="savedCallback">保存されたときのコールバック。</param>
        void SaveScreenShot(string filePath, Action<string> savedCallback);

        /// <summary>
        /// ローディングに情報を送信します。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        void SendToLoading(Dictionary<string, object> parameters);
    }
}
