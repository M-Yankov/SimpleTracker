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
using System.IO;
using System.Threading.Tasks;

namespace SimpleTracker.Database
{
    public class SimpleGpsDatabase
    {
        public const string DatabaseName = "SimpleGps.db";
        private static string databasePath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), DatabaseName);
        private readonly SQLiteConnection databaseConnection;

        private static object syncLock = new object();
        private static SimpleGpsDatabase instance;

        protected SimpleGpsDatabase(string path)
        {
            this.databaseConnection = new SQLiteConnection(path);

            this.databaseConnection.CreateTable<SimpleGpsLocation>();
            this.databaseConnection.CreateTable<SimpleGpsRoute>();
        }

        public static SimpleGpsDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new SimpleGpsDatabase(databasePath);
                        }
                    }
                }

                return instance;
            }
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

        public int ClearAllRoutes()
        {
            int deletedObjectsCount = 0;
            deletedObjectsCount += this.databaseConnection.DeleteAll<SimpleGpsLocation>();
            deletedObjectsCount += this.databaseConnection.DeleteAll<SimpleGpsRoute>();

            return deletedObjectsCount;
        }

        public List<SimpleGpsRoute> GetAllRoutes()
        {
            return this.databaseConnection.Table<SimpleGpsRoute>().ToList();
        }
    }
}