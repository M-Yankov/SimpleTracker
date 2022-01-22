namespace StravaIntegrator.Models
{
    internal class UploadActivityResult
    {
        public string Id_str { get; set; }

        public long? Activity_id { get; set; }

        public string External_id { get; set; }

        /// <summary>
        /// Id of the upload.
        /// </summary>
        public long Id { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }
}