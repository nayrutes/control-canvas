using System;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.ReactiveInspector;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class AutomaticContentView : INodeContent
    {
        public VisualElement CreateView(IControl control)
        {
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var rps = vm.GetAllReactiveProperties();
            
            VisualElement view = new();
            
            foreach (var keyValuePair in rps)
            {
                Type t = keyValuePair.Value.GetType().GetGenericArguments().First();
                VisualElement field = GenericField.AddGenericField(t, keyValuePair.Key);
                LinkVisualElementWithReactiveProperty(field, keyValuePair.Value, keyValuePair.Key, t);
                view.Add(field);
            }

            return view;
        }

        
        
        
        private static void LinkVisualElementWithReactiveProperty(VisualElement visualElement, IDisposable reactiveProperty, string labelName, Type rpValueType=null)
        {
            if (rpValueType == null)
            {
                rpValueType = reactiveProperty.GetType().GetGenericArguments().First();
            }
            
            Type visualElementType = visualElement.GetType();
            while (visualElementType != null)
            {
                if (visualElementType.IsGenericType && visualElementType.GetGenericTypeDefinition() == typeof(BaseField<>))
                {
                    Type baseFieldType = visualElementType.GetGenericArguments().First();
                    if (baseFieldType == rpValueType)
                    {
                        var methodInfo = typeof(AutomaticContentView).GetMethod(nameof(LinkBaseFieldWithReactiveProperty),
                            BindingFlags.NonPublic | BindingFlags.Static);
                        methodInfo.MakeGenericMethod(baseFieldType).Invoke(null, new object[] {visualElement, reactiveProperty, labelName});
                    }
                    else
                    {
                        Debug.LogWarning($"Type mismatch between {visualElementType} and {rpValueType}");
                    }
                    break;
                }
                visualElementType = visualElementType.BaseType;
            }
            
            if(visualElementType == null)
            {
                Debug.LogWarning($"No link found for type {rpValueType}");
            }
        }

        private static void LinkBaseFieldWithReactiveProperty<T>(BaseField<T> baseField,
            ReactiveProperty<T> reactiveProperty, string labelName)
        {
            void SetRpValueFunction(ChangeEvent<T> evt)
            {
                reactiveProperty.Value = evt.newValue;
            }
            baseField.RegisterValueChangedCallback(SetRpValueFunction);
            
            
            reactiveProperty.Subscribe(value => baseField.value = value);
            reactiveProperty.Subscribe(_ => { }, () =>
            {
                baseField.UnregisterValueChangedCallback(SetRpValueFunction);
            });
            baseField.label = labelName;
        }
    }
}