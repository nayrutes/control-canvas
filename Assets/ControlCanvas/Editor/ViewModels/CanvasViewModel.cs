using System.Linq;
using ControlCanvas.Editor.Views;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class CanvasViewModel : BaseViewModel<CanvasData>
    {
        public ReactiveProperty<CanvasData> canvasDataContainer;
        //CompositeDisposable disposables = new ();
        public GraphViewModel GraphViewModel { get; private set; }

        public InspectorViewModel InspectorViewModel { get; private set; }

        public ReactiveProperty<string> canvasName;

        public ReactiveCollection<NodeViewModel> NodeViewModels { get; private set; }
        public ReactiveCollection<EdgeViewModel> EdgeViewModels { get; private set; }


        public CanvasViewModel()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            canvasDataContainer = new ReactiveProperty<CanvasData>();
            canvasName = new ReactiveProperty<string>();
            NodeViewModels = new ReactiveCollection<NodeViewModel>();
            EdgeViewModels = new ReactiveCollection<EdgeViewModel>();
            canvasDataContainer.SkipLatestValueOnSubscribe().Subscribe(data =>
            {
                LoadDataInternal(data);
                
            }).AddTo(disposables);
            
            GraphViewModel = new GraphViewModel(this);
            InspectorViewModel = new InspectorViewModel();
        }
        
        // public override void Dispose()
        // {
        //     disposables.Dispose();
        // }

        protected override void Dispose(bool disposing)
        {
            canvasDataContainer.Dispose();
        }
        
        
        // public void LoadCanvasData(CanvasData canvasData)
        // {
        //     
        // }

        // public void NewCanvasData()
        // {
        //     LoadDataInternal(new CanvasData());
        // }

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
            NodeViewModels.ToList().ForEach(nodeViewModel =>
            {
                data.Nodes.Add(nodeViewModel.DataProperty.Value);
            });
            
            data.Edges.Clear();
            EdgeViewModels.ToList().ForEach(edgeViewModel =>
            {
                data.Edges.Add(edgeViewModel.edgeData.Value);
            });
        }


        public void SerializeData(string path)
        {
            
            XMLHelper.SerializeToXML(path, canvasDataContainer.Value);
        }

        public void DeserializeData(string path)
        {
            XMLHelper.DeserializeFromXML(path, out var canvasData);
            LoadData(canvasData);
        }

        private void LoadData(CanvasData canvasData)
        {
            canvasDataContainer.Value = canvasData;
        }

        public void OnSelectionChanged(SelectedChangedArgs obj)
        {
            InspectorViewModel.OnSelectionChanged(obj, canvasDataContainer.Value);
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