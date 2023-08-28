using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Editor.Views;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class CanvasViewModel : BaseViewModel<CanvasData>
    {
        public GraphViewModel GraphViewModel { get; private set; }

        public InspectorViewModel InspectorViewModel { get; private set; }

        public ReactiveProperty<string> canvasName = new();
        public ReactiveProperty<string> canvasPath = new();

        //defining the ReactiveProperty as a property allows referencing it easier
        public ReactiveProperty<ReactiveCollection<NodeData>> Nodes { get; private set; } = new();
        public ReactiveProperty<ReactiveCollection<EdgeData>> Edges { get; private set; } = new();

        public ReactiveCommand<NodeViewModel> MakeInitialNodeCommand { get; private set; } = new();
        
        public ReactiveProperty<string> InitialNode { get; private set; } = new();
        public ReactiveProperty<string> CurrentDebugNode { get; private set; } = new();
        
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
            disposables.Add(GraphViewModel);
            disposables.Add(InspectorViewModel);

            string fieldName = "InitialNode";

            //Example of how to get a ReactiveProperty from a dataField
            //var rp = GetReactiveProperty<ReactiveProperty<string>>(fieldName);

            Nodes.Subscribe(x =>
            {
                x.SubscribeAndProcessExisting(y =>
                {
                    var cvm = GetChildViewModel<NodeViewModel>(y);
                    cvm.MakeStartNodeCommand.Subscribe(z =>
                    {
                        MakeInitialNodeCommand.Execute(z);
                    }).AddTo(disposables);
                }).AddTo(disposables);
            }).AddTo(disposables);

            MakeInitialNodeCommand.Subscribe(x =>
            {
                InitialNode.Value = x.DataProperty.Value.guid;
            });

            DataProperty.Where(nn => nn != null).Subscribe(c =>
            {
                InitialNode.DoWithLast(x =>
                    {
                        if (x != null)
                        {
                            var isInitialNode = GetViewModelByGuid(x)?.IsInitialNode;
                            if (isInitialNode != null) isInitialNode.Value = false;
                        }
                    })
                    .Subscribe(x =>
                    {
                        if (x != null)
                        {
                            var isInitialNode = GetViewModelByGuid(x)?.IsInitialNode;
                            if (isInitialNode != null) isInitialNode.Value = true;
                        }
                    }).AddTo(disposables);
            }).AddTo(disposables);
            
            
            
            CurrentDebugNode.DoWithLast(x =>
                {
                    if (x != null)
                    {
                        var isDebugNode = GetViewModelByGuid(x)?.IsDebugNode;
                        if (isDebugNode != null) isDebugNode.Value = false;
                    }
                })
                .Subscribe(x =>
            {
                if (x != null)
                {
                    var isDebugNode = GetViewModelByGuid(x)?.IsDebugNode;
                    if (isDebugNode != null) isDebugNode.Value = true;
                }
            });
        }

        private NodeViewModel GetViewModelByGuid(string guid)
        {
            var node = Nodes.Value.ToList().Find(x => x.guid == guid);
            if (node != null)
            {
                return GetChildViewModel<NodeViewModel>(node);
            }
            return null;
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
            canvasPath.Value = path;
        }

        public void DeserializeData(string path)
        {
            XMLHelper.DeserializeFromXML(path, out var canvasData);
            LoadData(canvasData);
            canvasPath.Value = path;
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
        }

        public void DeleteNode(NodeData nodeData)
        {
            Nodes.Value.Remove(nodeData);
        }

        public void CreateEdge(NodeData from, NodeData to)
        {
            EdgeData edgeData = EdgeViewModel.CreateEdgeData(from.guid, to.guid);
            Edges.Value.Add(edgeData);
        }

        public void DeleteEdge(EdgeData edgeData)
        {
            Edges.Value.Remove(edgeData);
        }

        public void SetCurrentDebugState(IState state)
        {
            CurrentDebugNode.Value = NodeManager.Instance.GetGuidForState(state);
        }
    }
}