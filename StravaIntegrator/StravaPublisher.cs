using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;

using SimpleDatabase;

using StravaIntegrator.GpxEntities;

using Xamarin.Android.Net;

namespace StravaIntegrator
{
    public static class StravaPublisher
    {
        public static void Publish(IEnumerable<SimpleGpsLocation> simpleGpsLocations, string routeName)
        {
            var handler = new AndroidClientHandler
            {
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };

            var httpClient = new HttpClient(handler);

            var msg = new HttpRequestMessage(HttpMethod.Post, "https://www.strava.com/api/v3/uploads");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(routeName), "Name");
            content.Add(new StringContent("Integrated by SimpleTracker [URL]"), "Description");
            content.Add(new StringContent("gpx"), "data_type");
            // content.Add(new StringContent("activity_type"), "Hike"); // Run, Ride

            byte[] fileData = ConvertLocationsData(simpleGpsLocations, routeName);
            content.Add(new ByteArrayContent(fileData), "file", "route.gpx");
            msg.Content = content;

            string accessToken = "---";
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (httpClient.SendAsync(msg).GetAwaiter().GetResult() is AndroidHttpResponseMessage res)
            {
                string response = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        internal static byte[] ConvertLocationsData(IEnumerable<SimpleGpsLocation> gpsLocations, string routeName)
        {
            IEnumerable<gpxTrkTrkpt> points = gpsLocations
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
                    name = routeName,
                    trkseg = points.ToArray(),
                }
            };

            var stream = new MemoryStream();

            //using XmlWriter writer = XmlWriter.Create(destination, new XmlWriterSettings());
            new XmlSerializer(exportdata.GetType())
               .Serialize(stream, exportdata);
            return stream.ToArray();
        }
    }
}
