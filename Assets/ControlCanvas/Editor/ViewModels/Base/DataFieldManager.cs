using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public static class DataFieldManager
    {
        private static Dictionary<Type, Dictionary<string, FieldInfo>> DataFieldsCache = new();

        public static void GatherDataFields<TData>()
        {
            Type dataType = typeof(TData);
            if (dataType.IsInterface)
            {
                Debug.LogError($"Cannot gather data fields for interface {dataType}");
            }
            else
            {
                Debug.LogWarning($"Gathering data fields for {dataType} which is no interface, so this could be optimized");
            }
            GatherDataFields(dataType);
        }
        public static void GatherDataFields(object data)
        {
            Type dataType = data.GetType();
            GatherDataFields(dataType);
        }

        public static void GatherDataFields(Type type)
        {
            if (!DataFieldsCache.TryGetValue(type, out var dataFields))
            {
                dataFields = new Dictionary<string, FieldInfo>();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var fieldInfo in fields)
                {
                    dataFields.Add(fieldInfo.Name, fieldInfo);
                }

                DataFieldsCache[type] = dataFields;
                LogFindings(type, dataFields);
            }
        }
        
        public static Dictionary<string, FieldInfo> GetDataFields(object data)
        {
            GatherDataFields(data);
            return DataFieldsCache[data.GetType()];
        }

        private static void LogFindings(Type type, Dictionary<string, FieldInfo> dataFields)
        {
            string findings = $"DataFields for {type}:\n";
            foreach (var dataField in dataFields)
            {
                findings += $"{dataField.Key} {dataField.Value.FieldType}\n";
            }

            Debug.Log(findings);
        }

        // public static object GetFieldData<TData>(string getFieldByName, TData data)
        // {
        //     var dataFields = GetDataFields<TData>();
        //     return dataFields[getFieldByName].GetValue(data);
        // }
        //
        // public static List<object> GetCollectionData<TData>(string fieldByName, TData data)
        // {
        //     var dataFields = GetDataFields<TData>();
        //     var collectionData = new List<object>();
        //     var collection = (IEnumerable)dataFields[fieldByName].GetValue(data);
        //     foreach (var item in collection)
        //     {
        //         collectionData.Add(item);
        //     }
        //     return collectionData;
        // }
    }
}