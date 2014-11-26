using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionJsonExtended.Core
{
    public enum Is
    {
        Base,
        Item,
        BaseLink,
        ItemLink,
        Template,
        Query,
        Create,
        Delete,
        Update
    }
    
    public interface IUrlInfoCache
    {
        void Add(UrlInfoBase urlInfo);

        IEnumerable<UrlInfoBase> Find(Type entityType);

        IEnumerable<UrlInfoBase> Find(Type entityType, Is kind);

        bool TryFindSingle(Type entityType, Is kind, out UrlInfoBase value);
    }
    
    public sealed class UrlInfoCache : IUrlInfoCache
    {
        readonly IList<UrlInfoBase> _collection;
        
        public UrlInfoCache()
        {
            _collection = new List<UrlInfoBase>();
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
            return _collection.Where(c => c.EntityType == entityType).ToList();
        }

        public IEnumerable<UrlInfoBase> Find(Type entityType, Is kind)
        {
            return Find(entityType).Where(c => c.Kind == kind).ToList();
        }

        public IEnumerable<TInfo> Find<TInfo>(Type entityType) where TInfo : UrlInfoBase
        {
            return _collection.OfType<TInfo>().Where(c => c.EntityType == entityType).ToList();
        }

        public IEnumerable<TInfo> Find<TInfo>(Type entityType, Is kind) where TInfo : UrlInfoBase
        {
            return Find<TInfo>(entityType).Where(c => c.Kind == kind).ToList();
        }

        public bool TryFindSingle(Type entityType, Is kind, out UrlInfoBase value)
        {
            value = Find(entityType, kind).SingleOrDefault();
            return value != null;
        }
        
        public bool TryFindSingle<TInfo>(Type entityType, Is kind, out TInfo value) where TInfo : UrlInfoBase
        {
            value = Find<TInfo>(entityType, kind).SingleOrDefault();
            return value != null;
        }


    }
}