using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Attributes;

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
            
            //TODO IDENTIFIER IN CLIENT
            //Check if a property has the CollectionJsonPropertyAttribute with identifier true.
            //else look for public (get) of an Id property.
            //CACHE THE RESULTS!!!!!!
            //DO all the build up in client and only passthe Identifier credentials here

            var virtualPath = urlInfo.VirtualPath + "TODOOOOOOOO";
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
