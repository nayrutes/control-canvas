using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class ReactivePropertyManager
    {
        private Dictionary<string, IDisposable> VmReactivePropertiesTyped = new();
        private static Dictionary<Type, Type> reactivePropertyTypeCache = new();
        private static Dictionary<Type, Type> reactiveCollectionTypeCache = new();

        public void GatherVmReactiveProperties(IViewModel viewModel)
        {
            Type type = viewModel.GetType();
            string reactivePropertyFieldNames = "";
            type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(fieldInfo =>
                {
                    if (fieldInfo.FieldType.IsReactiveProperty())
                    {
                        string
                            fieldName = fieldInfo.Name;
                        reactivePropertyFieldNames += fieldName + ", ";
                        RegisterTypedReactiveProperty(fieldName, (IDisposable)fieldInfo.GetValue(viewModel));
                    }
                });
            string reactivePropertyPropertyNames = "";
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(propertyInfo =>
                {
                    if (propertyInfo.PropertyType.IsReactiveProperty())
                    {
                        string propertyName = propertyInfo.Name;
                        reactivePropertyPropertyNames += propertyName + ", ";
                        RegisterTypedReactiveProperty(propertyName, (IDisposable)propertyInfo.GetValue(viewModel));
                    }
                });
            Debug.Log($"Found ReactiveProperty on {type} \nfields: {reactivePropertyFieldNames} \n" +
                      $"properties: {reactivePropertyPropertyNames}");
            //Debug.Log($"Found ReactiveProperty properties {GetType()}: {reactivePropertyPropertyNames}");
        }

        private void RegisterTypedReactiveProperty(string reactivePropertyName, IDisposable reactiveProperty)
        {
            VmReactivePropertiesTyped.Add(reactivePropertyName, reactiveProperty);
        }

        public string CreateAndAddReactiveProperty(KeyValuePair<string, FieldInfo> dataField, string propertyName)
        {
            Type fieldType = dataField.Value.FieldType;
            Type reactivePropertyValueType = GetReactivePropertyValueType(fieldType);
            Type vmReactivePropertyType = GetVmReactivePropertyType(reactivePropertyValueType);

            var vmReactivePropertyInstance = Activator.CreateInstance(vmReactivePropertyType);
            VmReactivePropertiesTyped.Add(propertyName, vmReactivePropertyInstance as IDisposable);
            //KeyValuePair<string, string> mapping = new(dataField.Key, propertyName);
            //fieldToPropertyMapper.Add(dataField.Key, propertyName);

            Debug.Log($"Could not find reactive property for {dataField.Key}. Created it dynamically with type {vmReactivePropertyType}...");
            return propertyName;
        }
        private Type GetReactivePropertyValueType(Type fieldType)
        {
            if (fieldType.IsCollection())
            {
                Type elementType = fieldType.GetInnerType();
                if (!reactiveCollectionTypeCache.TryGetValue(elementType, out Type reactivePropertyValueType))
                {
                    reactivePropertyValueType = typeof(ReactiveCollection<>).MakeGenericType(elementType);
                    reactiveCollectionTypeCache[elementType] = reactivePropertyValueType;
                }

                return reactivePropertyValueType;
            }
            else if (fieldType.IsGenericType)
            {
                throw new Exception("Generic types are not supported (yet)");
            }
            else
            {
                return fieldType;
            }
        }
        
        private Type GetVmReactivePropertyType(Type reactivePropertyValueType)
        {
            if (!reactivePropertyTypeCache.TryGetValue(reactivePropertyValueType, out Type vmReactivePropertyType))
            {
                vmReactivePropertyType = typeof(ReactiveProperty<>).MakeGenericType(reactivePropertyValueType);
                reactivePropertyTypeCache[reactivePropertyValueType] = vmReactivePropertyType;
            }

            return vmReactivePropertyType;
        }


        public void WarnForUnmappedReactiveProperties(Dictionary<string,string> fieldToPropertyMap)
        {
            foreach (KeyValuePair<string, IDisposable> keyValuePair in VmReactivePropertiesTyped)
            {
                if (!fieldToPropertyMap.ContainsValue(keyValuePair.Key))
                {
                    Debug.LogWarning($"Could not find data field for {keyValuePair.Key}");
                }
            }
        }

        public void SetReactivePropertyInitValue(string propertyName, object value)
        {
            if (VmReactivePropertiesTyped.TryGetValue(propertyName, out var reactiveProperty))
            {
                Type reactivePropertyType = reactiveProperty.GetType();
                Type innerReactivePropertyType = reactivePropertyType.GetGenericArguments()[0];
                PropertyInfo reactivePropertyInstanceField = reactivePropertyType.GetProperty("Value");
                if (innerReactivePropertyType.IsReactiveCollection())
                {
                    //create new reactive collection and write values from list or array to it
                    var reactiveCollection = Activator.CreateInstance(innerReactivePropertyType);

                    foreach (var item in (IEnumerable)value)
                    {
                        reactiveCollection.GetType().GetMethod("Add").Invoke(reactiveCollection, new[] { item });
                    }

                    reactivePropertyInstanceField.SetValue(reactiveProperty, reactiveCollection);
                }
                else
                {
                    reactivePropertyInstanceField.SetValue(reactiveProperty, value);
                }
            }
            else
            {
                Debug.LogError($"Could not find reactive property for {propertyName}");
            }
        }

        public IDisposable GetReactiveProperty(string propertyName)
        {
            if (VmReactivePropertiesTyped.TryGetValue(propertyName, out var reactiveProperty))
            {
                return reactiveProperty;
            }
            else
            {
                Debug.LogError($"Could not find reactive property for {propertyName}");
                return null;
            }
        }


        public bool ContainsKey(string fieldName)
        {
            return VmReactivePropertiesTyped.ContainsKey(fieldName);
        }

        public IDisposable Get(string reactivePropertyName)
        {
            return VmReactivePropertiesTyped[reactivePropertyName];
        }

        public List<Type> GetAllInnerTypes()
        {
            return VmReactivePropertiesTyped.Values.Select(x => x.GetType().GetInnerType().GetInnerType()).ToList();
        }

        public List<string> GetFieldsOfType(Type type)
        {
            return VmReactivePropertiesTyped.Where(x => x.Value.GetType().GetInnerType() == type).Select(x => x.Key).ToList();
        }

        public List<string> GetCollectionsOfType(Type type)
        {
            return VmReactivePropertiesTyped.Where(x => x.Value.GetType().GetInnerType().IsReactiveCollection() 
                                                        && x.Value.GetType().GetInnerType().GetInnerType() == type).Select(x => x.Key).ToList();
        }
    }
}