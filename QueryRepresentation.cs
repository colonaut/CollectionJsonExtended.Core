using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Extensions;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class QueryRepresentation : RepresentationBase, IRepresentation
    {
        readonly UrlInfoBase _urlInfo;

        public QueryRepresentation(UrlInfoBase urlInfo,
            CollectionJsonSerializerSettings settings) //settings are needed,in order to directly serialize it
            : base(settings)
        {
            _urlInfo = urlInfo;
        }
        

        /*properties*/
        public string Href { get { return _urlInfo.VirtualPath; } }
        
        public string Rel { get { return _urlInfo.Relation; } }

        internal IEnumerable<QueryParam> Data
        {
            get
            {
                return _urlInfo.QueryParams
                    .Select(pi => new QueryParam(pi));
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prompt { get; set; } //TODO: prompt... how?S        
    }

    internal sealed class QueryParam
    {
        readonly ParameterInfo _queryParam;

        public QueryParam(ParameterInfo queryParam)
        {
            _queryParam = queryParam;
        }

        public string Name
        {
            get
            {
                return _queryParam.Name.CamelCase();

            }
        }

        //TODO: we might need a select or s.th. for enums.... and so on
        public object Value
        {
            get
            {
                return _queryParam.DefaultValue;
            }
        }
    }
    
}