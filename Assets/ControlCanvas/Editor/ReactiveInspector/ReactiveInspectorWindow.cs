using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Dictionary<Type, Func<VisualElement>> _fieldTypeToUIFieldWithReadonly = new Dictionary<Type, Func<VisualElement>>()
        {
            {typeof(int), () => AddBaseFieldReadOnly<int, IntegerField>()},
            {typeof(string), () => AddBaseFieldReadOnly<string, TextField>()},
            {typeof(float), () => AddBaseFieldReadOnly<float, FloatField>()},
            {typeof(bool), () => AddBaseFieldReadOnly<bool, Toggle>()},
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
            root.Add(CreateGenericInspector(dataContainer));
        }


        private static VisualElement CreateGenericInspector(object obj)
        {
            var type = obj.GetType();
            var name = type.Name;
            VisualElement visualElement = AddGenericField(type, name);
            LinkGenericField(obj, type, name, visualElement);
            return visualElement;
        }
        
        private static VisualElement AddGenericField(Type t, string name, bool onlyRead = false)
        {
            VisualElement element;
            if (!onlyRead && _fieldTypeToUIField.TryGetValue(t, out var handler))
            {
                var uiField = handler();
                uiField.name = name;
                element = uiField;
            }else if(onlyRead && _fieldTypeToUIFieldWithReadonly.TryGetValue(t, out var handler2))
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
            // if (elementType.IsPrimitive || elementType == typeof(string))
            // {
            //     treeView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            //     treeView.fixedItemHeight = 20;
            // }
            // else
            {
                treeView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            }
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            treeView.name = s+"-TreeView";
            treeView.makeItem = () => AddGenericField(elementType, s+"-Item", true);
            
            foldout.Add(treeView);
            return foldout;
        }

        private static TField AddBaseField<T, TField>()
            where TField : BaseField<T>, new()
        {
            var uiField = new TField { label = "Not-Linked"};
            return uiField;
        }


        private static TreeViewEntry<TValueType, TField> AddBaseFieldReadOnly<TValueType, TField>() where TField : BaseField<TValueType>, new()
        {
            var addBaseFieldReadOnly = new TreeViewEntry<TValueType, TField>();
            addBaseFieldReadOnly.Label = "Not-Linked TreeViewEntry";
            return addBaseFieldReadOnly;
        }
        
        public class TreeViewEntry<TValueType,TField> : VisualElement where TField : BaseField<TValueType>, new()
        {
            public bool OnlyRead
            {
                get
                {
                    return _onlyRead;
                }
                set
                {
                    _onlyRead = value;
                    baseField.SetEnabled(!_onlyRead);
                    baseField.visible = !_onlyRead;
                    baseField.style.display =  _onlyRead ? DisplayStyle.None : DisplayStyle.Flex;
                    labelField.SetEnabled(_onlyRead);
                    labelField.visible = _onlyRead;
                    labelField.style.display =  _onlyRead ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            public string Label
            {
                get => labelField.text;
                set
                {
                    labelField.text = value;
                    baseField.label = value;
                }
            }

            public TValueType Value
            {
                get => baseField.value;
                set => baseField.value = value;
            }

            private BaseField<TValueType> baseField;
            private Label labelField;
            private bool _onlyRead;

            public TreeViewEntry()
            {
                baseField = AddBaseField<TValueType,TField>();
                labelField = new Label();
                Add(labelField);
                Add(baseField);
                OnlyRead = false;
            }
        }
        
        private static void LinkGenericField(object fieldObject, Type fieldObjectType, string fieldName, VisualElement uiField)
        {
            if (uiField == null)
            {
                Debug.LogWarning($"UIField {fieldName} is null");
                return;
            }

            Type t = fieldObjectType;
            if (_fieldTypeToLinkUIField.TryGetValue(t, out var handler))
            {
                handler(fieldName, fieldObject, uiField);
            }else if (t.IsArray)
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
                //TODO this is hacky and wrong
                // if (fieldObject is Entry entry)
                // {
                //     fieldObject = entry.value;
                // }
                
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
                        var oldValue = entry.value;
                        entry.value = fieldInfo.GetValue(oldValue);
                        LinkGenericField(entry, fieldInfo.FieldType, fieldInfo.Name, uiFieldChild);
                    }
                    else
                    {
                        LinkGenericField(fieldInfo.GetValue(fieldObject), fieldInfo.FieldType, fieldInfo.Name, uiFieldChild);                        
                    }
                    
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
            //List<TreeViewItemData<object>> treeViewData = GetTreeViewItemData(array, depth, maxDepth, ref id);
            List<TreeViewItemData<Entry>> treeViewData = ConvertArrayToTreeView(array);

            if (uiField is TreeView treeView)
            {
                treeView.SetRootItems(treeViewData);
                
                treeView.bindItem = (VisualElement e, int index) =>
                {
                    Entry entry = treeView.GetItemDataForIndex<Entry>(index);
                    //string name = entry.name;
                    // if (entry.value == null)
                    // {
                    //     name = $"Found null value at index {index}";
                    // }
                    LinkGenericField(entry, fieldObjectType, "", e);
                };
                treeView.Rebuild();
            }
            else
            {
                Debug.LogError($"UIField {fieldName} is not a TreeView");
            }
        }
        
        private static void LinkBaseField<T, TField>(string label, object fieldObject, VisualElement uiField)
            where TField : BaseField<T>, new()
        {
            if(fieldObject is Entry entry)
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
            }else if (uiField is TreeViewEntry<T, TField> fieldRo)
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

        public struct Entry
        {
            public string name;
            public object value;
        }
        
        public static List<TreeViewItemData<Entry>> ConvertArrayToTreeView(Array array)
        {
            int id = 0;
            return ConvertArrayToTreeViewRecursive(array, new int[0], ref id, 0).children.ToList();
        }

        private static TreeViewItemData<Entry> ConvertArrayToTreeViewRecursive(Array array, int[] indices, ref int id,
            int index)
        {
            if (indices.Length == array.Rank)
            {
                
                // We've reached a leaf node.
                object data = array.GetValue(indices);
                return new TreeViewItemData<Entry>(id++, new Entry(){name=$"[{index}]", value = data}, new List<TreeViewItemData<Entry>>());
            }
            else
            {
                // We're at an internal node. Generate the children.
                List<TreeViewItemData<Entry>> children = new List<TreeViewItemData<Entry>>();
                for (int i = 0; i < array.GetLength(indices.Length); i++)
                {
                    int[] newIndices = new int[indices.Length + 1];
                    Array.Copy(indices, newIndices, indices.Length);
                    newIndices[newIndices.Length - 1] = i;
                    children.Add(ConvertArrayToTreeViewRecursive(array, newIndices, ref id, i));
                }
                return new TreeViewItemData<Entry>(id++, new Entry(){name=$">{indices.Length-1}", value = null}, children);
            }
        }

        
        public static TreeViewItemData<object> ConvertArrayToTreeViewInefficient(Array array)
        {
            TreeViewItemData<object> root = new TreeViewItemData<object>(0, null, new List<TreeViewItemData<object>>());
            List<TreeViewItemData<object>> currentNodes = new List<TreeViewItemData<object>>() { root };

            int rank = array.Rank;
            int arrayLength = array.Length;
            int[] indices = new int[rank];
            for (int i = 0; i < arrayLength; i++)
            {
                object data = array.GetValue(indices);
                TreeViewItemData<object> newNode = new TreeViewItemData<object>(indices[indices.Length - 1], data, new List<TreeViewItemData<object>>());

                List<TreeViewItemData<object>> newChildren = new List<TreeViewItemData<object>>(currentNodes[indices.Length - 1].children);
                newChildren.Add(newNode);
                TreeViewItemData<object> newParent = new TreeViewItemData<object>(currentNodes[indices.Length - 1].id, currentNodes[indices.Length - 1].data, newChildren);

                if (indices.Length < currentNodes.Count)
                {
                    currentNodes[indices.Length] = newNode;
                    currentNodes[indices.Length - 1] = newParent;
                }
                else
                {
                    currentNodes.Add(newNode);
                }

                for (int j = rank - 1; j >= 0; j--)
                {
                    indices[j]++;
                    if (indices[j] < array.GetLength(j))
                    {
                        break;
                    }

                    indices[j] = 0;
                    if (j > 0)
                    {
                        currentNodes[j] = currentNodes[j - 1];
                    }
                }
            }

            return root;
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
        public int[,] testInt2DArray = new int[3, 3] { { 1, 2, 12 }, { 3, 4, 34 }, { 5, 6, 56 } };
        public int[,,] testInt3DArray = new int[2, 2, 2] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };
        
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