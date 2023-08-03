using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class DataFieldManager<TData>
    {
        private Dictionary<string, FieldInfo> DataFields = new();
        
        public void GatherDataFields()
        {
            string dataFieldNames = "";
            FieldInfo[] fields;
            fields = typeof(TData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fields)
            {
                string fieldName = fieldInfo.Name;
                dataFieldNames += fieldName + ", ";
                DataFields.Add(fieldName, fieldInfo);
            }

            Debug.Log($"Found data fields on {typeof(TData)}: {dataFieldNames}");
        }

        public Dictionary<string, FieldInfo> GetDataFields()
        {
            return DataFields;
        }

        public object GetFieldData(string getFieldByName, object data)
        {
            return DataFields[getFieldByName].GetValue(data);
        }

        public List<object> GetCollectionData(string fieldByName, object data)
        {
            List<object> collectionData = new();
            var collection = (IEnumerable)DataFields[fieldByName].GetValue(data);
            foreach (var item in collection)
            {
                collectionData.Add(item);
            }
            return collectionData;
        }
    }
}