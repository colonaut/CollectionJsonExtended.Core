using System.Collections.Generic;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public class DataRepresentation : IRepresentation
    {
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<object> Values { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataObject Object { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<DataObject> Objects { get; set; }
    }

    public class DataObject
    {
        public IEnumerable<DataRepresentation> Data { get; set; }

        public string Type { get; set; }
    }
}