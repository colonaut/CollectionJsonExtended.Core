using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CollectionJsonExtended.Core
{
    public sealed class LinkRepresentation<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        readonly UrlInfoBase _urlInfo;
        readonly TEntity _entity;

        public LinkRepresentation(UrlInfoBase urlInfo,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _urlInfo = urlInfo;
        }

        public LinkRepresentation(TEntity entity,
            UrlInfoBase urlInfo,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _entity = entity;
            _urlInfo = urlInfo;
        }


        /*properties*/
        public string Rel
        {
            get { return _urlInfo.Relation; }
        }

        public string Href
        {
            get { return GetParsedVirtualPath(_entity, _urlInfo); }
        } 

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Render { get { return _urlInfo.Render; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prompt { get; set; }

        
        /*private static methods*/
        static string GetParsedVirtualPath(TEntity entity, UrlInfoBase urlInfo)
        {
            if (entity == null)
                return urlInfo.VirtualPath;
            var primaryKey = urlInfo.PrimaryKeyProperty.GetValue(entity).ToString();
            var virtualPath = urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate, primaryKey);
            return virtualPath;
        }
    }
}