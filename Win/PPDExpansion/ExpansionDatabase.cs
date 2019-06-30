using PPDExpansion.Model;
using PPDFramework;
using System.Data.SQLite;

namespace PPDExpansion
{
    class ExpansionDatabase : DataBase
    {
        private static ExpansionDatabase instance = new ExpansionDatabase();

        public override string Name
        {
            get { return "PPDExpansion"; }
        }

        public static ExpansionDatabase Instance
        {
            get
            {
                return instance;
            }
        }


        private ExpansionDatabase()
        {
            Open();
            Initialize();
        }

        private void Initialize()
        {
            if (!ExistTable("ResultTable"))
            {
                ExecuteNonQueryCommand(@"create table ResultTable(
hash varchar(64) primary key,
score int,
data blob
);");
            }
        }

        public Result FindResult(string hash)
        {
            using (var reader = ExecuteReader("select * from ResultTable where hash = @hash;", new SQLiteParameter[]{
                new SQLiteParameter("@hash", hash)
            }))
            {
                while (reader.Reader.Read())
                {
                    hash = (string)reader.Reader.GetValue(0);
                    var score = (int)reader.Reader.GetValue(1);
                    var data = (byte[])reader.Reader.GetValue(2);
                    return new Result(hash, score, data);
                }
            }
            return null;
        }

        public void InsertOrUpdateResult(Result result)
        {
            if (FindResult(result.Hash) == null)
            {
                ExecuteDataTable(@"insert into ResultTable(
                                   hash,
                                   score,
                                   data)
                                   values(
                                   @hash,
                                   @score,
                                   @data);",
                                           new SQLiteParameter[]{
                                       new SQLiteParameter("@hash",result.Hash),
                                       new SQLiteParameter("@score",result.Score),
                                       new SQLiteParameter("@data",result.DataAsByte)});
            }
            else
            {
                ExecuteDataTable(@"update ResultTable set score = @score, data = @data where hash = @hash;",
                                           new SQLiteParameter[]{
                                       new SQLiteParameter("@hash",result.Hash),
                                       new SQLiteParameter("@score",result.Score),
                                       new SQLiteParameter("@data",result.DataAsByte)});
            }
        }
    }
}
