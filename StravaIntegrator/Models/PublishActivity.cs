namespace StravaIntegrator.Models
{
    public class PublishActivity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        #region UpdateProperties
        /// <summary>
        /// Run, Hike, Ride ...
        /// </summary>
        public string Type { get; set; }

        public bool Muted { get; set; }
        #endregion
    }
}