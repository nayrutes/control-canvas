using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                AutoDataFieldToReactivePropertyNameMapping();
                DataProperty.Subscribe(data =>
                {
                    AutoSetInitValues();
                }).AddTo(disposables);
                SetupAutoDataSaving();
            }
            else
            {
                DataProperty.Subscribe(data =>
                {
                    LoadDataInternal(data);
                }).AddTo(disposables);
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
            FieldInfo[] fields;
            fields = typeof(TData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fields)
            {
                string fieldName = fieldInfo.Name;
                DataFields.Add(fieldName, fieldInfo);
            }
        }

        protected void GatherVmReactiveProperties()
        {
            GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(fieldInfo =>
                {
                    if (fieldInfo.FieldType.IsGenericType &&
                        fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
                    {
                        string
                            fieldName = fieldInfo.Name;
                        Debug.Log($"Found ReactiveProperty field {fieldInfo.Name}, saving it as {fieldName}");
                        RegisterTypedReactiveProperty(fieldName, (IDisposable)fieldInfo.GetValue(this));
                    }
                });
            GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList()
                .ForEach(propertyInfo =>
                {
                    if (propertyInfo.PropertyType.IsGenericType &&
                        propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
                    {
                        string
                            propertyName =
                                propertyInfo.Name;
                        Debug.Log($"Found ReactiveProperty property {propertyInfo.Name}, saving it as {propertyName}");
                        RegisterTypedReactiveProperty(propertyName, (IDisposable)propertyInfo.GetValue(this));
                    }
                });
        }

        private void RegisterTypedReactiveProperty(string reactivePropertyName, IDisposable reactiveProperty)
        {
            VmReactivePropertiesTyped.Add(reactivePropertyName, reactiveProperty);
        }


        protected void AutoDataFieldToReactivePropertyNameMapping()
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
                    Debug.Log(
                        $"Could not find reactive property for {fieldName}. Creating it dynamically...");

                    Type vmReactivePropertyType;
                    if (!reactivePropertyTypeCache.TryGetValue(dataField.Value.FieldType, out vmReactivePropertyType))
                    {
                        vmReactivePropertyType = typeof(ReactiveProperty<>).MakeGenericType(dataField.Value.FieldType);
                        reactivePropertyTypeCache[dataField.Value.FieldType] = vmReactivePropertyType;
                    }

                    var vmReactivePropertyInstance = Activator.CreateInstance(vmReactivePropertyType);
                    //vmReactivePropertyType.Name = fieldNameUpper;
                    string propertyName = fieldNameUpper;
                    VmReactivePropertiesTyped.Add(propertyName, vmReactivePropertyInstance as IDisposable);
                    DataFieldToReactivePropertyName.Add(fieldName, propertyName);
                }
            }

            foreach (KeyValuePair<string, IDisposable> keyValuePair in VmReactivePropertiesTyped)
            {
                if (DataFieldToReactivePropertyName.ContainsValue(keyValuePair.Key)) continue;
                Debug.LogWarning($"Could not find data field for {keyValuePair.Key}");
            }
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
                var reactivePropertyType = reactiveProperty.GetType();
                var reactivePropertyInstanceField = reactivePropertyType.GetProperty("Value");
                reactivePropertyInstanceField.SetValue(reactiveProperty, value);
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
            typedProperty.Subscribe(value =>
            {
                fieldInfo.SetValue(DataProperty.Value, value);
            }).AddTo(disposables);
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