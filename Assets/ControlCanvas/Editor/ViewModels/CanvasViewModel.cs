using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.ViewModels.Base;
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

        //defining the ReactiveProperty as a property allows referencing it easier
        public ReactiveProperty<ReactiveCollection<NodeData>> Nodes { get; private set; } = new();
        public ReactiveProperty<ReactiveCollection<EdgeData>> Edges { get; private set; } = new();

        protected override Dictionary<string, string> InitializeMappingDictionary()
        {
            return new Dictionary<string, string>()
            {
                { nameof(DataProperty.Value.Name), nameof(canvasName) },
            };
        }

        
        public CanvasViewModel() : base()
        {
            Initialize();
        }

        public void Initialize()
        {
            GraphViewModel = new GraphViewModel(this);
            InspectorViewModel = new InspectorViewModel();
            //TODO: fetch all viewModels in base class and add them to the disposables list
            disposables.Add(GraphViewModel);
            disposables.Add(InspectorViewModel);
            
            string fieldName = "Nodes";
            
            //Example of how to get a ReactiveProperty from a dataField
            var rp = GetReactiveProperty<ReactiveProperty<ReactiveCollection<NodeData>>>(fieldName);
        }

        protected override CanvasData CreateData()
        {
            CanvasData newData = new();
            newData.Name = "New Canvas";
            return newData;
        }

        protected override void LoadDataInternal(CanvasData canvasData)
        {

        }

        protected override void SaveDataInternal(CanvasData data)
        {
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
            NodeViewModel cvm = AddChildViewModel<NodeViewModel, NodeData>(new NodeViewModel(), Nodes);
            return cvm;
            // NodeData nodeData = NodeViewModel.CreateNodeData();
            // Nodes.Value.Add(nodeData);
            // NodeViewModel nvm = GetChildViewModel<NodeViewModel>(nodeData);
            // return nvm;
        }

        public void DeleteNode(NodeData nodeData)
        {
            Nodes.Value.Remove(nodeData);
        }

        public void CreateEdge(NodeData from, NodeData to)
        {
            EdgeData edgeData = EdgeViewModel.CreateEdgeData(from.guid, to.guid);
            Edges.Value.Add(edgeData);
            //return edgeData;
        }

        public void DeleteEdge(EdgeData edgeData)
        {
            Edges.Value.Remove(edgeData);
        }
    }
}