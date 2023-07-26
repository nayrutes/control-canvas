using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public static class GenericField
    {
        public static VisualElement CreateGenericInspector(object obj)
        {
            var type = obj.GetType();
            var name = type.Name;
            VisualElement visualElement = AddGenericField(type, name);
            LinkGenericField(obj, type, name, visualElement);
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

        internal static void LinkGenericField(object fieldObject, Type fieldObjectType, string fieldName,
            VisualElement uiField)
        {
            if (uiField == null)
            {
                Debug.LogWarning($"UIField {fieldName} is null");
                return;
            }

            Type t = fieldObjectType;
            if (GenericSimpleFields.FieldTypeToLinkUIField.TryGetValue(t, out var handler))
            {
                handler(fieldName, fieldObject, uiField);
            }
            else if (t.IsArray)
            {
                if (fieldObject == null)
                {
                    Debug.LogWarning($"FieldArray {fieldName} is null");
                    return;
                }

                Type elementType = t.GetElementType();
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
                    GenericArrayFields.LinkArrayField(fieldObject, elementType, fieldName, uiField.Q<TreeView>());
                }
            }
            else if (t.IsClass)
            {
                
                
                if (fieldObject == null)
                {
                    Debug.LogWarning($"Field {fieldName} is null");
                    return;
                }

                FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo fieldInfo in fields)
                {
                    var uiFieldChild = uiField.Q(fieldInfo.Name);
                    if (fieldObject is Entry entry)
                    {
                        // if(t.IsEnum)
                        // {
                        //     GenericSimpleFields.LinkEnumField(fieldObject, t, fieldName, uiField);
                        // }
                        
                        var oldValue = entry.value;
                        entry.value = fieldInfo.GetValue(oldValue);
                        LinkGenericField(entry, fieldInfo.FieldType, fieldInfo.Name, uiFieldChild);
                    }
                    else
                    {
                        LinkGenericField(fieldInfo.GetValue(fieldObject), fieldInfo.FieldType, fieldInfo.Name,
                            uiFieldChild);
                    }
                }
            }else if(t.IsEnum)
            {
                GenericSimpleFields.LinkEnumField(fieldObject, t, fieldName, uiField);
            }
            else
            {
                Debug.LogError($"Linking Type {t} not implemented yet");
            }
        }
    }

    public struct Entry
    {
        public string name;
        public object value;
    }
}