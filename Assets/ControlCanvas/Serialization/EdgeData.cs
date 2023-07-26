using System;

namespace ControlCanvas.Serialization
{
    [Serializable]
    public class EdgeData
    {
        public string Guid;
        public string StartNodeGuid;
        public string EndNodeGuid;
        
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