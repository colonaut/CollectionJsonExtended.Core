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
            var links = SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof(TEntity), Is.ItemLink)
                .Select(ui => new LinkRepresentation<TEntity>(entity, ui, settings))
                .ToList();

            var referenceLinks = GetReferenceLinkRepresentations(entity, settings);
            links.AddRange(referenceLinks);

            return links.Any()
                ? links : null;
        }

        static IEnumerable<LinkRepresentation<TEntity>> GetReferenceLinkRepresentations(TEntity entity,
            CollectionJsonSerializerSettings settings)
        {
            return SingletonFactory<ReferenceUrlInfoCollection>.Instance
                .Find(typeof (TEntity))
                .Select(tuple =>
                    new LinkRepresentation<TEntity>(entity,
                        tuple.Item1,
                        tuple.Item2,
                        settings));
        }


        private class ReferenceUrlInfoCollection
        {
            readonly IDictionary<Type, IList<Tuple<UrlInfoBase, PropertyInfo>>> _referenceUrlInfos;

            public ReferenceUrlInfoCollection()
            {
                _referenceUrlInfos = new Dictionary<Type, IList<Tuple<UrlInfoBase, PropertyInfo>>>();
            }


            public IEnumerable<Tuple<UrlInfoBase, PropertyInfo>> Find(Type entityType)
            {
                IList<Tuple<UrlInfoBase, PropertyInfo>> referenceUrlInfos;
                if (_referenceUrlInfos.TryGetValue(entityType, out referenceUrlInfos))
                    return referenceUrlInfos;
                
                referenceUrlInfos = new List<Tuple<UrlInfoBase, PropertyInfo>>();
                foreach (var propertyInfo in typeof (TEntity).GetProperties())
                {
                    var attr = propertyInfo
                        .GetCustomAttribute<CollectionJsonReferenceAttribute>();
                    if (attr == null)
                        continue;

                    var itemUrlInfo = SingletonFactory<UrlInfoCollection>.Instance
                        .Find(attr.ReferenceType, Is.Item).SingleOrDefault();
                    if (itemUrlInfo == null)
                        continue;

                    if (itemUrlInfo.PrimaryKeyProperty.PropertyType.Name
                        != propertyInfo.PropertyType.Name)
                        throw new TypeLoadException(
                            "Referenced primary key property type does not match" +
                            " primary key property type of found Is.Item UrlInfo");

                    referenceUrlInfos.Add(new Tuple<UrlInfoBase, PropertyInfo>(itemUrlInfo, propertyInfo));
                }
                //TODO: more levels. only one level for now....
                _referenceUrlInfos.Add(entityType, referenceUrlInfos);

                return referenceUrlInfos;
            }
        }
    }
}