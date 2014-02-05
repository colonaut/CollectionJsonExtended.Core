using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Attributes;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public sealed class ItemRepresentation<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity: class, new()
    {
        TEntity _entity;
        
        /* Ctor */
        public ItemRepresentation(TEntity entity,
            CollectionJsonSerializerSettings settings)
            : base(settings)
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
            //TODO here we would want to find external entity links if we find a collectionjson reference attribuet somewhere deep in this entity....
            //OR!!!! we try to get that or provide the info when instance of attribute is created...?
            
            //we want to cahe this shit here... so we would have reflection, but only once.

            var links = SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof(TEntity), Is.LinkForItem)
                .Select(ui => new LinkRepresentation<TEntity>(entity, ui, settings))
                .ToList();

            links.AddRange(GetReferenceUrlInfos(typeof(TEntity))
                //TODO: the link representation must get antother constructor... we need to pass the primaryKey... somehow... because we have it here for instance in a Slog... or wany other reference type.
                //and we must check the relation..... we could add another relation (xxx.reference or s.th.) and the href is totally wrong but that might change with the new signature
                .Select(ui => new LinkRepresentation<TEntity>(ui, settings))
                .ToList());

            return links.Any()
                ? links : null;
        }

        //TODO: better cache... static in generic....
        static readonly IDictionary<Type, IEnumerable<UrlInfoBase>> ReferenceUrlInfos =
            new Dictionary<Type, IEnumerable<UrlInfoBase>>();

        static IEnumerable<UrlInfoBase> GetReferenceUrlInfos(Type entityType)
        {
            IEnumerable<UrlInfoBase> referenceUrlInfos;
            if (ReferenceUrlInfos.TryGetValue(entityType, out referenceUrlInfos))
                return referenceUrlInfos;

            var urlInfos = new List<UrlInfoBase>();
            foreach (var propertyInfo in entityType.GetProperties())
            {

                if (propertyInfo.Name == "SlogId")
                {
                    var y = propertyInfo
                        .GetCustomAttribute<CollectionJsonReferenceAttribute>();
                    
                    var x = "Hu";

                }
                
                
                if (propertyInfo
                    .GetCustomAttribute<CollectionJsonReferenceAttribute>()
                    == null)
                    continue;

                var itemUrlInfo = SingletonFactory<UrlInfoCollection>.Instance
                    .Find(propertyInfo.ReflectedType, Is.Item).SingleOrDefault();
                if (itemUrlInfo != null
                    && itemUrlInfo.PrimaryKeyProperty.PropertyType.Name
                    == propertyInfo.PropertyType.Name)
                    urlInfos.Add(itemUrlInfo);
            }

            //TODO: more levels. only one level for now....
            ReferenceUrlInfos.Add(entityType, urlInfos);
            return urlInfos;
        }
    }
}