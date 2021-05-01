

using SQLite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpxCreator.DatabaseCopy
{
    public class SimpleGpsDatabase
    {
        private SQLiteConnection databaseConnection;

        public SimpleGpsDatabase(string path)
        {
            this.databaseConnection = new SQLiteConnection(path);

            this.databaseConnection.CreateTable<SimpleGpsLocation>();
            this.databaseConnection.CreateTable<SimpleGpsRoute>();
        }

        public int Add(SimpleGpsLocation model)
        {
            return this.databaseConnection.Insert(model);
        }

        public int Add(SimpleGpsRoute model)
        {
            return this.databaseConnection.Insert(model);
        }
    }
}