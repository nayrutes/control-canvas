using System;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    [CustomView(dataType:typeof(BlackboardVariable<>), viewModelType:typeof(BlackboardVariableVm<>))]
    public class BlackboardVariableView<T> : VisualElement, IView<BlackboardVariableVm<T>>
    {
        public void SetViewModel(BlackboardVariableVm<T> viewModel)
        {
            string blackboardTypeName = nameof(BlackboardVariable<T>.blackboardType);
            var reactiveProperty = viewModel.GetReactiveProperty<ReactiveProperty<Type>>(blackboardTypeName);
            Add(ViewCreator.CreateLinkedDropDownField<Type>(reactiveProperty, blackboardTypeName, BlackboardManager.GetBlackboardTypeChoices()));
            VisualElement content = new();
            reactiveProperty.Where(x=>x!=null).Subscribe(x =>
            {
                SetReferenceContent(content, viewModel, x);
            });
            Add(content);
            //this.Add(ViewCreator.CreateLinkedGenericField(viewModel, nameof(BlackboardVariable<Vector3>.blackboardKey)));
        }

        public void UnsetViewModel()
        {
            throw new NotImplementedException();
        }

        private void SetReferenceContent(VisualElement content, BlackboardVariableVm<T> viewModel, Type type)
        {
            content.Clear();
            string fieldName = nameof(BlackboardVariable<T>.blackboardKey);
            var reactiveProperty = viewModel.GetReactiveProperty<ReactiveProperty<string>>(fieldName);
            content.Add(ViewCreator.CreateLinkedDropDownField(reactiveProperty, fieldName, BlackboardManager.GetBlackboardVariableChoicesTyped<T>(type)));
        }
    }
}