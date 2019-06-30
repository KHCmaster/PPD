using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// データベースの基底クラスです
    /// </summary>
    public abstract class DataBase
    {
        SQLiteConnection connection;
        readonly object lockObject = new object();

        /// <summary>
        /// データベースの名前、例えばHoge
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// データベースのファイルの名前です(Name+".db"を返します)
        /// </summary>
        public string DBName
        {
            get
            {
                return String.Format(Name + ".db");
            }
        }

        /// <summary>
        /// データベースのパスです
        /// </summary>
        public virtual string DBPath
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, DBName);
            }
        }

        /// <summary>
        /// 閉じているかどうかです
        /// </summary>
        public bool IsClosed
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        protected DataBase()
        {
            IsClosed = true;
        }

        /// <summary>
        /// データベースを開きます
        /// </summary>
        public virtual void Open()
        {
            if (!IsClosed)
            {
                return;
            }

            IsClosed = false;

            connection = new SQLiteConnection(String.Format("Data Source={0}", DBPath));
            connection.Open();
        }

        /// <summary>
        /// データベースを閉じます
        /// </summary>
        public void Close()
        {
            if (IsClosed)
            {
                return;
            }

            connection.Close();
            connection = null;
        }

        public void Begin()
        {
            ExecuteNonQueryCommand("BEGIN");
        }

        public void Commit()
        {
            ExecuteNonQueryCommand("COMMIT");
        }

        public void Rollback()
        {
            ExecuteNonQueryCommand("ROLLBACK");
        }

        public DataBaseTransaction BeginTransaction()
        {
            return new DataBaseTransaction();
        }

        /// <summary>
        /// SQLを実行します
        /// </summary>
        /// <param name="sql"></param>
        public void ExecuteNonQueryCommand(string sql)
        {
            lock (lockObject)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// データ変更SQLを実行します
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public long ExecuteDataTable(string sql, SQLiteParameter[] parameters)
        {
            lock (lockObject)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                while (cmd.Parameters.Count > 0)
                {
                    cmd.Parameters.RemoveAt(0);
                }
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                var adapter = new SQLiteDataAdapter(cmd);
                var data = new DataTable();
                adapter.Fill(data);
                return connection.LastInsertRowId;
            }
        }

        /// <summary>
        /// 読み取りのSQLを実行します
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataBaseReader ExecuteReader(string sql, SQLiteParameter[] parameters)
        {
            var lockTaken = false;
            Monitor.TryEnter(lockObject, ref lockTaken);
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            while (cmd.Parameters.Count > 0)
            {
                cmd.Parameters.RemoveAt(0);
            }
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            var reader = cmd.ExecuteReader();
            return new DataBaseReader(reader, lockObject, lockTaken);
        }

        /// <summary>
        /// データベースにテーブルがあるかを返します
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ExistTable(string tableName)
        {
            bool existTable = false;
            using (var reader = ExecuteReader("SELECT * FROM sqlite_master  WHERE type='table' AND name='" + tableName + "';", null))
            {
                while (reader.Reader.Read())
                {
                    if (reader.Reader.HasRows)
                    {
                        existTable = true;
                        break;
                    }
                }
            }
            return existTable;
        }

        /// <summary>
        /// データベースにインデックスがあるかどうかを返します
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public bool ExistIndex(string indexName)
        {
            bool existIndex = false;
            using (var reader = ExecuteReader("SELECT * FROM sqlite_master  WHERE type='index' AND name='" + indexName + "';", null))
            {
                while (reader.Reader.Read())
                {
                    if (reader.Reader.HasRows)
                    {
                        existIndex = true;
                        break;
                    }
                }
            }
            return existIndex;
        }

        /// <summary>
        /// テーブルがカラムを持つかどうかを返します
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool HasColumn(string tableName, string columnName)
        {
            bool hasColumn = false;
            using (var reader = ExecuteReader(String.Format("PRAGMA table_info({0});", tableName), null))
            {
                while (reader.Reader.Read())
                {
                    if (reader.Reader[1].ToString() == columnName)
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }
            return hasColumn;
        }

        /// <summary>
        /// カラムのタイプを取得します。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string GetColumnType(string tableName, string columnName)
        {
            using (var reader = ExecuteReader(String.Format("PRAGMA table_info({0});", tableName), null))
            {
                while (reader.Reader.Read())
                {
                    if (reader.Reader[1].ToString() == columnName)
                    {
                        return reader.Reader[2].ToString().ToLower();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// テーブルにカラムを追加します
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        public void AddColumn(string tableName, string columnName, string columnType)
        {
            ExecuteNonQueryCommand(string.Format("ALTER TABLE {0} ADD COLUMN {1} {2};", tableName, columnName, columnType));
        }

        /// <summary>
        /// テーブルにカラムを追加します
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <param name="defaultValue"></param>
        public void AddColumn(string tableName, string columnName, string columnType, string defaultValue)
        {
            ExecuteNonQueryCommand(string.Format("ALTER TABLE {0} ADD COLUMN {1} {2} default {3};", tableName, columnName, columnType, defaultValue));
        }
    }
}
