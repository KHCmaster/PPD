using PPDConfiguration;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PPDFramework
{
    class SongInformationComparer : IComparer<SongInformation>
    {
        #region IComparer<SongInformation> メンバ
        public int Compare(SongInformation x, SongInformation y)
        {
            if (x == null) return -1;
            if (y == null) return 1;
            if (!x.IsPPDSong && !y.IsPPDSong)
            {
                return StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, false).Compare(x.DirectoryName, y.DirectoryName);
            }
            else if (!x.IsPPDSong)
            {
                return -1;
            }
            else if (!y.IsPPDSong)
            {
                return 1;
            }
            else
            {
                return StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, false).Compare(x.DirectoryName, y.DirectoryName);
            }
        }

        #endregion
    }

    /// <summary>
    /// 譜面情報クラス
    /// </summary>
    public class SongInformation
    {
        /// <summary>
        /// 更新されたときに呼ばれます
        /// </summary>
        public static event EventHandler Updated;

        /// <summary>
        /// 利用可能な難易度
        /// </summary>
        [Flags]
        public enum AvailableDifficulty
        {
            /// <summary>
            /// なし
            /// </summary>
            None = 0,
            /// <summary>
            /// EASY
            /// </summary>
            Easy = 1,
            /// <summary>
            /// NORMAL
            /// </summary>
            Normal = 2,
            /// <summary>
            /// HARD
            /// </summary>
            Hard = 4,
            /// <summary>
            /// EXTREME
            /// </summary>
            Extreme = 8
        }
        static AvailableDifficulty[] DifficultyArray = (AvailableDifficulty[])Enum.GetValues(typeof(AvailableDifficulty));

        string directoryname = "";
        string directorypath = "";
        string authorname = "";
        string moviepath = "";
        string[] scores;
        string[] difficultystring;
        float thumbstarttime;
        float thumbendtime;
        float starttime;
        float endtime;
        float bpm;
        float latency;
        MovieTrimmingData trimmingdata;
        int userVolume;
        bool loadFromDir;

        /// <summary>
        /// 譜面のディレクトリか
        /// </summary>
        public bool IsPPDSong
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面作者名
        /// </summary>
        public string AuthorName
        {
            get { return authorname; }
            private set
            {
                authorname = value;
            }
        }

        /// <summary>
        /// ディレクトリのパス
        /// </summary>
        public string DirectoryPath
        {
            get { return directorypath; }
            private set
            {
                directorypath = value;
                ParentDirectory = Path.GetDirectoryName(directorypath);
                DirectoryName = Path.GetFileName(directorypath);
            }
        }

        /// <summary>
        /// ディレクトリ名（譜面名)
        /// </summary>
        public string DirectoryName
        {
            get { return directoryname; }
            private set
            {
                directoryname = value;
            }
        }

        /// <summary>
        /// 動画パス
        /// </summary>
        public string MoviePath
        {
            get
            {
                if (!File.Exists(moviepath) && IsPPDSong)
                {
                    moviepath = FindMovieFile(directorypath);
                }
                return moviepath;
            }
            private set
            {
                moviepath = value;
            }
        }

        /// <summary>
        /// 動画トリミングデータ
        /// </summary>
        public MovieTrimmingData TrimmingData
        {
            get { return trimmingdata; }
            private set
            {
                trimmingdata = value;
            }
        }

        /// <summary>
        /// 親ディレクトリ
        /// </summary>
        public string ParentDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// 利用可能な難易度
        /// </summary>
        public AvailableDifficulty Difficulty
        {
            get;
            private set;
        }

        /// <summary>
        /// サムネ開始時間
        /// </summary>
        public float ThumbStartTime
        {
            get { return thumbstarttime; }
            private set
            {
                thumbstarttime = value;
            }
        }

        /// <summary>
        /// サムネ終了時間
        /// </summary>
        public float ThumbEndTime
        {
            get { return thumbendtime; }
            private set { thumbendtime = value; }
        }

        /// <summary>
        /// 動画開始時間
        /// </summary>
        public float StartTime
        {
            get { return starttime; }
            private set { starttime = value; }
        }

        /// <summary>
        /// 動画終了時間
        /// </summary>
        public float EndTime
        {
            get { return endtime; }
            private set { endtime = value; }
        }

        /// <summary>
        /// スコアを取得する
        /// </summary>
        /// <param name="difficulty">難易度</param>
        /// <returns></returns>
        public string GetScore(Difficulty difficulty)
        {
            if (difficulty == PPDFrameworkCore.Difficulty.Other)
            {
                return "0";
            }
            return scores[(int)difficulty];
        }

        /// <summary>
        /// 難易度の文字列表記を取得する
        /// </summary>
        /// <param name="difficulty">難易度</param>
        /// <returns></returns>
        public string GetDifficultyString(Difficulty difficulty)
        {
            var index = (int)difficulty;
            if (index < 0 || index >= difficultystring.Length)
            {
                return "";
            }
            return difficultystring[(int)difficulty] ?? "";
        }

        /// <summary>
        /// BPM
        /// </summary>
        public float BPM
        {
            get { return bpm; }
            private set { bpm = value; }
        }

        /// <summary>
        /// BPMの文字列表記
        /// </summary>
        public string BPMString
        {
            get;
            private set;
        }

        /// <summary>
        /// 動画の最大ボリューム
        /// </summary>
        public int MovieVolume
        {
            get;
            private set;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID
        {
            get;
            private set;
        }

        /// <summary>
        /// フォルダ更新日
        /// </summary>
        public string UpdateDate
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面ID
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 古いフォーマットかどうか
        /// </summary>
        public bool IsOld
        {
            get;
            private set;
        }

        /// <summary>
        /// 遅延調整
        /// </summary>
        public float Latency
        {
            get { return latency; }
            private set { latency = value; }
        }

        /// <summary>
        /// Easyの譜面のファイルハッシュ
        /// </summary>
        public byte[] EasyHash
        {
            get;
            private set;
        }

        /// <summary>
        /// Normalの譜面のファイルハッシュ
        /// </summary>
        public byte[] NormalHash
        {
            get;
            private set;
        }

        /// <summary>
        /// Hardの譜面のファイルハッシュ
        /// </summary>
        public byte[] HardHash
        {
            get;
            private set;
        }

        /// <summary>
        /// Extremeの譜面のファイルハッシュ
        /// </summary>
        public byte[] ExtremeHash
        {
            get;
            private set;
        }

        /// <summary>
        /// Easyの譜面のノーツタイプ
        /// </summary>
        public NoteType EasyNoteType
        {
            get;
            private set;
        }

        /// <summary>
        /// Normalの譜面のノーツタイプ
        /// </summary>
        public NoteType NormalNoteType
        {
            get;
            private set;
        }

        /// <summary>
        /// Hardの譜面のノーツタイプ
        /// </summary>
        public NoteType HardNoteType
        {
            get;
            private set;
        }

        /// <summary>
        /// Extremeの譜面のノーツタイプ
        /// </summary>
        public NoteType ExtremeNoteType
        {
            get;
            private set;
        }

        /// <summary>
        /// EASYがAC譜面かどうか
        /// </summary>
        public bool IsEasyAC
        {
            get { return EasyNoteType == NoteType.AC; }
        }

        /// <summary>
        /// NormalがAC譜面かどうか
        /// </summary>
        public bool IsNormalAC
        {
            get { return NormalNoteType == NoteType.AC; }
        }

        /// <summary>
        /// HardがAC譜面かどうか
        /// </summary>
        public bool IsHardAC
        {
            get { return HardNoteType == NoteType.AC; }
        }

        /// <summary>
        /// ExtremeがAC譜面かどうか
        /// </summary>
        public bool IsExtremeAC
        {
            get { return ExtremeNoteType == NoteType.AC; }
        }

        /// <summary>
        /// AC譜面があるかどうか
        /// </summary>
        public bool HasAC
        {
            get
            {
                return IsEasyAC || IsNormalAC || IsHardAC || IsExtremeAC;
            }
        }

        /// <summary>
        /// EasyがACFTかどうか
        /// </summary>
        public bool IsEasyACFT
        {
            get { return EasyNoteType == NoteType.ACFT; }
        }

        /// <summary>
        /// NormalがACFTかどうか
        /// </summary>
        public bool IsNormalACFT
        {
            get { return NormalNoteType == NoteType.ACFT; }
        }

        /// <summary>
        /// HardがACFTかどうか
        /// </summary>
        public bool IsHardACFT
        {
            get { return HardNoteType == NoteType.ACFT; }
        }

        /// <summary>
        /// ExtremeがACFTかどうか
        /// </summary>
        public bool IsExtremeACFT
        {
            get { return ExtremeNoteType == NoteType.ACFT; }
        }

        /// <summary>
        /// AC譜面があるかどうか
        /// </summary>
        public bool HasACFT
        {
            get
            {
                return IsEasyACFT || IsNormalACFT || IsHardACFT || IsExtremeACFT;
            }
        }

        /// <summary>
        /// 通常譜面を持つかどうか
        /// </summary>
        public bool HasNormal
        {
            get
            {
                return (Difficulty.HasFlag(AvailableDifficulty.Easy) && !IsEasyAC && !IsEasyACFT) ||
                    (Difficulty.HasFlag(AvailableDifficulty.Normal) && !IsNormalAC && !IsNormalACFT) ||
                    (Difficulty.HasFlag(AvailableDifficulty.Hard) && !IsHardAC && !IsHardACFT) ||
                    (Difficulty.HasFlag(AvailableDifficulty.Extreme) && !IsExtremeAC && !IsExtremeACFT);
            }
        }

        /// <summary>
        /// 階層の深さ(root = 0)
        /// </summary>
        public int Depth
        {
            get;
            private set;
        }

        internal int[] resultIDs = { -1, -1, -1, -1 };

        /// <summary>
        /// ユーザーの設定した音量
        /// </summary>
        public int UserVolume
        {
            get { return userVolume; }
            set
            {
                if (value >= 200)
                {
                    value = 200;
                }
                else if (value <= 0)
                {
                    value = 0;
                }
                userVolume = value;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SongInformation()
        {
            scores = new string[Enum.GetValues(typeof(Difficulty)).Length - 1];
            difficultystring = new string[scores.Length];
            UserVolume = 100;
        }

        static SongInformation root;

        /// <summary>
        /// ルートの要素です
        /// </summary>
        public static SongInformation Root
        {
            get
            {
                if (root == null)
                {
                    root = new SongInformation
                    {
                        children = GetFromDirectory(PPDSetting.Setting.SongDir)
                    };
                    foreach (SongInformation child in root.children)
                    {
                        child.parent = root;
                        child.Depth = root.Depth + 1;
                    }
                }
                return root;
            }
        }

        /// <summary>
        /// 全ての譜面情報です
        /// </summary>
        public static SongInformation[] All
        {
            get
            {
                var ret = new List<SongInformation>();
                var queue = new Queue<SongInformation>();
                queue.Enqueue(Root);
                while (queue.Count > 0)
                {
                    var info = queue.Dequeue();
                    if (info.IsPPDSong)
                    {
                        ret.Add(info);
                    }
                    else
                    {
                        foreach (SongInformation child in info.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

                return ret.ToArray();
            }
        }

        /// <summary>
        /// IDからSongInformationを取得します
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public static SongInformation FindSongInformationByID(int ID)
        {
            var queue = new Queue<SongInformation>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                var info = queue.Dequeue();
                if (info.IsPPDSong)
                {
                    if (info.ID == ID)
                    {
                        return info;
                    }
                }
                else
                {
                    foreach (SongInformation child in info.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ハッシュと難易度からSongInformationを取得します
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static SongInformation FindSongInformationByHash(byte[] hash, Difficulty difficulty)
        {
            return FindSongInformationByHash(hash, ConvertDifficulty(difficulty));
        }

        /// <summary>
        /// ハッシュからSongInformationを取得します
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static SongInformation FindSongInformationByHash(byte[] hash)
        {
            return FindSongInformationByHash(hash, AvailableDifficulty.Easy | AvailableDifficulty.Normal | AvailableDifficulty.Hard | AvailableDifficulty.Extreme);
        }

        private static SongInformation FindSongInformationByHash(byte[] hash, AvailableDifficulty difficulty)
        {
            var queue = new Queue<SongInformation>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                var info = queue.Dequeue();
                if (info.IsPPDSong)
                {
                    if (difficulty.HasFlag(AvailableDifficulty.Easy))
                    {
                        if (info.EasyHash != null && PPDDatabase.SameHash(hash, info.EasyHash))
                        {
                            return info;
                        }
                    }
                    if (difficulty.HasFlag(AvailableDifficulty.Normal))
                    {
                        if (info.NormalHash != null && PPDDatabase.SameHash(hash, info.NormalHash))
                        {
                            return info;
                        }
                    }
                    if (difficulty.HasFlag(AvailableDifficulty.Hard))
                    {
                        if (info.HardHash != null && PPDDatabase.SameHash(hash, info.HardHash))
                        {
                            return info;
                        }
                    }
                    if (difficulty.HasFlag(AvailableDifficulty.Extreme))
                    {
                        if (info.ExtremeHash != null && PPDDatabase.SameHash(hash, info.ExtremeHash))
                        {
                            return info;
                        }
                    }
                }
                else
                {
                    foreach (SongInformation child in info.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            return null;
        }

        SongInformation[] children;

        /// <summary>
        /// フォルダの場合の子要素です
        /// </summary>
        public SongInformation[] Children
        {
            get
            {
                if (children == null && !IsPPDSong)
                {
                    children = GetFromDirectory(directorypath);
                    foreach (SongInformation child in children)
                    {
                        child.parent = this;
                        child.Depth = this.Depth + 1;
                    }
                }
                return children;
            }
        }

        /// <summary>
        /// 子要素の数です
        /// </summary>
        public int ChildrenCount
        {
            get
            {
                if (Children != null)
                {
                    return Children.Length;
                }
                return 0;
            }
        }

        SongInformation parent;

        /// <summary>
        /// 親要素です
        /// </summary>
        public SongInformation Parent
        {
            get
            {
                return parent;
            }
        }

        private static SongInformation[] GetFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return new SongInformation[0];
            }

            var list = new List<SongInformation>(10);
            using (var reader = PPDDatabase.DB.ExecuteReader("select * from ScoreTable where parentpath = @parentpath", new SQLiteParameter[] { new SQLiteParameter("@parentpath", directory) }))
            {
                while (reader.Reader.Read())
                {
                    try
                    {
                        var si = new SongInformation
                        {
                            IsPPDSong = DatabaseUtility.GetInt32(reader.Reader, 26) == 1,
                            ID = DatabaseUtility.GetInt32(reader.Reader, 0),
                            DirectoryPath = reader.Reader.GetString(1)
                        };
                        if (si.IsPPDSong)
                        {
                            si.ThumbStartTime = DatabaseUtility.GetFloat(reader.Reader, 2);
                            si.ThumbEndTime = DatabaseUtility.GetFloat(reader.Reader, 3);
                            si.StartTime = DatabaseUtility.GetFloat(reader.Reader, 4);
                            si.EndTime = DatabaseUtility.GetFloat(reader.Reader, 5);
                            si.BPM = DatabaseUtility.GetFloat(reader.Reader, 6);
                            si.difficultystring = new string[]{
                                reader.Reader.GetString(7),
                                reader.Reader.GetString(8),
                                reader.Reader.GetString(9),
                                reader.Reader.GetString(10)
                            };
                            si.Difficulty |= DatabaseUtility.GetInt32(reader.Reader, 11) == 1 ? AvailableDifficulty.Easy : AvailableDifficulty.None;
                            si.Difficulty |= DatabaseUtility.GetInt32(reader.Reader, 12) == 1 ? AvailableDifficulty.Normal : AvailableDifficulty.None;
                            si.Difficulty |= DatabaseUtility.GetInt32(reader.Reader, 13) == 1 ? AvailableDifficulty.Hard : AvailableDifficulty.None;
                            si.Difficulty |= DatabaseUtility.GetInt32(reader.Reader, 14) == 1 ? AvailableDifficulty.Extreme : AvailableDifficulty.None;
                            si.resultIDs = new int[]{
                                DatabaseUtility.GetInt32(reader.Reader, 15),
                                DatabaseUtility.GetInt32(reader.Reader, 16),
                                DatabaseUtility.GetInt32(reader.Reader, 17),
                                DatabaseUtility.GetInt32(reader.Reader, 18)
                            };
                            si.TrimmingData = new MovieTrimmingData(
                                DatabaseUtility.GetFloat(reader.Reader, 21),
                                DatabaseUtility.GetFloat(reader.Reader, 19),
                                DatabaseUtility.GetFloat(reader.Reader, 20),
                                DatabaseUtility.GetFloat(reader.Reader, 22)
                            );
                            si.GUID = reader.Reader.GetString(23);
                            si.AuthorName = reader.Reader.GetString(24);
                            si.UpdateDate = reader.Reader.GetString(25);
                            si.MovieVolume = DatabaseUtility.GetInt32(reader.Reader, 27);
                            si.moviepath = reader.Reader.GetString(28);
                            si.ParentDirectory = reader.Reader.GetString(29);
                            si.IsOld = DatabaseUtility.GetInt32(reader.Reader, 30) == 1;
                            si.Latency = DatabaseUtility.GetFloat(reader.Reader, 31);

                            if ((si.Difficulty & AvailableDifficulty.Easy) == AvailableDifficulty.Easy)
                            {
                                si.EasyHash = new byte[32];
                                reader.Reader.GetBytes(32, 0, si.EasyHash, 0, 32);
                            }
                            if ((si.Difficulty & AvailableDifficulty.Normal) == AvailableDifficulty.Normal)
                            {
                                si.NormalHash = new byte[32];
                                reader.Reader.GetBytes(33, 0, si.NormalHash, 0, 32);
                            }
                            if ((si.Difficulty & AvailableDifficulty.Hard) == AvailableDifficulty.Hard)
                            {
                                si.HardHash = new byte[32];
                                reader.Reader.GetBytes(34, 0, si.HardHash, 0, 32);
                            }
                            if ((si.Difficulty & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme)
                            {
                                si.ExtremeHash = new byte[32];
                                reader.Reader.GetBytes(35, 0, si.ExtremeHash, 0, 32);
                            }
                            si.EasyNoteType = (NoteType)DatabaseUtility.GetInt32(reader.Reader, 36);
                            si.NormalNoteType = (NoteType)DatabaseUtility.GetInt32(reader.Reader, 37);
                            si.HardNoteType = (NoteType)DatabaseUtility.GetInt32(reader.Reader, 38);
                            si.ExtremeNoteType = (NoteType)DatabaseUtility.GetInt32(reader.Reader, 39);
                            si.BPMString = reader.Reader.GetString(40);
                            si.UserVolume = DatabaseUtility.GetInt32(reader.Reader, 41);
                        }
                        list.Add(si);
                    }
                    catch
                    {
                    }
                }
            }
            foreach (SongInformation si in list)
            {
                si.UpdateScore();
            }
            list.Sort(new SongInformationComparer());
            return list.ToArray();
        }

        internal void UpdateScore()
        {
            scores = ScoreReaderWriter.ReadScore(resultIDs);
        }

        /// <summary>
        /// 更新します
        /// </summary>
        public static void Update()
        {
            Update(null);
        }

        /// <summary>
        /// 更新します。
        /// </summary>
        public static void Update(SongInformation[] songInformations)
        {
            root = null;
            PPDDatabase.DB.Update(false, songInformations);
            if (Updated != null)
            {
                Updated.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 指定したフォルダの譜面情報を取得します。（使わないでください)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static SongInformation ReadData(string directory)
        {
            var ret = new SongInformation
            {
                loadFromDir = true,
                ID = -1,
                DirectoryPath = directory,
                MoviePath = FindMovieFile(directory)
            };

            if (File.Exists(Path.Combine(directory, "easy.ppd"))) ret.Difficulty |= AvailableDifficulty.Easy;
            if (File.Exists(Path.Combine(directory, "normal.ppd"))) ret.Difficulty |= AvailableDifficulty.Normal;
            if (File.Exists(Path.Combine(directory, "hard.ppd"))) ret.Difficulty |= AvailableDifficulty.Hard;
            if (File.Exists(Path.Combine(directory, "extreme.ppd"))) ret.Difficulty |= AvailableDifficulty.Extreme;
            if (File.Exists(Path.Combine(directory, "data.ini")))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(directory, "data.ini")))
                {
                    var ss = sr.ReadToEnd();
                    var setting = new SettingReader(ss);
                    ret.IsOld = !setting.IsIniFormat;
                    if (!float.TryParse(setting.ReadString("bpm"), NumberStyles.Float, CultureInfo.InvariantCulture, out ret.bpm))
                    {
                        ret.bpm = 100;
                    }
                    ret.BPMString = setting.ReadString("bpmstring");
                    ret.difficultystring[0] = setting.ReadString("difficulty easy");
                    ret.difficultystring[1] = setting.ReadString("difficulty normal");
                    ret.difficultystring[2] = setting.ReadString("difficulty hard");
                    ret.difficultystring[3] = setting.ReadString("difficulty extreme");
                    ret.AuthorName = setting.ReadString("authorname");
                    float.TryParse(setting.ReadString("moviecutleft"), NumberStyles.Float, CultureInfo.InvariantCulture, out float left);
                    float.TryParse(setting.ReadString("moviecutright"), NumberStyles.Float, CultureInfo.InvariantCulture, out float right);
                    float.TryParse(setting.ReadString("moviecuttop"), NumberStyles.Float, CultureInfo.InvariantCulture, out float top);
                    float.TryParse(setting.ReadString("moviecutbottom"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bottom);
                    ret.TrimmingData = new MovieTrimmingData(top, left, right, bottom);
                    float.TryParse(setting.ReadString("thumbtimestart"), NumberStyles.Float, CultureInfo.InvariantCulture, out ret.thumbstarttime);
                    float.TryParse(setting.ReadString("thumbtimeend"), NumberStyles.Float, CultureInfo.InvariantCulture, out ret.thumbendtime);
                    float.TryParse(setting.ReadString("start"), NumberStyles.Float, CultureInfo.InvariantCulture, out ret.starttime);
                    float.TryParse(setting.ReadString("end"), NumberStyles.Float, CultureInfo.InvariantCulture, out ret.endtime);
                    // float.TryParse(setting["latency"], NumberStyles.Float, CultureInfo.InvariantCulture, out ret.latency);
                    ret.GUID = setting.ReadString("guid");
                    ret.IsOld = !setting.IsIniFormat;
                }
            }
            else
            {
                MessageBox.Show("there is no data.ini in " + directory);
            }

            ret.EasyHash = GetHash(Path.Combine(directory, "easy.ppd"));
            ret.NormalHash = GetHash(Path.Combine(directory, "normal.ppd"));
            ret.HardHash = GetHash(Path.Combine(directory, "hard.ppd"));
            ret.ExtremeHash = GetHash(Path.Combine(directory, "extreme.ppd"));


            ret.MovieVolume = -1000;
            //read .evd
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                if (difficulty != AvailableDifficulty.None)
                {
                    string filePath = ret.IsOld ? Path.Combine(directory, difficulty + ".evd") : Path.Combine(directory, difficulty + ".ppd");
                    if (File.Exists(filePath) && (ret.Difficulty & difficulty) == difficulty)
                    {
                        ret = AnalyzeEVD(filePath, ret, ConvertAvailable(difficulty));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 利用可能な難易度を変換します。
        /// </summary>
        /// <param name="difficulty">利用可能な難易度。</param>
        /// <returns>難易度。</returns>
        public static Difficulty ConvertAvailable(AvailableDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AvailableDifficulty.Easy:
                    return PPDFrameworkCore.Difficulty.Easy;
                case AvailableDifficulty.Normal:
                    return PPDFrameworkCore.Difficulty.Normal;
                case AvailableDifficulty.Hard:
                    return PPDFrameworkCore.Difficulty.Hard;
                case AvailableDifficulty.Extreme:
                    return PPDFrameworkCore.Difficulty.Extreme;
            }
            return PPDFrameworkCore.Difficulty.Other;
        }

        /// <summary>
        /// 難易度を変換します。
        /// </summary>
        /// <param name="difficulty">難易度。</param>
        /// <returns>利用可能な難易度。</returns>
        public static AvailableDifficulty ConvertDifficulty(Difficulty difficulty)
        {
            AvailableDifficulty diff = AvailableDifficulty.None;
            switch (difficulty)
            {
                case PPDFrameworkCore.Difficulty.Easy:
                    diff = AvailableDifficulty.Easy;
                    break;
                case PPDFrameworkCore.Difficulty.Normal:
                    diff = AvailableDifficulty.Normal;
                    break;
                case PPDFrameworkCore.Difficulty.Hard:
                    diff = AvailableDifficulty.Hard;
                    break;
                case PPDFrameworkCore.Difficulty.Extreme:
                    diff = AvailableDifficulty.Extreme;
                    break;
            }

            return diff;
        }

        private static string FindMovieFile(string directory)
        {
            string moviePath = "";
            if (!Directory.Exists(directory))
            {
                return moviePath;
            }
            foreach (string fn in Directory.GetFiles(directory, "movie.*"))
            {
                moviePath = fn;
                break;
            }
            if (String.IsNullOrEmpty(moviePath))
            {
                foreach (string movieExt in PPDSetting.Setting.MovieExtensions)
                {
                    foreach (string fn in Directory.GetFiles(directory, String.Format("*.{0}", movieExt)))
                    {
                        if (Path.GetExtension(fn) == String.Format(".{0}", movieExt))
                        {
                            moviePath = fn;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(moviePath))
                    {
                        break;
                    }
                }
            }
            if (String.IsNullOrEmpty(moviePath))
            {
                foreach (string movieExt in PPDSetting.Setting.MusicExtensions)
                {
                    foreach (string fn in Directory.GetFiles(directory, String.Format("*.{0}", movieExt)))
                    {
                        if (Path.GetExtension(fn) == String.Format(".{0}", movieExt))
                        {
                            moviePath = fn;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(moviePath))
                    {
                        break;
                    }
                }
            }
            return moviePath;
        }

        private static byte[] GetHash(string path)
        {
            if (!File.Exists(path))
            {
                return new byte[0];
            }
            else
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    var SHA256 = new SHA256Managed();
                    var hash = SHA256.ComputeHash(data);
                    return hash;
                }
            }
        }

        private static SongInformation AnalyzeEVD(string path, SongInformation si, Difficulty difficulty)
        {
            try
            {
                IEVDData[] data = null;
                if (si.IsOld)
                {
                    data = EVDReader.Read(path);
                }
                else
                {
                    var pr = new PackReader(path);
                    var pppsr = pr.Read("evd");
                    data = EVDReader.Read(pppsr);
                    pr.Close();
                }
                foreach (ChangeVolumeEvent evddata in Array.FindAll(data, (evd) => (evd is ChangeVolumeEvent)))
                {
                    if (evddata.Channel == 0 && evddata.Time == 0)
                    {
                        si.MovieVolume = -100 * (100 - evddata.Volume);
                        break;
                    }
                }
                foreach (ChangeNoteTypeEvent evddata in Array.FindAll(data, (evd) => (evd is ChangeNoteTypeEvent)))
                {
                    if (evddata.NoteType == NoteType.AC || evddata.NoteType == NoteType.ACFT)
                    {
                        switch (difficulty)
                        {
                            case PPDFrameworkCore.Difficulty.Easy:
                                si.EasyNoteType = evddata.NoteType;
                                break;
                            case PPDFrameworkCore.Difficulty.Normal:
                                si.NormalNoteType = evddata.NoteType;
                                break;
                            case PPDFrameworkCore.Difficulty.Hard:
                                si.HardNoteType = evddata.NoteType;
                                break;
                            case PPDFrameworkCore.Difficulty.Extreme:
                                si.ExtremeNoteType = evddata.NoteType;
                                break;
                        }
                        break;
                    }
                }
            }
            catch
            {
            }
            return si;
        }

        /// <summary>
        /// Latencyを変更します。使用しないでください
        /// </summary>
        /// <param name="latency"></param>
        public void UpdateLatency(float latency)
        {
            PPDDatabase.DB.ChangeLatency(this, latency);
            Latency = latency;
        }

        /// <summary>
        /// ランキングを取得します
        /// </summary>
        /// <returns></returns>
        public Ranking GetRanking(bool forceUpdate = false)
        {
            return RankingCache.Cache.GetRanking(GetPrimaryHash(), forceUpdate);
        }

        /// <summary>
        /// ライバルのランキングを取得します。
        /// </summary>
        /// <param name="forceUpdate"></param>
        /// <returns></returns>
        public Ranking GetRivalRanking(bool forceUpdate = false)
        {
            return RankingCache.Cache.GetRivalRanking(GetPrimaryHash(), forceUpdate);
        }

        /// <summary>
        /// BPMの文字列表記を取得します。
        /// </summary>
        /// <returns></returns>
        public string GetBPMString()
        {
            return String.IsNullOrEmpty(BPMString) ? bpm.ToString() : BPMString;
        }

        /// <summary>
        /// easyから順にスコアのハッシュがあるか探して取得します。
        /// </summary>
        /// <returns></returns>
        public byte[] GetPrimaryHash()
        {
            if (EasyHash != null)
            {
                return EasyHash;
            }
            else if (NormalHash != null)
            {
                return NormalHash;
            }
            else if (HardHash != null)
            {
                return HardHash;
            }
            else if (ExtremeHash != null)
            {
                return ExtremeHash;
            }

            return null;
        }

        /// <summary>
        /// 難易度を取得します。
        /// </summary>
        /// <param name="scoreHash">譜面ハッシュ。</param>
        /// <returns>難易度</returns>
        public Difficulty GetDifficulty(string scoreHash)
        {
            if (EasyHash != null && CryptographyUtility.Getx2Encoding(EasyHash) == scoreHash)
            {
                return PPDFrameworkCore.Difficulty.Easy;
            }
            if (NormalHash != null && CryptographyUtility.Getx2Encoding(NormalHash) == scoreHash)
            {
                return PPDFrameworkCore.Difficulty.Normal;
            }
            if (HardHash != null && CryptographyUtility.Getx2Encoding(HardHash) == scoreHash)
            {
                return PPDFrameworkCore.Difficulty.Hard;
            }
            if (ExtremeHash != null && CryptographyUtility.Getx2Encoding(ExtremeHash) == scoreHash)
            {
                return PPDFrameworkCore.Difficulty.Extreme;
            }
            return PPDFrameworkCore.Difficulty.Other;
        }

        /// <summary>
        /// 難易度のスコアハッシュを取得します
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public byte[] GetScoreHash(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case PPDFrameworkCore.Difficulty.Easy:
                    return EasyHash;
                case PPDFrameworkCore.Difficulty.Normal:
                    return NormalHash;
                case PPDFrameworkCore.Difficulty.Hard:
                    return HardHash;
                case PPDFrameworkCore.Difficulty.Extreme:
                    return ExtremeHash;
            }
            return null;
        }

        /// <summary>
        /// 対象の難易度がAC譜面かどうかを調べます
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetIsAC(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case PPDFrameworkCore.Difficulty.Easy:
                    return IsEasyAC;
                case PPDFrameworkCore.Difficulty.Normal:
                    return IsNormalAC;
                case PPDFrameworkCore.Difficulty.Hard:
                    return IsHardAC;
                case PPDFrameworkCore.Difficulty.Extreme:
                    return IsExtremeAC;
            }
            return false;
        }

        /// <summary>
        /// 対象の難易度がACFT譜面かどうかを調べます
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetIsACFT(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case PPDFrameworkCore.Difficulty.Easy:
                    return IsEasyACFT;
                case PPDFrameworkCore.Difficulty.Normal:
                    return IsNormalACFT;
                case PPDFrameworkCore.Difficulty.Hard:
                    return IsHardACFT;
                case PPDFrameworkCore.Difficulty.Extreme:
                    return IsExtremeACFT;
            }
            return false;
        }

        /// <summary>
        /// ノーツのタイプを取得します。
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public NoteType GetNoteType(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case PPDFrameworkCore.Difficulty.Easy:
                    return EasyNoteType;
                case PPDFrameworkCore.Difficulty.Normal:
                    return NormalNoteType;
                case PPDFrameworkCore.Difficulty.Hard:
                    return HardNoteType;
                case PPDFrameworkCore.Difficulty.Extreme:
                    return ExtremeNoteType;
            }
            return NoteType.Normal;
        }

        /// <summary>
        /// 難易度の数値化したデータを取得します。
        /// </summary>
        /// <param name="difficulty">難易度。</param>
        /// <returns>数値化された難易度。</returns>
        public ScoreDifficultyMeasureResult CalculateDifficulty(Difficulty difficulty)
        {
            var path = Path.Combine(DirectoryPath, String.Format("{0}.ppd", difficulty));
            MarkDataBase[] data = null;
            if (IsOld)
            {
                data = PPDReader.Read(path);
            }
            else
            {
                try
                {
                    using (PackReader pr = new PackReader(path))
                    {
                        var pppsr = pr.Read("ppd");
                        data = PPDReader.Read(pppsr);
                    }
                }
                catch
                {
                    try
                    {
                        data = PPDReader.Read(path);
                    }
                    catch
                    {

                    }
                }
            }
            return ScoreDifficultyMeasure.Measure(data);
        }

        /// <summary>
        /// ユーザーボリュームを保存します。
        /// </summary>
        public void SaveUserVolume()
        {
            if (loadFromDir)
            {
                return;
            }
            PPDDatabase.DB.ChangeUserVolume(this, UserVolume);
        }
    }
}
