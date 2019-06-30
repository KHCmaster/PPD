using System;

namespace PPDFramework
{
    /// <summary>
    /// PPD例外タイプ
    /// </summary>
    public enum PPDExceptionType
    {
        /// <summary>
        /// songsファイルが空
        /// </summary>
        NoSong = 0,
        /// <summary>
        /// 動画を開けない
        /// </summary>
        CannotOpenMovie = 1,
        /// <summary>
        /// 動画ファイルが無い
        /// </summary>
        NoMovieFileInDirectory = 2,
        /// <summary>
        /// 致命的なエラー
        /// </summary>
        FatalError = 3,
        /// <summary>
        /// スキンが適切に実装されていない
        /// </summary>
        SkinIsNotCorrectlyImplemented = 4,
        /// <summary>
        /// サウンド読み込みエラー
        /// </summary>
        SoundReadError = 5,
        /// <summary>
        /// 無効な文字列
        /// </summary>
        InvalidString = 6,
        /// <summary>
        /// 画像読み込みエラー
        /// </summary>
        ImageReadError = 7
    }

    /// <summary>
    /// PPD例外クラス
    /// </summary>
    public class PPDException : Exception
    {
        /// <summary>
        /// 例外タイプ
        /// </summary>
        public PPDExceptionType ExceptionType
        {
            get;
            private set;
        }
        /// <summary>
        /// 詳細の文字列
        /// </summary>
        public string Detail
        {
            get;
            private set;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="type">例外タイプ</param>
        /// <param name="exception">例外</param>
        public PPDException(PPDExceptionType type, Exception exception) :
            this(type, "Fatal Error in PPD", exception)
        {
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="type">例外タイプ</param>
        /// <param name="detail">詳細</param>
        /// <param name="exception">例外</param>
        public PPDException(PPDExceptionType type, string detail, Exception exception)
            : base(detail, exception)
        {
            ExceptionType = type;
            if (detail == null) detail = "";
            Detail = detail;
        }
    }
}
