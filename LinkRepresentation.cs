using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CollectionJsonExtended.Core
{
    public sealed class LinkRepresentation<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        readonly UrlInfoBase _urlInfo;
        readonly TEntity _entity;
        readonly PropertyInfo _referencePrimaryKeyProperty;

        public LinkRepresentation(UrlInfoBase referenceUrlInfo,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _urlInfo = referenceUrlInfo;
        }

        public LinkRepresentation(TEntity entity,
            UrlInfoBase urlInfo,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _entity = entity;
            _urlInfo = urlInfo;
        }

        public LinkRepresentation(TEntity entity,
            UrlInfoBase referenceUrlInfo,
            PropertyInfo referencePrimaryKeyProperty,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _entity = entity;
            _urlInfo = referenceUrlInfo;
            _referencePrimaryKeyProperty = referencePrimaryKeyProperty;
        }

        /*properties*/
        public string Rel
        {
            get
            {
                return _urlInfo.Relation;
            }
        }

        public string Href
        {
            get
            {
                return GetParsedVirtualPath(_entity,
                    _urlInfo,
                    _referencePrimaryKeyProperty);
            }
        } 

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Render { get { return _urlInfo.Render; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prompt { get; set; }

        
        /*private static methods*/
        static string GetParsedVirtualPath(TEntity entity,
            UrlInfoBase urlInfo,
            PropertyInfo referencePrimaryKeyProperty = null)
        {
            if (entity == null)
                return urlInfo.VirtualPath;

            var primaryKeyValue = referencePrimaryKeyProperty != null
                ? referencePrimaryKeyProperty.GetValue(entity).ToString()
                : urlInfo.PrimaryKeyProperty.GetValue(entity).ToString();

            var virtualPath = urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate, primaryKeyValue);
            return virtualPath;
        }
    }
}