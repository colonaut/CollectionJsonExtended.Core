using System;
using System.Collections.Generic;
using System.Reflection;

namespace CollectionJsonExtended.Core
{
    public class UrlInfoBase
    {
        protected UrlInfoBase(Type entityType)
        {
            EntityType = entityType;
        }


        //TODO we need host / areas / and so on here (maybe only host, area could be prefix...??? check in mvc attribute routing)
        public Type EntityType { get; private set; }

        public Is Kind { get; set; }

        public string Relation { get; set; }

        public string Render { get; set; }

        public string VirtualPath { get; set; }
        public string PrimaryKeyTemplate { get; set; }
        public PropertyInfo PrimaryKeyProperty { get; set; }

        public ParameterInfo[] QueryParams { get; set; } 

        bool _isPublished;
        public virtual void Publish()
        {
            if (_isPublished)
                return;
            SingletonFactory<UrlInfoCache>.Instance
                .Add(this);
            _isPublished = true;
        }

        public virtual UrlInfoBase Clone() //depr! remove this in favor of denrefurlinfo
        {
            return new UrlInfoBase(this.EntityType)
                   {
                       Kind = Kind,
                       PrimaryKeyProperty = PrimaryKeyProperty,
                       PrimaryKeyTemplate = PrimaryKeyTemplate,
                       QueryParams = QueryParams,
                       Relation = Render,
                       Render = Relation,
                       VirtualPath = VirtualPath,
                       _isPublished = _isPublished
                   };
        }
    }

}