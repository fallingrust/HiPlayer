using HiPlayer.Model;
using SQLite;

namespace HiPlayer.Helper
{
    public class DbHelper : SQLiteConnection
    {
        public TableQuery<UrlModel> UrlModels { get { return this.Table<UrlModel>(); } }
        public DbHelper(string path) : base(path)
        {
            CreateTable<UrlModel>();
        }
    }
}
