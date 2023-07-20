using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor
{
    public class InspectorView : VisualElement
    {
        private ControlCanvasSO mControlCanvasSo;

        public new class UxmlFactory : UxmlFactory<InspectorView, InspectorElement.UxmlTraits>
        {
        
        }

        public void OnSelectionChanged(SelectedChangedArgs args)
        {
            Clear();
            var scrollView = new ScrollView() { viewDataKey = "InspectorScrollView" };
            if (args.Selectables.Count == 0)
            {
                AddInspectorElement(scrollView, mControlCanvasSo);
            }
            if (args.Selectables.Count == 1)
            {
                AddInspectorElement(scrollView, args.Selectables[0]);
            }
            Add(scrollView);
            // else
            // {
            //     foreach (var obj in args.Selectables)
            //     {
            //         AddInspectorElement(scrollView, obj);
            //     }
            // }
            
            
        }

        private void AddInspectorElement(ScrollView scrollView, object selectedObject)
        {
            if (selectedObject is ControlCanvasSO canvasSo)
            {
                scrollView.Add(new InspectorElement(canvasSo));
            }else if(selectedObject is VisualNode visualNode)
            {
                scrollView.Add(CreateNodeInspector(visualNode.node));
            }
        }

        private VisualElement CreateNodeInspector(Node node)
        {
            var container = new VisualElement();
            var label = new Label("Node Inspector");
            container.Add(label);
            var nameField = new TextField("Name") { value = node.Name };
            nameField.RegisterValueChangedCallback(evt => node.Name = evt.newValue);
            container.Add(nameField);
            return container;
        }

        public void SetCurrentCanvas(ControlCanvasSO mControlCanvasSo)
        {
            this.mControlCanvasSo = mControlCanvasSo;
        }
    }
}