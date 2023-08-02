using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels
{
    public interface IViewModel : IDisposable
    {
    }

    public abstract class BaseViewModel<TData> : IViewModel
    {
        protected CompositeDisposable disposables = new();

        public ReactiveProperty<TData> DataProperty { get; private set; } = new();

        private Dictionary<string, IDisposable> VmReactivePropertiesTyped = new();
        private Dictionary<string, FieldInfo> DataFields = new();


        private Dictionary<string, string> DataFieldToReactivePropertyName;

        private static Dictionary<Type, Type> reactivePropertyTypeCache = new();
        private static Dictionary<Type, Type> reactiveCollectionTypeCache = new();

        /// <summary>
        /// This should be self contained and don't rely on members of the class
        /// </summary>
        /// <returns></returns>
        protected abstract TData CreateData();

        public BaseViewModel(bool autobind = true)
        {
            TData data = CreateData();
            Setup(data, autobind);
        }

        public BaseViewModel(TData data, bool autobind = true)
        {
            Setup(data, autobind);
        }

        private void Setup(TData data, bool autobind)
        {
            DataFieldToReactivePropertyName = InitializeMappingDictionary();
            DataProperty.Value = data;
            if (autobind)
            {
                GatherDataFields();
                GatherVmReactiveProperties();
                if (AutoDataFieldToReactivePropertyNameMapping())
                {
                    return;
                }

                DataProperty.Subscribe(data => { AutoSetInitValues(); }).AddTo(disposables);
                SetupAutoDataSaving();
            }
            else
            {
                DataProperty.Subscribe(data => { LoadDataInternal(data); }).AddTo(disposables);
            }
        }

        /// <summary>
        /// Override this to manual map data fields to reactive properties
        /// </summary>
        protected virtual Dictionary<string, string> InitializeMappingDictionary()
        {
            return new();
        }

        /// <summary>
        /// Override this to load data from the data object if no AutoBind is used
        /// </summary>
        /// <param name="data"></param>
        protected virtual void LoadDataInternal(TData data)
        {
        }

        /// <summary>
        /// Override this to save data to the data object if no AutoBind is used
        /// </summary>
        /// <param name="data"></param>
        protected virtual void SaveDataInternal(TData data)
        {
        }

        protected void GatherDataFields()
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

        protected void GatherVmReactiveProperties()
        {
            string reactivePropertyFieldNames = "";
            GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(fieldInfo =>
                {
                    if (fieldInfo.FieldType.IsReactiveProperty())
                    {
                        string
                            fieldName = fieldInfo.Name;
                        reactivePropertyFieldNames += fieldName + ", ";
                        RegisterTypedReactiveProperty(fieldName, (IDisposable)fieldInfo.GetValue(this));
                    }
                });
            string reactivePropertyPropertyNames = "";
            GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(propertyInfo =>
                {
                    if (propertyInfo.PropertyType.IsReactiveProperty())
                    {
                        string propertyName = propertyInfo.Name;
                        reactivePropertyPropertyNames += propertyName + ", ";
                        RegisterTypedReactiveProperty(propertyName, (IDisposable)propertyInfo.GetValue(this));
                    }
                });
            Debug.Log($"Found ReactiveProperty on {GetType()} \nfields: {reactivePropertyFieldNames} \n" +
                      $"properties: {reactivePropertyPropertyNames}");
            //Debug.Log($"Found ReactiveProperty properties {GetType()}: {reactivePropertyPropertyNames}");
        }

        private void RegisterTypedReactiveProperty(string reactivePropertyName, IDisposable reactiveProperty)
        {
            VmReactivePropertiesTyped.Add(reactivePropertyName, reactiveProperty);
        }


        protected bool AutoDataFieldToReactivePropertyNameMapping()
        {
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                //Skip if manual mapping is already done
                if (DataFieldToReactivePropertyName.ContainsKey(dataField.Key)) continue;

                //try mapping ReactiveProperty to data field by name
                string fieldName = dataField.Key;
                string fieldNameLower = dataField.Key.First().ToString().ToLower() + dataField.Key.Substring(1);
                string fieldNameUpper = fieldNameLower.First().ToString().ToUpper() + fieldNameLower.Substring(1);
                if (VmReactivePropertiesTyped.ContainsKey(fieldNameLower))
                {
                    DataFieldToReactivePropertyName.Add(dataField.Key, fieldNameLower);
                }
                else if (VmReactivePropertiesTyped.ContainsKey(fieldNameUpper))
                {
                    DataFieldToReactivePropertyName.Add(dataField.Key, fieldNameUpper);
                }
                else
                {
                    Type vmReactivePropertyType;
                    Type fieldType = dataField.Value.FieldType;
                    Type
                        reactivePropertyValueType; // = dataField.Value.FieldType;//typeof(ReactiveProperty<>).GetGenericArguments()[0];

                    if (fieldType.IsArray)
                    {
                        //reactivePropertyValueType = fieldType.GetElementType();
                        Type elementType = fieldType.GetElementType();
                        if (!reactiveCollectionTypeCache.TryGetValue(elementType, out reactivePropertyValueType))
                        {
                            reactivePropertyValueType = typeof(ReactiveCollection<>).MakeGenericType(elementType);
                            reactiveCollectionTypeCache[elementType] = reactivePropertyValueType;
                        }
                    }
                    else if (fieldType.IsGenericType)
                    {
                        throw new Exception("Generic types are not supported (yet)");
                        Debug.LogError("Generic types are not supported (yet)");
                        //reactivePropertyValueType = fieldType.GetGenericArguments()[0];
                    }
                    else
                    {
                        reactivePropertyValueType = fieldType;
                    }

                    if (!reactivePropertyTypeCache.TryGetValue(reactivePropertyValueType, out vmReactivePropertyType))
                    {
                        vmReactivePropertyType = typeof(ReactiveProperty<>).MakeGenericType(reactivePropertyValueType);
                        reactivePropertyTypeCache[reactivePropertyValueType] = vmReactivePropertyType;
                    }

                    var vmReactivePropertyInstance = Activator.CreateInstance(vmReactivePropertyType);
                    //vmReactivePropertyType.Name = fieldNameUpper;
                    string propertyName = fieldNameUpper;
                    VmReactivePropertiesTyped.Add(propertyName, vmReactivePropertyInstance as IDisposable);
                    DataFieldToReactivePropertyName.Add(fieldName, propertyName);

                    Debug.Log(
                        $"Could not find reactive property for {fieldName}." +
                        $"Created it dynamically with type {vmReactivePropertyType}...");
                }
            }

            bool error = false;
            //Check if types match
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                string reactivePropertyName = DataFieldToReactivePropertyName[dataField.Key];
                var reactiveProperty = VmReactivePropertiesTyped[reactivePropertyName];
                Type innerPropertyType = reactiveProperty.GetType().GetGenericArguments()[0];
                Type dataFieldType = dataField.Value.FieldType;
                if (dataFieldType.IsCollection())
                {
                    Type innerPropertyGenericType = innerPropertyType.GetGenericTypeDefinition();
                    Type innerFieldType = dataFieldType.GetInnerType();
                    Type innerCollectionType = innerPropertyType.GetGenericArguments()[0];
                    //Check if innerPropertyType is a ReactiveCollection of innerFieldType
                    if (innerPropertyType.IsReactiveCollection())
                    {
                        if (innerCollectionType != innerFieldType)
                        {
                            Debug.LogError(
                                $"Type mismatch between data field {dataField.Key} and reactive property {reactivePropertyName}: {innerPropertyType} != {innerFieldType}");
                            error = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(
                            $"Type mismatch between data field {dataField.Key} and reactive property {reactivePropertyName}: {innerPropertyGenericType} != {typeof(ReactiveCollection<>)}");
                        error = true;
                    }
                }
                else
                {
                    if (innerPropertyType != dataFieldType)
                    {
                        Debug.LogError(
                            $"Type mismatch between data field {dataField.Key} and reactive property {reactivePropertyName} : {innerPropertyType} != {dataFieldType}");
                        error = true;
                    }
                }
            }

            foreach (KeyValuePair<string, IDisposable> keyValuePair in VmReactivePropertiesTyped)
            {
                if (DataFieldToReactivePropertyName.ContainsValue(keyValuePair.Key)) continue;
                Debug.LogWarning($"Could not find data field for {keyValuePair.Key}");
            }

            return error;
        }

        protected void AutoSetInitValues()
        {
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                if (!DataFieldToReactivePropertyName.ContainsKey(dataField.Key))
                {
                    Debug.LogWarning($"Could not find reactive property for {dataField.Key}. Skipping Init value");
                    continue;
                }

                SetReactivePropertyInitValue(DataFieldToReactivePropertyName[dataField.Key],
                    dataField.Value.GetValue(DataProperty.Value));
            }
        }

        protected void SetReactivePropertyInitValue(string propertyName, object value)
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

        protected IDisposable GetReactiveProperty(string propertyName)
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

        protected void SetupAutoDataSaving()
        {
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                if (DataFieldToReactivePropertyName.TryGetValue(dataField.Key, out var reactivePropertyName))
                {
                    var reactiveProperty = GetReactiveProperty(reactivePropertyName);
                    SetupDataSaving(reactiveProperty, dataField.Key);
                }
                else
                {
                    Debug.LogError($"Could not find reactive property for {dataField.Key}");
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        //protected is needed for the reflection
        protected void SetupDataSaving(IDisposable property, string dataFieldName)
        {
            FieldInfo fieldInfo; // = DataProperty.Value.GetType().GetField(dataVariableName);

            fieldInfo = DataFields[dataFieldName];

            // Get the type of T in ReactiveProperty<T>
            Type valueType = property.GetType().GetGenericArguments()[0];

            // Get the helper method and make it generic
            MethodInfo helperMethod =
                GetType().GetMethod("SetupDataSavingHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(valueType);

            // Invoke the helper method
            genericHelperMethod.Invoke(this, new object[] { property, fieldInfo });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMember.Global
        // protected is needed for the reflection
        // Helper method for setting up data saving
        protected void SetupDataSavingHelper<T>(IDisposable property, FieldInfo fieldInfo)
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;

            // Check if T is a ReactiveCollection
            if (typeof(T).IsReactiveCollection())
            {
                // Cast the value to ReactiveCollection and subscribe to its changes
                typedProperty.Subscribe(value =>
                {
                    var reactiveCollection = value as ReactiveCollection<object>;
                    if (reactiveCollection != null)
                    {
                        reactiveCollection.ObserveAdd().Subscribe(addEvent =>
                        {
                            // Handle addition to the collection
                            // For example, you can update the field value
                            fieldInfo.SetValue(DataProperty.Value, reactiveCollection.ToList());
                        }).AddTo(disposables);

                        reactiveCollection.ObserveRemove().Subscribe(removeEvent =>
                        {
                            // Handle removal from the collection
                            // For example, you can update the field value
                            fieldInfo.SetValue(DataProperty.Value, reactiveCollection.ToList());
                        }).AddTo(disposables);
                    }
                }).AddTo(disposables);
            }
            else
            {
                typedProperty.Subscribe(value =>
                {
                    fieldInfo.SetValue(DataProperty.Value, value);
                }).AddTo(disposables);
            }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
            DataProperty.Dispose();
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}