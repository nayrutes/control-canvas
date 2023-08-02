using System;
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
    }
}