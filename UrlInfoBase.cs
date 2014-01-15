using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;

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
            SingletonFactory<UrlInfoCollection>.Instance
                .Add(this);
            _isPublished = true;
        }        
    }

}