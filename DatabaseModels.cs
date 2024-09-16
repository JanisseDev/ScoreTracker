using LiteDB;

namespace ScoreTracker.DbModels
{
    public class Setting
    {
        [BsonId] public string Name { get; set; }
        public object Value { get; set; }

        public Setting(string name, object value = null)
        {
            Name = name;
            Value = value;
        }

        public const string RANDOM_LETTER_MODE = "RandomLetterMode";
        public const string ELIMINATED_MODE_LETTER_POOL = "EliminatedModeLetterPool";
    }

    public class PlayerData
    {
        [BsonId] public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Points { get; set; } = new List<int>();

        public int TotalPoints { get { return Points.Sum(); } }
    }
}
