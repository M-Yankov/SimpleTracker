using System;
using System.IO;

using SQLite;

using GpxCreator.DatabaseCopy;

using GpxCreator.GpxEntities;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace GpxCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SimpleGps.db");


            SQLiteConnection databaseConnection = new SQLiteConnection(dbPath);

            databaseConnection.CreateTable<SimpleGpsLocation>();
            databaseConnection.CreateTable<SimpleGpsRoute>();

            var routes = databaseConnection.Table<SimpleGpsRoute>()
                .Where(x => x.Id >= 26)
                .ToList();

            for (int i = 0; i < routes.Count; i++)
            {
                SimpleGpsRoute item = routes[i];
                IEnumerable<gpxTrkTrkpt> points = databaseConnection.Table<SimpleGpsLocation>().Where(x => x.SimpleGpsRouteId == item.Id)
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


                //using XmlWriter writer = XmlWriter.Create(destination, new XmlWriterSettings());
                new XmlSerializer(exportdata.GetType())
                    .Serialize(new StreamWriter(destination), exportdata);
            }
        }
    }
}
