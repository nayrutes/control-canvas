using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public class ReactiveInspectorWindow : EditorWindow
    {
        private static Dictionary<Type, Func<VisualElement>> _fieldTypeToUIField = new Dictionary<Type, Func<VisualElement>>()
        {
            {typeof(int), () => AddBaseField<int, IntegerField>()},
            {typeof(string), () => AddBaseField<string, TextField>()},
            {typeof(float), () => AddBaseField<float, FloatField>()},
            {typeof(bool), () => AddBaseField<bool, Toggle>()},
        };
        
        private static Dictionary<Type, Action<string, object, VisualElement>> _fieldTypeToLinkUIField = new ()
        {
            {typeof(int), LinkBaseField<int, IntegerField>},
            {typeof(string), LinkBaseField<string, TextField>},
            {typeof(float), LinkBaseField<float, FloatField>},
            {typeof(bool), LinkBaseField<bool, Toggle>},
        };

        [MenuItem("Window/Test ReactiveInspectorWindow")]
        public static void ShowWindow()
        {
            GetWindow<ReactiveInspectorWindow>("My Custom Inspector");
        }

        private void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Create a DataContainer and add it to the root
            DataContainer dataContainer = new DataContainer();
            root.Clear();
            root.Add(GetGenericInspector(dataContainer));
        }


        private static VisualElement GetGenericInspector(object obj)
        {
            var type = obj.GetType();
            var name = type.Name;
            VisualElement visualElement = AddGenericField(type, name);
            LinkGenericField(obj, type, name, visualElement);
            return visualElement;
        }
        
        private static VisualElement AddGenericField(Type t, string name)
        {
            VisualElement element;
            if (_fieldTypeToUIField.TryGetValue(t, out var handler))
            {
                var uiField = handler();
                uiField.name = name;
                element = uiField;
            }else if (t.IsArray)
            {
                //Debug.Log("Adding Array not implemented yet");
                //element = new Label("Array not implemented yet");
                
                Type elementType = t.GetElementType();
                if (elementType == null)
                {
                    element = new Label($"Element Type is null");
                }else if (elementType.IsArray)
                {
                    element = new Label($"Jagged Array not supported");
                }
                else
                {
                    element = AddArrayField(elementType, name);
                }
                
            }
            else if(t.IsClass)
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
            }else
            {
                Debug.Log("Type not implemented yet: " + t);
                element = new Label("Type not implemented yet: " + t);
            }

            return element;
        }

        private static VisualElement AddArrayField(Type elementType, string s)
        {
            var foldout = new Foldout();
            foldout.text = s;
            foldout.name = s;
            
            var treeView = new TreeView();
            treeView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            //treeView.fixedItemHeight = 20;
            // if (elementType.IsClass)
            // {
            //     treeView.fixedItemHeight = 80;
            // }
            
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            treeView.name = s+"-TreeView";
            treeView.makeItem = () => AddGenericField(elementType, s+"-Item");
            
            foldout.Add(treeView);
            return foldout;
        }


        
        
        private static TField AddBaseField<T, TField>()
            where TField : BaseField<T>, new()
        {
            var uiField = new TField { label = "Not-Linked"};
            return uiField;
        }

        private static void LinkGenericField(object fieldObject, Type fieldObjectType, string fieldName, VisualElement uiField)
        {
            if (uiField == null)
            {
                Debug.LogError($"UIField {fieldName} is null");
                return;
            }

            Type t = fieldObjectType;//fieldObject.GetType();
            if (_fieldTypeToLinkUIField.TryGetValue(t, out var handler))
            {
                handler(fieldName, fieldObject, uiField);
            }else if (t.IsArray)
            {
                //Debug.Log("Linking Array not implemented yet");

                if (fieldObject == null)
                {
                    Debug.LogWarning($"FieldArray {fieldName} is null");
                    return;
                }
                Type elementType = t.GetElementType();
                if (elementType == null)
                {
                    Debug.LogWarning($"Linking Element Type is null");
                }else if (elementType.IsArray)
                {
                    Debug.LogWarning($"Linking Jagged Array not supported");
                }
                else
                {
                    LinkArrayField(fieldObject, elementType, fieldName, uiField.Q<TreeView>());
                }
                
            }
            else if(t.IsClass)
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
                    LinkGenericField(fieldInfo.GetValue(fieldObject), fieldInfo.FieldType, fieldInfo.Name, uiFieldChild);
                }
            }
            else
            {
                Debug.LogError($"Linking Type {t} not implemented yet");
            }
        }
        
        private static void LinkArrayField(object fieldObject, Type fieldObjectType, string fieldName,
            VisualElement uiField)
        {
            Array array = (Array)fieldObject;
            int id = 0;
            int depth = 0;
            int maxDepth = 10;
            List<TreeViewItemData<object>> treeViewData = GetTreeViewItemData(array, depth, maxDepth, ref id);

            if (uiField is TreeView treeView)
            {
                treeView.SetRootItems(treeViewData);
                
                treeView.bindItem = (VisualElement e, int index) =>
                {
                    object entry = treeView.GetItemDataForIndex<object>(index);
                    
                    LinkGenericField(entry, fieldObjectType, fieldName + index, e);
                };
                treeView.Rebuild();
            }
            else
            {
                Debug.LogError($"UIField {fieldName} is not a TreeView");
            }
        }
        
        private static void LinkBaseField<T, TField>(string label, object fieldObject, VisualElement uiField)
            where TField : BaseField<T>
        {
            if (uiField is TField field)
            {
                field.label = label;
                if (fieldObject == null)
                {
                    //Instantiate default value
                    fieldObject = default(T);
                }
                field.value = (T)fieldObject;
            }
            else
            {
                Debug.LogError($"UIField is not of type {typeof(TField)}");
            }
        }
        
        private static List<TreeViewItemData<object>> GetTreeViewItemData(Array array, int id_depth, int id_maxDepth,
            ref int id)
        {
            List<TreeViewItemData<object>> treeViewData = new List<TreeViewItemData<object>>();

            if (id_depth > id_maxDepth)
            {
                //treeViewData.Add(new TreeViewItemData<object>(id_depth, "Max Depth Reached"));
                //return treeViewData;
                return null;
            }

            foreach (var item in array)
            {
                if (item is Array subArray)
                {
                    var subTree = GetTreeViewItemData(subArray, id_depth + 1, id_maxDepth, ref id);
                    treeViewData.Add(new TreeViewItemData<object>(id++, item, subTree));
                }
                else
                {
                    treeViewData.Add(new TreeViewItemData<object>(id++, item));
                }
            }

            return treeViewData;
        }
    }

    public class DataContainer
    {
        public int testInt;
        public string testString;
        public float testFloat;
        public bool testBool;
        public int[] testIntArray;
        public string[] testStringArray = new[] { "a", "b", "c" };
        public float[] testFloatArray;
        public bool[] testBoolArray = new[] { true, false, true, false, true, false };
        // public int[,] testInt2DArray = new int[2, 2] { { 1, 2 }, { 3, 4 } };
        // public int[][] testIntJaggedArray = new int[2][] { new int[2] { 1, 2 }, new int[2] { 3, 4 } };
        // public string[][] testStringJaggedArray = new string[2][] { new string[2] { "a", "b" }, new string[2] { "c", "d" } };

        public DataContainer2 testContainer2 = new DataContainer2()
        {
            testInt2 = 2
        };
        
        public DataContainer2[] testContainer2Array = new DataContainer2[2]
        {
            new DataContainer2()
            {
                testInt2 = 3
            },
            new DataContainer2()
            {
                testInt2 = 4
            }
        };
    }

    public class DataContainer2
    {
        public int testInt2;
    }
}