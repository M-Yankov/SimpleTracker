

using SQLite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpxCreator.DatabaseCopy
{
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