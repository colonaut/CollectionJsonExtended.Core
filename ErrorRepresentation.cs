using System.Net;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class ErrorRepresentation : IRepresentation
    {

        readonly HttpStatusCode _httpStatusCode;

        public ErrorRepresentation(HttpStatusCode httpStatusCode, string message = null)
        {
            _httpStatusCode = httpStatusCode;
            Message = message;
        }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title
        {
            get
            {
                return _httpStatusCode.ToString();
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Code
        {
            get { return (int)_httpStatusCode; }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

    }
}