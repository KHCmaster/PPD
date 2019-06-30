using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace PPDFramework
{
    class LogicFolderInformationComparer : IComparer<LogicFolderInfomation>
    {
        private List<int> _orderList;
        public LogicFolderInformationComparer(List<int> orderList)
        {
            _orderList = orderList;
        }
        #region IComparer<LogicFolderInfomation> メンバ

        public int Compare(LogicFolderInfomation x, LogicFolderInfomation y)
        {
            return _orderList.IndexOf(x.ID) - _orderList.IndexOf(y.ID);
        }

        #endregion
    }
    /// <summary>
    /// 論理フォルダのクラス
    /// </summary>
    public class LogicFolderInfomation
    {
        /// <summary>
        /// 追加される後に発生するイベントです。
        /// </summary>
        public static event EventHandler StaticAfterAdd;

        /// <summary>
        /// 削除される前に発生するイベントです
        /// </summary>
        public static event EventHandler StaticBeforeRemove;

        /// <summary>
        /// インデックスが変更される前に発生するイベントです
        /// </summary>
        public static event EventHandler StaticBeforeChangeIndex;

        bool disposed;

        /// <summary>
        /// 破棄されたか、Removeをしようしたかが分かります
        /// </summary>
        public bool Disposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面ID
        /// </summary>
        public int ScoreID
        {
            get;
            private set;
        }

        /// <summary>
        /// フォルダかどうか（そうでなければ譜面)
        /// </summary>
        public bool IsFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// 名前(表示名)
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 階層の深さ(root = 0)
        /// </summary>
        public int Depth
        {
            get;
            private set;
        }

        /// <summary>
        /// 作成されてた日付
        /// </summary>
        public DateTime DateTime
        {
            get;
            private set;
        }

        private string ChildIDs
        {
            get;
            set;
        }

        LogicFolderInfomation parent;
        /// <summary>
        /// 親
        /// </summary>
        public LogicFolderInfomation Parent
        {
            get
            {
                return parent;
            }
        }

        List<LogicFolderInfomation> children;
        /// <summary>
        /// 子
        /// </summary>
        public LogicFolderInfomation[] Children
        {
            get
            {
                return ChildrenList.ToArray();
            }
        }

        private List<LogicFolderInfomation> ChildrenList
        {
            get
            {
                if (children == null)
                {
                    children = GetInfos(this.ChildIDs, this);
                }
                return children;
            }
        }

        /// <summary>
        /// 子の数
        /// </summary>
        public int ChildrenCount
        {
            get
            {
                return Children.Length;
            }
        }

        static LogicFolderInfomation root;

        /// <summary>
        /// ルート
        /// </summary>
        public static LogicFolderInfomation Root
        {

            get
            {
                if (root == null)
                {
                    using (var reader = PPDDatabase.DB.ExecuteReader("select * from LogicFolder where name = @name;", new SQLiteParameter[] {
                           new SQLiteParameter("@name",PPDDatabase.LogicFolderRoot)}))
                    {
                        while (reader.Reader.Read())
                        {
                            root = new LogicFolderInfomation
                            {
                                ID = reader.Reader.GetInt32(0),
                                IsFolder = reader.Reader.GetInt32(2) == 1,
                                Name = reader.Reader.GetString(3),
                                ChildIDs = reader.Reader.GetString(4)
                            };
                            break;
                        }
                    }

                    PPDDatabase.DB.RemoveUnconnectedLinks();
                    SongInformation.Updated += SongInformation_Updated;
                }
                return root;
            }
        }

        /// <summary>
        /// 全てのロジックフォルダ情報です
        /// </summary>
        public static LogicFolderInfomation[] All
        {
            get
            {
                var ret = new List<LogicFolderInfomation>();
                var queue = new Queue<LogicFolderInfomation>();
                queue.Enqueue(Root);
                while (queue.Count > 0)
                {
                    var info = queue.Dequeue();
                    if (!info.IsFolder)
                    {
                        ret.Add(info);
                    }
                    else
                    {
                        foreach (LogicFolderInfomation child in info.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

                return ret.ToArray();
            }
        }

        static void SongInformation_Updated(object sender, EventArgs e)
        {
            SongInformation.Updated -= SongInformation_Updated;
            root = null;
        }

        private static string GetChildIDs(int id)
        {
            string childids = string.Empty;
            using (var reader = PPDDatabase.DB.ExecuteReader("select * from LogicFolder where folderid = @folderid;", new SQLiteParameter[] {
                new SQLiteParameter("@folderid",id)}))
            {
                while (reader.Reader.Read())
                {
                    childids = reader.Reader.GetString(4);
                    break;
                }
            }
            return childids;
        }

        private static List<LogicFolderInfomation> GetInfos(string ids, LogicFolderInfomation parent)
        {
            if (string.IsNullOrEmpty(ids)) return new List<LogicFolderInfomation>();

            var orderList = PPDDatabase.ParseStringToList(ids);
            ids = ids.Replace('|', ',');
            var list = new List<LogicFolderInfomation>();
            /*            using (SQLiteDataReader reader = PPDDatabase.ExecuteReader("select * from LogicFolder where folderid in (@ids);", new SQLiteParameter[] {
                            new SQLiteParameter("@ids",ids)}))
                        {*/
            using (var reader = PPDDatabase.DB.ExecuteReader(String.Format("select * from LogicFolder where folderid in ({0}) order by folderid;", ids), new SQLiteParameter[0]))
            {
                while (reader.Reader.Read())
                {
                    var info = new LogicFolderInfomation
                    {
                        ID = reader.Reader.GetInt32(0),
                        IsFolder = reader.Reader.GetInt32(2) == 1,
                        Name = reader.Reader.GetString(3)
                    };
                    if (info.IsFolder)
                    {
                        info.ChildIDs = reader.Reader.GetString(4);
                    }
                    else
                    {
                        info.ScoreID = reader.Reader.GetInt32(1);
                    }
                    info.DateTime = DateTime.Parse(reader.Reader.GetString(5), CultureInfo.InvariantCulture);
                    info.parent = parent;
                    info.Depth = parent.Depth + 1;
                    list.Add(info);
                }
            }
            list.Sort(new LogicFolderInformationComparer(orderList));

            return list;
        }

        private List<LogicFolderInfomation> GetFromFolder(LogicFolderInfomation info)
        {
            if (info.IsFolder)
            {
                return GetInfos(info.ChildIDs, info);
            }
            else
            {
                return new List<LogicFolderInfomation>();
            }
        }

        private static LogicFolderInfomation AddFolder(LogicFolderInfomation parent, string name)
        {
            if (parent == null || !parent.IsFolder) return null;
            PPDDatabase.DB.ExecuteDataTable("insert into LogicFolder(isfolder,name,childids,date) values(@isfolder,@name,@childids,@date);", new SQLiteParameter[] {
                new SQLiteParameter("@isfolder",1),                new SQLiteParameter("@name",name),                new SQLiteParameter("@childids",string.Empty),                new SQLiteParameter("@date",DateTime.Now.ToString(CultureInfo.InvariantCulture))            });
            var newfolder = new LogicFolderInfomation();
            using (var reader = PPDDatabase.DB.ExecuteReader("select * from LogicFolder where ROWID = last_insert_rowid();", null))
            {
                while (reader.Reader.Read())
                {
                    newfolder.ID = reader.Reader.GetInt32(0);
                    newfolder.IsFolder = reader.Reader.GetInt32(2) == 1;
                    newfolder.Name = reader.Reader.GetString(3);
                    newfolder.ChildIDs = reader.Reader.GetString(4);
                    newfolder.DateTime = DateTime.Parse(reader.Reader.GetString(5), CultureInfo.InvariantCulture);
                    newfolder.parent = parent;
                    newfolder.Depth = parent.Depth + 1;
                    parent.ChildrenList.Add(newfolder);
                    break;
                }
            }
            if (newfolder != null)
            {
                var ids = PPDDatabase.ParseStringToList(parent.ChildIDs);
                ids.Add(newfolder.ID);
                parent.ChildIDs = PPDDatabase.ConverListToString(ids);
                UpdateChildIDs(parent.ChildIDs, parent.ID);
            }
            StaticAfterAdd?.Invoke(newfolder, EventArgs.Empty);
            return newfolder;
        }

        /// <summary>
        /// フォルダを追加します
        /// </summary>
        /// <param name="name"></param>
        public LogicFolderInfomation AddFolder(string name)
        {
            return AddFolder(this, name);
        }

        private static LogicFolderInfomation AddScore(LogicFolderInfomation parent, SongInformation si)
        {
            return AddScore(parent, si, si.DirectoryName);
        }

        private static LogicFolderInfomation AddScore(LogicFolderInfomation parent, SongInformation si, string linkName)
        {
            if (parent == null || si == null || !parent.IsFolder || !si.IsPPDSong) return null;
            PPDDatabase.DB.ExecuteDataTable("insert into LogicFolder(scoreid,isfolder,name,date) values(@scoreid,@isfolder,@name,@date);", new SQLiteParameter[] {
                new SQLiteParameter("@scoreid",si.ID),                new SQLiteParameter("@isfolder","0"),                new SQLiteParameter("@name",linkName),                new SQLiteParameter("@date",DateTime.Now.ToString(CultureInfo.InvariantCulture))            });
            var newscore = new LogicFolderInfomation();
            using (var reader = PPDDatabase.DB.ExecuteReader("select * from LogicFolder where ROWID = last_insert_rowid();", null))
            {
                while (reader.Reader.Read())
                {
                    newscore.ID = reader.Reader.GetInt32(0);
                    newscore.IsFolder = reader.Reader.GetInt32(2) == 1;
                    newscore.Name = reader.Reader.GetString(3);
                    newscore.ScoreID = reader.Reader.GetInt32(1);
                    newscore.DateTime = DateTime.Parse(reader.Reader.GetString(5), CultureInfo.InvariantCulture);
                    newscore.parent = parent;
                    newscore.Depth = parent.Depth + 1;
                    parent.ChildrenList.Add(newscore);
                    break;
                }
            }
            if (newscore != null)
            {
                var ids = PPDDatabase.ParseStringToList(parent.ChildIDs);
                ids.Add(newscore.ID);
                parent.ChildIDs = PPDDatabase.ConverListToString(ids);
                UpdateChildIDs(parent.ChildIDs, parent.ID);
            }
            StaticAfterAdd?.Invoke(newscore, EventArgs.Empty);
            return newscore;
        }

        /// <summary>
        /// 譜面のリンクを追加します
        /// </summary>
        /// <param name="si"></param>
        public LogicFolderInfomation AddScore(SongInformation si)
        {
            return AddScore(si, si.DirectoryName);
        }

        /// <summary>
        /// 譜面のリンクを追加します
        /// </summary>
        /// <param name="si"></param>
        /// <param name="linkName"></param>
        /// <returns></returns>
        public LogicFolderInfomation AddScore(SongInformation si, string linkName)
        {
            return AddScore(this, si, linkName);
        }

        private static void UpdateChildIDs(string childIDs, int ID)
        {
            PPDDatabase.DB.ExecuteDataTable("update LogicFolder set childids = @childids where folderid = @folderid;", new SQLiteParameter[]{
                    new SQLiteParameter("@childids",childIDs),
                    new SQLiteParameter("@folderid",ID)
            });
        }

        private void Remove(LogicFolderInfomation info)
        {
            if (info == null || info.Name == PPDDatabase.LogicFolderRoot) return;
            if (StaticBeforeRemove != null)
            {
                StaticBeforeRemove.Invoke(info, EventArgs.Empty);
            }
            Remove(info, true);
        }

        /// <summary>
        /// 削除します
        /// </summary>
        public void Remove()
        {
            Remove(this);
        }

        private static void Remove(LogicFolderInfomation info, bool updateParent)
        {
            if (info.IsFolder)
            {
                foreach (LogicFolderInfomation childinfo in info.Children)
                {
                    Remove(childinfo, false);
                }
                Delete(info);
            }
            else
            {
                Delete(info);
            }
            if (updateParent)
            {
                var list = PPDDatabase.ParseStringToList(info.Parent.ChildIDs);
                if (list.Contains(info.ID))
                {
                    list.Remove(info.ID);
                }
                info.parent.ChildIDs = PPDDatabase.ConverListToString(list);
                UpdateChildIDs(info.Parent.ChildIDs, info.Parent.ID);
            }
            RemoveRelation(info);
        }

        private static void RemoveRelation(LogicFolderInfomation info)
        {
            if (info.parent != null)
            {
                info.parent.ChildrenList.Remove(info);
                info.parent = null;
            }
            info.disposed = true;
        }

        private static void Delete(LogicFolderInfomation info)
        {
            if (info == null) return;
            PPDDatabase.DB.ExecuteDataTable("delete from LogicFolder where folderid == @folderid;", new SQLiteParameter[]{
                new SQLiteParameter("@folderid",info.ID)
            });
        }

        /// <summary>
        /// リネームします
        /// </summary>
        /// <param name="newName">新しい名前</param>
        public void Rename(string newName)
        {
            Rename(this, newName);
        }

        private static void Rename(LogicFolderInfomation info, string newName)
        {
            info.Name = newName;
            PPDDatabase.DB.ExecuteDataTable("update LogicFolder set name = @name where folderid = @folderid;", new SQLiteParameter[]{
                    new SQLiteParameter("@name",newName),
                    new SQLiteParameter("@folderid",info.ID)
            });
        }

        /// <summary>
        /// 特定のLogicFolderInformationを含むか、あるいは自身かを調べます
        /// </summary>
        /// <param name="info">対象</param>
        /// <returns></returns>
        public bool ContainAsChildrenOrSelf(LogicFolderInfomation info)
        {
            if (info == null) return false;
            return ContainAsChildrenOrSelf(this, info);
        }

        private static bool ContainAsChildrenOrSelf(LogicFolderInfomation parent, LogicFolderInfomation info)
        {
            if (parent == info)
            {
                return true;
            }
            else
            {
                foreach (LogicFolderInfomation child in parent.Children)
                {
                    if (child == info)
                    {
                        return true;
                    }
                    if (child.IsFolder && ContainAsChildrenOrSelf(child, info))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// インデックスを変更します
        /// </summary>
        /// <param name="index"></param>
        public void ChangeIndex(int index)
        {
            if (this == Root) return;
            ChangeIndex(this, index);
        }

        private static void ChangeIndex(LogicFolderInfomation info, int index)
        {
            if (StaticBeforeChangeIndex != null)
            {
                StaticBeforeChangeIndex.Invoke(info, EventArgs.Empty);
            }
            var currentIndex = Array.IndexOf(info.Parent.Children, info);
            LogicFolderInfomation tempParent = info.Parent;
            info.Parent.children.Remove(info);
            info.Parent.children.Insert(index, info);
            var temp = info.Parent.Children[0].ID.ToString();
            for (int i = 1; i < info.Parent.ChildrenCount; i++)
            {
                temp += String.Format("|{0}", info.Parent.Children[i].ID);
            }
            info.Parent.ChildIDs = temp;
            UpdateChildIDs(info.Parent.ChildIDs, info.Parent.ID);
        }
    }
}
