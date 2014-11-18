using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
            //TODO CRITICAL how to deal with renderType (TryFindSingle for i.e. Is.Item crashes, when we have another Is.Item that is for another renderType...)
            //MayBe add another method?
            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance //THIS could be the error, it's not single! (we must cjeck render type... or urlInfo type or something)
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
            var denormalizedReferenceLinks   = GetDenormalizedReferenceUrlInfos(typeof (TEntity))
                .Select(dnrui => new LinkRepresentation<TEntity>(entity, dnrui, settings))
                .ToList();
            links.AddRange(denormalizedReferenceLinks);

            //var referenceLinks = GetReferenceLinkRepresentations(entity, settings);
            //links.AddRange(referenceLinks);

            return links.Any()
                ? links : null;
        }

        static List<Type> _handledDenormalizedReferenceTypes = new List<Type>();
        static IEnumerable<DenormalizedReferenceInfo> GetDenormalizedReferenceUrlInfos(Type entityType)
        {
            if (_handledDenormalizedReferenceTypes.Contains(entityType))
                return SingletonFactory<UrlInfoCollection>.Instance
                    .Find<DenormalizedReferenceInfo>(entityType);
            
            _handledDenormalizedReferenceTypes.Add(entityType);

            var result = new List<DenormalizedReferenceInfo>();
            foreach (var propertyInfo in typeof (TEntity).GetProperties())
            {
                //TODO make better
                var aa = propertyInfo.PropertyType.IsGenericType;
                //var bb = (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (DenormalizedReference<>));
                var c = propertyInfo.PropertyType.Name.Contains("Denormalized");

                if (propertyInfo.PropertyType.IsGenericType
                    && propertyInfo.PropertyType.Name.Contains("Denormalized"))
                {
                    var normalizedReferenceType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    
                    UrlInfoBase urlInfoBase;
                    if (SingletonFactory<UrlInfoCollection>.Instance
                        .TryFindSingle(normalizedReferenceType, Is.Item, out urlInfoBase))
                    {
                        var denormalizedReferenceInfo
                            = new DenormalizedReferenceInfo(urlInfoBase,
                                typeof(TEntity),
                                propertyInfo);
                        SingletonFactory<UrlInfoCollection>.Instance
                            .Add(denormalizedReferenceInfo);
                        result.Add(denormalizedReferenceInfo);
                    }
                }
            }
            return result;
        }
        

        static IEnumerable<LinkRepresentation<TEntity>> GetReferenceLinkRepresentations(TEntity entity,
            CollectionJsonSerializerSettings settings)
        {
            //must:LinkRepresentation<PropertyType>(propertyInfo.Value(entity)) //<-if not null
                    //var x = new LinkRepresentation<>()

            return SingletonFactory<ReferenceUrlInfoCollection>.Instance
                .Find(typeof (TEntity))
                .Where(tuple => tuple.Item2.GetValue(entity) != null) //ignore this resharper warning, we do mnot change start or end of the sequenxe while it's executed
                .Select(tuple => new LinkRepresentation<TEntity>(entity,
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

                    var aa = propertyInfo.PropertyType.IsGenericType;
                    //var bb = (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (DenormalizedReference<>));
                    var c = propertyInfo.PropertyType.Name.Contains("Denormalized");

                    
                    //the old attribute way
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


    internal sealed class DenormalizedReferenceInfo : UrlInfoBase
    {
        DenormalizedReferenceInfo(Type entityType)
            : base(entityType)
        {
        }

        public DenormalizedReferenceInfo(UrlInfoBase entityUrlInfo,
            Type pointerType,
            PropertyInfo pointerProperty)
            : this(entityUrlInfo.EntityType)
        {
            Kind = entityUrlInfo.Kind;
            PrimaryKeyProperty = entityUrlInfo.PrimaryKeyProperty;
            PrimaryKeyTemplate = entityUrlInfo.PrimaryKeyTemplate;
            QueryParams = entityUrlInfo.QueryParams;
            Relation = entityUrlInfo.Render;
            Render = entityUrlInfo.Relation;
            VirtualPath = entityUrlInfo.VirtualPath;

            PointerType = pointerType;
            PointerProperty = pointerProperty;

            var denormalizedReferenceType =
                    typeof(DenormalizedReference<>).MakeGenericType(this.EntityType);
            DenormalizedPrimaryKeyProperty =
                denormalizedReferenceType.GetProperty(this.PrimaryKeyProperty.Name);
        }

        public Type PointerType { get; private set; }
        public PropertyInfo PointerProperty { get; private set; }
        public PropertyInfo DenormalizedPrimaryKeyProperty { get; private set; }
        public new string Relation { get; private set; }
    }
}