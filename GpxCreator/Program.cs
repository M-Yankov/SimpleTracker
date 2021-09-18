using System;
using System.IO;

using SQLite;

using GpxCreator.DatabaseCopy;

using GpxCreator.GpxEntities;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualBasic;

namespace GpxCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            // ExportRoutes();
            ImportRoutes();
        }

        public static void ImportRoutes()
        {
            string from = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SimpleGps.db");
            string to = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SimpleGps2.db");

            SQLiteConnection databaseConnection = new SQLiteConnection(from);
            SQLiteConnection databaseConnection2 = new SQLiteConnection(to);

            databaseConnection.CreateTable<SimpleGpsLocation>();
            databaseConnection.CreateTable<SimpleGpsRoute>();

            databaseConnection2.CreateTable<SimpleGpsLocation>();
            databaseConnection2.CreateTable<SimpleGpsRoute>();

            var sourceRoutes = databaseConnection.Table<SimpleGpsRoute>()
                .ToList();

            foreach (SimpleGpsRoute route in sourceRoutes)
            {
                SimpleGpsRoute newRoute = new SimpleGpsRoute()
                {
                    Name = route.Name
                };

                databaseConnection2.Insert(newRoute);

                IEnumerable<SimpleGpsLocation> points = databaseConnection
                    .Table<SimpleGpsLocation>()
                    .Where(x => x.SimpleGpsRouteId == route.Id)
                    .ToList()
                    .Select(x => new SimpleGpsLocation()
                    {
                        Altitude = x.Altitude,
                        DateTime = x.DateTime,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        SimpleGpsRouteId = newRoute.Id.Value
                    });

                databaseConnection2.InsertAll(points);
            }
        }

        public static void ExportRoutes()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SimpleGps.db");

            SQLiteConnection databaseConnection = new SQLiteConnection(dbPath);

            databaseConnection.CreateTable<SimpleGpsLocation>();
            databaseConnection.CreateTable<SimpleGpsRoute>();

            var routes = databaseConnection.Table<SimpleGpsRoute>()
                .ToList();

            for (int i = 0; i < routes.Count; i++)
            {
                SimpleGpsRoute item = routes[i];
                IEnumerable<gpxTrkTrkpt> points = databaseConnection
                    .Table<SimpleGpsLocation>()
                    .Where(x => x.SimpleGpsRouteId == item.Id)
                    .ToList()
                    .Select(x => new gpxTrkTrkpt()
                    {
                        ele = x.Altitude,//.ToString("F1"),
                        lat = x.Latitude,//.ToString("F7"),
                        lon = x.Longitude,//.ToString("F7"),
                        time = x.DateTime,//.ToString("u").Replace(' ', 'T')
                    });

                var exportdata = new gpx()
                {
                    creator = "com.mihyan.simpletracker",
                    version = 1.1m,
                    metadata = new gpxMetadata()
                    {
                        link = new gpxMetadataLink() { href = "http://localhost:8080", text = "localhost" },
                        time = points.OrderBy(x => x.time).First().time,
                    },
                    trk = new gpxTrk()
                    {
                        name = "[Simple tracker]",
                        trkseg = points.ToArray(),
                    }
                };

                string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"route{i + 1}.gpx");

                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    NewLineHandling = NewLineHandling.Replace
                };

                using XmlWriter writer = XmlWriter.Create(destination, new XmlWriterSettings());
                new XmlSerializer(exportdata.GetType())
                   .Serialize(new StreamWriter(destination), exportdata);

                /* Find discrepancies:
                var gpsPoints = databaseConnection.Table<SimpleGpsLocation>().Where(x => x.SimpleGpsRouteId == item.Id).ToList();
                List<Discrepancy> d = new List<Discrepancy>();

                for (int t = 0; t < gpsPoints.Count - 1; t++)
                {
                    SimpleGpsLocation previousPoint = gpsPoints[t];
                    SimpleGpsLocation nextPoint = gpsPoints[t + 1];

                    double distanceInMeters = getDistance(
                        previousPoint.Latitude,
                        previousPoint.Longitude,
                        nextPoint.Latitude,
                        nextPoint.Longitude);

                    d.Add(new Discrepancy() { Distance = distanceInMeters, NextPoint = nextPoint, PreviousPoint = previousPoint });
                }

                var ordered = d.OrderByDescending(x => x.Distance);
                */
            }

            double getDistance(double lat1, double lon1, double lat2, double lon2)
            {
                double rlat1 = Math.PI * lat1 / 180;
                double rlat2 = Math.PI * lat2 / 180;
                double theta = lon1 - lon2;
                double rtheta = Math.PI * theta / 180;
                double dist =
                    Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                    Math.Cos(rlat2) * Math.Cos(rtheta);
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515;

                return dist * 1609.344; // in km is 1.609344
            }
        }
    }

    public class Discrepancy
    {
        internal SimpleGpsLocation PreviousPoint { get; set; }

        internal SimpleGpsLocation NextPoint { get; set; }

        public double Distance { get; set; }
    }
}
