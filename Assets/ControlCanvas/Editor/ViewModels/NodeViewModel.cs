using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using PlasticPipe.Server;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Guid { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveCommand MakeStartNodeCommand { get; set; } = new();

        public ReactiveProperty<string> ClassName { get;} = new();
        public List<string> ClassChoices { get;} = new();

        //[NonSerialized]
        public Blackboard blackboardCanvas;

        //[NonSerialized]
        public ControlAgent controlAgent;

        public ReactiveProperty<NodeType> NodeType = new();

        public NodeViewModel(NodeData nodeData) : base(nodeData)
        {
            
        }

        public NodeViewModel() : base()
        {
            ClassChoices.Add("None");
            ClassChoices.AddRange(NodeManager.stateDictionary.Keys);
        }
        
        public NodeViewModel(NodeData data, bool autobind) : base(data, autobind)
        {
            
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