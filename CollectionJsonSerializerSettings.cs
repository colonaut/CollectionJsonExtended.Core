using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public enum DataPropertyCasing
    {
        CamelCase,
        PascalCase,
        None
    }

    public enum ConversionMethod
    {
        Entity,
        Data
    }

    public enum ReadOnlyPropertyHandling //for templates only
    {
        Ignore,
        Include
    }

    public enum PrimaryKeyPropertyHandling //for all
    {
        Ignore,
        Include
    }

    public class CollectionJsonSerializerSettings
    {
        DataPropertyCasing _dataPropertyCasing = DataPropertyCasing.CamelCase;
        ConversionMethod _conversionMethod = ConversionMethod.Data;
        Formatting _formatting = Formatting.None;


        public DataPropertyCasing DataPropertyCasing
        {
            get { return _dataPropertyCasing; } 
            set { _dataPropertyCasing = value; }
        }
        
        public ConversionMethod ConversionMethod
        {
            get { return _conversionMethod; }
            set { _conversionMethod = value; }
        }

        public Formatting Formatting
        {
            get { return _formatting; }
            set { _formatting = value; }
        }

        //TODO: take care of Newtonsoft settings which reflect these settings here. do not do sth for string enum converter as this is done in our converter, when the json attribute is set in a model
    }
}