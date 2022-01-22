using SQLite;

namespace SimpleDatabase
{
    public class SimpleGpsRoute
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public string Name { get; set; }

        public long? StravaActivityId { get; set; }
    }
}