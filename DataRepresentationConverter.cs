using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Attributes;
using CollectionJsonExtended.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CollectionJsonExtended.Core
{
    public class DataRepresentationConverter : JsonConverter
    {
        static readonly object[] EmptyObjects = new object[0];

        static PropertyInfo GetPrimaryKeyProperty(Type entityType) //TODO an Type binden....
        {
            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance
                .TryFindSingle(entityType, Is.Item, out urlInfo))
                return null;
            return urlInfo.PrimaryKeyProperty;            
        }

        static bool IsReference(PropertyInfo propertyInfo) //TODO an PropertyType binden...
        {
            var attribute =
                propertyInfo.GetCustomAttribute<CollectionJsonReferenceAttribute>();

            if (attribute == null)
                return false;

            var primaryKeyProperty = GetPrimaryKeyProperty(attribute.ReferenceType);
            if (primaryKeyProperty == null)
                return false;

            return primaryKeyProperty.PropertyType.Name
                == propertyInfo.PropertyType.Name;
        }


        //TODO evaluate UIHint attribute (template), as fpr example a date or something can be treated different. ensure type is them mvchtml string or something. only valid for items, not for templates!!!

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var entityType = value as Type;
            if (entityType != null)
                WriteDataRepresentation(writer, entityType, serializer);
            
            else
                WriteDataRepresentation(writer, value, serializer);

        }

        private void WriteDataRepresentation(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                //TODO: if we have an external entity here, we must provide it's query..., query with id1,id2, etc.. for collection of externals... /this apples to relations. (how?), for embedded the embedded item must provide it's link....? 
                //for i.e. items this only happens if we convert to data! seriralizing to entity will not trigger this...
                if (IsReference(propertyInfo))
                {
                    var x = "YES";
                }


                var propertyType = propertyInfo.PropertyType;
                
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                serializer.Serialize(writer, propertyInfo.Name.CamelCase());

                if (propertyType == typeof(string))
                    WriteRepresentationValue(writer, propertyInfo.GetValue(obj), serializer);

                else if (propertyType.IsEnum)
                    WriteRepresentationValue(writer, propertyInfo.IsJsonStringEnumConverter()
                                                 ? Enum.GetName(propertyType, propertyInfo.GetValue(obj))
                                                 : propertyInfo.GetValue(obj), serializer);
                
                else if (propertyType.IsArray || propertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var itemType = propertyType.GetElementType() ?? propertyType.GetGenericArguments()[0];
                    if (itemType == null)
                        throw new NullReferenceException("Type of items could not be determined");
                    if (itemType.IsClass && itemType != typeof(string))
                        WriteRepresentationObjects(writer, propertyInfo.GetValue(obj) as IEnumerable<object>, serializer);
                    else
                        WriteRepresentationValues(writer, propertyInfo.GetValue(obj) as IEnumerable<object>, serializer);
                }
                
                else if (propertyType.IsClass)
                    WriteRepresentationObject(writer, propertyInfo.GetValue(obj), serializer);

                else if (propertyType.IsValueType)
                    WriteRepresentationValue(writer, propertyInfo.GetValue(obj) ?? propertyInfo.GetDefaultValue(), serializer);
                    
                else
                    throw new NotImplementedException("Type is not suppoerted yet");

                writer.WritePropertyName("prompt");
                serializer.Serialize(writer, propertyInfo.GetCollectionJsonPrompt());

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        private void WriteDataRepresentation(JsonWriter writer, Type type, JsonSerializer serializer)
        {
            //TODO cache this result somewhere, so we do not have to male the reflection for every call!
            //json.net seems to provide good caching... check that!

            var primaryKeyPropertyInfo = GetPrimaryKeyProperty(type);

            writer.WriteStartArray();

            foreach (var propertyInfo in type.GetProperties())
            {
                //TODO we must find private setters and primary key properties here... and we must NOT serialize them
                //TODO we must find a way to provide a query link or s.th in the template that offers a choice of entities for external entities! (we must extend the cj spec for that)

                //Here we evaluate if wwe have a reference attribute anf try to find
                //an entry with the primary key matching they type to the property.
                //if so we must create a link...

                if (IsReference(propertyInfo))
                {
                    var x = "YES";
                }


                if (propertyInfo == primaryKeyPropertyInfo)
                    continue;
                
                if (!propertyInfo.CanWrite)
                    continue;

                if (propertyInfo.SetMethod == null
                    || propertyInfo.SetMethod.IsPrivate)
                    continue;

                var propertyType = propertyInfo.PropertyType;
                var resolvedTypeName = propertyType.GetSimpleTypeName();// propertyType.Name; //TODO use simpleTypeNames
                
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                serializer.Serialize(writer, propertyInfo.Name.CamelCase());

                if (propertyType == typeof (string))
                    WriteRepresentationValue(writer, string.Empty, serializer);

                #region Write Enums
                else if (propertyType.IsEnum)
                {
                    if (propertyInfo.IsJsonStringEnumConverter())
                        WriteRepresentationValue(writer,
                            Enum.GetName(propertyType, 0),
                            Enum.GetNames(propertyType),
                            serializer);
                    else
                        WriteRepresentationValue(writer,
                            Enum.GetValues(propertyType).GetValue(0),
                            Enum.GetValues(propertyType),
                            serializer);
                }
                #endregion

                #region Write Arrays
                else if (propertyType.IsArray)
                {
                    var elementType = propertyType.GetElementType();

                    if (elementType.IsAbstract)
                        throw new NotImplementedException("Specs and validate handling for Abstract[]");
                        //WriteRepresentationAbstracts(writer, elementType, jsonAbstractPropertyAttributes.Select(a => a.Type), serializer);
                    
                    else if (elementType.IsInterface)
                        throw new NotImplementedException("Handling for Interface[]");
                    
                    else if (elementType.IsClass && elementType != typeof (string))
                        WriteRepresentationObjects(writer,
                            elementType,
                            serializer);
                    else
                        WriteRepresentationValues(writer, EmptyObjects, serializer);
                }
                #endregion

                #region Write IEnumerables
                else if (propertyType.GetInterfaces().Contains(typeof(IEnumerable))) //TODO Take care of IDictionary which also implement IEnumerable
                {
                    var genericType = propertyType.GetGenericArguments()[0];
                    if (genericType != null)
                    {
                        resolvedTypeName = string.Format("{0}[]", genericType.GetSimpleTypeName());
                        if (genericType.IsAbstract)
                            WriteRepresentationAbstracts(writer,
                                genericType,
                                genericType.GetInstanceTypes(),
                                serializer);

                        else if (genericType.IsInterface)
                            throw new Exception("Implement Handling for IEnumerable<Interface>");

                        else if (genericType.IsClass && genericType != typeof (string))
                            WriteRepresentationObjects(writer,
                                genericType,
                                serializer);
                        else
                            WriteRepresentationValues(writer,
                                EmptyObjects,
                                serializer);
                    }
                }
                #endregion

                else if (propertyType.IsAbstract)
                    WriteRepresentationAbstract(writer,
                        propertyType,
                        propertyType.GetInstanceTypes(),
                        serializer);
                
                else if (propertyType.IsClass)
                    WriteRepresentationObject(writer,
                        propertyType,
                        serializer);

                else if (propertyType.IsValueType)
                    WriteRepresentationValue(writer,
                        propertyInfo.GetDefaultValue(),
                        serializer);

                else
                    throw new NotImplementedException(string.Format(
                        "Handling for Type {0} is not yet supported",
                        propertyType.Name));

                writer.WritePropertyName("prompt");
                serializer.Serialize(writer,
                    propertyInfo.GetCollectionJsonPrompt());

                writer.WritePropertyName("type");
                serializer.Serialize(writer,
                    resolvedTypeName);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }


        private void WriteRepresentationValue(JsonWriter writer, object resolvedValue, IEnumerable resolvedOptions,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("value");
            serializer.Serialize(writer, resolvedValue);
            writer.WritePropertyName("options");
            serializer.Serialize(writer, resolvedOptions);
        }

        private void WriteRepresentationValue(JsonWriter writer, object resolvedValue,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("value");
            serializer.Serialize(writer, resolvedValue);
        }

        private void WriteRepresentationValues(JsonWriter writer, IEnumerable<object> resolvedValues,
                               JsonSerializer serializer)
        {
            writer.WritePropertyName("values");
            serializer.Serialize(writer, resolvedValues);
        }

        private void WriteRepresentationObject(JsonWriter writer, Type type,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("object");
            //serializer.Serialize(writer, EmptyObject);
            writer.WriteNull();
            writer.WritePropertyName("data");
            WriteDataRepresentation(writer, type, serializer);
        }

        private void WriteRepresentationAbstract(JsonWriter writer, Type abstractType, IEnumerable<Type> subTypes,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("object");
            //serializer.Serialize(writer, EmptyObject);
            writer.WriteNull();
            writer.WritePropertyName("select");
            writer.WriteStartArray();
            foreach (var subType in subTypes)
            {
                if (!subType.IsSubclassOf(abstractType))
                    continue;
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                serializer.Serialize(writer, subType.Name);
                writer.WritePropertyName("data");
                WriteDataRepresentation(writer, subType, serializer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private void WriteRepresentationInterface(JsonWriter writer, Type interfaceType, IEnumerable<Type> concreteTypes,
                               JsonSerializer serializer)
        {
            writer.WritePropertyName("object");
            //serializer.Serialize(writer, EmptyObject);
            writer.WriteNull();
            writer.WritePropertyName("select");
            writer.WriteStartArray();
            foreach (var concreteType in concreteTypes)
            {
                if (!concreteType.IsAssignableFrom(interfaceType))
                    continue;
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                serializer.Serialize(writer, concreteType.Name);
                writer.WritePropertyName("data");
                WriteDataRepresentation(writer, concreteType, serializer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private void WriteRepresentationObjects(JsonWriter writer, Type childType,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("objects");
            serializer.Serialize(writer, EmptyObjects);
            writer.WritePropertyName("data");
            WriteDataRepresentation(writer, childType, serializer);
        }

        private void WriteRepresentationAbstracts(JsonWriter writer, Type abstractType, IEnumerable<Type> subTypes,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("objects");
            serializer.Serialize(writer, EmptyObjects);
            writer.WritePropertyName("select");
            writer.WriteStartArray();
            foreach (var subType in subTypes)
            {
                if (!subType.IsSubclassOf(abstractType))
                    continue;
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                serializer.Serialize(writer, subType.Name);
                writer.WritePropertyName("data");
                WriteDataRepresentation(writer, subType, serializer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private void WriteRepresentationObject(JsonWriter writer, object resolvedObject,
                                JsonSerializer serializer)
        {
            writer.WritePropertyName("object");
            if (resolvedObject == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartObject();
            writer.WritePropertyName("data");
            WriteDataRepresentation(writer, resolvedObject, serializer);
            writer.WriteEndObject();
        }

        private void WriteRepresentationObjects(JsonWriter writer, IEnumerable<object> resolvedObjects,
                                  JsonSerializer serializer)
        {
            writer.WritePropertyName("objects");
            if (resolvedObjects == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            foreach (var resolvedObject in resolvedObjects)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("data");
                WriteDataRepresentation(writer, resolvedObject, serializer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

        }

       

        //DEPRECEATED (read json throws not implemented)

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            throw new NotImplementedException("This should be done with readTemplate reflection");
            
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("data must be of type [] (JArray)");
            
            var jArray = JArray.Load(reader);
            var entity = Activator.CreateInstance(objectType); //building the entity becaus of data array

            ReadJsonData(jArray, entity, serializer);
            
            return entity;
        }

        private void ReadJsonData(IEnumerable<JToken> jArray, object obj, JsonSerializer serializer)
        {
            foreach (var jToken in jArray)
            {
                var jObject = jToken as JObject;
                if (jObject == null)
                    throw new NullReferenceException("JToken could not be converted to JObject. Please check validity of given data");

                var objPropertyName = (string)jObject.Property("name").Value;
                var objPropertyInfo = obj.GetType().GetProperty(objPropertyName.PascalCase());
                if (objPropertyInfo == null) // we might not throw, but just ignore the fact we did not find the property and continue?
                    throw new NullReferenceException(string.Format("Could not convert json property name \"{0}\" for {1}, property {2} does not exist", objPropertyName, obj.GetType().Name, objPropertyName.PascalCase()));
                    //continue;
                
                var objPropertyType = objPropertyInfo.PropertyType;
                var jDataValue = jObject.Property("value");

                if (objPropertyType == typeof(string))
                    objPropertyInfo.SetValue(obj, serializer.Deserialize(jDataValue.Value.CreateReader(), objPropertyType));
                
                else if (objPropertyType.IsEnum)
                {
                    if (!objPropertyInfo.TrySetValue(obj, serializer.Deserialize(jDataValue.Value.CreateReader(), objPropertyType)))
                        //throw new InvalidDataException(string.Format("Enum for EnumType {0} could not be deserialized", ePropType.Name));
                        objPropertyInfo.SetValue(obj, Enum.Parse(objPropertyType, "0")); //Apply the default for the enum, if given value is not part of the enum
                }
                
                else if (objPropertyType.IsArray || objPropertyType.GetInterfaces().Contains(typeof (IEnumerable)))
                    ReadJsonDataArray(jObject, objPropertyInfo, obj, serializer);
                
                else if (objPropertyType.IsClass)
                    ReadJsonDataObjcet(jObject, objPropertyInfo, obj, serializer);
                
                else //treat as value types
                    if (!objPropertyInfo.TrySetValue(obj, serializer.Deserialize(jDataValue.Value.CreateReader(), objPropertyType)))
                        throw new NotImplementedException(string.Format("Type {0} is not supported", objPropertyType.Name));
            }
        }

        private void ReadJsonDataArray(JObject jObject, PropertyInfo objPropertyInfo, object obj, JsonSerializer serializer)
        {
            var objPropertyType = objPropertyInfo.PropertyType;
            var itemType = objPropertyType.IsArray
                               ? objPropertyType.GetElementType()
                               : objPropertyType.GetGenericArguments()[0];
            if (itemType == null)
                throw new NullReferenceException("could not determine itemType TODO better information in exception");

            if (!itemType.IsClass || itemType == typeof(string))
            {
                var jDataArray = jObject.Property("array").Value;
                if (!objPropertyInfo.TrySetValue(obj, serializer.Deserialize(jDataArray.CreateReader(), objPropertyType)))
                    throw new InvalidDataException(string.Format("Array or Enumerable for Type {0} could not be deserialized", objPropertyType.Name));
                return;
            }

            var jDataObjects = jObject.Property("array").Value.AsJEnumerable();
            var arrayInstance = Array.CreateInstance(itemType, jDataObjects.Count());

            for (var i = 0; i < jDataObjects.Count(); i++)
            {
                var jDataObject = jDataObjects[i]as JObject;
                if (jDataObject == null)
                    throw new NullReferenceException("Attempt to read json objects from array json array attribute: object is null");
                var jData = jDataObject.Property("data").Value as JArray;
                if (jData == null)
                    throw new NullReferenceException("Attempt to read a list of json data from json data attribute: item is not a data array");
                
                var arrayitem = Activator.CreateInstance(itemType);
                arrayInstance.SetValue(arrayitem, i);

                ReadJsonData(jData, arrayitem, serializer);
            }
            
            if (objPropertyType.IsGenericType)
            {
                var genericCollectionConstructor = objPropertyType.GetConstructor(new[] {objPropertyType});
                if (genericCollectionConstructor == null) //this will happen if the collection type is an interface. we would invoke a list then
                    genericCollectionConstructor = typeof(List<>).MakeGenericType(itemType).GetConstructor(new[] {objPropertyType});
                    if (genericCollectionConstructor == null)
                        throw new NullReferenceException(string.Format("Could not get constructor for Type {0}", objPropertyType));

                var collectionInstance = genericCollectionConstructor.Invoke(new object[] {arrayInstance});
                if (!objPropertyInfo.TrySetValue(obj, collectionInstance))
                    throw new InvalidDataException(
                        string.Format("Could not set created collection instance as value for {0} which should be of Type {1}", objPropertyInfo.Name, objPropertyType.Name));
            }

            else if (!objPropertyInfo.TrySetValue(obj, arrayInstance))
                throw new InvalidDataException(string.Format("Could not set created array instance as value for {0} which should be of Type {1}", objPropertyInfo.Name, objPropertyType.Name));
        }
        
        private void ReadJsonDataObjcet(JObject jObject, PropertyInfo objPropertyInfo, object obj, JsonSerializer serializer)
        {
            //we need constructed generic type also here ???
            
            var objPropertyType = objPropertyInfo.PropertyType;
            var childObj = Activator.CreateInstance(objPropertyType);
            var jChildObject = jObject.Property("object").Value as JObject;
            if (jChildObject == null)
                throw new NullReferenceException("Type is Class but json property obj could not be found");

            var jChildObjDataArray = jChildObject.Property("data").Value as JArray;
            if (jChildObjDataArray == null)
                throw new NullReferenceException("array of data property of jObject could not be converted to JArray. Please check validity of given data");

            if (objPropertyInfo.TrySetValue(obj, childObj))
                ReadJsonData(jChildObjDataArray, childObj, serializer);
            else
                throw new OperationCanceledException(string.Format("Could not set child object of Type {0} to calculated property {1}", objPropertyType.Name, objPropertyType.Name));
        }


        public override bool CanConvert(Type objectType)
        {
            //TODO implement usage of can convert in DataRepresentationConverter
            return typeof(CollectionJsonWriter<>).IsAssignableFrom(objectType);
        }

    }
}