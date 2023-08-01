using System;
using System.Reflection;
using ControlCanvas.Serialization;
using PlasticPipe.Server;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        //public ReactiveProperty<NodeData> nodeData { get; private set; } = new ReactiveProperty<NodeData>();

        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Guid { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ReactiveProperty<SerializableVector2>();

        

        //[NonSerialized]
        public Blackboard blackboardCanvas;

        //[NonSerialized]
        public ControlAgent controlAgent;

        public ReactiveProperty<ControlCanvasEditorWindow.NodeType> NodeType = new();

        public NodeViewModel()
        {
            NodeData newData = new();
            newData.name = "New Node";
            newData.guid = System.Guid.NewGuid().ToString();
            //newData.Guid = GUID.Generate().ToString();
            
            DataProperty.Value = newData;
            Initialize();
        }

        public NodeViewModel(NodeData node)
        {
            DataProperty.Value = node;
            Initialize();
        }

        public void Initialize()
        {
            //DataProperty.Subscribe(LoadDataInternal).AddTo(disposables);
            
            AutoBindReactivePropertiesToDataFields();
            
            // SetupDataSaving(Name, nameof(DataProperty.Value.name));
            // SetupDataSaving(Guid, nameof(DataProperty.Value.guid));
            // SetupDataSaving(Position, nameof(DataProperty.Value.position));
            // SetupDataSaving(Size, nameof(DataProperty.Value.size));
        }

        
        
        // public override void Dispose()
        // {
        //     disposables.Dispose();
        // }

        // protected override void SaveDataInternal(NodeData nodeData)
        // {
        //     nodeData.name = Name.Value;
        //     nodeData.guid = Guid.Value;
        //     nodeData.position = Position.Value;
        //     nodeData.size = Size.Value;
        // }
        //
        // protected override void LoadDataInternal(NodeData nodeData)
        // {
        //     Name.Value = nodeData.name;
        //     Guid.Value = nodeData.guid;
        //     Position.Value = nodeData.position;
        //     Size.Value = nodeData.size;
        // }
    }
}