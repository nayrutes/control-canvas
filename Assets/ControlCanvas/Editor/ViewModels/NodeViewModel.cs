using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Guid { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveCommand<NodeViewModel> MakeStartNodeCommand { get; set; } = new();

        public ReactiveProperty<string> ClassName { get;} = new();
        public List<string> ClassChoices { get;} = new();

        public ReactiveProperty<IState> specificState { get; } = new();
        public ReactiveProperty<bool> IsInitialNode { get; private set; } = new();
        
        public ReactiveProperty<bool> IsDebugNode { get; private set; } = new();
        
        //[NonSerialized]
        public Blackboard blackboardCanvas;

        //[NonSerialized]
        public ControlAgent controlAgent;

        public ReactiveProperty<NodeType> NodeType = new();

        public NodeViewModel(NodeData nodeData) : base(nodeData)
        {
            InitNode();
        }

        public NodeViewModel() : base()
        {
            InitNode();
        }
        
        public NodeViewModel(NodeData data, bool autobind) : base(data, autobind)
        {
            InitNode();
        }

        void InitNode()
        {
            ClassChoices.Add("None");
            ClassChoices.AddRange(NodeManager.stateDictionary.Keys);
            
            specificState.Subscribe(state =>
            {
                if (state != null)
                {
                    ClassName.Value = state.GetType().Name;
                }
            });
            
            ClassName.Subscribe(className =>
            {
                if (className == null || className == "None")
                {
                    specificState.Value = null;
                }
                else if(specificState.Value == null || specificState.Value.GetType().Name != className)
                {
                    if (NodeManager.stateDictionary.TryGetValue(className, out var value))
                    {
                        specificState.Value = (IState)Activator.CreateInstance(value);
                    }
                    else
                    {
                        specificState.Value = null;
                    }
                }
            });
        }
        
        protected override NodeData CreateData()
        {
            return CreateNodeData();
        }
        
        public static NodeData CreateNodeData()
        {
            NodeData newData = new();
            newData.name = "New Node";
            newData.guid = System.Guid.NewGuid().ToString();
            return newData;
        }
        
    }
}