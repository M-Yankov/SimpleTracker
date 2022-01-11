using System;

namespace StravaIntegrator.Models
{
    public class AuthorizationTokens
    {
        /// <summary>
        /// With that token we can access information from authorized Strava user.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// With that token we can get access tokens, when it expires.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The number of seconds since the epoch when the provided access token will expire.
        /// </summary>
        public int ExpiresAt { get; set; }

        /// <summary>
        /// In UTC format.
        /// </summary>
        public DateTime AccessTokenExpireDate
        {
            get => DateTimeOffset.FromUnixTimeSeconds(this.ExpiresAt).DateTime;
        }

        /// <summary>
        /// Seconds until the short-lived access token will expire (6 hours).
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// In case the get token request failed.
        /// </summary>
        public string ErrorResponse { get; set; }
    }
}