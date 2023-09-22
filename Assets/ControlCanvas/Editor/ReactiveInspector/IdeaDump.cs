using System;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public class IdeaDump
    {
        private static
            Dictionary<Type, Action<FieldInfo, object, VisualElement, Dictionary<VisualElement, IDisposable>>>
            fieldHandlers =
                new Dictionary<Type, Action<FieldInfo, object, VisualElement, Dictionary<VisualElement, IDisposable>>>
                {
                    { typeof(int), AddField<int, IntegerField> },
                    { typeof(float), AddField<float, FloatField> },
                    { typeof(bool), AddField<bool, Toggle> },
                    { typeof(string), AddField<string, TextField> },
                    // { typeof(int[]), AddArrayField<int, IntegerField> },
                    // { typeof(float[]), AddArrayField<float, FloatField> },
                    // { typeof(bool[]), AddArrayField<bool, Toggle> },
                    // { typeof(string[]), AddArrayField<string, TextField> },
                    // Add more types as needed...
                };

        private static Dictionary<Type,Type> typeToBaseFieldType = new Dictionary<Type, Type>
        {
            {typeof(int), typeof(IntegerField)},
            {typeof(float), typeof(FloatField)},
            {typeof(bool), typeof(Toggle)},
            {typeof(string), typeof(TextField)},
            // {typeof(int[]), typeof(IntegerField)},
            // {typeof(float[]), typeof(FloatField)},
            // {typeof(bool[]), typeof(Toggle)},
            // {typeof(string[]), typeof(TextField)},
        }; 

        private static Dictionary<Type,Func<object, string, VisualElement >> typeToCreateBaseFieldType = new ()
        {
            {typeof(int), AddField<int,IntegerField>},
            {typeof(float), AddField<float,FloatField>},
            {typeof(bool), AddField<bool,Toggle>},
            {typeof(string), AddField<string,TextField>},
            
        }; 
        
        // Create a dictionary to store the ReactiveProperties
        private Dictionary<VisualElement, IDisposable>
            reactiveProperties = new Dictionary<VisualElement, IDisposable>();

        [SerializeField] protected VisualTreeAsset uxmlTreeView;

        

        private VisualElement AddFieldsToRoot(object obj)
        {
            VisualElement holder = new VisualElement();

            if (obj == null)
            {
                //Debug.Log("Null object");
                holder.Add(new Label($"Null object"));
                return holder;
            }
            
            Type type = obj.GetType();
            
            if(type.IsArray)
            {
                //Debug.Log("Array type");
                holder.Add(new Label($"Array of type {type}"));
                
                Type elementType = type.GetElementType();
                if (elementType == null)
                {
                    holder.Add(new Label($"Element Type is null"));
                }
                else if (elementType.IsArray)
                {
                    //Process jaggedArrays
                    // foreach (var element in array)
                    // {
                    //     arrayElement.Add(ProcessArray(element));  // Recurse into sub-arrays
                    // }
                    holder.Add(new Label($"Jagged Array not supported"));
                }
                else
                {
                    //Process non-jagged arrays
                    return AddArrayField(obj);
                }
                
                
            }else if (type.IsPrimitive)
            {
                Debug.Log("Primitive type");
            }else if (type.IsClass)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType.IsPrimitive || field.FieldType == typeof(string))
                    {
                        holder.Add(ChooseField(field, obj));
                    }else if (field.FieldType.IsClass)
                    {
                        var foldout = new Foldout();
                        foldout.text = field.Name;
                        foldout.Add(AddFieldsToRoot(field.GetValue(obj)));
                        holder.Add(foldout);
                    }
                    else
                    {
                        Debug.Log($"Inner Type {field.FieldType} not supported");
                    }
                }
            }
            else
            {
                Debug.Log($"Type {type} not supported");
            }
            return holder;
        }

        private VisualElement ChooseField(FieldInfo field, object objectHolder)
        {
            if (typeToCreateBaseFieldType.TryGetValue(field.FieldType, out var baseFieldType))
            {
                return baseFieldType(field.GetValue(objectHolder), field.Name);
            }
            return new Label("No field type found");
        }

        public VisualElement ProcessArray(object obj)
        {
            Type type = obj.GetType();
            if (obj is Array array)
            {
                VisualElement arrayElement = new VisualElement();
                if (type.GetElementType().IsArray)
                {
                    //Process jaggedArrays
                    foreach (var element in array)
                    {
                        arrayElement.Add(ProcessArray(element));  // Recurse into sub-arrays
                    }
                }
                else
                {
                    //Process non-jagged arrays
                    return AddArrayField(obj);
                }
                
                return arrayElement;
            }
            else
            {
                // Process non-array element
                
                
                
                //Console.WriteLine(obj);
                return AddField(obj);
            }
            //
            // if (field.FieldType.IsArray)
            // {
            //     //ProcessArray(field.GetValue(obj));
            //     AddArrayField(fieldObject);
            // }
            // else
            // {
            //     AddField(fieldObject);
            // }
        }

        private static TField AddField<T, TField>(object fieldObject, string label)
            where TField : BaseField<T>, new()
        {
            //T value = (T)fieldInfo.GetValue(holderObject);
            
            //Type type = fieldObject.GetType();
            //Type type = fieldInfo.Name.GetType();
            var uiField = new TField { label = $"{label}", value = (T)fieldObject };
            return uiField;
        }
        
        
        private void OnDestroy()
        {
            // Dispose all ReactiveProperties
            // foreach (var reactiveProperty in reactiveProperties)
            // {
            //     reactiveProperty.Value.Dispose();
            // }
        }
        
        private VisualElement AddField(object fieldObject)
        {
            Type type = fieldObject.GetType();
            if (typeToCreateBaseFieldType.TryGetValue(type, out var baseFieldType))
            {
                VisualElement uiField = baseFieldType(fieldObject, "?");
                //var uiField = new TField { label = field.Name, value = value };
                
                //Not using for testing now 
                // var reactiveProperty = new ReactiveProperty<object>(fieldObject);
                // reactiveProperty.Subscribe(x => uiField.value = x);
                // uiField.RegisterValueChangedCallback(evt => reactiveProperty.Value = evt.newValue);
                //
                // // Store the ReactiveProperty in the dictionary
                // reactiveProperties[uiField] = reactiveProperty;

                return uiField;
            }
            return new Label($"No handler for fieldObject with type {type}.");
        }
        private static void AddField<T, TField>(FieldInfo field, object obj, VisualElement root,
            Dictionary<VisualElement, IDisposable> reactiveProperties)
            where TField : BaseField<T>, new()
        {
            // Get the value of the field
            T value = (T)field.GetValue(obj);

            // Create a UIElement for the field
            var uiField = new TField { label = field.Name, value = value };
            var reactiveProperty = new ReactiveProperty<T>(value);
            reactiveProperty.Subscribe(x => uiField.value = x);
            uiField.RegisterValueChangedCallback(evt => reactiveProperty.Value = evt.newValue);

            // Add the field to the root
            root.Add(uiField);

            // Store the ReactiveProperty in the dictionary
            reactiveProperties[uiField] = reactiveProperty;
        }

        
        private VisualElement AddArrayField(object fieldObject)
        {
            Type type = fieldObject.GetType();
            Type elementType = type.GetElementType();
            Array array = (Array)fieldObject;

            int id = 0;
            int depth = 0;
            int maxDepth = 10;
            List<TreeViewItemData<object>> treeViewData = GetTreeViewItemData(array, depth, maxDepth, ref id);
            
            // Foldout foldout = new Foldout
            // {
            //     text = "Header"
            // };

            var treeView = new TreeView();
            //foldout.Add(treeView);
            treeView.fixedItemHeight = 20;
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            
            treeView.SetRootItems(treeViewData);
            treeView.makeItem = () =>
            {
                // typeToBaseFieldType.TryGetValue(elementType, out var t);
                // return (BaseField<object>)t.Instantiate();
                //return AddField(null);
                return new Label("Test");
            };
            treeView.bindItem = (VisualElement element, int index) =>
            {
                if (element is BaseField<object> field)
                {
                    field.value = treeView.GetItemDataForIndex<object>(index);
                    field.label = $"[{index}]";
                }else if (element is Label label)
                {
                    label.text = $"[{index}]   {treeView.GetItemDataForIndex<object>(index)}";
                }
                else
                {
                    Debug.LogError("Element is not a BaseField<object>");
                }
            };
            treeView.Rebuild();
            return treeView;

        }
        
        
        
        
        // private int Depth(object obj, VisualElement root, FieldInfo field, int depth, int maxDepth)
        // {
        //     if (field.FieldType.IsPrimitive || fieldHandlers.ContainsKey(field.FieldType)) //Allow custom types
        //     {
        //         // Check if we have a handler for this type
        //         if (fieldHandlers.TryGetValue(field.FieldType, out var handler))
        //         {
        //             // Call the handler to add the field to the root
        //             handler(field, obj, root, reactiveProperties);
        //         }
        //         else
        //         {
        //             // If we don't have a handler, log a warning and add a label to the root
        //             Debug.LogWarning($"No handler for type {field.FieldType}.");
        //             root.Add(new Label($"No handler for field {field.Name} with type {field.FieldType}."));
        //         }
        //     }
        //     else if (field.FieldType.IsArray)
        //     {
        //         
        //         // Get the type of the elements of the array
        //         Type elementType = field.FieldType.GetElementType();
        //
        //         //Detect jagged arrays and recurse
        //         if (elementType != null && elementType.IsArray)
        //         {
        //             var arrayFieldValue = field.GetValue(obj) as Array;
        //             if (arrayFieldValue != null)
        //             {
        //                 foreach (var array in arrayFieldValue)
        //                 {
        //                     //depth = Depth(array, root, field., depth, maxDepth);
        //                 }
        //             }
        //         }
        //         //Handle MultiDimensional Arrays and normal arrays
        //         else// if (field.FieldType.GetArrayRank() > 1)
        //         {
        //             AddArrayField(field, obj, root, reactiveProperties, fieldHandlers);
        //         }
        //         
        //     }
        //     else
        //     {
        //         if (depth < maxDepth)
        //         {
        //             depth++;
        //             // If the field is not a primitive, recursively call AddFieldsToRoot
        //             AddFieldsToRoot(field.GetValue(obj), root);
        //         }
        //         else
        //         {
        //             root.Add(new Label($"Max depth reached for field {field.Name} with type {field.FieldType}."));
        //         }
        //     }
        //
        //     return depth;
        // }


        private static void AddArrayField(FieldInfo field, object o, VisualElement root, Dictionary<VisualElement, IDisposable> disposables, Dictionary<Type, Action<FieldInfo, object, VisualElement, Dictionary<VisualElement, IDisposable>>> dictionary)
        {
            int id_depth = 0;
            int id_maxDepth = 8;
            Array array = (Array)field.GetValue(o);

            if (array == null)
            {
                root.Add(new Label($"Field {field.Name} with type {field.FieldType} is null."));
                return;
            }

            int id = 0;
            List<TreeViewItemData<object>> treeViewData = GetTreeViewItemData(array, id_depth, id_maxDepth, ref id);
            //IList<TreeViewItemData<PlanetsWindow.IPlanetOrGroup>> treeViewData = PlanetsWindow.treeRoots;

            // List<TreeViewItemData<object>> rootData = new List<TreeViewItemData<object>>()
            // {
            //     new TreeViewItemData<object>(id, "Root", treeViewData)
            // };

            Foldout foldout = new Foldout
            {
                text = field.Name
            };

            var treeView = new TreeView();
            foldout.Add(treeView);
            
            // VisualElement holder = new VisualElement();
            // VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ControlCanvas/Editor/ReactiveInspector/GenericTreeView.uxml");
            // var templateContainer = visualTree.Instantiate();
            // holder.Add(templateContainer);
            // visualTree.CloneTree(holder);
            //
            //
            // TreeView treeView = holder.Q<TreeView>();


            //treeView.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ControlCanvas/Editor/ReactiveInspector/ReactiveInspectorWindow.uss"));
            treeView.fixedItemHeight = 20;
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            
            treeView.SetRootItems(treeViewData);
            treeView.makeItem = () =>
            {
                typeToBaseFieldType.TryGetValue(field.FieldType, out var type);
                return (BaseField<object>)Activator.CreateInstance(type);
                //return (BaseField<object>)type.Instantiate();
            };
            treeView.bindItem = (VisualElement element, int index) =>
            {
                if (element is BaseField<object> field)
                {
                    field.value = treeView.GetItemDataForIndex<object>(index);
                    field.label = $"[{index}]";
                }else
                {
                    Debug.LogError("Element is not a BaseField<object>");
                }
            };
            
            // treeView.makeItem = () => new TField { };
            // treeView.bindItem = (VisualElement element, int index) =>
            // {
            //     if (element is TField)
            //     {
            //         (element as TField).value = treeView.GetItemDataForIndex<T>(index);
            //         (element as TField).label = $"[{index}]";
            //     }else
            //     {
            //         Debug.LogError("Element is not a TField");
            //     }
            // };

            // treeView.makeItem = () => new Label();
            // treeView.bindItem = (VisualElement element, int index) => (element as Label).text = treeView.GetItemDataForIndex<PlanetsWindow.IPlanetOrGroup>(index).name;

            treeView.Rebuild();
            //root.Add(holder);
            root.Add(foldout);
        }

        private static void AddArrayField<T, TField>(FieldInfo field, object obj, VisualElement root,
            Dictionary<VisualElement, IDisposable> reactiveProperties)
            where TField : BaseField<T>, new()
        {
            // Convert the array and any sub arrays to a list of TreeViewItemData
            
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
    
    // // Get the array from the field
// T[] array = (T[])field.GetValue(obj);
//
// // Handle null array
// if (array == null)
// {
//     array = new T[0];
//     field.SetValue(obj, array);
// }
//
// //Create a TreeView for the array
// var treeView = new TreeView();
// treeView.SetRootItems(array);
            
            
            
// Create a ListView for the array
// var listView = new ListView(array, itemHeight: 25, makeItem: () => new TField(), bindItem: (element, i) =>
// {
//     var uiField = (TField)element;
//     uiField.label = $"[{i}]";
//     uiField.value = array[i];
//
//     var reactiveProperty = new ReactiveProperty<T>(array[i]);
//     reactiveProperty.Subscribe(x => uiField.value = x);
//     uiField.RegisterValueChangedCallback(evt =>
//     {
//         reactiveProperty.Value = evt.newValue;
//         array[i] = evt.newValue;
//     });
//
//     // Store the ReactiveProperty in the dictionary
//     reactiveProperties[uiField] = reactiveProperty;
// });
//
// listView.style.height = 150;
// listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
//
// // Add the ListView to the root
// root.Add(listView);
}