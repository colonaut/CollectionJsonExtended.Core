using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CollectionJsonExtended.Core.Extensions
{
    public static class CollectionJsonWriterExtensions
    {
        public static string GetVirtualPath<TEntity>(this ItemRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            
            var urlInfo = UrlInfoBase.Find(typeof (TEntity)).SingleOrDefault(ui => ui.Kind == Is.Item);
            if (urlInfo != null)
            {
                var virtualPath = urlInfo.VirtualPath;
                foreach (var paramInfo in urlInfo.Params)
                    virtualPath = virtualPath.Replace("{" + paramInfo.Name + "}", "hamsterbackeInWriterExtensions");
                return virtualPath;
            }
            return null;
        }

        public static string GetVirtualPath<TEntity>(this CollectionRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            var urlInfo = UrlInfoBase.Find(typeof (TEntity)).SingleOrDefault(ui => ui.Kind == Is.Base);
            if (urlInfo != null)
                return urlInfo.VirtualPath;
            return null;
        }

    }

}
