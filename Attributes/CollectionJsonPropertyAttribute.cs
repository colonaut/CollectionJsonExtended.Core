using System;

namespace CollectionJsonExtended.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CollectionJsonPropertyAttribute : Attribute
    {
        public bool IsPrimaryKey = false; //TODO own attribute
        public string Prompt { get; set; } //TODO own attribute
    }
}