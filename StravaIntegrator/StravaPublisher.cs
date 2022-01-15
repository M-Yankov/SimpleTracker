using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;

using Newtonsoft.Json;

using SimpleDatabase;

using StravaIntegrator.GpxEntities;
using StravaIntegrator.Models;

using Xamarin.Android.Net;

namespace StravaIntegrator
{
    public static class StravaPublisher
    {
        public static UploadActivityModel Publish(
            IEnumerable<SimpleGpsLocation> simpleGpsLocations,
            string accessToken,
            PublishActivity activity)
        {
            var handler = new AndroidClientHandler
            {
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };

            var httpClient = new HttpClient(handler);

            var msg = new HttpRequestMessage(HttpMethod.Post, "https://www.strava.com/api/v3/uploads");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(activity.Name), "Name");
            content.Add(new StringContent("Integrated by SimpleTracker [URL]"), "description"); // is this working?
            content.Add(new StringContent("gpx"), "data_type");

            byte[] fileData = ConvertLocationsData(simpleGpsLocations, activity.Name);
            content.Add(new ByteArrayContent(fileData), "file", "route.gpx");
            msg.Content = content;

            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Work Async
            HttpResponseResult<UploadActivityResult> uploadResult = ExecuteRequest<UploadActivityResult>(msg, httpClient);
            if (!string.IsNullOrWhiteSpace(uploadResult.Error)
                || !string.IsNullOrWhiteSpace(uploadResult.Value.Error))
            {
                return new UploadActivityModel { Error = uploadResult.Error ?? uploadResult.Value.Error };
            }

            do
            {
                System.Threading.Tasks.Task.Delay(5000).GetAwaiter().GetResult();

                HttpRequestMessage getUploadResultStatus = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://www.strava.com/api/v3/uploads/{uploadResult.Value.Id}");

                getUploadResultStatus.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                uploadResult = ExecuteRequest<UploadActivityResult>(getUploadResultStatus, httpClient);

                // When the upload is ready the status message is "Your activity is ready.", but the better option is to check activityId.
            } while (!string.IsNullOrEmpty(uploadResult.Error) 
                    || !string.IsNullOrEmpty(uploadResult.Value?.Error) 
                    || uploadResult.Value.Activity_id.HasValue == false);

            return new UploadActivityModel()
            {
                Error = uploadResult.Value.Error,
                Id = uploadResult.Value.Activity_id.Value,
            };

            // content.Add(new StringContent("type"), "Hike"); // Run, Ride
            // content.Add(new StringContent("hide_from_home"), "true"); // Run, Ride - control this with from settings
            //var msg2 = new HttpRequestMessage(HttpMethod.Put, "https://www.strava.com/api/v3/activities/{id}");
            //msg2.Content = new StringContent(,) { } 
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

        private static HttpResponseResult<T> ExecuteRequest<T>(HttpRequestMessage request, HttpClient client) where T : class
        {
            HttpResponseResult<T> result = new HttpResponseResult<T>();
            if (client.SendAsync(request).GetAwaiter().GetResult() is AndroidHttpResponseMessage res)
            {
                string response = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // in most cases 201
                if (res.IsSuccessStatusCode)
                {
                    result.Value = JsonConvert.DeserializeObject<T>(response);
                }
                else
                {
                    result.Error = response;
                }
            }
            else
            {
                result.Error = "General error";
            }

            return result;
        }
    }
}
