using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CollectionJsonExtended.Core.Attributes
{
    public enum TemplateValueHandling
    {
        Include,
        Ignore
    }

    public enum ItemValueHandling
    {
        Include,
        Ignore
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CollectionJsonPropertyAttribute : Attribute
    {
        public bool IsPrimaryKey = false; //TODO own attribute? (no)

        public TemplateValueHandling TemplateValueHandling { get; set; }

        public ItemValueHandling ItemValueHandling { get; set; }

        public string Prompt { get; set; } //TODO own attribute? (maybe)
    }
}