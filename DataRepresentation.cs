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
        public DataObject Abstract { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataObject Interface { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<DataObject> Objects { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<DataObject> Abstracts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<DataObject> Interfaces { get; set; }
    }

    public class DataObject
    {
        public IEnumerable<DataRepresentation> Data { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //TODO: this should be called instance, or typename.... and abstracts could be objects??? i dunno...
        public string Concrete { get; set; }
    }
}