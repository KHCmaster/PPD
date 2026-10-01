using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDFramework
{
    /// <summary>
    /// PPDDataBaseです
    /// </summary>
    public class PPDDatabase : DataBase
    {
        static PPDDatabase db;
        internal const string LogicFolderRoot = "LogicFolderRoot";
        readonly string[] ppds = [
            "easy",
            "normal",
            "hard",
            "extreme"
        ];

        /// <summary>
        /// 名前
        /// </summary>
        public override string Name
        {
            get { return PPDSetting.Setting.IsDebug ? "PPD.Debug" : "PPD"; }
        }

        /// <summary>
        /// DB
        /// </summary>
        public static PPDDatabase DB
        {
            get
            {
                if (db == null)
                {
                    db = new PPDDatabase();
                    db.Initialize();
                }
                return db;
            }
        }

        private PPDDatabase()
        {
            Open();
        }

        private void Initialize()
        {
            bool forceUpdate = false;
            if (!ExistTable("ScoreTable"))
            {
                ExecuteNonQueryCommand(@"create table ScoreTable(
   scoreid integer primary key autoincrement,
   directorypath varchar(50),
   thumbtimestart real,
   thumbtimeend real,
   start real,
   end real,
   bpm real,
   difficultyeasy varchar(50),
   difficultynormal varchar(50),
   difficultyhard varchar(50),
   difficultyextreme varchar(50),
   availableeasy int,
   availablenormal int,
   availablehard int,
   availableextreme int,
   bestscoreeasyid int default -1,
   bestscorenormalid int default -1,
   bestscorehardid int default -1,
   bestscoreextremeid int default -1,
   moviecutleft real,
   moviecutright real,
   moviecuttop real,
   moviecutbottom real,
   guid varchar(50),
   authorname varchar(50),
   updatedate varchar(50),
   isppd int,
   movievolume int,
   moviepath varchar(50),
   parentpath varchar(50),
   isold int,
   latency real,
   easyhash blob,
   normalhash blob,
   hardhash blob,
   extremehash blob,
   isaceasy int,
   isacnormal int,
   isachard int,
   isacextreme int,
   bpmstring varchar(50),
   uservolume int default 100
);");
                ExecuteNonQueryCommand(@"create index ScoreTableIndex on ScoreTable(directorypath);");
            }
            else
            {

                var columns = new string[][]{
                    ["isaceasy","int"],
                    ["isacnormal","int"],
                    ["isachard","int"],
                    ["isacextreme","int"],
                    ["bpmstring","varchar(50)"],
                    ["uservolume","int", "100"]
                };

                foreach (string[] column in columns)
                {
                    if (!HasColumn("ScoreTable", column[0]))
                    {
                        switch (column.Length)
                        {
                            case 2:
                                AddColumn("ScoreTable", column[0], column[1]);
                                break;
                            case 3:
                                AddColumn("ScoreTable", column[0], column[1], column[2]);
                                break;
                        }
                        forceUpdate = true;
                    }
                }

                if (GetColumnType("ScoreTable", "moviecutleft") != "real")
                {
                    ExecuteNonQueryCommand(@"alter table ScoreTable rename to ScoreTableTemp;");
                    ExecuteNonQueryCommand(@"create table ScoreTable(
   scoreid integer primary key autoincrement,
   directorypath varchar(50),
   thumbtimestart real,
   thumbtimeend real,
   start real,
   end real,
   bpm real,
   difficultyeasy varchar(50),
   difficultynormal varchar(50),
   difficultyhard varchar(50),
   difficultyextreme varchar(50),
   availableeasy int,
   availablenormal int,
   availablehard int,
   availableextreme int,
   bestscoreeasyid int default -1,
   bestscorenormalid int default -1,
   bestscorehardid int default -1,
   bestscoreextremeid int default -1,
   moviecutleft real,
   moviecutright real,
   moviecuttop real,
   moviecutbottom real,
   guid varchar(50),
   authorname varchar(50),
   updatedate varchar(50),
   isppd int,
   movievolume int,
   moviepath varchar(50),
   parentpath varchar(50),
   isold int,
   latency real,
   easyhash blob,
   normalhash blob,
   hardhash blob,
   extremehash blob,
   isaceasy int,
   isacnormal int,
   isachard int,
   isacextreme int,
   bpmstring varchar(50),
   uservolume int default 100
);");
                    var columnNames = new string[]{
                        "scoreid",   "directorypath",   "thumbtimestart","thumbtimeend","start","end","bpm","difficultyeasy",
                        "difficultynormal","difficultyhard","difficultyextreme","availableeasy","availablenormal","availablehard",
                        "availableextreme","bestscoreeasyid","bestscorenormalid","bestscorehardid","bestscoreextremeid","moviecutleft",
                        "moviecutright","moviecuttop","moviecutbottom","guid","authorname","updatedate","isppd","movievolume","moviepath",
                        "parentpath","isold","latency","easyhash","normalhash","hardhash","extremehash","isaceasy","isacnormal","isachard",
                        "isacextreme","bpmstring","uservolume" };
                    ExecuteNonQueryCommand(String.Format(@"INSERT INTO ScoreTable({0}) SELECT {0} FROM ScoreTableTemp;", String.Join(", ", columnNames)));
                    ExecuteNonQueryCommand(@"DROP TABLE ScoreTableTemp;");
                    ExecuteNonQueryCommand(@"create index ScoreTableIndex on ScoreTable(directorypath);");
                    ExecuteNonQueryCommand(@"VACUUM;");
                }
            }

            if (!ExistTable("ResultTable"))
            {
                ExecuteNonQueryCommand(@"create table ResultTable(
   resultid integer primary key autoincrement,
   scoreid int,
   difficulty int,
   evaluate int,
   score int,
   coolcount int,
   goodcount int,
   safecount int,
   sadcount int,
   worstcount int,
   maxcombo int,
   finishtime real,
   date varchar(50)
);");
                ExecuteNonQueryCommand(@"create index ResultTableIndex on ResultTable(scoreid, difficulty);");
            }

            if (!ExistTable("LogicFolder"))
            {
                ExecuteNonQueryCommand(@"create table LogicFolder(
   folderid integer primary key autoincrement,
   scoreid int,
   isfolder int,
   name varchar(50),
   childids varchar(50),
   date varchar(50)
);");
                ExecuteNonQueryCommand(@"create index LogicFolderIndex on LogicFolder(folderid);");
                ExecuteDataTable("insert into LogicFolder(isfolder,name,childids,date) values(@isfolder,@name,@childids,@date)", [
                    new("@isfolder",1),
                    new("@name",LogicFolderRoot),
                    new("@childids",string.Empty),
                    new("@date",DateTime.Now.ToString(CultureInfo.InvariantCulture))
                ]);
            }

            if (!ExistTable("ScoreStorage"))
            {
                ExecuteNonQueryCommand(@"create table ScoreStorage(
   storageid integer primary key autoincrement,
   scoreid int,
   data text
);");
            }
            Update(forceUpdate);
        }

        internal void Update(bool forceUpdate)
        {
            Update(forceUpdate, null);
        }

        internal void Update(bool forceUpdate, SongInformation[] songInfos)
        {
            if (!Directory.Exists(PPDSetting.Setting.SongDir))
            {
                return;
            }
            var addedScoreInfo = new List<ModifiedPPDScoreInfo>();
            using var transaction = new DataBaseTransaction();
            CheckExist(out List<ModifiedPPDScoreInfo> deletedScoreInfo);
            UpdatePPDSongInformation(PPDSetting.Setting.SongDir, "", forceUpdate, out bool hasppd, addedScoreInfo, songInfos);
            ReplaceInfo(deletedScoreInfo, addedScoreInfo);
        }

        internal void RemoveUnconnectedLinks()
        {
            foreach (LogicFolderInfomation info in LogicFolderInfomation.All)
            {
                var songInfo = SongInformation.FindSongInformationByID(info.ScoreID);
                if (songInfo == null)
                {
                    info.Remove();
                }
            }
        }

        private void ReplaceInfo(List<ModifiedPPDScoreInfo> deleted, List<ModifiedPPDScoreInfo> added)
        {
            foreach (ModifiedPPDScoreInfo delete in deleted)
            {
                // change score id
                var index = added.FindIndex((add) =>
                {
                    return (delete.Guid != string.Empty && delete.Guid == add.Guid) || ComapreHashes(add, delete);
                });
                if (index >= 0)
                {
                    ExecuteDataTable("update ResultTable set scoreid = @newid where scoreid == @oldid", [
                        new("@newid", added[index].ID),
                        new("@oldid", delete.ID)
                    ]);
                    ExecuteDataTable("update LogicFolder set scoreid = @newid where scoreid == @oldid", [
                        new("@newid", added[index].ID),
                        new("@oldid", delete.ID)
                    ]);
                    ExecuteDataTable("update ScoreStorage set scoreid = @newid where scoreid == @oldid", [
                        new("@newid", added[index].ID),
                        new("@oldid", delete.ID)
                    ]);
                    ExecuteDataTable(@"update ScoreTable set 
bestscoreeasyid = @bestscoreeasyid, 
bestscorenormalid = @bestscorenormalid, 
bestscorehardid = @bestscorehardid, 
bestscoreextremeid = @bestscoreextremeid, 
latency = @latency,
uservolume = @uservolume where scoreid = @newid", [
                        new("@bestscoreeasyid", delete.BestEasyID),
                        new("@bestscorenormalid", delete.BestNormalID),
                        new("@bestscorehardid", delete.BestHardID),
                        new("@bestscoreextremeid", delete.BestExtremeID),
                        new("@latency", delete.Latency),
                        new("@newid", added[index].ID),
                        new("@uservolume", delete.UserVolume)
                    ]);
                }
                else
                {
                    ExecuteDataTable("delete from LogicFolder where scoreid == @oldid", [
                        new("@oldid",delete.ID)
                    ]);
                }
            }
        }

        private static bool ComapreHashes(ModifiedPPDScoreInfo mppdsi1, ModifiedPPDScoreInfo mppdsi2)
        {
            if (mppdsi1.EasyHash.Length > 0 && mppdsi2.EasyHash.Length > 0 && SameHash(mppdsi1.EasyHash, mppdsi2.EasyHash))
            {
                return true;
            }
            if (mppdsi1.NormalHash.Length > 0 && mppdsi2.NormalHash.Length > 0 && SameHash(mppdsi1.NormalHash, mppdsi2.NormalHash))
            {
                return true;
            }
            if (mppdsi1.HardHash.Length > 0 && mppdsi2.HardHash.Length > 0 && SameHash(mppdsi1.HardHash, mppdsi2.HardHash))
            {
                return true;
            }
            if (mppdsi1.ExtremeHash.Length > 0 && mppdsi2.ExtremeHash.Length > 0 && SameHash(mppdsi1.ExtremeHash, mppdsi2.ExtremeHash))
            {
                return true;
            }
            return false;
        }

        internal static bool SameHash(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length) return false;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }
            return true;
        }


        class ModifiedPPDScoreInfo(int id, string guid)
        {
            public string Guid
            {
                get;
                private set;
            } = guid;
            public int ID
            {
                get;
                private set;
            } = id;
            public int BestEasyID
            {
                get;
                set;
            }
            public int BestNormalID
            {
                get;
                set;
            }
            public int BestHardID
            {
                get;
                set;
            }
            public int BestExtremeID
            {
                get;
                set;
            }

            public byte[] EasyHash
            {
                get;
                set;
            }
            public byte[] NormalHash
            {
                get;
                set;
            }
            public byte[] HardHash
            {
                get;
                set;
            }
            public byte[] ExtremeHash
            {
                get;
                set;
            }
            public float Latency
            {
                get;
                set;
            }
            public int UserVolume
            {
                get;
                set;
            }
        }

        private void CheckExist(out List<ModifiedPPDScoreInfo> deletedScoreInfo)
        {
            var deleteIds = new List<object[]>();
            deletedScoreInfo = [];
            using (var reader = ExecuteReader("select * from ScoreTable;", []))
            {
                while (reader.Reader.Read())
                {
                    var directorypath = (string)reader.Reader["directorypath"];
                    var parentpath = (string)reader.Reader["parentpath"];
                    var scoreid = DatabaseUtility.GetInt32(reader.Reader, 0);
                    bool isppd = DatabaseUtility.GetInt32(reader.Reader, 26) == 1;
                    if (!Directory.Exists(directorypath))
                    {
                        deleteIds.Add([scoreid, Directory.Exists(parentpath), parentpath]);
                        if (isppd)
                        {
                            var guid = (string)reader.Reader["guid"];
                            var bests = new int[]{
                                DatabaseUtility.GetInt32(reader.Reader, 15),
                                DatabaseUtility.GetInt32(reader.Reader, 16),
                                DatabaseUtility.GetInt32(reader.Reader, 17),
                                DatabaseUtility.GetInt32(reader.Reader, 18)
                            };
                            var deleted = new ModifiedPPDScoreInfo(scoreid, guid)
                            {
                                BestEasyID = bests[0],
                                BestNormalID = bests[1],
                                BestHardID = bests[2],
                                BestExtremeID = bests[3],
                                Latency = DatabaseUtility.GetFloat(reader.Reader, 31),
                                EasyHash = GetHash(reader.Reader, 32),
                                NormalHash = GetHash(reader.Reader, 33),
                                HardHash = GetHash(reader.Reader, 34),
                                ExtremeHash = GetHash(reader.Reader, 35),
                                UserVolume = DatabaseUtility.GetInt32(reader.Reader, 41)
                            };
                            deletedScoreInfo.Add(deleted);
                        }
                    }
                }
            }
            foreach (object[] data in deleteIds)
            {
                var id = (int)data[0];
                var parentexist = (bool)data[1];
                var parentpath = (string)data[2];
                ExecuteDataTable(@"delete from ScoreTable where scoreid == @scoreid;", [
                    new("@scoreid",id)
                ]);
            }
        }

        private static byte[] GetHash(SQLiteDataReader reader, int index)
        {
            byte[] ret = new byte[reader.GetBytes(index, 0, null, 0, 0)];
            reader.GetBytes(index, 0, ret, 0, ret.Length);
            return ret;
        }

        private void UpdatePPDSongInformation(string dir, string parentdir, bool forceUpdate, out bool hasppd, List<ModifiedPPDScoreInfo> addedScoreInfo, SongInformation[] songInfos)
        {
            hasppd = false;
            var time = Directory.GetLastWriteTime(dir);
            // check files
            if (!CheckPPDDirectory(dir))
            {
                foreach (string childdir in Directory.GetDirectories(dir))
                {
                    UpdatePPDSongInformation(childdir, dir, forceUpdate, out bool temp, addedScoreInfo, songInfos);
                    hasppd |= temp;
                }
                if (hasppd)
                {
                    IsUpdatedDirectory(dir, time.ToString(CultureInfo.InvariantCulture), out bool IsExistDir);
                    if (!IsExistDir)
                    {
                        ExecuteDataTable("insert into ScoreTable(directorypath,isppd,parentpath,updatedate) values(@dir,@isppd,@parentdir,@updatedate);", [
                            new("@dir",dir),
                            new("@isppd","0"),
                            new("@parentdir",parentdir),
                            new("@updatedate",time.ToString(CultureInfo.InvariantCulture))
                        ]);
                    }
                }
                else
                {
                    // PPDの譜面がないフォルダ
                    // DBにフォルダがある場合はそのデータを削除
                    ExecuteDataTable("delete from ScoreTable where directorypath == @dir", [
                        new("@dir",dir)
                    ]);
                }
            }
            else
            {
                hasppd = true;
                if (File.Exists(Path.Combine(dir, "data.ini")))
                {
                    time = GetLastFileTime(Path.Combine(dir, "data.ini"));
                }
                bool shouldUpdate = (IsUpdatedDirectory(dir, time.ToString(CultureInfo.InvariantCulture), out bool IsExistDir) || forceUpdate) || (songInfos != null && songInfos.FirstOrDefault(s => s.DirectoryPath.ToLower() == dir.ToLower()) != null);
                if (!shouldUpdate)
                {
                    return;
                }
                var info = SongInformation.ReadData(dir);
                if (!IsExistDir)
                {
                    ExecuteDataTable(@"insert into ScoreTable(
                                directorypath,
                                thumbtimestart,
                                thumbtimeend,
                                start,
                                end,
                                bpm,
                                difficultyeasy,
                                difficultynormal,
                                difficultyhard,
                                difficultyextreme,
                                availableeasy,
                                availablenormal,
                                availablehard,
                                availableextreme,
                                moviecutleft,
                                moviecutright,
                                moviecuttop,
                                moviecutbottom,
                                guid,
                                authorname,
                                updatedate,
                                isppd,
                                movievolume,
                                moviepath,
                                parentpath,
                                isold,
                                latency,
                                easyhash,
                                normalhash,
                                hardhash,
                                extremehash,
                                isaceasy,
                                isacnormal,
                                isachard,
                                isacextreme,
                                bpmstring
                        ) values(
                                @directorypath,
                                @thumbtimestart,
                                @thumbtimeend,
                                @start,
                                @end,
                                @bpm,
                                @difficultyeasy,
                                @difficultynormal,
                                @difficultyhard,
                                @difficultyextreme,
                                @availableeasy,
                                @availablenormal,
                                @availablehard,
                                @availableextreme,
                                @moviecutleft,
                                @moviecutright,
                                @moviecuttop,
                                @moviecutbottom,
                                @guid,
                                @authorname,
                                @updatedate,
                                @isppd,
                                @movievolume,
                                @moviepath,
                                @parentpath,
                                @isold,
                                @latency,
                                @easyhash,
                                @normalhash,
                                @hardhash,
                                @extremehash,
                                @isaceasy,
                                @isacnormal,
                                @isachard,
                                @isacextreme,
                                @bpmstring
                            );", [
                        new("@directorypath",dir),
                        new("@thumbtimestart",info.ThumbStartTime),
                        new("@thumbtimeend",info.ThumbEndTime),
                        new("@start",info.StartTime),
                        new("@end",info.EndTime),
                        new("@bpm",info.BPM),
                        new("@difficultyeasy",info.GetDifficultyString(Difficulty.Easy)),
                        new("@difficultynormal",info.GetDifficultyString(Difficulty.Normal)),
                        new("@difficultyhard",info.GetDifficultyString(Difficulty.Hard)),
                        new("@difficultyextreme",info.GetDifficultyString(Difficulty.Extreme)),
                        new("@availableeasy",(info.Difficulty & SongInformation.AvailableDifficulty.Easy )== SongInformation.AvailableDifficulty.Easy?1:0),
                        new("@availablenormal",(info.Difficulty & SongInformation.AvailableDifficulty.Normal )== SongInformation.AvailableDifficulty.Normal?1:0),
                        new("@availablehard",(info.Difficulty & SongInformation.AvailableDifficulty.Hard )== SongInformation.AvailableDifficulty.Hard?1:0),
                        new("@availableextreme",(info.Difficulty & SongInformation.AvailableDifficulty.Extreme )== SongInformation.AvailableDifficulty.Extreme?1:0),
                        new("@moviecutleft",info.TrimmingData.Left),
                        new("@moviecutright",info.TrimmingData.Right),
                        new("@moviecuttop" ,info.TrimmingData.Top),
                        new("@moviecutbottom",info.TrimmingData.Bottom),
                        new("@guid",info.GUID),
                        new("@authorname",info.AuthorName),
                        new("@updatedate",time.ToString(CultureInfo.InvariantCulture)),
                        new("@isppd",1),
                        new("@movievolume",info.MovieVolume),
                        new("@moviepath",info.MoviePath),
                        new("@parentpath",parentdir),
                        new("@isold",info.IsOld?1:0),
                        new("@latency",info.Latency),
                        new("@easyhash",info.EasyHash),
                        new("@normalhash",info.NormalHash),
                        new("@hardhash",info.HardHash),
                        new("@extremehash",info.ExtremeHash),
                        new("@isaceasy",(int)info.EasyNoteType),
                        new("@isacnormal",(int)info.NormalNoteType),
                        new("@isachard",(int)info.HardNoteType),
                        new("@isacextreme",(int)info.ExtremeNoteType),
                        new("@bpmstring",info.BPMString)
                    ]);
                }
                else
                {
                    ExecuteDataTable(@"update ScoreTable set
                                thumbtimestart     =@thumbtimestart,
                                thumbtimeend       =@thumbtimeend,
                                start              =@start,
                                end                =@end,
                                bpm                =@bpm,
                                difficultyeasy     =@difficultyeasy,
                                difficultynormal   =@difficultynormal,
                                difficultyhard     =@difficultyhard,
                                difficultyextreme  =@difficultyextreme,
                                availableeasy      =@availableeasy,
                                availablenormal    =@availablenormal,
                                availablehard      =@availablehard,
                                availableextreme   =@availableextreme,
                                moviecutleft       =@moviecutleft,
                                moviecutright      =@moviecutright,
                                moviecuttop        =@moviecuttop ,
                                moviecutbottom     =@moviecutbottom,
                                guid               =@guid,
                                authorname         =@authorname,
                                updatedate         =@updatedate,
                                isppd              =@isppd,
                                movievolume        =@movievolume,
                                moviepath          =@moviepath,
                                parentpath         =@parentpath,
                                isold              =@isold,
                                easyhash           =@easyhash,
                                normalhash         =@normalhash,
                                hardhash           =@hardhash,
                                extremehash        =@extremehash,
                                isaceasy           =@isaceasy,
                                isacnormal         =@isacnormal,
                                isachard           =@isachard,
                                isacextreme        =@isacextreme,
                                bpmstring          =@bpmstring
                        where directorypath = @directorypath;", [
                        new("@directorypath",dir),
                        new("@thumbtimestart",info.ThumbStartTime),
                        new("@thumbtimeend",info.ThumbEndTime),
                        new("@start",info.StartTime),
                        new("@end",info.EndTime),
                        new("@bpm",info.BPM),
                        new("@difficultyeasy",info.GetDifficultyString(Difficulty.Easy)),
                        new("@difficultynormal",info.GetDifficultyString(Difficulty.Normal)),
                        new("@difficultyhard",info.GetDifficultyString(Difficulty.Hard)),
                        new("@difficultyextreme",info.GetDifficultyString(Difficulty.Extreme)),
                        new("@availableeasy",(info.Difficulty & SongInformation.AvailableDifficulty.Easy )== SongInformation.AvailableDifficulty.Easy?1:0),
                        new("@availablenormal",(info.Difficulty & SongInformation.AvailableDifficulty.Normal )== SongInformation.AvailableDifficulty.Normal?1:0),
                        new("@availablehard",(info.Difficulty & SongInformation.AvailableDifficulty.Hard )== SongInformation.AvailableDifficulty.Hard?1:0),
                        new("@availableextreme",(info.Difficulty & SongInformation.AvailableDifficulty.Extreme )== SongInformation.AvailableDifficulty.Extreme?1:0),
                        new("@moviecutleft",info.TrimmingData.Left),
                        new("@moviecutright",info.TrimmingData.Right),
                        new("@moviecuttop" ,info.TrimmingData.Top),
                        new("@moviecutbottom",info.TrimmingData.Bottom),
                        new("@guid",info.GUID),
                        new("@authorname",info.AuthorName),
                        new("@updatedate",time.ToString(CultureInfo.InvariantCulture)),
                        new("@isppd",1),
                        new("@movievolume",info.MovieVolume),
                        new("@moviepath",info.MoviePath),
                        new("@parentpath",parentdir),
                        new("@isold",info.IsOld?1:0),
                        new("@easyhash",info.EasyHash),
                        new("@normalhash",info.NormalHash),
                        new("@hardhash",info.HardHash),
                        new("@extremehash",info.ExtremeHash),
                        new("@isaceasy",(int)info.EasyNoteType),
                        new("@isacnormal",(int)info.NormalNoteType),
                        new("@isachard",(int)info.HardNoteType),
                        new("@isacextreme",(int)info.ExtremeNoteType),
                        new("@bpmstring",info.BPMString)
                    ]);
                }
                using var reader = ExecuteReader("select * from ScoreTable where directorypath = @directorypath;",
                     [new("@directorypath", dir)]);
                while (reader.Reader.Read())
                {
                    var index = reader.Reader.GetInt32(0);
                    var guid = reader.Reader.GetString(23);
                    if (!IsExistDir)
                    {
                        var mppdsi = new ModifiedPPDScoreInfo(index, guid)
                        {
                            EasyHash = info.EasyHash,
                            NormalHash = info.NormalHash,
                            HardHash = info.HardHash,
                            ExtremeHash = info.ExtremeHash
                        };
                        addedScoreInfo.Add(mppdsi);
                    }
                    break;
                }
            }
        }

        DateTime GetLastFileTime(string filePath)
        {
            DateTime dt1 = File.GetCreationTime(filePath), dt2 = File.GetLastWriteTime(filePath);
            return DateTime.Compare(dt1, dt2) > 0 ? dt1 : dt2;
        }

        bool IsUpdatedDirectory(string directorypath, string currentDate, out bool isExistDir)
        {
            bool isUpdated = true;
            isExistDir = false;
            using (var reader = ExecuteReader("select * from ScoreTable where directorypath = @directorypath", [new("@directorypath", directorypath)]))
            {
                while (reader.Reader.Read())
                {
                    isExistDir = true;
                    var date = (string)reader.Reader["updatedate"];
                    if (date == currentDate)
                    {
                        isUpdated = false;
                        break;
                    }
                }
            }
            return isUpdated;
        }

        internal static string ConverListToString(List<int> list)
        {
            if (list == null) return "";
            var sb = new StringBuilder();
            foreach (int data in list)
            {
                if (sb.Length > 0)
                {
                    sb.Append("|");
                }
                sb.Append(data.ToString());
            }
            return sb.ToString();
        }

        internal static List<int> ParseStringToList(string data)
        {
            var list = new List<int>();
            if (string.IsNullOrEmpty(data)) return list;
            foreach (string split in data.Split('|'))
            {
                if (int.TryParse(split, out int val))
                {
                    list.Add(val);
                }
            }
            return list;
        }

        bool CheckPPDDirectory(string dir)
        {
            bool isppddirectory = false;
            foreach (string ppdfile in Directory.GetFiles(dir, "*.ppd"))
            {
                if (Array.IndexOf(ppds, Path.GetFileNameWithoutExtension(ppdfile).ToLower()) != -1)
                {
                    isppddirectory = true;
                    break;
                }
            }
            return isppddirectory;
        }

        internal bool FindScore(int resultID, out int score)
        {
            bool found = false;
            score = 0;
            try
            {
                using var reader = ExecuteReader("select * from ResultTable where resultid = @resultid;", [new("@resultid", resultID)]);
                while (reader.Reader.Read())
                {
                    score = reader.Reader.GetInt32(4);
                    found = true;
                    break;
                }
            }
            catch
            {
            }
            return found;
        }

        internal bool WriteScore(int scoreID, Difficulty difficulty, int[] counts, int maxcombo, int score, ResultEvaluateType resulttype, float finishtime, SongInformation songInfo)
        {
            bool highscore = false;
            int bestscoreid = -1;
            using (var reader = ExecuteReader("select * from ScoreTable where scoreid = @scoreid;", [new("@scoreid", scoreID)]))
            {
                while (reader.Reader.Read())
                {
                    switch (difficulty)
                    {
                        case Difficulty.Easy:
                            bestscoreid = reader.Reader.GetInt32(15);
                            break;
                        case Difficulty.Normal:
                            bestscoreid = reader.Reader.GetInt32(16);
                            break;
                        case Difficulty.Hard:
                            bestscoreid = reader.Reader.GetInt32(17);
                            break;
                        case Difficulty.Extreme:
                            bestscoreid = reader.Reader.GetInt32(18);
                            break;
                    }
                    break;
                }
            }
            if (bestscoreid != -1)
            {
                using var reader = ExecuteReader("select * from ResultTable where resultid = @resultid;", [new("@resultid", bestscoreid)]);
                while (reader.Reader.Read())
                {
                    var bestscore = reader.Reader.GetInt32(4);
                    var bestcounts = new int[]{
                            reader.Reader.GetInt32(5),
                            reader.Reader.GetInt32(6),
                            reader.Reader.GetInt32(7),
                            reader.Reader.GetInt32(8),
                            reader.Reader.GetInt32(9)
                        };
                    var bestmaxcombo = reader.Reader.GetInt32(10);
                    highscore = IsHighScore(score, counts, maxcombo, bestscore, bestcounts, bestmaxcombo);
                }
            }
            else
            {
                highscore = true;
            }
            highscore = highscore && resulttype != ResultEvaluateType.Mistake;
            var resultId = (int)ExecuteDataTable(@"insert into ResultTable(
                                   scoreid,
                                   difficulty,
                                   evaluate,
                                   score,
                                   coolcount,
                                   goodcount,
                                   safecount,
                                   sadcount,
                                   worstcount,
                                   maxcombo,
                                   finishtime,
                                   date
                                ) values(
                                   @scoreid,
                                   @difficulty,
                                   @evaluate,
                                   @score,
                                   @coolcount,
                                   @goodcount,
                                   @safecount,
                                   @sadcount,
                                   @worstcount,
                                   @maxcombo,
                                   @finishtime,
                                   @date
                                );", [
                new("@scoreid",scoreID),
                new("@difficulty",(int)difficulty),
                new("@evaluate",(int)resulttype),
                new("@score",score),
                new("@coolcount",counts[0]),
                new("@goodcount",counts[1]),
                new("@safecount",counts[2]),
                new("@sadcount",counts[3]),
                new("@worstcount",counts[4]),
                new("@maxcombo",maxcombo),
                new("@finishtime",finishtime),
                new("@date",DateTime.Now.ToString(CultureInfo.InvariantCulture))
            ]);
            if (highscore)
            {

                ExecuteDataTable("update ScoreTable set " + GetSQLColumn(difficulty) + " = @resultid where scoreid = @scoreid;", [
                        new("@scoreid", scoreID),
                        new("@resultid", resultId)
                    ]);
                songInfo.resultIDs[(int)difficulty] = resultId;
                songInfo.UpdateScore();
            }
            return highscore;
        }

        internal ResultInfo[] GetResults(int id)
        {
            var ret = new List<ResultInfo>();
            using (var reader = ExecuteReader("select * from ResultTable where scoreid = @scoreid;", [
                new("@scoreid",id)
            ]))
            {
                while (reader.Reader.Read())
                {
                    var resultInfo = new ResultInfo
                    {
                        ID = reader.Reader.GetInt32(0),
                        ScoreID = reader.Reader.GetInt32(1),
                        Difficulty = (Difficulty)reader.Reader.GetInt32(2),
                        ResultEvaluate = (ResultEvaluateType)reader.Reader.GetInt32(3),
                        Score = reader.Reader.GetInt32(4),
                        CoolCount = reader.Reader.GetInt32(5),
                        GoodCount = reader.Reader.GetInt32(6),
                        SafeCount = reader.Reader.GetInt32(7),
                        SadCount = reader.Reader.GetInt32(8),
                        WorstCount = reader.Reader.GetInt32(9),
                        MaxCombo = reader.Reader.GetInt32(10),
                        FinishTime = reader.Reader.GetFloat(11),
                        Date = DateTime.Parse(reader.Reader.GetString(12), CultureInfo.InvariantCulture)
                    };
                    ret.Add(resultInfo);
                }
            }
            ret.Sort(ResultInfoComparer.Comparer);
            return [.. ret];
        }

        internal void DeleteResult(int resultId)
        {
            ExecuteDataTable("delete from ResultTable where resultid = @resultid;", [
                new("@resultid", resultId)
            ]);
        }

        private string GetSQLColumn(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "bestscoreeasyid",
                Difficulty.Normal => "bestscorenormalid",
                Difficulty.Hard => "bestscorehardid",
                Difficulty.Extreme => "bestscoreextremeid",
                _ => "",
            };
        }

        private bool IsHighScore(int newscore, int[] newcounts, int newmaxcombo, int bestscore, int[] bestcounts, int bestmaxcombo)
        {
            if (newscore > bestscore) return true;
            else if (newscore == bestscore)
            {
                for (int i = 0; i < newcounts.Length; i++)
                {
                    if (newcounts[i] > bestcounts[i]) return true;
                    else if (newcounts[i] == bestcounts[i]) continue;
                    else return false;
                }
                if (newmaxcombo > bestmaxcombo) return true;
            }
            return false;
        }

        internal void ChangeLatency(SongInformation songInfo, float latency)
        {
            ExecuteDataTable("update ScoreTable set latency = @latency where scoreid = @scoreid;", [
                new("@scoreid", songInfo.ID),
                new("@latency",latency)
            ]);
        }

        internal void ChangeUserVolume(SongInformation songInfo, int userVolume)
        {
            ExecuteDataTable("update ScoreTable set uservolume = @uservolume where scoreid = @scoreid;", [
                new("@scoreid", songInfo.ID),
                new("@uservolume",userVolume)
            ]);
        }

        internal ScoreStorage GetScoreStorage(int id)
        {
            using (var reader = ExecuteReader("select * from ScoreStorage where scoreid = @scoreid;", [
                new("@scoreid",id)
            ]))
            {
                while (reader.Reader.Read())
                {
                    var storageId = reader.Reader.GetInt32(0);
                    var scoreId = reader.Reader.GetInt32(1);
                    var data = reader.Reader.GetString(2);
                    return new ScoreStorage(storageId, id, data);
                }
            }
            return new ScoreStorage(id);
        }

        internal void SaveScoreStorage(ScoreStorage scoreStorage)
        {
            if (scoreStorage.ID < 0)
            {
                var lastId = (int)ExecuteDataTable(@"insert into ScoreStorage(
                                scoreid,
                                data
                                ) values(
                                @scoreid,
                                @data);", [
                    new("@scoreid",scoreStorage.SongInformationID),
                    new("@data",scoreStorage.GetString())
                ]);
                scoreStorage.UpdateId(lastId);
            }
            else
            {
                ExecuteDataTable(@"update ScoreStorage set
                                data     =@data
                                where storageid = @storageid;", [
                    new("@storageid",scoreStorage.ID),
                    new("@data", scoreStorage.GetString())
                ]);
            }
        }
    }
}
