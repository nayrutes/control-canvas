using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Editor.Views;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using NUnit.Framework.Constraints;
using UniRx;
using UnityEngine;

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
                
                x.ObserveRemove().Subscribe(RemoveEdgesOfNode).AddTo(disposables);
                
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
                        var isDebugNode = GetViewModelByGuid(x)?.IsCurrentDebugNode;
                        if (isDebugNode != null) isDebugNode.Value = false;
                    }
                })
                .Subscribe(x =>
            {
                if (x != null)
                {
                    var isDebugNode = GetViewModelByGuid(x)?.IsCurrentDebugNode;
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

        public NodeViewModel CreateNode(NodeType nodeType = NodeType.State)
        {
            NodeViewModel cvm = AddChildViewModel<NodeViewModel, NodeData>(new NodeViewModel(nodeType), Nodes);
            return cvm;
        }

        public void DeleteNode(NodeData nodeData)
        {
            Nodes.Value.Remove(nodeData);
        }

        public void CreateEdge(NodeData from, NodeData to, string startPortName = null, string endPortName = null)
        {
            EdgeData edgeData = EdgeViewModel.CreateEdgeData(from.guid, to.guid, startPortName, endPortName);
            Edges.Value.Add(edgeData);
        }

        private void RemoveEdgesOfNode(CollectionRemoveEvent<NodeData> nodeData)
        {
            foreach (var edge in Edges.Value.ToList())
            {
                if (edge.StartNodeGuid == nodeData.Value.guid || edge.EndNodeGuid == nodeData.Value.guid)
                {
                    DeleteEdge(edge);
                }
            }
        }
        
        public void DeleteEdge(EdgeData edgeData)
        {
            if (edgeData == null)
            {
                Debug.LogWarning("Deleting null edgeData");
                return;
            }
            Edges.Value.Remove(edgeData);
        }

        public void SetCurrentDebugControl(IControl control)
        {
            CurrentDebugNode.Value = NodeManager.Instance.GetGuidForControl(control);
        }

        public void SetNextDebugControl(IControl nextControl, bool active)
        {
            GetViewModelByGuid(NodeManager.Instance.GetGuidForControl(nextControl))?.SetNextDebugControl(active);
        }
        
        public void SetDebugBehaviourState(IControl control, State? controlRunnerLatestBehaviourState)
        {
            GetViewModelByGuid(NodeManager.Instance.GetGuidForControl(control))?.SetCurrentDebugBehaviourState(controlRunnerLatestBehaviourState);
        }

        public NodeViewModel CreateRoutingNode(NodeData node1, NodeData node2)
        {
            EdgeData oldEdge = Edges.Value.ToList().Find(x => x.StartNodeGuid == node1.guid && x.EndNodeGuid == node2.guid);
            if (oldEdge != null)
            {
                DeleteEdge(oldEdge);
                NodeViewModel routingNode = CreateNode(NodeType.Routing);
                routingNode.NodeType.Value = NodeType.Routing;
                
                CreateEdge(node1, routingNode.DataProperty.Value, oldEdge.StartPortName, "In/Out");
                CreateEdge(routingNode.DataProperty.Value, node2, "In/Out", oldEdge.EndPortName);
                return routingNode;
            }
            return null;
        }
    }
}