using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class VisualNodeView : Node, IView<NodeViewModel>
    {
        //public Node node;
        public NodeViewModel nodeViewModel;
        
        
        public Port portIn;
        public Port portOut;
        public Port portOut2;

        private VisualElement m_DynamicContent;
        private CompositeDisposable disposables = new();

        public VisualNodeView() : base("Assets/ControlCanvas/Editor/VisualNodeUXML.uxml")
        {
            //this.UseDefaultStyling();
            //StyleSheet styleSheet = EditorGUIUtility.Load("StyleSheets/GraphView/Node.uss") as StyleSheet;
            //StyleSheet styleSheet = EditorGUIUtility.Load("Assets/ControlCanvas/Editor/Node.uss") as StyleSheet;
            //this.styleSheets.Add(styleSheet);  
            
            //WriteStyleSheetToFile(styleSheet, "Assets/ControlCanvas/Editor/Node.uss");
            
            
        }

        private void OnTypeChanged(ChangeEvent<Enum> evt)
        {
            nodeViewModel.NodeType.Value = (NodeType)evt.newValue;
        }
        
        private void SetNewType(NodeType type)
        {
            m_DynamicContent.Clear();
            m_DynamicContent.Add(new Label($"This is a {type} node"));
            this.Q<EnumField>("type-enum").SetValueWithoutNotify(type);
            switch (type)
            {
                case NodeType.State:
                    HidePort2(true);
                    break;
                case NodeType.Behaviour:
                    HidePort2(false);
                    break;
                case NodeType.Decision:
                    HidePort2(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        private void SetNewClass(IControl control)
        {
            m_DynamicContent.Clear();
            var label = new Label($"This Node represents class {control?.GetType()}");
            m_DynamicContent.Add(label);

            this.Q<DropdownField>("class-dropdown").value = control?.GetType().Name ?? "None";
            
            if (control != null)// && NodeManager.stateDictionary.TryGetValue(className, out var t))
            {
                if (control.GetType() == typeof(DebugState))
                {
                    //m_DynamicContent.Add(new DebugStateView());
                    TextField visualElement = new TextField("Message");
                    m_DynamicContent.Add(visualElement);
                    var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
                    var vmBase = vm as BaseViewModel<DebugState>;
                    //var vm = nodeViewModel.GetChildViewModel(nodeViewModel.specificState.Value) as BaseViewModel<DebugState>;
                    var rp = vmBase.GetReactiveProperty<ReactiveProperty<string>>("nodeMessage");
                    rp.Subscribe(x=> visualElement.value = x);
                    visualElement.RegisterValueChangedCallback(evt => rp.Value = evt.newValue);
                    
                    //visualElement.RegisterValueChangedCallback(evt => nodeViewModel.Message.Value = evt.newValue);
                }else if (control.GetType() == typeof(DebugBehaviour))
                {
                    EnumField enumField = new EnumField("State");
                    enumField.Init(State.Running);
                    m_DynamicContent.Add(enumField);
                    var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
                    var vmBase = vm as BaseViewModel<DebugBehaviour>;
                    //var vm = nodeViewModel.GetChildViewModel(nodeViewModel.specificState.Value) as BaseViewModel<DebugState>;
                    
                    var rp = vmBase.GetReactiveProperty<ReactiveProperty<State>>("nodeState");
                    rp.Subscribe(x=> enumField.SetValueWithoutNotify(x));
                    enumField.RegisterValueChangedCallback(evt => rp.Value = (State)evt.newValue);
                }else if (control.GetType() == typeof(MoveToControl))
                {
                    IntegerField integerField = new IntegerField("Index");
                    m_DynamicContent.Add(integerField);
                    var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
                    var vmBase = vm as BaseViewModel<MoveToControl>;
                    
                    var rp = vmBase.GetReactiveProperty<ReactiveProperty<int>>("index");
                    rp.Subscribe(x=> integerField.SetValueWithoutNotify(x));
                    integerField.RegisterValueChangedCallback(evt => rp.Value = evt.newValue);
                    
                }else if (control.GetType() == typeof(Repeater))
                {
                    EnumField enumField = new EnumField("Mode");
                    enumField.Init(RepeaterMode.Loop);
                    m_DynamicContent.Add(enumField);
                    var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
                    var vmBase = vm as BaseViewModel<Repeater>;
                    //var vm = nodeViewModel.GetChildViewModel(nodeViewModel.specificState.Value) as BaseViewModel<DebugState>;

                    var rp = vmBase.GetReactiveProperty<ReactiveProperty<RepeaterMode>>("mode");
                    rp.Subscribe(x => enumField.SetValueWithoutNotify(x));
                    enumField.RegisterValueChangedCallback(evt => rp.Value = (RepeaterMode)evt.newValue);
                }
            }
        }
        
        public void CreatePorts()
        {
            portIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (portIn != null)
            {
                portIn.portName = "portIn";
                portIn.name = "portIn";
                inputContainer.Add(portIn);
            }

            portOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut != null)
            {
                portOut.portName = "portOut";
                portOut.name = "portOut";
                outputContainer.Add(portOut);
            }
            
            portOut2 = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut2 != null)
            {
                portOut2.portName = "portOut-2";
                portOut2.name = "portOut-2";
                mainContainer.Q<VisualElement>("output-2").Add(portOut2);
                //outputContainer.Add(portOut2);
            }
        }
        
        public static void WriteStyleSheetToFile(StyleSheet styleSheet, string path)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type type = assembly.GetType("Unity.UI.Builder.StyleSheetToUss");
                if (type != null)
                {
                    // Get the method itself
                    MethodInfo method = type.GetMethod("WriteStyleSheet", BindingFlags.Static | BindingFlags.Public);

                    if (method == null)
                    {
                        Debug.LogError("Could not find method WriteStyleSheet");
                        return;
                    }

                    // Create an instance of UssExportOptions
                    Type optionsType = assembly.GetType("Unity.UI.Builder.UssExportOptions");
                    object options = Activator.CreateInstance(optionsType);

                    // Call the method
                    method.Invoke(null, new object[] { styleSheet, path, options });
                    return;
                }
            }
        }


        public void SetViewModel(NodeViewModel viewModel)
        {
            UnbindViewFromViewModel();
            UnbindViewModelFromView();
            
            m_DynamicContent = this.Q<VisualElement>("dynamic-content");
            nodeViewModel = viewModel;

            
            this.Q<DropdownField>("class-dropdown").choices = viewModel.ClassChoices;
            
            BindViewToViewModel();
            BindViewModelToView();
            //this.viewDataKey = nodeViewModel.Guid;

            //TODO test if destroy ports is needed
            CreatePorts();

            //this.Q<EnumField>("type-enum").value = nodeViewModel.NodeType;
            //SetNewType(nodeViewModel.NodeType);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            nodeViewModel.Position.Value = evt.newRect.position;
            nodeViewModel.Size.Value = evt.newRect.size;
        }

        private void BindViewToViewModel()
        {
            nodeViewModel.Name
                .CombineLatest(nodeViewModel.Guid, (name, guid) => $"{name} {guid}")
                .Subscribe(name => this.title = name).AddTo(disposables);
            
            nodeViewModel.Position.CombineLatest(nodeViewModel.Size, (position, size) => new Rect(position, size))
                .Subscribe(rect => this.SetPosition(rect)).AddTo(disposables);
            
            nodeViewModel.NodeType.Subscribe(type => SetNewType(type)).AddTo(disposables);
            
            nodeViewModel.specificControl.Subscribe(control => SetNewClass(control)).AddTo(disposables);
            
            nodeViewModel.IsInitialNode.Subscribe(x =>
            {
                if (x)
                {
                    this.AddToClassList("initial-node");
                }
                else
                {
                    this.RemoveFromClassList("initial-node");
                }
            }).AddTo(disposables);
            
            nodeViewModel.IsDebugNode.Subscribe(x =>
            {
                if (x)
                {
                    this.AddToClassList("debug-node");
                }
                else
                {
                    this.RemoveFromClassList("debug-node");
                }
            }).AddTo(disposables);
            
            nodeViewModel.CurrentDebugBehaviourState.Subscribe(x =>
            {
                this.RemoveFromClassList("debug-node-success");
                this.RemoveFromClassList("debug-node-failure");
                this.RemoveFromClassList("debug-node-running");
                switch (x)
                {
                    case State.Success:
                        this.AddToClassList("debug-node-success");
                        break;
                    case State.Failure:
                        this.AddToClassList("debug-node-failure");
                        break;
                    case State.Running:
                        this.AddToClassList("debug-node-running");
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }
            }).AddTo(disposables);
            //nodeViewModel.HidePort2.Subscribe(x => { HidePort2(x); }).AddTo(disposables);
        }

        private void HidePort2(bool x)
        {
            if (x)
            {
                this.Q<VisualElement>("output-2").AddToClassList("hide-port-2");
            }
            else
            {
                this.Q<VisualElement>("output-2").RemoveFromClassList("hide-port-2");
            }
        }

        private void UnbindViewFromViewModel()
        {
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void BindViewModelToView()
        {
            RegisterCallback((GeometryChangedEvent evt) => OnGeometryChanged(evt));
            this.Q<EnumField>("type-enum").RegisterValueChangedCallback(OnTypeChanged);
            
            this.Q<DropdownField>("class-dropdown").RegisterValueChangedCallback(evt =>
            {
                nodeViewModel.ClassName.Value = evt.newValue;
            });
        }
        
        private void UnbindViewModelFromView()
        {
            UnregisterCallback((GeometryChangedEvent evt) => OnGeometryChanged(evt));
            this.Q<EnumField>("type-enum").UnregisterValueChangedCallback(OnTypeChanged);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Make Start Node", (a) => nodeViewModel.MakeStartNodeCommand.Execute(nodeViewModel), DropdownMenuAction.AlwaysEnabled);
        }
    }
}