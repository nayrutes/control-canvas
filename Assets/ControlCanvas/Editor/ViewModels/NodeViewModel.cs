using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        public ReactiveProperty<NodeData> nodeData { get; private set; } = new ReactiveProperty<NodeData>();

        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Guid { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ReactiveProperty<SerializableVector2>();

        CompositeDisposable disposables = new();

        //[NonSerialized]
        public Blackboard blackboardCanvas;

        //[NonSerialized]
        public ControlAgent controlAgent;

        public ReactiveProperty<ControlCanvasEditorWindow.NodeType> NodeType = new();

        public NodeViewModel()
        {
            Initialize();
            NodeData newData = new();
            newData.Name = "New Node";
            newData.Guid = System.Guid.NewGuid().ToString();
            //newData.Guid = GUID.Generate().ToString();
            
            nodeData.Value = newData;
        }

        public NodeViewModel(NodeData node)
        {
            Initialize();
            nodeData.Value = node;
        }

        public void Initialize()
        {
            nodeData.SkipLatestValueOnSubscribe().Subscribe(LoadDataInternal).AddTo(disposables);
        }

        public void Terminate()
        {
            disposables.Dispose();
        }

        protected override void SaveDataInternal(NodeData nodeData)
        {
            nodeData.Name = Name.Value;
            nodeData.Guid = Guid.Value;
            nodeData.Position = Position.Value;
            nodeData.Size = Size.Value;
        }

        protected override void LoadDataInternal(NodeData nodeData)
        {
            Name.Value = nodeData.Name;
            Guid.Value = nodeData.Guid;
            Position.Value = nodeData.Position;
            Size.Value = nodeData.Size;
        }
    }
}