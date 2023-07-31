using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public static class GenericSimpleFields
    {
        internal static Dictionary<Type, Func<VisualElement>> FieldTypeToUIField =
            new Dictionary<Type, Func<VisualElement>>()
            {
                { typeof(int), () => AddBaseField<int, IntegerField>() },
                { typeof(string), () => AddBaseField<string, TextField>() },
                { typeof(float), () => AddBaseField<float, FloatField>() },
                { typeof(bool), () => AddBaseField<bool, Toggle>() },
                //{ typeof(Enum), () => AddBaseField<Enum, EnumField>() },
            };


        internal static Dictionary<Type, Action<Entry, VisualElement, ICollection<IDisposable>>>
            FieldTypeToLinkUIField = new()
            {
                {
                    typeof(int),
                    (entry, visualElement, disposableCollection) =>
                        LinkBaseField<int, IntegerField>(entry, visualElement, disposableCollection)
                },
                {
                    typeof(string),
                    (entry, visualElement, disposableCollection) =>
                        LinkBaseField<string, TextField>(entry, visualElement, disposableCollection)
                },
                {
                    typeof(float),
                    (entry, visualElement, disposableCollection) =>
                        LinkBaseField<float, FloatField>(entry, visualElement, disposableCollection)
                },
                {
                    typeof(bool),
                    (entry, visualElement, disposableCollection) =>
                        LinkBaseField<bool, Toggle>(entry, visualElement, disposableCollection)
                },
                //{ typeof(Enum), LinkBaseField<Enum, EnumField> },
            };

        internal static TField AddBaseField<T, TField>()
            where TField : BaseField<T>, new()
        {
            var uiField = new TField { label = "Not-Linked" };
            return uiField;
        }

        private static void LinkBaseField<T, TField>(Entry entry, VisualElement visualElement,
            ICollection<IDisposable> disposableCollection)
            where TField : BaseField<T>, new()
        {
            if (visualElement is IExtendedBaseField<T> valueElement)
            {
                // Now you can use valueElement as IValue<TValueType>
                SetUpReactive(valueElement, entry, disposableCollection);
            }
            else if (visualElement is BaseField<T> baseField)
            {
                // If it's a BaseField, wrap it in an adapter
                IExtendedBaseField<T> adapter = new BaseFieldAdapter<T>(baseField);
                SetUpReactive(adapter, entry, disposableCollection);
            }
            else
            {
                // Handle the case where the VisualElement is not compatible with IValue
                Debug.LogError($"UIField is not of type {typeof(TField)}");
            }
        }

        public static void SetUpReactive<T>(IExtendedBaseField<T> adapter, Entry entry,
            ICollection<IDisposable> disposableCollection)
        {
            adapter.Label = entry.name;
            ReactiveProperty<object> rp = entry.viewModel?.GetReactiveProperty(entry.name);

            //Handle viewmodel existing (two way binding)
            if (rp != null)
            {
                void NewFunction(ChangeEvent<T> evt)
                {
                    rp.Value = evt.newValue;
                }

                adapter.GetBaseField().RegisterValueChangedCallback(NewFunction);
                adapter.Value = (T)rp.Value;

                rp.Subscribe(value => { adapter.Value = (T)value; }).AddTo(disposableCollection);
                
                rp.Subscribe(_ => { }, () =>
                {
                    adapter.GetBaseField().UnregisterValueChangedCallback(NewFunction);
                }).AddTo(disposableCollection);
            }
            //Handle no viewmodel
            else
            {
                //Read only on Reload View
                adapter.GetBaseField()?.SetEnabled(false);
                object someValue = entry.value;
                if (someValue == null)
                {
                    //Instantiate default value
                    someValue = default(T);
                }

                Debug.Log($"Setting value {someValue} to {adapter.Label}");
                adapter.Value = (T)someValue;

                //TODO: One way binding (writing to data container)
            }
        }

        public static VisualElement AddEnumField(Type type, string name)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type must be an enum", nameof(type));
            }

            var enumValues = Enum.GetValues(type);
            Enum defaultEnumValue = enumValues.Length > 0 ? (Enum)enumValues.GetValue(0) : null;

            var uiField = new EnumField(name, defaultEnumValue);
            uiField.name = name;
            uiField.Init(defaultEnumValue);
            return uiField;
        }


        // public static VisualElement AddEnumField<T>(string name) where T : Enum
        // {
        //     var uiField = new EnumField(name, default(T));
        //     return uiField;
        // }

        public static void LinkEnumField(Entry entry, VisualElement uiField,
            ICollection<IDisposable> disposableCollection)
        {
            var obj = entry.value;
            var name = entry.name;
            var type = obj.GetType();

            if (!type.IsEnum)
            {
                throw new ArgumentException("Type must be an enum", nameof(type));
            }

            if (uiField is IExtendedBaseField<Enum> valueElement)
            {
                // Now you can use valueElement as IValue<TValueType>
                ((EnumField)valueElement.GetBaseField()).Init((Enum)obj);
                SetUpReactive(valueElement, entry, disposableCollection);
            }
            else if (uiField is EnumField enumField)
            {
                // If it's a BaseField, wrap it in an adapter
                enumField.Init((Enum)obj);
                IExtendedBaseField<Enum> adapter = new BaseFieldAdapter<Enum>(enumField);
                SetUpReactive(adapter, entry, disposableCollection);
            }
            else
            {
                // Handle the case where the VisualElement is not compatible with IValue
                Debug.LogError($"UIField {type} is not of type {typeof(EnumField)}");
            }


            // if (uiField is EnumField field)
            // {
            //     field.Init((Enum)obj);
            // }
            // else if (uiField is GenericArrayFields.TreeViewEntry<Enum, EnumField> fieldRo)
            // {
            //     fieldRo.Label = name;
            //     fieldRo.Value = (Enum)obj;
            // }
            // else
            // {
            //     Debug.LogError($"UIField is not of type {typeof(EnumField)}");
            // }
        }
    }

    public class BaseFieldAdapter<TValueType> : IExtendedBaseField<TValueType>
    {
        private BaseField<TValueType> _baseField;

        public bool OnlyRead { get; set; }

        public string Label
        {
            get { return _baseField.label; }
            set { _baseField.label = value; }
        }

        public BaseField<TValueType> GetBaseField()
        {
            return _baseField;
        }

        public BaseFieldAdapter(BaseField<TValueType> baseField)
        {
            _baseField = baseField;
        }

        public TValueType Value
        {
            get { return _baseField.value; }
            set { _baseField.value = value; }
        }
    }
}