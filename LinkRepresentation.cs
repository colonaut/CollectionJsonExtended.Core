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

            var entityForPrimaryKeyProperty = referenceProperty != null
               ? referenceProperty.GetValue(entity)
               : entity;
            //try this attempt in stead of the o
            var dnrUrlInfo = urlInfo as DenormalizedReferenceUrlInfo;
            if (dnrUrlInfo != null && dnrUrlInfo.PointerProperty != null)
            {
                var pval = dnrUrlInfo.PointerProperty.GetValue(entity);
                var xval = dnrUrlInfo.DenormalizedPrimaryKeyProperty.GetValue(pval);
                
                return dnrUrlInfo.VirtualPath.Replace(dnrUrlInfo.PrimaryKeyTemplate,

                        xval.ToString());
            }


           

            if (entityForPrimaryKeyProperty == null)
                return urlInfo.VirtualPath; //TODO CANTHISHAPPEN handle to not create the link... or this sgould be done before....?

            var primaryKeyValue = urlInfo.PrimaryKeyProperty.GetValue(entityForPrimaryKeyProperty); //TODO: SOLVE we must giv the complete reference in here, not the entity ^^

            var virtualPath = urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate,
                primaryKeyValue.ToString());

            //TODO CANTHISHAPPEN if we get null (i.e at a reference with no id set or being null) a link should not be created (???)

            return virtualPath;
        }
    }
}