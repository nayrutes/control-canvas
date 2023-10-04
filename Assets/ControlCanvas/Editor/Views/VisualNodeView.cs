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
    public class VisualNodeView : Node, IView<NodeViewModel>, IVisualNode
    {
        //public Node node;
        public NodeViewModel nodeViewModel;
        
        
        public Port portIn;
        public Port portOut;
        public Port portOut2;
        public Port portOutParallel;
        
        private VisualElement portInVisualElementContainer;
        private VisualElement portOutVisualElementContainer;
        private VisualElement portOut2VisualElementContainer;
        private VisualElement portOutParallelVisualElementContainer;

        private VisualElement m_DynamicContent;
        private CompositeDisposable disposables = new();
        private VisualNodeSettings _visualNodeSettings = new();

        public VisualNodeView() : base("Assets/ControlCanvas/Editor/VisualNodeUXML.uxml")
        {
            //this.UseDefaultStyling();
            //StyleSheet styleSheet = EditorGUIUtility.Load("StyleSheets/GraphView/Node.uss") as StyleSheet;
            //StyleSheet styleSheet = EditorGUIUtility.Load("Assets/ControlCanvas/Editor/Node.uss") as StyleSheet;
            //this.styleSheets.Add(styleSheet);  
            
            //WriteStyleSheetToFile(styleSheet, "Assets/ControlCanvas/Editor/Node.uss");
            
            
        }

        public void SetViewModel(NodeViewModel viewModel)
        {
            UnbindViewFromViewModel();
            UnbindViewModelFromView();
            
            m_DynamicContent = this.Q<VisualElement>("dynamic-content");
            portInVisualElementContainer = this.Q<VisualElement>("input");
            portOutVisualElementContainer = this.Q<VisualElement>("output");
            portOut2VisualElementContainer = this.Q<VisualElement>("output-2");
            portOutParallelVisualElementContainer = this.Q<VisualElement>("output-p");
            nodeViewModel = viewModel;

            
            this.Q<DropdownField>("class-dropdown").choices = viewModel.ClassChoices;
            
            BindViewToViewModel();
            BindViewModelToView();
            //this.viewDataKey = nodeViewModel.Guid;

            //TODO test if destroy ports is needed
            CreatePorts();
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
            _visualNodeSettings = new VisualNodeSettings();
            switch (type)
            {
                case NodeType.State:
                    _visualNodeSettings.portOut2Visible = false;
                    break;
                case NodeType.Behaviour:
                    break;
                case NodeType.Decision:
                    _visualNodeSettings.portParallelVisible = false;
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
            
            if (control != null)
            {
                m_DynamicContent.Add(ViewCreator.CreateViewFromControl(control));
            }
            ApplyVisualNodeSettings(ViewCreator.GetVisualSettings(control, _visualNodeSettings));
        }

        private void ApplyVisualNodeSettings(VisualNodeSettings getVisualSettings)
        {
            HidePortContainer(portInVisualElementContainer, !getVisualSettings.portInVisible);
            HidePortContainer(portOutVisualElementContainer, !getVisualSettings.portOutVisible);
            HidePortContainer(portOut2VisualElementContainer, !getVisualSettings.portOut2Visible);
            HidePortContainer(portOutParallelVisualElementContainer, !getVisualSettings.portParallelVisible);
        }

        public void CreatePorts()
        {
            portIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (portIn != null)
            {
                portIn.portName = "In";
                portIn.name = PortTypeToName(PortType.In);
                inputContainer.Add(portIn);
            }

            portOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut != null)
            {
                portOut.portName = "Out";
                portOut.name = PortTypeToName(PortType.Out);
                outputContainer.Add(portOut);
            }
            
            portOut2 = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut2 != null)
            {
                portOut2.portName = "Failure";
                portOut2.name = PortTypeToName(PortType.Out2);
                mainContainer.Q<VisualElement>("output-2").Add(portOut2);
                //outputContainer.Add(portOut2);
            }
            
            portOutParallel = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOutParallel != null)
            {
                portOutParallel.portName = "Parallel";
                portOutParallel.name = PortTypeToName(PortType.Parallel);
                mainContainer.Q<VisualElement>("output-p").Add(portOutParallel);
            }
        }
        
        public static string PortTypeToName(PortType portType)
        {
            switch (portType)
            {
                case PortType.In:
                    return "portIn";
                case PortType.Out:
                    return "portOut";
                case PortType.Out2:
                    return "portOut-2";
                case PortType.Parallel:
                    return "portOutParallel";
                case PortType.InOut:
                    return "In/Out";
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }
        
        public static PortType PortNameToType(string portName)
        {
            switch (portName)
            {
                case "portIn":
                    return PortType.In;
                case "portOut":
                    return PortType.Out;
                case "portOut-2":
                    return PortType.Out2;
                case "portOutParallel":
                    return PortType.Parallel;
                case "In/Out":
                    return PortType.InOut;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portName), portName, null);
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
            
            nodeViewModel.IsCurrentDebugNode.Subscribe(x =>
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
            
            nodeViewModel.IsNextDebugNode.Subscribe(x =>
            {
                if (x)
                {
                    this.AddToClassList("debug-node-next");
                }
                else
                {
                    this.RemoveFromClassList("debug-node-next");
                }
            }).AddTo(disposables);
        }

        private void HidePortContainer(VisualElement container, bool hide)
        {
            if(hide)
                container.AddToClassList("hide-port-2");
            else
                container.RemoveFromClassList("hide-port-2");
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
            evt.menu.AppendAction("Copy Guid", (a) => GUIUtility.systemCopyBuffer = nodeViewModel.Guid.Value, DropdownMenuAction.AlwaysEnabled);
        }

        public string GetVmGuid()
        {
            return nodeViewModel.Guid.Value;
        }

        public Port GetPort(PortType portType)
        {
            switch (portType)
            {
                case PortType.In:
                    return portIn;
                case PortType.Out:
                    return portOut;
                case PortType.Out2:
                    return portOut2;
                case PortType.Parallel:
                    return portOutParallel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }
        
        public NodeViewModel GetViewModel()
        {
            return nodeViewModel;
        }
    }

    public class VisualNodeSettings
    {
        public bool portInVisible = true;
        public bool portOutVisible = true;
        public bool portOut2Visible = true;
        public bool portParallelVisible = true;
        
    }
}