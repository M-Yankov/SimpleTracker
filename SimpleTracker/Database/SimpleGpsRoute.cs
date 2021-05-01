using SQLite;

namespace SimpleTracker.Database
{
    public class SimpleGpsRoute
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}