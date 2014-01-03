using System;

namespace CollectionJsonExtended.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CollectionJsonPropertyAttribute : Attribute
    {
        public string Prompt { get; set; }

    }

}