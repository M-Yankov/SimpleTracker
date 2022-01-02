using SQLite;

using System;
using System.Diagnostics;

namespace GpxCreator.DatabaseCopy
{
    [DebuggerDisplay("Lat = {Latitude}, Lon = {Longitude}")]
    public class SimpleGpsLocation
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        /// <summary>
        /// Elevation.
        /// </summary>
        public double Altitude { get; set; }

        public DateTime DateTime { get; set; }

        public int SimpleGpsRouteId { get; set; }
    }
}