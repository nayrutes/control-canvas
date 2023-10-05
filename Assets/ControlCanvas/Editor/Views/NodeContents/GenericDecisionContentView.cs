using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine.UIElements;
using ValueType = ControlCanvas.Runtime.ValueType;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(GenericDecision))]
    public class GenericDecisionContentView : INodeContent
    {
        public VisualElement CreateView(IControl control)
        {
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var vmBase = vm as BaseViewModel<GenericDecision>;
            
            VisualElement view = new();
            VisualElement viewRow = new();
            VisualElement viewRow2 = new();
            VisualElement viewRow2Left = new();
            VisualElement viewRow2Center = new();
            VisualElement viewRow2Right = new();
            viewRow.style.flexDirection = FlexDirection.Row;
            viewRow2.style.flexDirection = FlexDirection.Row;
            view.Add(viewRow);
            view.Add(viewRow2);
            viewRow2.Add(viewRow2Left);
            viewRow2.Add(viewRow2Center);
            viewRow2.Add(viewRow2Right);
            //Automatic view element creation
            viewRow.Add(ViewCreator.CreateLinkedGenericField(vm, nameof(GenericDecision.variableType1)));
            viewRow.Add(ViewCreator.CreateLinkedGenericField(vm, nameof(GenericDecision.decisionType)));
            viewRow.Add(ViewCreator.CreateLinkedGenericField(vm, nameof(GenericDecision.variableType2)));

            vmBase.GetReactiveProperty<ReactiveProperty<VariableType>>(nameof(GenericDecision.variableType1))
                .Subscribe(x => { SetContentType(viewRow2Left, vmBase, x, true);});
            vmBase.GetReactiveProperty<ReactiveProperty<VariableType>>(nameof(GenericDecision.variableType2))
                .Subscribe(x => { SetContentType(viewRow2Right, vmBase, x, false);});
            
            //manual view element creation
            // DropdownField exitEvents = new DropdownField("Exit Events");
            // exitEvents.choices = Blackboard.GetExitEventNames();
            // var rpExitEventIndex = vmBase.GetReactiveProperty<ReactiveProperty<int>>(nameof(GenericDecision.exitEventIndex));
            // rpExitEventIndex.Subscribe(x=> exitEvents.value = exitEvents.choices[x]);
            // exitEvents.RegisterValueChangedCallback(evt => rpExitEventIndex.Value = exitEvents.choices.IndexOf(evt.newValue));
            // view.Add(exitEvents);
            
            return view;
        }

        private void SetContentType(VisualElement view, BaseViewModel<GenericDecision> vmBase, VariableType variableType, bool isLeft)
        {
            view.Clear();
            VisualElement content = new();
            switch (variableType)
            {
                case VariableType.Constant:
                    string valueTypeName = isLeft ? nameof(GenericDecision.valueType1) : nameof(GenericDecision.valueType2);
                    view.Add(ViewCreator.CreateLinkedGenericField(vmBase, valueTypeName));
                    vmBase.GetReactiveProperty<ReactiveProperty<ValueType>>(valueTypeName)
                        .Subscribe(x =>
                        {
                            SetConstantContent(content, vmBase, x, isLeft);
                        });
                    break;
                case VariableType.Reference:
                    string blackboardTypeName = isLeft ? nameof(GenericDecision.blackboardType1) : nameof(GenericDecision.blackboardType2);
                    var reactiveProperty = vmBase.GetReactiveProperty<ReactiveProperty<Type>>(blackboardTypeName);
                    view.Add(ViewCreator.CreateLinkedDropDownField<Type>(reactiveProperty, blackboardTypeName, BlackboardManager.GetBlackboardTypeChoices()));
                    reactiveProperty.Where(x=>x!=null).Subscribe(x =>
                    {
                        SetReferenceContent(content, vmBase, x, isLeft);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, null);
            }
            view.Add(content);
        }

        private void SetConstantContent(VisualElement view, BaseViewModel<GenericDecision> vmBase, ValueType valueType, bool isLeft)
        {
            view.Clear();
            string fieldName;
            switch (valueType)
            {
                case ValueType.Bool:
                    fieldName = isLeft ? nameof(GenericDecision.bool1) : nameof(GenericDecision.bool2);
                    break;
                case ValueType.Int:
                    fieldName = isLeft ? nameof(GenericDecision.int1) : nameof(GenericDecision.int2);
                    break;
                case ValueType.Float:
                    fieldName = isLeft ? nameof(GenericDecision.float1) : nameof(GenericDecision.float2);
                    break;
                case ValueType.String:
                    fieldName = isLeft ? nameof(GenericDecision.string1) : nameof(GenericDecision.string2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
            }
            view.Add(ViewCreator.CreateLinkedGenericField(vmBase, fieldName));
        }
        
        private void SetReferenceContent(VisualElement view, BaseViewModel<GenericDecision> vmBase, Type type, bool isLeft)
        {
            view.Clear();
            string fieldName = isLeft ? nameof(GenericDecision.blackboardKey1) : nameof(GenericDecision.blackboardKey2);
            var reactiveProperty = vmBase.GetReactiveProperty<ReactiveProperty<string>>(fieldName);
            view.Add(ViewCreator.CreateLinkedDropDownField(reactiveProperty, fieldName, BlackboardManager.GetBlackboardVariableChoices(type)));
        }
        
    }
}