using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public static class GenericArrayFields
    {
        private static TreeViewEntry<TValueType, TField> AddBaseFieldReadOnly<TValueType, TField>()
            where TField : BaseField<TValueType>, new()
        {
            var addBaseFieldReadOnly = new TreeViewEntry<TValueType, TField>();
            addBaseFieldReadOnly.Label = "Not-Linked TreeViewEntry";
            return addBaseFieldReadOnly;
        }

        internal static Dictionary<Type, Func<VisualElement>> FieldTypeToUIFieldWithReadonly =
            new Dictionary<Type, Func<VisualElement>>()
            {
                { typeof(int), () => AddBaseFieldReadOnly<int, IntegerField>() },
                { typeof(string), () => AddBaseFieldReadOnly<string, TextField>() },
                { typeof(float), () => AddBaseFieldReadOnly<float, FloatField>() },
                { typeof(bool), () => AddBaseFieldReadOnly<bool, Toggle>() },
                //{ typeof(Enum), () => AddBaseFieldReadOnly<Enum, EnumField>() },
            };

        public interface IExtendedBaseField<TValueType>
        {
            TValueType Value { get; set; }
            bool OnlyRead { get; set; }
            string Label { get; set; }

            public BaseField<TValueType> GetBaseField();
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

        internal class TreeViewEntry<TValueType, TField> : VisualElement, IExtendedBaseField<TValueType> 
            where TField : BaseField<TValueType>, new()
        {
            public bool OnlyRead
            {
                get { return _onlyRead; }
                set
                {
                    _onlyRead = value;
                    baseField.SetEnabled(!_onlyRead);
                    baseField.visible = !_onlyRead;
                    baseField.style.display = _onlyRead ? DisplayStyle.None : DisplayStyle.Flex;
                    labelField.SetEnabled(_onlyRead);
                    labelField.visible = _onlyRead;
                    labelField.style.display = _onlyRead ? DisplayStyle.Flex : DisplayStyle.None;
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

            public BaseField<TValueType> GetBaseField()
            {
                return baseField;
            }

            public TValueType Value
            {
                get => baseField.value;
                set => baseField.value = value;
            }

            private BaseField<TValueType> baseField;
            private Label labelField;
            private bool _onlyRead;

            public BaseField<TValueType> BaseField => baseField;

            public TreeViewEntry()
            {
                baseField = GenericSimpleFields.AddBaseField<TValueType, TField>();
                labelField = new Label();
                Add(labelField);
                Add(baseField);
                OnlyRead = false;
            }

            //public TValueType Value { get; set; }
        }

        internal static VisualElement AddArrayField(Type elementType, string s)
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
            treeView.name = s + "-TreeView";
            treeView.makeItem = () => GenericField.AddGenericField(elementType, s + "-Item", true);

            foldout.Add(treeView);
            return foldout;
        }

        internal static void LinkArrayField(Array array, string fieldName, VisualElement uiField,
            ICollection<IDisposable> disposableCollection)
        {
            //Array array = (Array)fieldObject;
            // int id = 0;
            // int depth = 0;
            // int maxDepth = 10;
            //List<TreeViewItemData<object>> treeViewData = GetTreeViewItemData(array, depth, maxDepth, ref id);
            //TODO: safety check for maxDepth
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
                    
                    //GenericField.LinkGenericField(entry, fieldObjectType, "", e);
                    GenericField.LinkGenericEntry(entry, e, disposableCollection);
                };
                treeView.Rebuild();
            }
            else
            {
                Debug.LogError($"UIField {fieldName} is not a TreeView");
            }
        }

        private static List<TreeViewItemData<Entry>> ConvertArrayToTreeView(Array array)
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
                return new TreeViewItemData<Entry>(id++, new Entry($"[{index}]", data, array.GetType().GetElementType()),
                    new List<TreeViewItemData<Entry>>());
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

                return new TreeViewItemData<Entry>(id++, new Entry($">{indices.Length - 1}", null, array.GetType().GetElementType()),
                    children);
            }
        }
    }
}