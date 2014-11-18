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
        readonly PropertyInfo _referenceProperty; //TODO: obolete

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
        
        [Obsolete]
        public LinkRepresentation(TEntity entity,
            UrlInfoBase referenceUrlInfo,
            PropertyInfo referenceProperty,
            CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            _entity = entity;
            _urlInfo = referenceUrlInfo;
            _referenceProperty = referenceProperty;
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
                    _referenceProperty);
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
            PropertyInfo referenceProperty = null)
        {
            if (entity == null)
                return urlInfo.VirtualPath;

            var denormalizedReferenceInfo = urlInfo as DenormalizedReferenceInfo;
            if (denormalizedReferenceInfo != null && denormalizedReferenceInfo.PointerProperty != null)
            {
                var pointerPropertyValue = denormalizedReferenceInfo.PointerProperty.GetValue(entity);
                var denormalizedPrimaryKeyValue = denormalizedReferenceInfo.DenormalizedPrimaryKeyProperty.GetValue(pointerPropertyValue);

                return denormalizedReferenceInfo.VirtualPath.Replace(denormalizedReferenceInfo.PrimaryKeyTemplate,
                    denormalizedPrimaryKeyValue.ToString());
            }

            var primaryKeyValue = urlInfo.PrimaryKeyProperty.GetValue(entity);

            return urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate,
                primaryKeyValue.ToString());

        }
    }
}