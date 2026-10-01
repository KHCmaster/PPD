using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace PPDFramework
{
    /// <summary>
    /// 設定用のデータベースクラスです
    /// <remarks>
    /// 使い方
    /// 
    /// class TestSetting : SettingDataBase
    /// {
    ///     private static TestSetting setting = new TestSetting();
    ///     public override string Name
    ///     {
    ///         get { return "Test.setting"; }
    ///     }
    /// 
    ///     public static TestSetting Setting
    ///     {
    ///         get
    ///         {
    ///             return setting;
    ///         }
    ///     }
    /// }
    /// TestSetting.Setting["key"] = value;
    /// string value = TestSetting.Setting["key"];
    /// </remarks>
    /// </summary>
    public abstract class SettingDataBase : DataBase
    {
        /// <summary>
        /// コンストラクタです
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "派生クラスは DB の名前とテーブルの初期値（OnInitialize）だけを決める設計で、派生クラスのフィールドには依存しない")]
        protected SettingDataBase()
        {
            this.Open();
            if (!ExistTable("SettingTable"))
            {
                ExecuteNonQueryCommand("CREATE TABLE SettingTable(key text, value text);");
                OnInitialize();
            }
        }

        /// <summary>
        /// 実行ファイル(dllファイル)のあるディレクトリにdbファイルを生成します
        /// </summary>
        public override string DBPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location), DBName);
            }
        }

        /// <summary>
        /// テーブル初期化時に呼ばれます
        /// </summary>
        protected virtual void OnInitialize()
        {

        }

        /// <summary>
        /// 設定を取得、設定します
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                string ret = null;
                using (var reader = ExecuteReader("select * from SettingTable where key = @key;", new SQLiteParameter[] { new("@key", key) }))
                {
                    while (reader.Reader.Read())
                    {
                        ret = reader.Reader[1].ToString();
                        break;
                    }
                }
                return ret;
            }
            set
            {
                if (this[key] != null)
                {
                    ExecuteDataTable("Update SettingTable set value = @value where key = @key;", new SQLiteParameter[] {
                        new("@key", key),
                        new("@value", value)
                    });
                }
                else
                {
                    ExecuteDataTable("Insert into SettingTable(key, value) values(@key, @value);", new SQLiteParameter[] {
                        new("@key", key),
                        new("@value", value)
                    });
                }
            }
        }
    }
}
