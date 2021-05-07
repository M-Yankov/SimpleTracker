using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTracker.Database
{
    public class SimpleGpsDatabase
    {
        public const string DatabaseName = "SimpleGps.db";
        private readonly SQLiteConnection databaseConnection;

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

        public int Add(IEnumerable<SimpleGpsLocation> locations)
        {
            return this.databaseConnection.InsertAll(locations);
        }

        public int Add(SimpleGpsRoute model)
        {
            return this.databaseConnection.Insert(model);
        }
    }
}