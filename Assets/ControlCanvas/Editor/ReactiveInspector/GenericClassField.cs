using System;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public static class GenericClassField
    {
        internal static void LinkClass(Entry entry, VisualElement visualElement,
            ICollection<IDisposable> disposableCollection)
        {
            object obj = entry.value;

            //visual element should be a foldout?
            if (visualElement is Foldout foldout)
            {
                ClassFieldFoldoutAdapter<object> adapter = new ClassFieldFoldoutAdapter<object>(foldout, obj);
                GenericSimpleFields.SetUpReactive(adapter, entry, disposableCollection);
                SetUpReactiveClass(adapter, entry, disposableCollection);
            }
            else
            {
                // Handle the case where the VisualElement is not compatible with IValue
                Debug.LogError($"VisualElement is not of type {typeof(Foldout)}");
            }
        }

        private static void SetUpReactiveClass(ClassFieldFoldoutAdapter<object> adapter, Entry entry,
            ICollection<IDisposable> disposableCollection)
        {
            ReactiveProperty<object> rp = entry.viewModel?.GetReactiveProperty(entry.name);
            if (rp != null)
            {
                adapter.GetFoldout().RegisterValueChangedCallback(evt =>
                {
                    //TODO use foldout to lazy load?
                });

                rp.Subscribe(obj =>
                {
                    //TODO Bug: Linked fields have to deregister
                    //Change viewModel as well?
                    //entry.viewModel = GenericViewModel.GetViewModel(obj);
                    entry.value = obj;
                    entry.intendedType = obj.GetType();

                    LinkClassSubFields(adapter, entry, disposableCollection);
                }).AddTo(disposableCollection);
            }
            else
            {
                LinkClassSubFields(adapter, entry, disposableCollection);
            }
        }

        private static void LinkClassSubFields(ClassFieldFoldoutAdapter<object> adapter, Entry entry,
            ICollection<IDisposable> disposableCollection)
        {
            Type t = entry.intendedType;
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var parentViewModel = entry.viewModel;
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
                GenericField.LinkGenericEntry(entryChild, uiFieldChild, disposableCollection);
            }
        }
    }

    public class ClassFieldFoldoutAdapter<TValueType> : IExtendedBaseField<TValueType>
    {
        private Foldout _foldout;
        private TValueType _value;
        private bool _onlyRead;

        public TValueType Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool OnlyRead
        {
            get => _onlyRead;
            set
            {
                _foldout.contentContainer.SetEnabled(value);
                _onlyRead = value;
            }
        }

        public string Label
        {
            get => _foldout.text;
            set => _foldout.text = value;
        }

        public ClassFieldFoldoutAdapter(Foldout foldout, TValueType value)
        {
            _foldout = foldout;
            _value = value;
        }

        public BaseField<TValueType> GetBaseField()
        {
            return null;
        }

        public Foldout GetFoldout()
        {
            return _foldout;
        }
    }
}