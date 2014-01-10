using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace CollectionJsonExtended.Core
{
    public enum Is
    {
        Base,
        Item,
        Template,
        Query,
        Create,
        Delete,
        Update
    }
    
    public class UrlInfoBase
    {
        protected UrlInfoBase(Type entityType)
        {
            EntityType = entityType;
        }


        //TODO we need host / areas / and so on here (maybe only host, area could be prefix...??? check in mvc attribute routing)
        public Type EntityType { get; private set; }

        //probably depreceated...
        public ParameterInfo[] Params { get; set; }

        public Is Kind { get; set; }

        public string Relation { get; set; }

        public string Render { get; set; }

        public string VirtualPath { get; set; }
        public string PrimaryKeyTemplate { get; set; }
        public PropertyInfo PrimaryKeyProperty { get; set; }

        bool _isPublished;
        public virtual void Publish()
        {
            if (_isPublished)
                return;
            new SingletonFactory<UrlInfoCollection>().GetInstance().Add(this);
            _isPublished = true;
        }        
    }


    public interface IUrlInfoCollection
    {
        void Add(UrlInfoBase urlInfo);

        IEnumerable<UrlInfoBase> Find(Type entityType);

        IEnumerable<UrlInfoBase> Find(Type entityType, Is kind);

        bool TryFindSingle(Type entityType, Is kind, out UrlInfoBase value);
    }

    public sealed class UrlInfoCollection : IUrlInfoCollection
    {
        readonly IDictionary<Type, IList<UrlInfoBase>> _collectionByEntityType;
        readonly IList<UrlInfoBase> _collection;
        
        public UrlInfoCollection()
        {
            _collection = new List<UrlInfoBase>();
            _collectionByEntityType = new Dictionary<Type, IList<UrlInfoBase>>();
        }


        public void Add(UrlInfoBase urlInfo)
        {
            //if (_collection.Contains(urlInfo)) //prevent multiple records of the same instance
            //    return false;
            _collection.Add(urlInfo);
            //return true;
        }

        public IEnumerable<UrlInfoBase> Find(Type entityType)
        {
            IList<UrlInfoBase> result;
            if (!_collectionByEntityType.TryGetValue(entityType, out result))
            {
                result = _collection.Where(c => c.EntityType == entityType).ToList();
                _collectionByEntityType.Add(entityType, result);
            }
            return _collectionByEntityType[entityType];
        }

        public IEnumerable<UrlInfoBase> Find(Type entityType, Is kind)
        {
            return Find(entityType).Where(c => c.Kind == kind).ToList();
        }

        public IEnumerable<TInfo> Find<TInfo>(Type entityType) where TInfo : UrlInfoBase
        {
            return _collection.OfType<TInfo>().Where(c => c.EntityType == entityType).ToList();
        }

        public bool TryFindSingle(Type entityType, Is kind, out UrlInfoBase value)
        {
            value = Find(entityType, kind).SingleOrDefault();
            if (value != null)
                return true;
            return false;
        }

    }

    

}