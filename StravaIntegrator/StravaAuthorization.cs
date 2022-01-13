using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web;

using Newtonsoft.Json;

using StravaIntegrator.Models;

namespace StravaIntegrator
{
    public class StravaAuthorization
    {
        /// <summary>
        /// Validates whether the user give required permission to get tokens.
        /// </summary>
        /// <param name="link">A link that with query parameters for validation.</param>
        /// <param name="requiredScopes">Required scopes separated by comma ",".</param>
        public OneTimeUsableCode TryGetAuthorizationCode(string link, string requiredScopes)
        {
            var result = new OneTimeUsableCode();

            if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
            {
                result.Error = "Invalid URL";
                return result;
            }

            NameValueCollection queryParametersList = HttpUtility.ParseQueryString(uri.Query);

            string errorValue = queryParametersList.Get("error");
            string oneTimeUsableCode = queryParametersList.Get("code");
            string scope = queryParametersList.Get("scope");
            if (!string.IsNullOrWhiteSpace(errorValue))
            {
                result.Error = $"Error: {errorValue}";
                return result;
            }

            if (string.IsNullOrWhiteSpace(oneTimeUsableCode)
                || string.IsNullOrWhiteSpace(scope))
            {
                result.Error = "Missing parameters after authorization code|scope";
                return result;
            }

            if (!scope.Contains(requiredScopes, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Error = "Please give required permissions!";
                return result;
            }

            result.Value = oneTimeUsableCode;
            return result;
        }

        /// <summary>
        /// Get refresh and access tokens from https://www.strava.com/api/v3/oauth/token
        /// </summary>
        /// <param name="oneTimeCode">It's a short lived token that can be used only once - to get refresh token.</param>
        public AuthorizationTokens GetAuthotizationsTokens(string oneTimeCode, string stravaClientId, string stravaSecret)
        {
            UriBuilder uriBuilder = new UriBuilder("https://www.strava.com/api/v3/oauth/token");
            NameValueCollection queryStringBuilder = HttpUtility.ParseQueryString(string.Empty);

            queryStringBuilder.Add("client_id", stravaClientId);
            queryStringBuilder.Add("client_secret", stravaSecret);
            queryStringBuilder.Add("code", oneTimeCode);
            queryStringBuilder.Add("grant_type", "authorization_code");

            uriBuilder.Query = queryStringBuilder.ToString();

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());

            HttpClient httpClient = new HttpClient();
            
            HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();
            
            string textResponse = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TokenExchangeResponse tokenExchangeResponse = JsonConvert.DeserializeObject<TokenExchangeResponse>(textResponse);
                return new AuthorizationTokens()
                {
                    AccessToken = tokenExchangeResponse.Access_token,
                    ExpiresAt = tokenExchangeResponse.Expires_at,
                    ExpiresIn = tokenExchangeResponse.Expires_in,
                    RefreshToken = tokenExchangeResponse.Refresh_token
                };
            }
            else
            {
                return new AuthorizationTokens()
                {
                    ErrorResponse = textResponse
                };
            }
        }
    }
}