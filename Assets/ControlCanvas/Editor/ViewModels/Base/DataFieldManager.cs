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
            if (!DataFieldsCache.TryGetValue(typeof(TData), out var dataFields))
            {
                dataFields = new Dictionary<string, FieldInfo>();
                var fields = typeof(TData).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var fieldInfo in fields)
                {
                    dataFields.Add(fieldInfo.Name, fieldInfo);
                }

                DataFieldsCache[typeof(TData)] = dataFields;
                LogFindings<TData>(dataFields);
            }
        }

        public static Dictionary<string, FieldInfo> GetDataFields<TData>()
        {
            GatherDataFields<TData>();
            return DataFieldsCache[typeof(TData)];
        }

        private static void LogFindings<TData>(Dictionary<string, FieldInfo> dataFields)
        {
            string findings = $"DataFields for {typeof(TData)}:\n";
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