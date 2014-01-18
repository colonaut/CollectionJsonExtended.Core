using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Extensions;

namespace CollectionJsonExtended.Core
{
    public sealed class QueryRepresentation : IRepresentation
    {
        readonly UrlInfoBase _urlInfo;

        public QueryRepresentation(UrlInfoBase urlInfo)
        {
            _urlInfo = urlInfo;
        }
        
        public string Href { get { return _urlInfo.VirtualPath; } }
        public string Rel { get { return _urlInfo.Relation; } }
        public string Prompt { get; set; } //TODO: prompt... how?

        public IEnumerable<QueryParam> Data
        {
            get
            {
                return _urlInfo.QueryParams
                    .Select(pi => new QueryParam(pi));
            }
        }
    }

    public sealed class QueryParam
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

/*
    "href" : "http://example.org/search",
      "rel" : "search",
      "prompt" : "Enter search string",
      "data" :
      [
        {"name" : "search", "value" : ""}
      ]
*/