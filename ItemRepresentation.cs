using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using CollectionJsonExtended.Core.Extensions;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class ItemRepresentation<TEntity> : IRepresentation<TEntity> where TEntity : class, new()
    {
        readonly CollectionJsonSerializerSettings _settings;
        readonly IEnumerable<UrlInfoBase> _urlInfoCollection;
        TEntity _entity;
        
        /* Ctor */
        ItemRepresentation(TEntity entity,
            IEnumerable<UrlInfoBase> urlInfoCollection)
        {
            _entity = entity;
            _urlInfoCollection = urlInfoCollection;
        }

        public ItemRepresentation(TEntity entity,
            IEnumerable<UrlInfoBase> urlInfoCollection,
            CollectionJsonSerializerSettings settings)
            : this(entity, urlInfoCollection)
        {
            _settings = settings;
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
        public IEnumerable<LinkRepresentation> Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TEntity Entity
        {
            get { return _settings.ConversionMethod == ConversionMethod.Entity ? _entity : null; }
            set { _entity = value; }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof (DataRepresentationConverter))]
        public object Data
        {
            get { return _settings.ConversionMethod == ConversionMethod.Data ? _entity : null; }
        }

        public CollectionJsonSerializerSettings Settings { get { return _settings; } }

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
    }
}