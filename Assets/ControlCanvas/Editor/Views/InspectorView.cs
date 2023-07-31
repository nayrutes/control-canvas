using System;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Serialization;
using UniRx;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class InspectorView : VisualElement, IView<InspectorViewModel>
    {
        //private ControlCanvasSO mControlCanvasSo;
        
        //private ControlCanvasEditorWindow mControlCanvasEditorWindow;
        //private SerializedProperty selectedNodeProperty;

        private InspectorViewModel mInspectorViewModel;
        CompositeDisposable disposables = new ();
        public new class UxmlFactory : UxmlFactory<InspectorView, InspectorElement.UxmlTraits>
        {
        
        }

        public void SetViewModel(InspectorViewModel inspectorViewModel)
        {
            mInspectorViewModel = inspectorViewModel;
            Initialize();
        }
        
        private void Initialize()
        {
            mInspectorViewModel.displayType.Subscribe(OnTypeChanged).AddTo(disposables);
        }
        
        private void Terminate()
        {
            disposables.Dispose();
        }
        
        private void OnTypeChanged(Type type)
        {
            Clear();
            if (type == null)
            {
                return;
            }
            if (type == typeof(CanvasData))
            {
                
                
            }
            else if (type == typeof(VisualNodeView))
            {
                var niv = new NodeInspectorView();
                niv.SetViewModel(mInspectorViewModel.GetViewModelOfSelected<NodeViewModel>());
                Add(niv);
            }
        }
        
        // public void OnSelectionChanged(SelectedChangedArgs args)
        // {
        //     Clear();
        //     var scrollView = new ScrollView() { viewDataKey = "InspectorScrollView" };
        //     if (args.Selectables.Count == 0)
        //     {
        //         AddInspectorElement(scrollView, mControlCanvasSo);
        //     }
        //     if (args.Selectables.Count == 1)
        //     {
        //         AddInspectorElement(scrollView, args.Selectables[0]);
        //     }
        //     Add(scrollView);
        //     // else
        //     // {
        //     //     foreach (var obj in args.Selectables)
        //     //     {
        //     //         AddInspectorElement(scrollView, obj);
        //     //     }
        //     // }
        //     
        //     
        //     
        // }

        // private void AddInspectorElement(ScrollView scrollView, object selectedObject)
        // {
        //     if (selectedObject is ControlCanvasSO canvasSo)
        //     {
        //         scrollView.Add(new InspectorElement(canvasSo));
        //     }else if(selectedObject is VisualNode visualNode)
        //     {
        //         scrollView.Add(CreateNodeInspector(visualNode.nodeViewModel));
        //         
        //     }
        // }
        //
        // private VisualElement CreateNodeInspector(Node node)
        // {
        //     var container = new VisualElement();
        //     var label = new Label("Node Inspector");
        //     container.Add(label);
        //     var nameField = new TextField("Name") { value = node.Name };
        //     nameField.RegisterValueChangedCallback(evt => node.Name = evt.newValue);
        //     container.Add(nameField);
        //
        //     var imguiContainer = new IMGUIContainer();
        //     imguiContainer.onGUIHandler = () =>
        //     {
        //         if (selectedNodeProperty != null)
        //         {
        //             mControlCanvasEditorWindow.canvasObject.Update();
        //             EditorGUILayout.PropertyField(selectedNodeProperty);
        //             mControlCanvasEditorWindow.canvasObject.ApplyModifiedProperties();
        //         }
        //     };
        //     container.Add(imguiContainer);
        //     
        //     mControlCanvasSo.selectedNode = node;
        //     selectedNodeProperty = mControlCanvasEditorWindow.canvasObject.FindProperty("selectedNode");
        //     
        //     return container;
        // }
        //
        // public void SetCurrentCanvas(ControlCanvasSO mControlCanvasSo)
        // {
        //     this.mControlCanvasSo = mControlCanvasSo;
        // }
        //
        // public void SetEditorWindow(ControlCanvasEditorWindow controlCanvasEditorWindow)
        // {
        //     mControlCanvasEditorWindow = controlCanvasEditorWindow;
        // }

    }
}