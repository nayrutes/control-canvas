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
            else if(t.IsEnum)
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
            }else if (t.IsArray)
            {
                LinkArray(entry, visualElement, disposableCollection);
            }
            else if (t.IsClass)
            {
                LinkClass(entry, visualElement, disposableCollection);
            }else if(t.IsEnum)
            {
                GenericSimpleFields.LinkEnumField(entry, visualElement, disposableCollection);
            }
            else
            {
                Debug.LogError($"Linking Type {t} not implemented yet");
            }
            
        }

        private static void LinkClass(Entry entry, VisualElement visualElement, ICollection<IDisposable> disposableCollection)
        {
            object obj = entry.value;
            
            //visual element should be a foldout?
            if (visualElement is Foldout foldout)
            {
                GenericArrayFields.ClassFieldFoldoutAdapter<object> adapter = new GenericArrayFields.ClassFieldFoldoutAdapter<object>(foldout, obj);
                GenericSimpleFields.SetUpReactive(adapter, entry, disposableCollection);
                SetUpReactiveClass(adapter, entry, disposableCollection);
                
            }else
            {
                // Handle the case where the VisualElement is not compatible with IValue
                Debug.LogError($"VisualElement is not of type {typeof(Foldout)}");
            }
            
        }

        private static void SetUpReactiveClass(GenericArrayFields.ClassFieldFoldoutAdapter<object> adapter, Entry entry, ICollection<IDisposable> disposableCollection)
        {
            ReactiveProperty<object> rp = entry.reactiveLink?.viewModel.GetReactiveProperty(entry.name);
            if (rp != null)
            {
                adapter.GetFoldout().RegisterValueChangedCallback(evt =>
                {
                    //TODO use foldout to lazy load?
                });
                
                rp.Subscribe(obj =>
                {
                    //TODO Bug: don't depend on the method parameters
                    //TODO Test if it working now
                    entry.value = obj;
                    
                    LinkClassSubFields(adapter, entry, disposableCollection);
                }).AddTo(disposableCollection);
            }
            else
            {
                LinkClassSubFields(adapter, entry, disposableCollection);
            }
        }
        
        private static void LinkClassSubFields(GenericArrayFields.ClassFieldFoldoutAdapter<object> adapter, Entry entry, ICollection<IDisposable> disposableCollection)
        {
            Type t = entry.intendedType;
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var parentViewModel = entry.reactiveLink?.viewModel;
            object obj = entry.value;
            
            var viewModel = GenericViewModel.GetViewModel(obj);
            //parentViewModel?.SetChildViewModel(entry.name, viewModel);//Is this even needed?
            
            foreach (FieldInfo fieldInfo in fields)
            {
                var uiFieldChild = adapter.GetFoldout().Q(fieldInfo.Name);
                object objectValue = fieldInfo.GetValue(obj);
                if (obj is Entry innerEntry)
                {
                    Debug.LogError("Inner Entry");
                    return;
                }

                Entry entryChild = new Entry(fieldInfo.Name, objectValue, fieldInfo.FieldType, viewModel);
                LinkGenericEntry(entryChild, uiFieldChild, disposableCollection);
                
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
            Array array = (Array) fieldObject;
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
        public ReactiveLink? reactiveLink;
        
        public Entry(string name, object value, Type intendedType, ReactiveLink link = default)
        {
            this.name = name;
            this.value = value;
            this.intendedType = intendedType;
            reactiveLink = null;
        }
        public Entry(string name, object value, Type intendedType, GenericViewModel viewModel)
        {
            this.name = name;
            this.value = value;
            this.intendedType = intendedType;
            reactiveLink = new ReactiveLink(){viewModel = viewModel};
        }
    }

    public struct ReactiveLink
    {
        public GenericViewModel viewModel;
        public ReactiveCollection<object> collection;
        public ReactiveProperty<object> property;
    }
}