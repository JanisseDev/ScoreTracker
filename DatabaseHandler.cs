using LiteDB;
using LiteDB.Realtime;
using LiteDB.Realtime.Subscriptions;

namespace ScoreTracker
{
    public class PlayerData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Points { get; set; } = new List<int>();
    }

    class DatabaseHandler
    {
        private RealtimeLiteDatabase db = new RealtimeLiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Storage.db"));

        private static DatabaseHandler instance = new DatabaseHandler();
        public static DatabaseHandler Instance { get { return instance; } }

        ~DatabaseHandler()
        {
            db.Dispose();
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return db.GetCollection<T>();
        }

        public ICollectionSubscriptionBuilder<T> RealtimeCollection<T>()
        {
            return db.Realtime.Collection<T>();
        }
    }
}
