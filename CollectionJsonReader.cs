using System.Collections.Generic;
using CollectionJsonExtended.Core.Extensions;
using Newtonsoft.Json;

namespace CollectionJsonExtended.Core
{
    public class CollectionJsonReader<TEntity> where TEntity : class, new()
    {
        public CollectionJsonReader()
        {
            Template = new ReadTemplateRepresentation<TEntity>();
        }
        
        
        public ReadTemplateRepresentation<TEntity> Template { get; set; } //TODO How to List of templates?

        public TEntity Entity { get { return Template.Entity; } }

        public IEnumerable<DataRepresentation> Data { get { return Template.Data; } } 


        public static CollectionJsonReader<TEntity> Deserialize(string jsonTemplate)
        {
            return JsonConvert.DeserializeObject(jsonTemplate, typeof(CollectionJsonReader<TEntity>)) as CollectionJsonReader<TEntity>
                ?? new CollectionJsonReader<TEntity>();
        }
    }

    
    public class ReadTemplateRepresentation<TEntity> where TEntity : class, new()
    {
        private TEntity _entity;
        private IList<DataRepresentation> _data = new List<DataRepresentation>();


        public TEntity Entity
        {
            get { return _entity ?? this.MapFromData(); }
            set { 
                _entity = value;
                //_data = null; //we could do this to have null for the data representation, if entity is already given. currently we gor for Data.Any()
            }
        }

        public IList<DataRepresentation> Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }
}


/*
{"template": {
      "data": [
        {
          "name": "age",
          "value": 0.0,
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
              "value": 0.0,
              "prompt": "Score",
              "type": "Decimal"
            },
            {
              "name": "points",
              "value": 0,
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
              "value": 0,
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
              "value": 0,
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
              "value": 0,
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
              "value": 0,
              "prompt": "Level",
              "type": "Int32"
            },
            {
              "name": "points",
              "value": 0,
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
              "value": 0.0,
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
                  "value": 0,
                  "prompt": "Level",
                  "type": "Int32"
                },
                {
                  "name": "parry",
                  "value": 0,
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
                  "value": 0,
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
              "value": 0.0,
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
                  "value": 0,
                  "prompt": "Level",
                  "type": "Int32"
                },
                {
                  "name": "parry",
                  "value": 0,
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
                  "value": 0,
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
          "value": 0,
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
          "value": 0,
          "prompt": "Setting Id",
          "type": "Int32"
        },
        {
          "name": "playerId",
          "value": 0,
          "prompt": "Player Id",
          "type": "Int32"
        }
      ]
    }
}
 */