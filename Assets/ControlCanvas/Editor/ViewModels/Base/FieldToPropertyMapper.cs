using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class FieldToPropertyMapper<TData>
    {
        ReactivePropertyManager reactivePropertyManager;

        private Dictionary<string, string> DataFieldToReactivePropertyName;

        public FieldToPropertyMapper(ReactivePropertyManager reactivePropertyManager)
        {
            this.reactivePropertyManager = reactivePropertyManager;
        }

        public void Init(Dictionary<string, string> initializeMappingDictionary)
        {
            DataFieldToReactivePropertyName = initializeMappingDictionary;
        }

        public bool AutoDataFieldToReactivePropertyNameMapping(object data)
        {
            MapDataFieldsToReactiveProperties(DataFieldManager.GetDataFields(data));
            bool error = CheckTypeMatch(DataFieldManager.GetDataFields(data));
            reactivePropertyManager.WarnForUnmappedReactiveProperties(DataFieldToReactivePropertyName);
            return error;
        }

        private void MapDataFieldsToReactiveProperties(Dictionary<string, FieldInfo> DataFields)
        {
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                if (DataFieldToReactivePropertyName.ContainsKey(dataField.Key)) continue;

                string fieldName = dataField.Key;
                string fieldNameLower = char.ToLower(fieldName[0]) + fieldName.Substring(1);
                string fieldNameUpper = char.ToUpper(fieldNameLower[0]) + fieldNameLower.Substring(1);

                if (TryAddMapping(dataField, fieldNameLower)) continue;
                if (TryAddMapping(dataField, fieldNameUpper)) continue;

                var mapping = reactivePropertyManager.CreateAndAddReactiveProperty(dataField, fieldNameUpper);
                if (mapping != null)
                {
                    DataFieldToReactivePropertyName.Add(dataField.Key, mapping);
                }
                else
                {
                    Debug.LogError($"No reactive property created for {dataField.Key}?!?");
                }
            }
        }

        private bool TryAddMapping(KeyValuePair<string, FieldInfo> dataField, string fieldName)
        {
            if (reactivePropertyManager.ContainsKey(fieldName))
            {
                DataFieldToReactivePropertyName.Add(dataField.Key, fieldName);
                return true;
            }

            return false;
        }

        private bool CheckTypeMatch(Dictionary<string, FieldInfo> DataFields)
        {
            bool error = false;
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                string reactivePropertyName = DataFieldToReactivePropertyName[dataField.Key];
                var reactiveProperty = reactivePropertyManager.Get(reactivePropertyName);
                Type innerPropertyType = reactiveProperty.GetType().GetGenericArguments()[0];
                Type dataFieldType = dataField.Value.FieldType;

                if (dataFieldType.IsCollection())
                {
                    error =
                        CheckCollectionTypeMatch(dataField, reactivePropertyName, innerPropertyType, dataFieldType) ||
                        error;
                }
                else
                {
                    error = CheckNonCollectionTypeMatch(dataField, reactivePropertyName, innerPropertyType,
                        dataFieldType) || error;
                }
            }

            return error;
        }

        private bool CheckCollectionTypeMatch(KeyValuePair<string, FieldInfo> dataField, string reactivePropertyName,
            Type innerPropertyType, Type dataFieldType)
        {
            bool error = false;
            Type innerFieldType = dataFieldType.GetInnerType();
            Type innerCollectionType = innerPropertyType.GetGenericArguments()[0];
            if (innerPropertyType.IsReactiveCollection())
            {
                if (innerCollectionType != innerFieldType)
                {
                    LogTypeError(dataField, reactivePropertyName, innerCollectionType, innerFieldType);
                    error = true;
                }
            }
            else
            {
                LogTypeError(dataField, reactivePropertyName, innerPropertyType.GetGenericTypeDefinition(),
                    typeof(ReactiveCollection<>));
                error = true;
            }

            return error;
        }

        private bool CheckNonCollectionTypeMatch(KeyValuePair<string, FieldInfo> dataField, string reactivePropertyName,
            Type innerPropertyType, Type dataFieldType)
        {
            if (innerPropertyType != dataFieldType)
            {
                LogTypeError(dataField, reactivePropertyName, innerPropertyType, dataFieldType);
                return true;
            }

            return false;
        }

        private void LogTypeError(KeyValuePair<string, FieldInfo> dataField, string reactivePropertyName,
            Type actualType, Type expectedType)
        {
            Debug.LogError(
                $"Type mismatch between data field {dataField.Key} and reactive property {reactivePropertyName}: {actualType} != {expectedType}");
        }


        public IDisposable GetReactiveProperty(string fieldName)
        {
            if (DataFieldToReactivePropertyName.TryGetValue(fieldName, out var reactivePropertyName))
            {
                return reactivePropertyManager.GetReactiveProperty(reactivePropertyName);
            }
            else
            {
                Debug.LogError($"Could not find reactive property for {fieldName}");
                return null;
            }
        }
        
        public TType GetReactiveProperty<TType>(string fieldName)
        {
            var rp = GetReactiveProperty(fieldName);
            if(rp is TType rpTyped)
                return rpTyped;
            else
            {
                Debug.LogError($"Could not find reactive property for {fieldName}. Type mismatch: {typeof(TType)} != {rp.GetType()}");
                return default;
            }
            // if (DataFieldToReactivePropertyName.TryGetValue(fieldName, out var reactivePropertyName))
            // {
            //     return (TType)reactivePropertyManager.GetReactiveProperty(reactivePropertyName);
            // }
            // else
            // {
            //     Debug.LogError($"Could not find reactive property for {fieldName}");
            //     return default;
            // }
        }

        public bool TryGetValue(string dataFieldKey, out string o)
        {
            return DataFieldToReactivePropertyName.TryGetValue(dataFieldKey, out o);
        }

        public bool ContainsKey(string dataFieldKey)
        {
            return DataFieldToReactivePropertyName.ContainsKey(dataFieldKey);
        }

        public string GetPropNameByFieldName(string dataFieldKey)
        {
            return DataFieldToReactivePropertyName[dataFieldKey];
        }

        public string GetFieldNameByPropName(string rpField)
        {
            foreach (var kvp in DataFieldToReactivePropertyName)
            {
                if (kvp.Value == rpField)
                {
                    return kvp.Key;
                }
            }

            return null;
        }
    }
}