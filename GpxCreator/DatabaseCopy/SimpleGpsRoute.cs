using SQLite;
using System.Collections.Generic;

namespace GpxCreator.DatabaseCopy
{
    public class SimpleGpsRoute
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}