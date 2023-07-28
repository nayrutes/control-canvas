using System;
using System.Collections.Generic;
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


        internal static Dictionary<Type, Action<string, object, VisualElement>> FieldTypeToLinkUIField = new()
        {
            { typeof(int), LinkBaseField<int, IntegerField> },
            { typeof(string), LinkBaseField<string, TextField> },
            { typeof(float), LinkBaseField<float, FloatField> },
            { typeof(bool), LinkBaseField<bool, Toggle> },
            //{ typeof(Enum), LinkBaseField<Enum, EnumField> },
        };

        internal static TField AddBaseField<T, TField>()
            where TField : BaseField<T>, new()
        {
            var uiField = new TField { label = "Not-Linked" };
            return uiField;
        }

        private static void LinkBaseField<T, TField>(string label, object fieldObject, VisualElement uiField)
            where TField : BaseField<T>, new()
        {
            if (fieldObject is Entry entry)
            {
                fieldObject = entry.value;
                label = entry.name + " " + label;
            }

            if (uiField is BaseField<T> field)
            {
                field.label = label;
                if (fieldObject == null)
                {
                    //Instantiate default value
                    fieldObject = default(T);
                }

                field.value = (T)fieldObject;
            }
            else if (uiField is GenericArrayFields.TreeViewEntry<T, TField> fieldRo)
            {
                fieldRo.Label = label;


                if (fieldObject == null)
                {
                    fieldRo.OnlyRead = true;
                    fieldRo.Value = default(T);
                }
                else
                {
                    fieldRo.OnlyRead = false;
                    fieldRo.Value = (T)fieldObject;
                }
            }
            else
            {
                Debug.LogError($"UIField is not of type {typeof(TField)}");
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

        
        public static VisualElement AddEnumField<T>(string name) where T : Enum
        {
            var uiField = new EnumField(name, default(T));
            return uiField;
        }

        public static void LinkEnumField(object fieldObject, Type type, string fieldName, VisualElement uiField)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type must be an enum", nameof(type));
            }

            if (uiField is EnumField field)
            {
                field.Init((Enum)fieldObject);
            }
            else if (uiField is GenericArrayFields.TreeViewEntry<Enum, EnumField> fieldRo)
            {
                fieldRo.Label = fieldName;
                fieldRo.Value = (Enum)fieldObject;
            }
            else
            {
                Debug.LogError($"UIField is not of type {typeof(EnumField)}");
            }
        }
    }
}