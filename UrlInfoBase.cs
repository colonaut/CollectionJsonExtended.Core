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
        static readonly IDictionary<Type, IList<UrlInfoBase>> EntityUrlInfoBaseCache;

        protected static readonly IList<UrlInfoBase> Cache; //protected for fake (cannot be private)

        static UrlInfoBase()
        {
            Cache = new List<UrlInfoBase>();
            EntityUrlInfoBaseCache = new Dictionary<Type, IList<UrlInfoBase>>();
        }

        protected UrlInfoBase(Type entityType)
        {
            EntityType = entityType;
        }


        //TODO we need host / areas / and so on here (maybe only host, area could be prefix...??? check in mvc attribute routing)
        public Type EntityType { get; private set; }

        public ParameterInfo[] Params { get; set; }

        public Is Kind { get; set; }

        public string Relation { get; set; }

        public string Render { get; set; }

        public string VirtualPath { get; set; }


        public virtual void Publish()
        {
            Cache.Add(this);            
        }

        public static void Remove(Type entityType)
        {
            EntityUrlInfoBaseCache.Remove(entityType);
            foreach (var urlInfo in Cache)
            {
                if (urlInfo.EntityType == entityType)
                    Cache.Remove(urlInfo);
            }
            
            EntityUrlInfoBaseCache.Clear();
            Cache.Clear();
        }

        
        public static IEnumerable<UrlInfoBase> Find(Type entityType)
        {
            IList<UrlInfoBase> result;
            if (!EntityUrlInfoBaseCache.TryGetValue(entityType, out result))
            {
                result = Cache.Where(c => c.EntityType == entityType).ToList();
                EntityUrlInfoBaseCache.Add(entityType, result);
            }
            return EntityUrlInfoBaseCache[entityType];
        } 

    }


    //we try to create a singleton.... UrlInfo Cache! (we want to mock it, this ts dumb with a static cache...)
    //http://msdn.microsoft.com/en-us/library/ms998558.aspx
    //http://stackoverflow.com/questions/1928264/object-that-is-needed-throughout-the-application
    public sealed class UrlInfoCache
    {
        private static volatile UrlInfoCache _instance;
        private static object _syncRoot = new Object();

        private UrlInfoCache() { }

        public static UrlInfoCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new UrlInfoCache();
                    }
                }

                return _instance;
            }
        }
    }

}