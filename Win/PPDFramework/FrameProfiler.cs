using System;
using System.Diagnostics;

namespace PPDFramework
{
    /// <summary>
    /// 1 フレームの中で計測する区間です。
    /// </summary>
    public enum FrameSection
    {
        /// <summary>
        /// FlowScript の実行
        /// </summary>
        FlowScript,
        /// <summary>
        /// ノーツの更新
        /// </summary>
        Marks,
        /// <summary>
        /// Effect2D の更新
        /// </summary>
        Effect2D,
        /// <summary>
        /// 文字列の作り直し（TextureString）
        /// </summary>
        Text,
        /// <summary>
        /// 文字のテクスチャの作成
        /// </summary>
        CharTexture,
        /// <summary>
        /// 画像の読み込み
        /// </summary>
        ImageLoad,
        /// <summary>
        /// ノーツの作成（スクリプトの処理を含む）
        /// </summary>
        MarkCreate,
        /// <summary>
        /// ノーツを作るときのスクリプトの処理
        /// </summary>
        MarkScript,
        /// <summary>
        /// 表示中のノーツの更新
        /// </summary>
        MarkUpdate,
    }

    /// <summary>
    /// メインスレッドの 1 フレームの中で、区間ごとの時間を計測します（FrameTimeStatistics が記録中のときだけ動く）。
    /// </summary>
    /// <remarks>
    /// 区間は入れ子になりうる（FlowScript から文字列を作り直すなど）ので、それぞれ包含時間になる。
    /// 同じ区間の入れ子は、外側だけを数える。メインスレッド以外（読み込みのスレッドなど）の呼び出しは数えない。
    /// </remarks>
    public static class FrameProfiler
    {
        internal static readonly int SectionCount = Enum.GetValues<FrameSection>().Length;

        static readonly long[] ticks = new long[SectionCount];
        static readonly int[] depths = new int[SectionCount];
        static volatile bool enabled;
        static int mainThreadId;
        // このフレームで起きた、重い処理の内容（文字のテクスチャの作成など）。重いフレームのログに出す
        static readonly System.Text.StringBuilder notes = new();
        const int MaxNotesLength = 200;

        /// <summary>
        /// 計測中かどうかを取得します（メインスレッドで、記録中のとき）。Note に渡す文字列を作る前の判定に使います。
        /// </summary>
        public static bool IsRecording
        {
            get
            {
                return enabled && Environment.CurrentManagedThreadId == mainThreadId;
            }
        }

        /// <summary>
        /// 区間を計測します。using で囲んで使います。
        /// </summary>
        public static Scope Measure(FrameSection section)
        {
            if (!enabled || Environment.CurrentManagedThreadId != mainThreadId)
            {
                return default;
            }
            var index = (int)section;
            if (depths[index]++ > 0)
            {
                return new Scope(index, 0);
            }
            return new Scope(index, Stopwatch.GetTimestamp());
        }

        /// <summary>
        /// フレームの始めに、メインスレッドから呼びます。
        /// </summary>
        internal static void BeginFrame(bool recording)
        {
            enabled = recording;
            mainThreadId = Environment.CurrentManagedThreadId;
            Array.Clear(ticks);
            Array.Clear(depths);
            notes.Clear();
        }

        /// <summary>
        /// このフレームで行った重い処理の内容を記録します（メインスレッドで、記録中のときだけ）。
        /// </summary>
        public static void Note(string note)
        {
            if (!enabled || Environment.CurrentManagedThreadId != mainThreadId || notes.Length >= MaxNotesLength)
            {
                return;
            }
            if (notes.Length > 0)
            {
                notes.Append(' ');
            }
            notes.Append(note);
        }

        /// <summary>
        /// このフレームの記録を取得します。
        /// </summary>
        internal static string GetNotes()
        {
            return notes.ToString();
        }

        /// <summary>
        /// このフレームの区間ごとの時間（ミリ秒）を書き込みます。
        /// </summary>
        internal static void CopyMilliseconds(Span<float> destination)
        {
            for (var i = 0; i < SectionCount; i++)
            {
                destination[i] = (float)(ticks[i] * 1000.0 / Stopwatch.Frequency);
            }
        }

        /// <summary>
        /// 計測の区間です。
        /// </summary>
        public readonly struct Scope : IDisposable
        {
            readonly int index;
            readonly long start;
            readonly bool active;

            internal Scope(int index, long start)
            {
                this.index = index;
                this.start = start;
                active = true;
            }

            /// <summary>
            /// 区間を終えます。
            /// </summary>
            public void Dispose()
            {
                if (!active)
                {
                    return;
                }
                depths[index]--;
                if (start != 0)
                {
                    ticks[index] += Stopwatch.GetTimestamp() - start;
                }
            }
        }
    }
}
