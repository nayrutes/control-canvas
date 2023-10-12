using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class AutoDataSaving<TData> : IDisposable
    {
        private readonly ReactiveProperty<TData> DataProperty;
        private readonly CompositeDisposable disposables;
        private readonly ReactivePropertyManager reactivePropertyManager;
        private readonly FieldToPropertyMapper<TData> fieldToPropertyMapper;

        public AutoDataSaving(ReactiveProperty<TData> dataProperty, ReactivePropertyManager reactivePropertyManager,
            FieldToPropertyMapper<TData> fieldToPropertyMapper)
        {
            DataProperty = dataProperty;
            this.disposables = new CompositeDisposable();
            this.reactivePropertyManager = reactivePropertyManager;
            this.fieldToPropertyMapper = fieldToPropertyMapper;
        }


        public void SetupAutoDataSaving(object data)
        {
            Dictionary<string, FieldInfo> dataFields = DataFieldManager.GetDataFields(data);
            foreach (KeyValuePair<string, FieldInfo> dataField in dataFields)
            {
                if (fieldToPropertyMapper.TryGetValue(dataField.Key, out var reactivePropertyName))
                {
                    var reactiveProperty = reactivePropertyManager.GetReactiveProperty(reactivePropertyName);
                    SetupDataSaving(reactiveProperty, dataField.Key, data);
                }
                else
                {
                    Debug.LogError($"Could not find reactive property for {dataField.Key}");
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        //protected is needed for the reflection
        protected void SetupDataSaving(IDisposable property, string dataFieldName, object data)
        {
            FieldInfo fieldInfo; // = DataProperty.Value.GetType().GetField(dataVariableName);

            fieldInfo = DataFieldManager.GetDataFields(data)[dataFieldName];

            // Get the type of T in ReactiveProperty<T>
            Type valueType = property.GetType().GetGenericArguments()[0];

            // Get the helper method and make it generic
            MethodInfo helperMethod =
                GetType().GetMethod("SetupDataSavingHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(valueType, valueType.GetInnerType());

            // Invoke the helper method
            genericHelperMethod.Invoke(this, new object[] { property, fieldInfo });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMember.Global
        // protected is needed for the reflection
        // Helper method for setting up data saving
        protected void SetupDataSavingHelper<T, TInner>(IDisposable property, FieldInfo fieldInfo)
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;

            // Check if T is a ReactiveCollection
            if (typeof(T).IsReactiveCollection())
            {
                // Cast the value to ReactiveCollection and subscribe to its changes
                typedProperty.Subscribe(value =>
                {
                    var reactiveCollection = value as ReactiveCollection<TInner>;
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
                typedProperty.Subscribe(value => { fieldInfo.SetValue(DataProperty.Value, value); }).AddTo(disposables);
            }
        }

        public void AutoSetInitValues(object data)
        {
            Dictionary<string, FieldInfo> DataFields = DataFieldManager.GetDataFields(data);
            foreach (KeyValuePair<string, FieldInfo> dataField in DataFields)
            {
                if (!fieldToPropertyMapper.ContainsKey(dataField.Key))
                {
                    Debug.LogWarning($"Could not find reactive property for {dataField.Key}. Skipping Init value");
                    continue;
                }

                reactivePropertyManager.SetReactivePropertyInitValue(
                    fieldToPropertyMapper.GetPropNameByFieldName(dataField.Key),
                    dataField.Value.GetValue(DataProperty.Value));
            }
        }

        private void ReleaseUnmanagedResources()
        {
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                disposables?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AutoDataSaving()
        {
            Debug.LogWarning(
                $"Dispose was not called on {this.GetType()}. You should call Dispose on IDisposable objects when you are done using them.");
            Dispose(false);
        }
    }
}