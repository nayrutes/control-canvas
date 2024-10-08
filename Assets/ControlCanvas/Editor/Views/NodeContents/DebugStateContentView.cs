﻿using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(DebugState))]
    public class DebugStateContentView : INodeContent
    {
        public VisualElement CreateView(IControl control)
        {
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var vmBase = vm as BaseViewModel<DebugState>;
            
            VisualElement view = new();
            
            //Automatic view element creation
            view.Add(ViewCreator.CreateLinkedGenericField(vm, nameof(DebugState.nodeMessage)));
            
            //manual view element creation
            DropdownField exitEvents = new DropdownField("Exit Events");
            exitEvents.choices = Blackboard.GetExitEventNames();
            var rpExitEventIndex = vmBase.GetReactiveProperty<ReactiveProperty<int>>(nameof(DebugState.exitEventIndex));
            rpExitEventIndex.Subscribe(x=> exitEvents.value = exitEvents.choices[x]);
            exitEvents.RegisterValueChangedCallback(evt => rpExitEventIndex.Value = exitEvents.choices.IndexOf(evt.newValue));
            view.Add(exitEvents);
            
            return view;
        }
    }
}