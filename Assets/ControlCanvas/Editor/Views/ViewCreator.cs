using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.ReactiveInspector;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class ViewCreator
    {
        private static Dictionary<Type, INodeContent> viewContentTypes = new();
        private static Dictionary<Type, INodeSettings> viewSettingsTypes = new();
        private static bool isInitialized = false;

        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributesTypes = assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<NodeContentAttribute>() != null).ToList();
            
            var nodeContentTypes = attributesTypes.Where(t=>typeof(INodeContent).IsAssignableFrom(t))
                .Select(t => (INodeContent)Activator.CreateInstance(t))
                .ToList();
            foreach (INodeContent nodeContentType in nodeContentTypes)
            {
                var targetType = nodeContentType.GetType().GetCustomAttribute<NodeContentAttribute>().ContentType;
                viewContentTypes.Add(targetType, nodeContentType);
            }

            var nodeSettingsTypes = attributesTypes.Where(t=>typeof(INodeSettings).IsAssignableFrom(t))
                .Select(t => (INodeSettings)Activator.CreateInstance(t))
                .ToList();
            foreach (INodeSettings nodeSettingsType in nodeSettingsTypes)
            {
                var targetType = nodeSettingsType.GetType().GetCustomAttribute<NodeContentAttribute>().ContentType;
                viewSettingsTypes.Add(targetType, nodeSettingsType);
            }
            
            isInitialized = true;
        }

        public static INodeContent GetContentViewCreator(Type dataType)
        {
            if (!isInitialized)
                Initialize();
            if (!viewContentTypes.TryGetValue(dataType, out var view))
            {
                throw new Exception($"No view found for type {dataType}");
            }

            return view;
        }

        public static bool IsTypeManuallyDefined(Type type)
        {
            if (!isInitialized)
                Initialize();
            return viewContentTypes.ContainsKey(type);
        }


        public static bool IsControlViewManuallyDefined(IControl control)
        {
            return IsTypeManuallyDefined(control.GetType());
        }

        public static VisualElement CreateViewFromControl(IControl control)
        {
            if (IsControlViewManuallyDefined(control))
            {
                return GetContentViewCreator(control.GetType()).CreateView(control);
            }
            else
            {
                return new AutomaticContentView().CreateView(control);
            }
        }


        public static VisualElement CreateLinkedGenericField(IViewModel viewModel, string name)
        {
            var rp = viewModel.GetReactiveProperty(name);
            var field = CreateAndLink(name, rp);
            return field;
        }

        public static VisualElement CreateAndLink(string name, IDisposable reactiveProperty)
        {
            Type rpValueType = reactiveProperty.GetType().GetGenericArguments().First();
            VisualElement field = GenericField.AddGenericField(rpValueType, name);
            LinkVisualElementWithReactiveProperty(field, reactiveProperty, name, rpValueType);
            return field;
        }

        public static void LinkVisualElementWithReactiveProperty(VisualElement visualElement,
            IDisposable reactiveProperty, string labelName, Type rpValueType = null)
        {
            if (rpValueType == null)
            {
                rpValueType = reactiveProperty.GetType().GetGenericArguments().First();
            }

            Type visualElementType = visualElement.GetType();

            if (rpValueType.IsEnum)
            {
                if (visualElementType == typeof(EnumField))
                {
                    var methodInfo = typeof(ViewCreator).GetMethod(nameof(LinkEnumFieldWithReactiveProperty),
                        BindingFlags.NonPublic | BindingFlags.Static);
                    methodInfo.MakeGenericMethod(rpValueType).Invoke(null,
                        new object[] { visualElement, reactiveProperty, labelName });
                }
                else
                {
                    Debug.LogError($"Type mismatch between {visualElementType} and {rpValueType}");
                }
            }
            else
            {
                while (visualElementType != null)
                {
                    if (visualElementType.IsGenericType &&
                        visualElementType.GetGenericTypeDefinition() == typeof(BaseField<>))
                    {
                        Type baseFieldType = visualElementType.GetGenericArguments().First();
                        if (baseFieldType == rpValueType)
                        {
                            var methodInfo = typeof(ViewCreator).GetMethod(nameof(LinkBaseFieldWithReactiveProperty),
                                BindingFlags.NonPublic | BindingFlags.Static);
                            methodInfo.MakeGenericMethod(baseFieldType).Invoke(null,
                                new object[] { visualElement, reactiveProperty, labelName });
                        }
                        else
                        {
                            Debug.LogError($"Type mismatch between {visualElementType} and {rpValueType}");
                        }

                        break;
                    }

                    visualElementType = visualElementType.BaseType;
                }

                if (visualElementType == null)
                {
                    Debug.LogError($"No link found for type {rpValueType}");
                }
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
            reactiveProperty.Subscribe(_ => { },
                () => { baseField.UnregisterValueChangedCallback(SetRpValueFunction); });
            baseField.label = labelName;
        }

        private static void LinkEnumFieldWithReactiveProperty<T>(EnumField enumField,
            ReactiveProperty<T> reactiveProperty, string labelName) where T : Enum
        {
            void SetRpValueFunction(ChangeEvent<Enum> evt)
            {
                reactiveProperty.Value = (T)evt.newValue;
            }

            enumField.RegisterValueChangedCallback(SetRpValueFunction);

            reactiveProperty.Subscribe(value => enumField.value = value);
            reactiveProperty.Subscribe(_ => { },
                () => { enumField.UnregisterValueChangedCallback(SetRpValueFunction); });
            enumField.label = labelName;
        }

        public static VisualNodeSettings GetVisualSettings(IControl control, VisualNodeSettings visualNodeSettings)
        {
            if (control == null)
                return visualNodeSettings;
            if(!isInitialized)
                Initialize();
            if(viewSettingsTypes.TryGetValue(control.GetType(), out var settings))
            {
                return settings.GetSettings(visualNodeSettings);
            }
            return visualNodeSettings;
        }
    }
}