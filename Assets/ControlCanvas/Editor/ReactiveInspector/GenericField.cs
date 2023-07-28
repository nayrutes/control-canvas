using System;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public static class GenericField
    {
        public static VisualElement CreateGenericInspector(object obj, ICollection<IDisposable> disposableCollection)
        {
            var type = obj.GetType();
            var name = type.Name;
            VisualElement visualElement = AddGenericField(type, name);
            Entry entry = new Entry(name, obj, type, GenericViewModel.GetViewModel(obj));
            LinkGenericEntry(entry, visualElement, disposableCollection);
            return visualElement;
        }

        internal static VisualElement AddGenericField(Type t, string name, bool onlyRead = false)
        {
            VisualElement element;
            if (!onlyRead && GenericSimpleFields.FieldTypeToUIField.TryGetValue(t, out var handler))
            {
                var uiField = handler();
                uiField.name = name;
                element = uiField;
            }
            else if (onlyRead && GenericArrayFields.FieldTypeToUIFieldWithReadonly.TryGetValue(t, out var handler2))
            {
                var uiField = handler2();
                uiField.name = name;
                element = uiField;
            }
            else if (t.IsArray)
            {
                Type elementType = t.GetElementType();
                if (elementType == null)
                {
                    element = new Label($"Element Type is null");
                }
                else if (elementType.IsArray)
                {
                    element = new Label($"Jagged Array not supported");
                }
                else
                {
                    element = GenericArrayFields.AddArrayField(elementType, name);
                }
            }
            else if (t.IsClass)
            {
                var foldout = new Foldout();
                foldout.text = name;
                foldout.name = name;

                FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var uiField = AddGenericField(field.FieldType, field.Name);
                    foldout.Add(uiField);
                }

                element = foldout;
            }
            else if (t.IsEnum)
            {
                element = GenericSimpleFields.AddEnumField(t, name);
            }
            else
            {
                Debug.Log("Type not implemented yet: " + t);
                element = new Label("Type not implemented yet: " + t);
            }

            return element;
        }

        internal static void LinkGenericEntry(Entry entry, VisualElement visualElement,
            ICollection<IDisposable> disposableCollection)
        {
            object obj = entry.value;
            // if (obj == null)
            // {
            //     Debug.LogWarning($"Obj {name} is null");
            //     return;
            // }

            Type t = entry.intendedType;
            if (GenericSimpleFields.FieldTypeToLinkUIField.TryGetValue(t, out var handler))
            {
                handler(entry, visualElement, disposableCollection);
            }
            else if (t.IsArray)
            {
                LinkArray(entry, visualElement, disposableCollection);
            }
            else if (t.IsClass)
            {
                GenericClassField.LinkClass(entry, visualElement, disposableCollection);
            }
            else if (t.IsEnum)
            {
                GenericSimpleFields.LinkEnumField(entry, visualElement, disposableCollection);
            }
            else
            {
                Debug.LogError($"Linking Type {t} not implemented yet");
            }
        }


        private static void LinkArray(Entry entry, VisualElement uiField, ICollection<IDisposable> disposableCollection)
        {
            object fieldObject = entry.value;
            if (fieldObject == null)
            {
                Debug.LogWarning($"FieldArray {entry.name} is null");
                return;
            }

            Array array = (Array)fieldObject;
            Type elementType = array.GetType().GetElementType();
            if (elementType == null)
            {
                Debug.LogWarning($"Linking Element Type is null");
            }
            else if (elementType.IsArray)
            {
                Debug.LogWarning($"Linking Jagged Array not supported");
            }
            else
            {
                GenericArrayFields.LinkArrayField(array, entry.name, uiField.Q<TreeView>(), disposableCollection);
            }
        }
    }

    public struct Entry
    {
        public string name;
        public object value;
        public Type intendedType;

        public GenericViewModel viewModel;
        //public ReactiveLink? reactiveLink;

        public Entry(string name, object value, Type intendedType)
        {
            this.name = name;
            this.value = value;
            this.intendedType = intendedType;
            this.viewModel = null;
            //reactiveLink = null;
        }

        public Entry(string name, object value, Type intendedType, GenericViewModel viewModel)
        {
            this.name = name;
            this.value = value;
            this.intendedType = intendedType;
            this.viewModel = viewModel;
            //reactiveLink = new ReactiveLink(){viewModel = viewModel};
        }
    }


    public interface IExtendedBaseField<TValueType>
    {
        TValueType Value { get; set; }
        bool OnlyRead { get; set; }
        string Label { get; set; }

        public BaseField<TValueType> GetBaseField();
    }

    public struct ReactiveLink
    {
        public GenericViewModel viewModel;
        public ReactiveCollection<object> collection;
        public ReactiveProperty<object> property;
    }
}