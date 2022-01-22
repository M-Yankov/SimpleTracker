namespace StravaIntegrator.Models
{
    internal class HttpResponseResult<T> where T : class
    {
        /// <summary>
        /// When status code is not successful.
        /// </summary>
        public string Error { get; set; }

        public T Value { get; set; }
    }
}