using System;
using System.Reflection;
using ControlCanvas.Editor.ViewModels.Base;
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
            
        }


        protected override NodeData CreateData()
        {
            NodeData newData = new();
            newData.name = "New Node";
            newData.guid = System.Guid.NewGuid().ToString();
            return newData;
        }
        
    }
}