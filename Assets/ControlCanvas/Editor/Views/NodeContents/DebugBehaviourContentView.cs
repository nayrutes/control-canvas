using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(DebugBehaviour))]
    public class DebugBehaviourContentView : INodeContent
    {
        public VisualElement CreateView(IControl control)
        {
            
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var vmBase = vm as BaseViewModel<DebugBehaviour>;
            
            VisualElement view = new();
            
            EnumField enumField = new EnumField("State");
            enumField.Init(State.Running);
            var rp = vmBase.GetReactiveProperty<ReactiveProperty<State>>( nameof(DebugBehaviour.nodeState));
            rp.Subscribe(x=> enumField.SetValueWithoutNotify(x));
            enumField.RegisterValueChangedCallback(evt => rp.Value = (State)evt.newValue);
            view.Add(enumField);
            
            return view;
        }
    }
}