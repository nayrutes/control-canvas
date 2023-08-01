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

        public ReactiveProperty<TData> DataProperty { get; private set; } = new ReactiveProperty<TData>();

        //private Dictionary<string, ReactiveProperty<object>> VmReactivePropertiesAsObject = new();
        private Dictionary<string, IDisposable> VmReactivePropertiesTyped = new();
        private Dictionary<string, FieldInfo> DataFields = new();
        protected Dictionary<string, string> DataFieldToReactivePropertyName = new();

        private static Dictionary<Type, Type> reactivePropertyTypeCache = new Dictionary<Type, Type>();

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


        protected void AutoBindReactivePropertiesToDataFields()
        {
            GatherDataFields();
            GatherVmReactiveProperties();
            AutoDataFieldToReactivePropertyNameMapping();
            AutoSetInitValues();
            SetupAutoDataSaving();
        }

        protected void GatherDataFields()
        {
            FieldInfo[] fields;
            fields = typeof(TData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fields)
            {
                DataFields.Add(fieldInfo.Name, fieldInfo);
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
                        string fieldName = fieldInfo.Name.First().ToString().ToLower() + fieldInfo.Name.Substring(1);
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
                        string propertyName = propertyInfo.Name.First().ToString().ToLower() +
                                              propertyInfo.Name.Substring(1);
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
                if (VmReactivePropertiesTyped.TryGetValue(dataField.Key, out var vmReactiveProperty))
                {
                    DataFieldToReactivePropertyName.Add(dataField.Key, dataField.Value.Name);
                }
                //In case the first letter is uppercase
                else if (VmReactivePropertiesTyped.TryGetValue(
                             dataField.Key.First().ToString().ToLower() + dataField.Key.Substring(1),
                             out vmReactiveProperty))
                {
                    DataFieldToReactivePropertyName.Add(dataField.Key,
                        dataField.Value.Name.First().ToString().ToLower() + dataField.Value.Name.Substring(1));
                }
                else
                {
                    Debug.LogWarning(
                        $"Could not find vm reactive property for {dataField.Key}. Creating it dynamically...");

                    Type vmReactivePropertyType;
                    if (!reactivePropertyTypeCache.TryGetValue(dataField.Value.FieldType, out vmReactivePropertyType))
                    {
                        vmReactivePropertyType = typeof(ReactiveProperty<>).MakeGenericType(dataField.Value.FieldType);
                        reactivePropertyTypeCache[dataField.Value.FieldType] = vmReactivePropertyType;
                    }

                    var vmReactivePropertyInstance = Activator.CreateInstance(vmReactivePropertyType);
                    // var vmReactivePropertyInstanceField = vmReactivePropertyType.GetProperty("Value");
                    // vmReactivePropertyInstanceField.SetValue(vmReactivePropertyInstance, dataField.Value.GetValue(DataProperty.Value));
                    //vmReactiveProperty = (ReactiveProperty<object>)vmReactivePropertyInstance;
                    VmReactivePropertiesTyped.Add(dataField.Key, vmReactivePropertyInstance as IDisposable);
                }
            }
        }

        protected void AutoSetInitValues()
        {
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                if (VmReactivePropertiesTyped.TryGetValue(dataField.Key, out var vmReactiveProperty))
                {
                    SetReactivePropertyInitValue(dataField.Key, dataField.Value.GetValue(DataProperty.Value));
                }
                else
                {
                    Debug.LogWarning($"Could not find vm reactive property for {dataField.Key}");
                }
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

        protected ReactiveProperty<T> GetReactiveProperty<T>(string propertyName)
        {
            if (VmReactivePropertiesTyped.TryGetValue(propertyName, out var reactiveProperty))
            {
                if (reactiveProperty is ReactiveProperty<T>)
                {
                    return reactiveProperty as ReactiveProperty<T>;
                }
                else
                {
                    Debug.LogError(
                        $"Could not find reactive property for {propertyName} and type {typeof(T)}. Only found {reactiveProperty.GetType()}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Could not find reactive property for {propertyName}");
                return null;
            }
        }

        // protected ReactiveProperty<object> GetReactiveProperty(string propertyName)
        // {
        //     if (VmReactiveProperties.TryGetValue(propertyName, out var reactiveProperty))
        //     {
        //         return reactiveProperty as ReactiveProperty<object>;
        //     }
        //     else
        //     {
        //         Debug.LogError($"Could not find reactive property for {propertyName}");
        //         return null;
        //     }
        // } 

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


        // ReSharper disable once MemberCanBePrivate.Global
        //protected is needed for the reflection
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

        // protected void SetupDataSaving(ReactiveProperty<object> property, string dataVariableName)
        // {
        //     FieldInfo fieldInfo = DataProperty.Value.GetType().GetField(dataVariableName);
        //     property.Subscribe(value =>
        //     {
        //         fieldInfo.SetValue(DataProperty.Value, value);
        //     }).AddTo(disposables);
        // }

        // ReSharper disable once MemberCanBePrivate.Global
        //protected is needed for the reflection
        protected void SetupDataSaving(IDisposable property, string dataVariableName)
        {
            FieldInfo fieldInfo = DataProperty.Value.GetType().GetField(dataVariableName);

            // Get the type of T in ReactiveProperty<T>
            Type valueType = property.GetType().GetGenericArguments()[0];

            // Get the helper method and make it generic
            MethodInfo helperMethod =
                GetType().GetMethod("SetupDataSavingHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            //MethodInfo[] methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(valueType);

            // Invoke the helper method
            genericHelperMethod.Invoke(this, new object[] { property, fieldInfo });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // protected is needed for the reflection
        // Helper method for setting up data saving
        protected void SetupDataSavingHelper<T>(IDisposable property, FieldInfo fieldInfo)
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;
            typedProperty.Subscribe(value => { fieldInfo.SetValue(DataProperty.Value, value); }).AddTo(disposables);
        }


        // protected void SetupDataSaving<T>(ReactiveProperty<T> property, string dataVariableName)
        // {
        //     FieldInfo fieldInfo = DataProperty.Value.GetType().GetField(dataVariableName);
        //     property.Subscribe(value =>
        //     {
        //         fieldInfo.SetValue(DataProperty.Value, value);
        //     }).AddTo(disposables);
        // }

        //PropertyInfo propertyInfo = nodeData.Value.GetType().GetProperty(dataVariableName);
        //propertyInfo.SetValue(nodeData.Value, value);

        private void SetupDataSaving<T>(ReactiveProperty<T> property, Action<T> action)
        {
            property.Subscribe(action).AddTo(disposables);
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
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}