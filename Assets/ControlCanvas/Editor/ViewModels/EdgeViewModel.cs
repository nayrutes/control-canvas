using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class EdgeViewModel : BaseViewModel<EdgeData>
    {
        public ReactiveProperty<EdgeData> edgeData { get; private set; } = new ReactiveProperty<EdgeData>();

        public string Guid => edgeData.Value.Guid;
        public string StartNodeGuid => edgeData.Value.StartNodeGuid;
        public string EndNodeGuid => edgeData.Value.EndNodeGuid;
        
        
        public EdgeViewModel(EdgeData edgeData)
        {
            this.edgeData.Value = edgeData;
        }

        // protected override void Dispose(bool dispose)
        // {
        //     
        // }
        
        public EdgeViewModel(NodeViewModel from, NodeViewModel to)
        {
            edgeData.Value.StartNodeGuid = from.DataProperty.Value.guid;
            edgeData.Value.EndNodeGuid = to.DataProperty.Value.guid;
        }

        protected override void LoadDataInternal(EdgeData data)
        {
            edgeData.Value.Guid = data.Guid;
            edgeData.Value.StartNodeGuid = data.StartNodeGuid;
            edgeData.Value.EndNodeGuid = data.EndNodeGuid;
        }

        protected override void SaveDataInternal(EdgeData data)
        {
            data.Guid = edgeData.Value.Guid;
            data.StartNodeGuid = edgeData.Value.StartNodeGuid;
            data.EndNodeGuid = edgeData.Value.EndNodeGuid;
        }
    }
}