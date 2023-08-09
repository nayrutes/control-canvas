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
        }
        
        private void SetNewClass(IState state)
        {
            m_DynamicContent.Clear();
            var label = new Label($"This Node represents class {state?.GetType()}");
            m_DynamicContent.Add(label);

            this.Q<DropdownField>("class-dropdown").value = state?.GetType().Name ?? "None";
            
            if (state != null)// && NodeManager.stateDictionary.TryGetValue(className, out var t))
            {
                if (state.GetType() == typeof(DebugState))
                {
                    //m_DynamicContent.Add(new DebugStateView());
                    TextField visualElement = new TextField("Message");
                    m_DynamicContent.Add(visualElement);
                    var vm = ViewModelCreator.CreateViewModel(state.GetType(), state);
                    var vmBase = vm as BaseViewModel<DebugState>;
                    //var vm = nodeViewModel.GetChildViewModel(nodeViewModel.specificState.Value) as BaseViewModel<DebugState>;
                    var rp = vmBase.GetReactiveProperty<ReactiveProperty<string>>("nodeMessage");
                    rp.Subscribe(x=> visualElement.value = x);
                    visualElement.RegisterValueChangedCallback(evt => rp.Value = evt.newValue);
                    
                    //visualElement.RegisterValueChangedCallback(evt => nodeViewModel.Message.Value = evt.newValue);
                }
            }
        }
        
        public void CreatePorts()
        {
            portIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (portIn != null)
            {
                portIn.portName = "portIn";
                inputContainer.Add(portIn);
            }

            portOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut != null)
            {
                portOut.portName = "portOut";
                outputContainer.Add(portOut);
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
            
            nodeViewModel.specificState.Subscribe(state => SetNewClass(state)).AddTo(disposables);
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
            evt.menu.AppendAction("Make Start Node", (a) => nodeViewModel.MakeStartNodeCommand.Execute(), DropdownMenuAction.AlwaysEnabled);
        }
    }
}