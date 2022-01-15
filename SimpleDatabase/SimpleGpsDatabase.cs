using System.Collections.Generic;
using System.IO;

using Android.Webkit;

using SQLite;

namespace SimpleDatabase
{
    public class SimpleGpsDatabase
    {
        public const string DatabaseName = "SimpleGps.db";
        private static readonly string databasePath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), DatabaseName);
        private readonly SQLiteConnection databaseConnection;

        private static object syncLock = new object();
        private static SimpleGpsDatabase instance;

        protected SimpleGpsDatabase(string path)
        {
            this.databaseConnection = new SQLiteConnection(path);

            this.databaseConnection.CreateTable<SimpleGpsLocation>();
            this.databaseConnection.CreateTable<SimpleGpsRoute>();
            this.databaseConnection.CreateTable<SimpleGpsSettings>();

            this.InitializeSettings();
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

        public void InitializeSettings()
        {
            SimpleGpsSettings settings = this.databaseConnection
                .Table<SimpleGpsSettings>()
                .FirstOrDefault();

            if (settings == null)
            {
                settings = new SimpleGpsSettings();
                this.databaseConnection.Insert(settings);
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

        public List<SimpleGpsRoute> GetAllRoutes() =>
            this.databaseConnection
                .Table<SimpleGpsRoute>()
                .ToList();

        public List<SimpleGpsLocation> GetRouteLocations(int routeId) =>
            this.databaseConnection
                .Table<SimpleGpsLocation>()
                .Where(x => x.SimpleGpsRouteId == routeId)
                .ToList();

        public SimpleGpsRoute GetRoute(int id) =>
            this.databaseConnection
                .Table<SimpleGpsRoute>()
                .FirstOrDefault(x => x.Id == id);

        public void UpdateRouteStravaActivityId(int id, long stravaActivityId)
        {
            SimpleGpsRoute route = GetRoute(id);

            if (route != null)
            {
                route.StravaActivityId = stravaActivityId;
                this.databaseConnection.Update(route);
            }
        }

        public void DeleteRouteWithPath(int routeId)
        {
            var list = GetRouteLocations(routeId);
            foreach (var item in list)
            {
                this.databaseConnection.Delete(item);
            }

            var route = GetRoute(routeId);
            this.databaseConnection.Delete(route);
        }

        /// <summary>
        /// It's expected to have only one record
        /// </summary>
        public SimpleGpsSettings GetSettings()
        {
            SimpleGpsSettings settings = this.databaseConnection
                .Table<SimpleGpsSettings>()
                .FirstOrDefault();

            if (settings == null)
            {
                throw new System.NullReferenceException("Setting are not initialized or manually deleted!");
            }

            return settings;
        }

        public bool UpdateSettings(SimpleGpsSettings newSettings)
        {
            SimpleGpsSettings currentSettings = this.GetSettings();

            currentSettings.ShowAgreement = newSettings.ShowAgreement;
            currentSettings.StravaAccessToken = newSettings.StravaAccessToken;
            currentSettings.StravaAccessTokenExpirationDate = newSettings.StravaAccessTokenExpirationDate;
            currentSettings.StravaRefreshToken = newSettings.StravaRefreshToken;
            
            int afectedRows = this.databaseConnection.Update(currentSettings);

            return afectedRows == 1;
        } 
    }
}