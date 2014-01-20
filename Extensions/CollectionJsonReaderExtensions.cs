using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CollectionJsonExtended.Core.Extensions
{
    internal static class CollectionJsonReaderExtensions
    {
        //TODO: dictionaries (before IEnumerable, as they implement it)

        private static readonly MethodInfo EnumarableCastMethod =
            typeof (Enumerable).GetMethod("Cast",
                BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo EnumerableToArrayMethod =
            typeof (Enumerable).GetMethod("ToArray",
                BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo EnumerableToListMethod =
            typeof (Enumerable).GetMethod("ToList",
                BindingFlags.Public | BindingFlags.Static);
        

        public static TEntity MapFromData<TEntity>(this ReaderTemplateRepresentation<TEntity> readerTemplateRepresentation)
            where TEntity : class, new()
        {
            if (readerTemplateRepresentation.Data.Any())
                return MapFromDataObjects(new TEntity(), readerTemplateRepresentation.Data) as TEntity; //if data list is empty, entity should be null (no content was set)
            return null;
        }

        static object MapFromDataObjects(object obj,
            IList<DataRepresentation> dataObjects)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;
                var dataObject = dataObjects.GetDataObject(propertyName);
                if (dataObject == null)
                    continue; //the property could not be found. we would do nothing, but still receive a valid Entity.

                #region Map StringValue
                if (propertyType == typeof(string))
                {
                    if (propertyInfo.TrySetValue(obj, dataObject.Value as string))
                        continue;
                    throw new InvalidDataException(string.Format("DataObject.Value as string could not be set to property {0}, {1}", propertyType.Name, propertyName));
                }
                #endregion

                #region Map Enum
                if (propertyType.IsEnum)
                {
                    if (propertyInfo.TrySetValue(obj, Enum.Parse(propertyType, dataObject.Value.ToString())))
                        continue;
                    propertyInfo.SetValue(obj, Enum.Parse(propertyType, "0")); //Apply the default for the enum, if given value is not part of the enum
                    continue;
                }
                #endregion

                #region Map Array
                if (propertyType.IsArray)
                {
                    var elementType = propertyType.GetElementType();
                    if (!elementType.IsClass || elementType == typeof (string))
                    {
                        if (propertyInfo.TrySetValue(obj, dataObject.ValuesAsArray(elementType)))
                            continue;
                        throw new InvalidDataException(string.Format("TODO Exception for {0}, {1}", propertyType.Name, propertyName));
                    }
                    
                    if (propertyInfo.TrySetValue(obj, dataObject.ObjectsAsArray(elementType)))
                        continue;
                    throw new InvalidDataException(string.Format("TODO Exception for {0}, {1}", propertyType.Name, propertyName));
                }
                #endregion

                #region Map IEnumerable
                if (propertyType.GetInterfaces().Contains(typeof (IEnumerable))) //Dictionaries also use IEnumerable. They have to be treated different above or within. Or maybe works....
                {
                    var genericType = propertyType.GetGenericArguments()[0];
                    if (genericType == null)
                        throw new NullReferenceException(string.Format(
                            "Could not determine genericType for {0}, {1}",
                            propertyType.Name, propertyName));

                    var list = genericType.IsAbstract
                        ? dataObject.AbstractsAsList(genericType, genericType.GetInstanceTypes().ToList())
                                : !genericType.IsClass || genericType == typeof (string)
                                   ? dataObject.ValuesAsList(genericType)
                                   : dataObject.ObjectsAsList(genericType);

                    if (propertyInfo.TrySetValue(obj, list))
                        continue;

                    var listAcceptingConstructor = propertyType.GetConstructor(new[] {list.GetType()});
                    if (listAcceptingConstructor != null
                        && propertyInfo.TrySetValue(obj, listAcceptingConstructor.Invoke(new[] {list})))
                        continue;
                    
                    throw new InvalidDataException(string.Format("TODO Exception for {0}, {1}", propertyType.Name, propertyName));
                }
                #endregion

                #region Map Abstract
                if (propertyType.IsAbstract)
                {
                    if (dataObject.Object == null) //object is null, let it be null or default!
                        continue;

                    Type instanceType;
                    if (!propertyType.TryGetInstanceType(dataObject.Object.Type, out instanceType))
                        throw new TypeLoadException(string.Format(
                            "Type {0} is not a valid instance type for abstract type {1}.",
                            dataObject.Object.Type,
                            propertyType.Name));

                    var abstractData = dataObject.Object.Data as IList<DataRepresentation>;
                    if (abstractData == null)
                        throw new NullReferenceException(string.Format(
                            "TODO Exception for {0}, {1}",
                            propertyType.Name,
                            propertyName));

                    if (propertyInfo.TrySetValue(obj,
                        MapFromDataObjects(Activator.CreateInstance(instanceType), abstractData)))
                        continue;

                    throw new InvalidDataException(string.Format(
                        "TODO Exception for {0}, {1}",
                        propertyType.Name,
                        propertyName));
                }
                #endregion

                if (propertyType.IsInterface)
                    throw new Exception("Implement attribute usage JsonInterfaceProperty or reflect dowm via assemply info (expensive!!!)");

                #region Map Class
                if (propertyType.IsClass)
                {
                    if (dataObject.Object == null) //object is null, let it be null or default!
                        continue;
                    var objectData = dataObject.Object.Data as IList<DataRepresentation>;
                    if (objectData == null)
                        throw new NullReferenceException(string.Format("TODO Exception for {0}, {1}", propertyType.Name, propertyName));

                    if (propertyInfo.TrySetValue(obj, MapFromDataObjects(Activator.CreateInstance(propertyType), objectData)))
                        continue;
                    throw new InvalidDataException(string.Format("TODO Exception for {0}, {1}", propertyType.Name, propertyName));
                }
                #endregion

                #region Map ValueType (Default)
                if (!propertyInfo.TrySetValue(obj, dataObject.ValueAsValueType(propertyType))) //valueTypes
                    throw new InvalidDataException(string.Format("Could not set Value for Type {0}, {1} (is not a ValueType?)", propertyType.Name, propertyName));
                #endregion
            }
            
            return obj;
        }



        static DataRepresentation GetDataObject(this IEnumerable<DataRepresentation> dataObjects,
            string propertyName)
        {
            return dataObjects.SingleOrDefault(o => string.Compare(o.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);
        }
        
        static DataRepresentation GetDataObject(this IEnumerable<DataRepresentation> dataObjects,
            PropertyInfo propertyInfo)
        {
            return dataObjects.SingleOrDefault(o => string.Compare(o.Name, propertyInfo.Name, StringComparison.OrdinalIgnoreCase) == 0);
        }


        
        static object ObjectsAsArray(this DataRepresentation dataRepresentation,
            Type type)
        {
            var dataObjectArray = Array.CreateInstance(type, dataRepresentation.Objects.Count);
            for (var i = 0; i < dataRepresentation.Objects.Count; i++)
            {
                var objectInstance = Activator.CreateInstance(type);
                dataObjectArray.SetValue(MapFromDataObjects(objectInstance, dataRepresentation.Objects[i].Data as IList<DataRepresentation>), i);
            }
            return dataObjectArray;
        }
        
        static object ObjectsAsList(this DataRepresentation dataRepresentation,
            Type type)
        {
            var dataObjectsList = new List<object>();
            foreach (var dataObject in dataRepresentation.Objects)
            {
                try
                {
                    if (type.IsInterface)
                        throw new Exception("Implement attribute usage JsonInterfaceProperty or reflect dowm via assemply info (expensive!!!)");
                    
                    var objectInstance = Activator.CreateInstance(type);
                    var resolvedObjectInstance = MapFromDataObjects(objectInstance, dataObject.Data as IList<DataRepresentation>);
                    dataObjectsList.Add(resolvedObjectInstance);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Could not create instance for {0}, Inner exception: {1}", type, e.Message));
                }
               
            }
            var castedGenericList = EnumarableCastMethod.MakeGenericMethod(type).Invoke(null, new object[] { dataObjectsList });
            return EnumerableToListMethod.MakeGenericMethod(type).Invoke(null, new object[] { castedGenericList });
        }

        static object AbstractsAsList(this DataRepresentation dataRepresentation,
            Type abstractType,
            IList<Type> instanceTypes)
        {
            var dataObjectsList = new List<object>();
            foreach (var dataObject in dataRepresentation.Objects)
            {
                if (dataObject.Type == null)
                    throw new InvalidDataException(string.Format(
                        "No concrete type specified for abstract type {0}", abstractType.Name));
                
                var instanceType = instanceTypes
                    .SingleOrDefault(t => String.Equals(t.Name, dataObject.Type, StringComparison.CurrentCultureIgnoreCase));
                
                if (instanceType == null)
                    throw new InvalidDataException(string.Format(
                        "Could not determine concrete type for abstract ",
                        abstractType.Name));

                var objectInstance = Activator.CreateInstance(instanceType);
                var resolvedObjectInstance = MapFromDataObjects(objectInstance,
                    dataObject.Data as IList<DataRepresentation>);
                dataObjectsList.Add(resolvedObjectInstance);
            }
            var castedGenericList = EnumarableCastMethod.MakeGenericMethod(abstractType).Invoke(null, new object[] { dataObjectsList });
            return EnumerableToListMethod.MakeGenericMethod(abstractType).Invoke(null, new object[] { castedGenericList });
        }

        static object ValueAsValueType(this DataRepresentation dataRepresentation,
            Type type)
        {
            return Convert.ChangeType(dataRepresentation.Value, type, CultureInfo.InvariantCulture);
        }

        static object ValuesAsList(this DataRepresentation dataRepresentation,
            Type itemType)
        {
            if (dataRepresentation.Values == null)
                throw new NullReferenceException("DataObject.Values is null");
            var castedEnumerable =  EnumarableCastMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { dataRepresentation.Values });
            return EnumerableToListMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { castedEnumerable });
        }

        static object ValuesAsArray(this DataRepresentation dataRepresentation,
            Type itemType)
        {
            if (dataRepresentation.Values == null)
                throw new NullReferenceException("DataRepresenation.Values is null");
            var castedEnumerable = EnumarableCastMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { dataRepresentation.Values });
            return EnumerableToArrayMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { castedEnumerable });
        }

    }
}

/*
{"template": {
      "data": [
        {
          "name": "age",
          "value": "",
          "prompt": "Age",
          "type": "Decimal"
        },
        {
          "name": "race",
          "value": "",
          "prompt": "Race",
          "type": "String"
        },
        {
          "name": "height",
          "value": "",
          "prompt": "Height",
          "type": "String"
        },
        {
          "name": "weight",
          "value": "",
          "prompt": "Weight",
          "type": "String"
        },
        {
          "name": "stats",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "score",
              "value": "",
              "prompt": "Score",
              "type": "Decimal"
            },
            {
              "name": "points",
              "value": "",
              "prompt": "Points",
              "type": "Int32"
            }
          ],
          "prompt": "Stats",
          "type": "IEnumerable`1[Stat]"
        },
        {
          "name": "cultures",
          "objects": [],
          "data": [],
          "prompt": "Cultures",
          "type": "IEnumerable`1[Culture]"
        },
        {
          "name": "languages",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "points",
              "value": "",
              "prompt": "Points",
              "type": "Int32"
            }
          ],
          "prompt": "Languages",
          "type": "IEnumerable`1[Language]"
        },
        {
          "name": "advantages",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "points",
              "value": "",
              "prompt": "Points",
              "type": "Int32"
            },
            {
              "name": "page",
              "value": "",
              "prompt": "Page",
              "type": "String"
            }
          ],
          "prompt": "Advantages",
          "type": "IEnumerable`1[Advantage]"
        },
        {
          "name": "disadvantages",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "points",
              "value": "",
              "prompt": "Points",
              "type": "Int32"
            },
            {
              "name": "page",
              "value": "",
              "prompt": "Page",
              "type": "String"
            }
          ],
          "prompt": "Disadvantages",
          "type": "IEnumerable`1[Advantage]"
        },
        {
          "name": "techniques",
          "objects": [],
          "data": [],
          "prompt": "Techniques",
          "type": "IEnumerable`1[Technique]"
        },
        {
          "name": "skills",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "level",
              "value": "",
              "prompt": "Level",
              "type": "Int32"
            },
            {
              "name": "points",
              "value": "",
              "prompt": "Points",
              "type": "Int32"
            },
            {
              "name": "step",
              "value": "",
              "prompt": "Step",
              "type": "String"
            },
            {
              "name": "page",
              "value": "",
              "prompt": "Page",
              "type": "String"
            }
          ],
          "prompt": "Skills",
          "type": "IEnumerable`1[Skill]"
        },
        {
          "name": "melee",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "page",
              "value": "",
              "prompt": "Page",
              "type": "String"
            },
            {
              "name": "weight",
              "value": "",
              "prompt": "Weight",
              "type": "Decimal"
            },
            {
              "name": "modes",
              "objects": [],
              "data": [
                {
                  "name": "type",
                  "value": "",
                  "prompt": "Type",
                  "type": "String"
                },
                {
                  "name": "damage",
                  "value": "",
                  "prompt": "Damage",
                  "type": "String"
                },
                {
                  "name": "level",
                  "value": "",
                  "prompt": "Level",
                  "type": "Int32"
                },
                {
                  "name": "parry",
                  "value": "",
                  "prompt": "Parry",
                  "type": "Int32"
                },
                {
                  "name": "reach",
                  "value": "",
                  "prompt": "Reach",
                  "type": "String"
                },
                {
                  "name": "minStr",
                  "value": "",
                  "prompt": "Min Str",
                  "type": "Int32"
                },
                {
                  "name": "notes",
                  "value": "",
                  "prompt": "Notes",
                  "type": "String"
                }
              ],
              "prompt": "Modes",
              "type": "IEnumerable`1[Mode]"
            }
          ],
          "prompt": "Melee",
          "type": "IEnumerable`1[Melee]"
        },
        {
          "name": "ranged",
          "objects": [],
          "data": [
            {
              "name": "name",
              "value": "",
              "prompt": "Name",
              "type": "String"
            },
            {
              "name": "page",
              "value": "",
              "prompt": "Page",
              "type": "String"
            },
            {
              "name": "weight",
              "value": "",
              "prompt": "Weight",
              "type": "Decimal"
            },
            {
              "name": "modes",
              "objects": [],
              "data": [
                {
                  "name": "type",
                  "value": "",
                  "prompt": "Type",
                  "type": "String"
                },
                {
                  "name": "damage",
                  "value": "",
                  "prompt": "Damage",
                  "type": "String"
                },
                {
                  "name": "level",
                  "value": "",
                  "prompt": "Level",
                  "type": "Int32"
                },
                {
                  "name": "parry",
                  "value": "",
                  "prompt": "Parry",
                  "type": "Int32"
                },
                {
                  "name": "reach",
                  "value": "",
                  "prompt": "Reach",
                  "type": "String"
                },
                {
                  "name": "minStr",
                  "value": "",
                  "prompt": "Min Str",
                  "type": "Int32"
                },
                {
                  "name": "notes",
                  "value": "",
                  "prompt": "Notes",
                  "type": "String"
                }
              ],
              "prompt": "Modes",
              "type": "IEnumerable`1[Mode]"
            }
          ],
          "prompt": "Ranged",
          "type": "IEnumerable`1[Ranged]"
        },
        {
          "name": "id",
          "value": "",
          "prompt": "Id",
          "type": "Int32"
        },
        {
          "name": "name",
          "value": "",
          "prompt": "Name",
          "type": "String"
        },
        {
          "name": "settingId",
          "value": "",
          "prompt": "Setting Id",
          "type": "Int32"
        },
        {
          "name": "playerId",
          "value": "",
          "prompt": "Player Id",
          "type": "Int32"
        }
      ]
    }
}
 */