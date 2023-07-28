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
            Entry entry = new Entry(name, obj, type);
            LinkGenericEntry(entry, visualElement);
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

        internal static void LinkGenericEntry(Entry entry, VisualElement visualElement)
        {
            string name = entry.name;
            object obj = entry.value;
            // if (obj == null)
            // {
            //     Debug.LogWarning($"Obj {name} is null");
            //     return;
            // }

            Type t = entry.intendedType;
            if (GenericSimpleFields.FieldTypeToLinkUIField.TryGetValue(t, out var handler))
            {
                handler(name, obj, visualElement);
            }else if (t.IsArray)
            {
                LinkArray(entry, visualElement);
            }
            else if (t.IsClass)
            {
                LinkClass(visualElement, t, obj);
            }else if(t.IsEnum)
            {
                GenericSimpleFields.LinkEnumField(obj, t, name, visualElement);
            }
            else
            {
                Debug.LogError($"Linking Type {t} not implemented yet");
            }
            
        }

        private static void LinkClass(VisualElement visualElement, Type t, object obj)
        {
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields)
            {
                var uiFieldChild = visualElement.Q(fieldInfo.Name);
                object objectValue = fieldInfo.GetValue(obj);
                if (obj is Entry innerEntry)
                {
                    Debug.LogError("Inner Entry");
                    return;
                }

                Entry entryChild = new Entry(fieldInfo.Name, objectValue, fieldInfo.FieldType);
                LinkGenericEntry(entryChild, uiFieldChild);
                
                // if (fieldInfo.FieldType.IsClass)
                // {
                //     Entry entryChild = new Entry(fieldInfo.Name, objectValue);
                //     LinkGenericEntry(entryChild, uiFieldChild);
                // }
                // else
                // {
                //     LinkGenericEntry(objectValue, fieldInfo, uiFieldChild);
                // }


                // if (obj is Entry entry)
                // {
                //     var oldValue = entry.value;
                //     entry.value = fieldInfo.GetValue(oldValue);
                //     LinkGenericField(entry, fieldInfo.FieldType, fieldInfo.Name, uiFieldChild);
                // }
                // else
                // {
                //     LinkGenericField(fieldInfo.GetValue(obj), fieldInfo.FieldType, fieldInfo.Name,
                //         uiFieldChild);
                // }
            }
        }

        // internal static void LinkGenericField(object fieldObject, FieldInfo fieldInfo, VisualElement uiField)
        // {
        //     if (uiField == null)
        //     {
        //         Debug.LogWarning($"UIField for {fieldInfo.Name} is null");
        //         return;
        //     }
        //
        //     Type t = fieldObjectType;
        //     if (GenericSimpleFields.FieldTypeToLinkUIField.TryGetValue(t, out var handler))
        //     {
        //         handler(fieldName, fieldObject, uiField);
        //     }
        //     else if (t.IsArray)
        //     {
        //         LinkArray(fieldObject, uiField, t);
        //     }
        //     else if (t.IsClass)
        //     {
        //         Debug.LogError("Object linking - wrong method called");
        //     }else if(t.IsEnum)
        //     {
        //         GenericSimpleFields.LinkEnumField(fieldObject, t, fieldName, uiField);
        //     }
        //     else
        //     {
        //         Debug.LogError($"Linking Type {t} not implemented yet");
        //     }
        // }

        private static void LinkArray(Entry entry, VisualElement uiField)
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
                GenericArrayFields.LinkArrayField(array, entry.name, uiField.Q<TreeView>());
            }
        }
    }

    public struct Entry
    {
        public string name;
        public object value;
        public Type intendedType;

        public Entry(string name, object value, Type intendedType)
        {
            this.name = name;
            this.value = value;
            this.intendedType = intendedType;
        }
    }
}