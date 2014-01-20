using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class ItemRepresentation<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        TEntity _entity;
        
        /* Ctor */
        public ItemRepresentation(TEntity entity,
            CollectionJsonSerializerSettings settings) : base(settings)
        {
            _entity = entity;            
        }


        /*public properties*/
        public string Href
        {
            get
            {
                return GetParsedVirtualPath(_entity);
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<LinkRepresentation<TEntity>> Links
        {
            get
            {
                return GetLinkRepresentations(_entity, Settings);                
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TEntity Entity
        {
            get
            {
                return Settings.ConversionMethod == ConversionMethod.Entity
                    ? _entity
                    : null;
            }
            set { _entity = value; }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof (DataRepresentationConverter))]
        public object Data
        {
            get
            {
                return Settings.ConversionMethod == ConversionMethod.Data ? _entity : null;
            }
        }


        /*private static methods*/
        static string GetParsedVirtualPath(TEntity entity)
        {
            //TODO: how to deal with renderType (TryFindSingle for i.e. Is.Item crashes, when we have another Is.Item that is for another renderType...)
            //MayBe add another method?
            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance
                .TryFindSingle(typeof (TEntity), Is.Item, out urlInfo))
                return null;

            var primaryKey = urlInfo.PrimaryKeyProperty.GetValue(entity).ToString();
            var virtualPath = urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate, primaryKey);
            return virtualPath;
        }

        static IEnumerable<LinkRepresentation<TEntity>> GetLinkRepresentations(TEntity entity,
            CollectionJsonSerializerSettings settings)
        {
            var links = SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof(TEntity), Is.LinkForItem)
                .Select(ui => new LinkRepresentation<TEntity>(entity, ui, settings))
                .ToList();
            return links.Any()
                ? links : null;
        }
    }
}