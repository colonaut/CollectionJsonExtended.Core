using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class CollectionRepresentation<TEntity> : CollectionJsonWriter, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        /* Private fields */
        string _version = "1.0";

        /* Ctor */
        public CollectionRepresentation(TEntity entity,
            CollectionJsonSerializerSettings settings) : base(settings)
        {
            Items = new List<ItemRepresentation<TEntity>>
                    {
                        new ItemRepresentation<TEntity>(entity, settings)
                    };
        }

        public CollectionRepresentation(IEnumerable<TEntity> entities,
            CollectionJsonSerializerSettings settings) : base(settings)
        {
            Items = new List<ItemRepresentation<TEntity>>(entities.Select(entity =>
                new ItemRepresentation<TEntity>(entity, settings)));
            
            Template = new WriteTemplateRepresentation<TEntity>(settings);
            
            Links = new List<LinkRepresentation>();

            Queries = GetQueryRepresentations(settings);
        }


        /* Properties */
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public string Href
        {
            get { return GetParsedVirtualPath(); }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<LinkRepresentation> Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<ItemRepresentation<TEntity>> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<QueryRepresentation> Queries { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WriteTemplateRepresentation<TEntity> Template { get; set; }


        /*private static methods*/
        static string GetParsedVirtualPath()
        {
            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance
                .TryFindSingle(typeof(TEntity), Is.Base, out urlInfo))
                throw new Exception("must be single..."); //TODO exception
            return urlInfo.VirtualPath;
        }

        static IList<QueryRepresentation> GetQueryRepresentations(CollectionJsonSerializerSettings settings)
        {
            var queries = SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof (TEntity), Is.Query)
                .Select(ui => new QueryRepresentation(ui, settings)).ToList();
            if (queries.Any())
                return queries;
            return null;
        }
    }
}