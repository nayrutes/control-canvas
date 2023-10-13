using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class NodeInspectorView : VisualElement, IView<NodeViewModel>
    {
        NodeViewModel nodeViewModel;

        CompositeDisposable disposables = new();


        TextField nameTextField;
        Label guidLabel;
        Vector2Field positionVector2Field;
        Vector2Field sizeVector2Field;

        public NodeInspectorView()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            Clear();
            var scrollView = new ScrollView() { viewDataKey = "NodeInspectorScrollView" };

            nameTextField = new TextField();
            guidLabel = new Label();
            positionVector2Field = new Vector2Field();
            sizeVector2Field = new Vector2Field();

            Add(scrollView);
        }
        
        public void SetViewModel(NodeViewModel nodeViewModel)
        {
            UnsetViewModel();
            this.nodeViewModel = nodeViewModel;
            BindViewToViewModel();
            BindViewModelToView();
        }

        public void UnsetViewModel()
        {
            UnbindViewFromViewModel();
            UnbindViewModelFromView();
            nodeViewModel = null;
        }

        private void BindViewToViewModel()
        {
            nodeViewModel.Name.Subscribe(OnNameChanged).AddTo(disposables);
            nodeViewModel.Guid.Subscribe(OnGuidChanged).AddTo(disposables);
            nodeViewModel.Position.Subscribe(OnPositionChanged).AddTo(disposables);
            nodeViewModel.Size.Subscribe(OnSizeChanged).AddTo(disposables);
        }
        
        private void UnbindViewFromViewModel()
        {
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void BindViewModelToView()
        {
            nameTextField.RegisterValueChangedCallback(OnNameTextFieldChanged);
            positionVector2Field.RegisterValueChangedCallback(OnPositionVector2FieldChanged);
            sizeVector2Field.RegisterValueChangedCallback(OnSizeVector2FieldChanged);
        }
        
        private void UnbindViewModelFromView()
        {
            nameTextField.UnregisterValueChangedCallback(OnNameTextFieldChanged);
            positionVector2Field.UnregisterValueChangedCallback(OnPositionVector2FieldChanged);
            sizeVector2Field.UnregisterValueChangedCallback(OnSizeVector2FieldChanged);
        }
        
        private void OnNameChanged(string name)
        {
            nameTextField.value = name;
        }
        
        private void OnGuidChanged(string guid)
        {
            guidLabel.text = guid;
        }
        
        private void OnPositionChanged(SerializableVector2 position)
        {
            positionVector2Field.value = position;
        }
        
        private void OnSizeChanged(SerializableVector2 size)
        {
            sizeVector2Field.value = size;
        }
        
        private void OnNameTextFieldChanged(ChangeEvent<string> evt)
        {
            nodeViewModel.Name.Value = evt.newValue;
        }
        
        private void OnPositionVector2FieldChanged(ChangeEvent<Vector2> evt)
        {
            nodeViewModel.Position.Value = evt.newValue;
        }
        
        private void OnSizeVector2FieldChanged(ChangeEvent<Vector2> evt)
        {
            nodeViewModel.Size.Value = evt.newValue;
        }
    }
}