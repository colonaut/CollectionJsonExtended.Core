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
            UrlInfoBase urlInfo;
            if (!Singleton<UrlInfoCollection>.Instance
                .TryFindSingle(typeof (TEntity), Is.Item, out urlInfo))
                return null;
            var virtualPath = urlInfo.VirtualPath;
            foreach (var paramInfo in urlInfo.Params)
                virtualPath = virtualPath.Replace("{" + paramInfo.Name + "}", "hamsterbackeInWriterExtensions");
            return virtualPath;

        }

        public static string GetVirtualPath<TEntity>(this CollectionRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            UrlInfoBase urlInfo;
            if (!Singleton<UrlInfoCollection>.Instance
                .TryFindSingle(typeof(TEntity), Is.Base, out urlInfo))
                return null;
            return urlInfo.VirtualPath;

        }

    }

}
