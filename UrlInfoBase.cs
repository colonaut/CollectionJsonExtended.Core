using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    
    public interface IUrlInfo
    {
        Type EntityType { get; }

        ParameterInfo[] Params { get; set; }

        Is Kind { get; set; }

        string Relation { get; set; }

        string Render { get; set; }

        string VirtualPath { get; set; }
    }

    public class UrlInfoBase : IUrlInfo
    {
        protected static readonly IDictionary<Type, IList<UrlInfoBase>> EntityUrlInfoBaseCache; //protected for fake

        protected static readonly List<IUrlInfo> Cache; //protected for fake (cannot be private)

        static UrlInfoBase()
        {
            Cache = new List<IUrlInfo>();
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



        public static IEnumerable<UrlInfoBase> Find(Type entityType)
        {
            IList<UrlInfoBase> result;
            if (!EntityUrlInfoBaseCache.TryGetValue(entityType, out result))
            {
                result =
                    Cache.Select(c => (UrlInfoBase)c)
                    .Where(c => c.EntityType == entityType)
                    .ToList();
                EntityUrlInfoBaseCache.Add(entityType, result);
            }
            return EntityUrlInfoBaseCache[entityType];
        }

    }
}