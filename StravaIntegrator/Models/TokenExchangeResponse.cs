namespace StravaIntegrator.Models
{
    /// <summary>
    /// https://developers.strava.com/docs/authentication/
    /// </summary>
    internal class TokenExchangeResponse
    {
        /// <summary>
        /// In most cases "Bearer"
        /// </summary>
        public string Token_type { get; set; }

        /// <summary>
        /// The number of seconds since the epoch when the provided access token will expire 
        /// </summary>
        public int Expires_at { get; set; }

        /// <summary>
        /// Seconds until the short-lived access token will expire 
        /// </summary>
        public int Expires_in { get; set; }

        /// <summary>
        /// The refresh token for this user, to be used to get the next access token for this user.
        /// Please expect that this value can change anytime you retrieve a new access token.
        /// Once a new refresh token code has been returned, the older code will no longer work. 
        /// </summary>
        public string Refresh_token { get; set; }

        /// <summary>
        /// Access tokens are used by applications to obtain and modify Strava resources on behalf of the authenticated athlete.
        /// Refresh tokens are used to obtain new access tokens when older ones expire.
        /// </summary>
        public string Access_token { get; set; }
    }
}