using System;

using SQLite;

namespace SimpleDatabase
{
    /// <summary>
    /// Application settings
    /// </summary>
    public class SimpleGpsSettings
    {
        // When creating a record the Id should be null, otherwise it's saved with zero.
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public string StravaRefreshToken { get; set; }
        
        public string StravaAccessToken { get; set; }
        
        public DateTime? StravaAccessTokenExpirationDate { get; set; }

        public bool ShowAgreement { get; set; } = true;
    }
}