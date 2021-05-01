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

            IEnumerable<gpxTrkTrkpt> points = databaseConnection.Table<SimpleGpsLocation>().Where(x => x.SimpleGpsRouteId == 5)
                .ToList()
                .Select(x => new gpxTrkTrkpt()
                {
                    ele = x.Altitude.ToString("F1"),
                    lat = x.Latitude.ToString("F7"),
                    lon = x.Longitude.ToString("F7"),
                    time = x.DateTime.ToString("u").Replace(' ', 'T')
                });

            var exportdata = new gpx()
            {
                creator = "com.mihayn.simpletracker",
                version = 1.1m,
                metadata = new gpxMetadata()
                {
                    link = new gpxMetadataLink() { href = "http://localhost:8080", text = "localhost" },
                    time = new DateTime(2021, 04, 30, 11, 6, 6),
                },
                trk = new gpxTrk()
                {
                    name = "[Simple tracker]",
                    trkseg = points.ToArray(),
                }
            };

            string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "route.gpx");

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None
            };

            using XmlWriter writer = XmlWriter.Create(destination, settings);
            new XmlSerializer(exportdata.GetType())
                .Serialize(writer, exportdata);
        }
    }
}
