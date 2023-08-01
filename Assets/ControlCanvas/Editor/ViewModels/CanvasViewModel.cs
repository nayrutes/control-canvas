using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Views;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class CanvasViewModel : BaseViewModel<CanvasData>
    {
        public GraphViewModel GraphViewModel { get; private set; }

        public InspectorViewModel InspectorViewModel { get; private set; }

        public ReactiveProperty<string> canvasName = new();

        public ReactiveCollection<NodeViewModel> NodeViewModels { get; private set; } = new();
        public ReactiveCollection<EdgeViewModel> EdgeViewModels { get; private set; } = new();

        protected override Dictionary<string, string> InitializeMappingDictionary()
        {
            return new Dictionary<string, string>()
            {
                { nameof(DataProperty.Value.Name), nameof(canvasName) },
            };
        }

        //TODO: Add support for ReactiveCollections so autobind does work
        public CanvasViewModel() : base(false)
        {
            Initialize();
        }

        public void Initialize()
        {
            GraphViewModel = new GraphViewModel(this);
            InspectorViewModel = new InspectorViewModel();
        }


        protected override CanvasData CreateData()
        {
            CanvasData newData = new();
            newData.Name = "New Canvas";
            return newData;
        }

        protected override void LoadDataInternal(CanvasData canvasData)
        {
            NodeViewModels.Clear();
            EdgeViewModels.Clear();

            if (canvasData == null)
            {
                canvasName.Value = "<No canvas Object>";
                return;
            }

            canvasName.Value = canvasData.Name;
            canvasData.Nodes.ForEach(nodeData =>
            {
                var nodeViewModel = new NodeViewModel(nodeData);
                NodeViewModels.Add(nodeViewModel);
            });

            canvasData.Edges.ForEach(edgeData =>
            {
                var edgeViewModel = new EdgeViewModel(edgeData);
                EdgeViewModels.Add(edgeViewModel);
            });
        }

        protected override void SaveDataInternal(CanvasData data)
        {
            data.Name = canvasName.Value;
            data.Nodes.Clear();
            NodeViewModels.ToList().ForEach(nodeViewModel => { data.Nodes.Add(nodeViewModel.DataProperty.Value); });

            data.Edges.Clear();
            EdgeViewModels.ToList().ForEach(edgeViewModel => { data.Edges.Add(edgeViewModel.DataProperty.Value); });
        }


        public void SerializeData(string path)
        {
            XMLHelper.SerializeToXML(path, DataProperty.Value);
        }

        public void DeserializeData(string path)
        {
            XMLHelper.DeserializeFromXML(path, out var canvasData);
            LoadData(canvasData);
        }

        private void LoadData(CanvasData canvasData)
        {
            DataProperty.Value = canvasData;
        }

        public void OnSelectionChanged(SelectedChangedArgs obj)
        {
            InspectorViewModel.OnSelectionChanged(obj, DataProperty.Value);
        }

        public NodeViewModel CreateNode()
        {
            NodeViewModel nodeViewModel = new NodeViewModel();
            NodeViewModels.Add(nodeViewModel);
            return nodeViewModel;
        }

        public void DeleteNode(NodeViewModel nodeViewModel)
        {
        }

        public EdgeViewModel CreateEdge(NodeViewModel from, NodeViewModel to)
        {
            EdgeViewModel edgeViewModel = new EdgeViewModel(from, to);
            EdgeViewModels.Add(edgeViewModel);
            return edgeViewModel;
        }

        public void DeleteEdge(EdgeViewModel edgeViewModel)
        {
        }
    }
}