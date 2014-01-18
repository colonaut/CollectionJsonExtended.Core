using System.Collections.Generic;
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

       
        public string Href
        {
            get { return this.GetParsedVirtualPath(_entity);}
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
        public object Data { get { return _settings.ConversionMethod == ConversionMethod.Data ? _entity : null; } }

    }
}