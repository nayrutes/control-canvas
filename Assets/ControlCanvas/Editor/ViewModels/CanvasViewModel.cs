using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Views;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels
{
    public class CanvasViewModel : BaseViewModel<CanvasData>
    {
        public GraphViewModel GraphViewModel { get; private set; }

        public InspectorViewModel InspectorViewModel { get; private set; }

        public ReactiveProperty<string> canvasName = new();

        //public ReactiveProperty<List<NodeData>> Nodes { get; private set; } = new();
        //public ReactiveProperty<List<EdgeData>> Edges { get; private set; } = new();

        public ReactiveCollection<NodeData> NodeDatas { get; private set; } = new();
        public ReactiveCollection<EdgeData> EdgeDatas { get; private set; } = new();

        public ReactiveProperty<ReactiveCollection<NodeData>> Nodes { get; private set; } = new();
        public ReactiveProperty<ReactiveCollection<EdgeData>> Edges { get; private set; } = new();

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
        public CanvasViewModel() : base()
        {
            Initialize();
        }

        public void Initialize()
        {
            GraphViewModel = new GraphViewModel(this);
            InspectorViewModel = new InspectorViewModel();
            Nodes.Subscribe(x =>
            {
                Debug.Log("Nodes changed");
                NodeViewModels.Clear();
                //Init handling entries
                x.ToList().ForEach(nodeData =>
                {
                    var nodeViewModel = new NodeViewModel(nodeData);
                    NodeViewModels.Add(nodeViewModel);
                });
                
                //handling changes
                x.ObserveAdd().Subscribe(y =>
                {
                    Debug.Log("Node added");
                    NodeViewModels.Add(new NodeViewModel(y.Value));
                }).AddTo(disposables);
            }).AddTo(disposables);
            
            Edges.Subscribe(x =>
            {
                Debug.Log("Edges changed");
                EdgeViewModels.Clear();
                //Init handling entries
                x.ToList().ForEach(edgeData =>
                {
                    var edgeViewModel = new EdgeViewModel(edgeData);
                    EdgeViewModels.Add(edgeViewModel);
                });
                
                //handling changes
                x.ObserveAdd().Subscribe(y =>
                {
                    Debug.Log("Edge added");
                    EdgeViewModels.Add(new EdgeViewModel(y.Value));
                }).AddTo(disposables);
            }).AddTo(disposables);
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