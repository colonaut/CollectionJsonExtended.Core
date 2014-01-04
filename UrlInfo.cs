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
    
    public abstract class UrlInfoProvider
    {
        protected static readonly IList<UrlInfoProvider> Cache;

        static UrlInfoProvider()
        {
            Cache = new List<UrlInfoProvider>();
        }


        protected UrlInfoProvider(Type entityType)
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
        
    }

    public sealed class UrlInfo : UrlInfoProvider
    {

        public UrlInfo(Type entityType)
            : base(entityType)
        {
        }

        public static IEnumerable<UrlInfo> Find(Type entityType)
        {
            return Cache.Where(r => r.EntityType == entityType) as IEnumerable<UrlInfo>;
        }
    }
}