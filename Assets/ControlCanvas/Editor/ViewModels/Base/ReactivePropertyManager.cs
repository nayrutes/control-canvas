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
        private Dictionary<string, bool> _propertyHasDataField = new();
        private static Dictionary<Type, Type> reactivePropertyTypeCache = new();
        private static Dictionary<Type, Type> reactiveCollectionTypeCache = new();

        private static Dictionary<Type, List<FieldInfo>> fieldCache = new();
        private static Dictionary<Type, List<PropertyInfo>> propertyCache = new();

        public void GatherVmReactiveProperties(IViewModel viewModel)
        {
            Type type = viewModel.GetType();

            if (!fieldCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(fieldInfo => fieldInfo.FieldType.IsReactiveProperty())
                    .ToList();
                fieldCache[type] = fields;
            }

            foreach (var fieldInfo in fields)
            {
                string fieldName = fieldInfo.Name;
                RegisterTypedReactiveProperty(fieldName, (IDisposable)fieldInfo.GetValue(viewModel));
            }

            if (!propertyCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(propertyInfo => propertyInfo.PropertyType.IsReactiveProperty())
                    .ToList();
                propertyCache[type] = properties;
            }

            foreach (var propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                RegisterTypedReactiveProperty(propertyName, (IDisposable)propertyInfo.GetValue(viewModel));
            }
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

            //Debug.Log($"Could not find reactive property for {dataField.Key}. Created it dynamically with type {vmReactivePropertyType}...");
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
                Debug.Log($"Generic types in ReactiveProperties are not fully tested. Type: {fieldType}");
                //throw new Exception("Generic types are not supported (yet)");
                return fieldType;
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


        public void WarnForUnmappedReactiveProperties(Dictionary<string, string> fieldToPropertyMap)
        {
            foreach (KeyValuePair<string, IDisposable> keyValuePair in VmReactivePropertiesTyped)
            {
                bool b = true;
                if (!fieldToPropertyMap.ContainsValue(keyValuePair.Key))
                {
                    Debug.Log($"Could not find data field for {keyValuePair.Key}");
                    b = false;
                }
                _propertyHasDataField[keyValuePair.Key] = b;
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

        public Dictionary<string, IDisposable> GetAllReactiveProperties(bool onlyWithField = false)
        {
            if(!onlyWithField)
                return VmReactivePropertiesTyped;
            
            return VmReactivePropertiesTyped.Where(x => _propertyHasDataField[x.Key]).ToDictionary(x => x.Key, x => x.Value);
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
            List<Type> types = new();
            foreach (var v1 in VmReactivePropertiesTyped.Values)
            {
                Type inner1 = v1.GetType().GetInnerType();
                if (inner1.IsReactiveProperty() || inner1.IsReactiveCollection())
                {
                    types.Add(inner1.GetInnerType());
                }
                else
                {
                    types.Add(inner1);
                }
            }

            return types;
            //return VmReactivePropertiesTyped.Values.Select(x => x.GetType().GetInnerType().GetInnerType()).ToList();
        }

        public List<IDisposable> GetFieldsOfType(Type type)
        {
            return VmReactivePropertiesTyped.Where(x => x.Value.GetType().GetInnerType() == type).Select(x => x.Value)
                .ToList();
        }

        public List<IDisposable> GetCollectionsOfType(Type type)
        {
            return VmReactivePropertiesTyped.Where(x => x.Value.GetType().GetInnerType().IsReactiveCollection()
                                                        && x.Value.GetType().GetInnerType().GetInnerType() == type)
                .Select(x => x.Value).ToList();
        }

        public string GetNameByReactiveProperty(IDisposable reactivePropertyData)
        {
            return VmReactivePropertiesTyped.FirstOrDefault(x => x.Value == reactivePropertyData).Key;
        }
    }
}