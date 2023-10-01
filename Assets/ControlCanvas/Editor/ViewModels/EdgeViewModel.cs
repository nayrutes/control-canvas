using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class EdgeViewModel : BaseViewModel<EdgeData>
    {
        //public ReactiveProperty<EdgeData> edgeData { get; private set; } = new ReactiveProperty<EdgeData>();

        // public string Guid => DataProperty.Value.Guid;
        // public string StartNodeGuid => DataProperty.Value.StartNodeGuid;
        // public string EndNodeGuid => DataProperty.Value.EndNodeGuid;
        
        // public string Guid;
        // public string StartNodeGuid;
        // public string EndNodeGuid;
        // public PortType StartPortType;
        // public PortType EndPortType;

        public ReactiveProperty<string> Guid { get; } = new();
        public ReactiveProperty<string> StartNodeGuid { get; } = new();
        public ReactiveProperty<string> EndNodeGuid { get; } = new();
        public ReactiveProperty<PortType> StartPortType { get; } = new();
        public ReactiveProperty<PortType> EndPortType { get; } = new();

        public EdgeViewModel(NodeViewModel from, NodeViewModel to, PortType startPortType, PortType endPortType) : base()
        {
            //edgeData.Value.Guid = System.Guid.NewGuid().ToString();
            DataProperty.Value.StartNodeGuid = from.DataProperty.Value.guid;
            DataProperty.Value.EndNodeGuid = to.DataProperty.Value.guid;
            DataProperty.Value.StartPortType = startPortType;
            DataProperty.Value.EndPortType = endPortType;
        }

        public EdgeViewModel(EdgeData data, bool autobind) : base(data, autobind)
        {
        }

        protected override EdgeData CreateData()
        {
            return CreateEdgeData();
        }
        
        public static EdgeData CreateEdgeData()
        {
            EdgeData newData = new();
            newData.Guid = System.Guid.NewGuid().ToString();
            return newData;
        }
        
        // public static EdgeData CreateEdgeData(string start, string end, PortType startPortType, PortType endPortType)
        // {
        //     EdgeData newData = CreateEdgeData();
        //     newData.StartNodeGuid = start;
        //     newData.EndNodeGuid = end;
        //     //if(startPortName != "portOut")
        //         newData.StartPortName = NodeData.PortTypeToName(startPortType);
        //     //if(endPortName != "portIn")
        //         newData.EndPortName = NodeData.PortTypeToName(endPortType);
        //     return newData;
        // }
    }
}