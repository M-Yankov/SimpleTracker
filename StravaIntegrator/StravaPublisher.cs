using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Android.Content.PM;

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
            PublishActivity activity,
            PackageInfo packageInfo)
        {
            var handler = new AndroidClientHandler
            {
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };

            #region UploadActivity
            var httpClient = new HttpClient(handler);

            var createRouteRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.strava.com/api/v3/uploads");
            var content = new MultipartFormDataContent();

            string description = "Integrated by SimpleTracker https://github.com/M-Yankov/SimpleTracker";
            if (!string.IsNullOrWhiteSpace(activity.Description))
            {
                description = $"{activity.Description}\n{description}";
            }

            content.Add(new StringContent(activity.Name), "name");
            content.Add(new StringContent(description), "description");

            content.Add(new StringContent("gpx"), "data_type");

            byte[] fileData = ConvertLocationsData(simpleGpsLocations, activity.Name, packageInfo);
            content.Add(new ByteArrayContent(fileData), "file", "route.gpx");

            createRouteRequest.Content = content;
            createRouteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Work Async
            HttpResponseResult<UploadActivityResult> uploadResult = ExecuteRequest<UploadActivityResult>(createRouteRequest, httpClient);
            if (!string.IsNullOrWhiteSpace(uploadResult.Error)
                || !string.IsNullOrWhiteSpace(uploadResult.Value.Error))
            {
                return new UploadActivityModel { Error = uploadResult.Error ?? uploadResult.Value.Error };
            }

            bool shouldCheckForUploadStatus;

            do
            {
                // the activity is not immediately ready, wait a little bit to not flood the service with requests.
                // ideally "webhooks" will do the job, but it's far away from them.
                Task.Delay(3000).GetAwaiter().GetResult();

                HttpRequestMessage getUploadStatusRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://www.strava.com/api/v3/uploads/{uploadResult.Value.Id}");

                getUploadStatusRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                uploadResult = ExecuteRequest<UploadActivityResult>(getUploadStatusRequest, httpClient);

                // When the upload is ready the status message is "Your activity is ready.", but the better option is to check activityId.
                shouldCheckForUploadStatus = !string.IsNullOrEmpty(uploadResult.Error)
                    || !string.IsNullOrEmpty(uploadResult.Value?.Error)
                    || uploadResult.Value.Activity_id.HasValue == false;

            } while (shouldCheckForUploadStatus);

            if (!string.IsNullOrEmpty(uploadResult.Error)
                || !string.IsNullOrEmpty(uploadResult.Value?.Error))
            {
                return new UploadActivityModel()
                {
                    Error = uploadResult.Error ?? uploadResult.Value.Error
                };
            }
            #endregion

            #region UpdateActivityType
            UpdateActivityModel updateActivityTypeModel = new UpdateActivityModel()
            {
                hide_from_home = activity.Muted,
                type = activity.Type
            };

            string updateBodyJson = JsonConvert.SerializeObject(updateActivityTypeModel);

            var updateActivityRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://www.strava.com/api/v3/activities/{uploadResult.Value.Activity_id.Value}");
            updateActivityRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            updateActivityRequest.Content = new StringContent(updateBodyJson, System.Text.Encoding.UTF8, "application/json");

            HttpResponseResult<object> updateActivityResponse = ExecuteRequest<object>(updateActivityRequest, httpClient);
            #endregion

            // need to wait a little bit until changing activity type is ready.
            Task.Delay(3000).GetAwaiter().GetResult();

            #region ResetActivityType
            UpdateActivityModel updateGearModel = new UpdateActivityModel()
            {
                hide_from_home = activity.Muted,
                gear_id = "none" // clears associated gear.
            };
            updateBodyJson = JsonConvert.SerializeObject(updateGearModel);

            var updateGearActivityRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://www.strava.com/api/v3/activities/{uploadResult.Value.Activity_id.Value}");
            updateGearActivityRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            updateGearActivityRequest.Content = new StringContent(updateBodyJson, System.Text.Encoding.UTF8, "application/json");

            updateActivityResponse = ExecuteRequest<object>(updateGearActivityRequest, httpClient);
            #endregion

            return new UploadActivityModel()
            {
                Error = updateActivityResponse.Error,
                Id = uploadResult.Value.Activity_id.Value,
            };
        }

        internal static byte[] ConvertLocationsData(
            IEnumerable<SimpleGpsLocation> gpsLocations,
            string routeName,
            PackageInfo packageInfo)
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
                creator = packageInfo.PackageName,
                version = packageInfo.VersionName,
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
