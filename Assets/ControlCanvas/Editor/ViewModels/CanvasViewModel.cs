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
        //Reactive properties with equivalent field in CanvasData
        public ReactiveProperty<ReactiveCollection<NodeData>> Nodes { get; private set; } = new();
        public ReactiveProperty<ReactiveCollection<EdgeData>> Edges { get; private set; } = new();
        public ReactiveProperty<string> InitialNode { get; private set; } = new();
        public ReactiveProperty<string> CanvasName { get; set; } = new();

        protected override Dictionary<string, string> InitializeMappingDictionary()//TODO: change to use attributes instead
        {
            //Manual definition of mapping between field names and reactive property names
            return new Dictionary<string, string>()
            {
                { nameof(DataProperty.Value.Name), nameof(CanvasName) },
            };
        }
        
        //Special child view models TODO: check why/how they are explicit tracked and not over viewmodelTracker
        public GraphViewModel GraphViewModel { get; private set; }
        public InspectorViewModel InspectorViewModel { get; private set; }

        //Reactive properties for internal functions
        public ReactiveProperty<string> CanvasPath { get; set; } = new();

        //Reactive properties for debugging
        public ReactiveProperty<string> CurrentDebugNode { get; private set; } = new();
        
        //Commands
        public ReactiveCommand<NodeViewModel> MakeInitialNodeCommand { get; private set; } = new();
        public ReactiveCommand<bool> SetCoreDebuggingCommand { get; private set; } = new();
        public ReactiveCommand<bool> ExpandContent { get; set; } = new();

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
            
            SetCoreDebuggingCommand.Subscribe(x =>
            {
                foreach (var node in Nodes.Value)
                {
                    var cvm = GetChildViewModel<NodeViewModel>(node);
                    cvm.CoreDebugging.Value = x;
                }
            });
            
            ExpandContent.Subscribe(x =>
            {
                foreach (var node in Nodes.Value)
                {
                    var cvm = GetChildViewModel<NodeViewModel>(node);
                    cvm.ExpandContent.Value = x;
                }
            });
            
            DataProperty.Where(nn => nn != null).Subscribe(c =>
            {
                InitialNode.DoWithLast(x =>
                    {
                        if (x != null)
                        {
                            var isInitialNode = GetNodeViewModelByGuid(x)?.IsInitialNode;
                            if (isInitialNode != null) isInitialNode.Value = false;
                        }
                    })
                    .Subscribe(x =>
                    {
                        if (x != null)
                        {
                            var isInitialNode = GetNodeViewModelByGuid(x)?.IsInitialNode;
                            if (isInitialNode != null) isInitialNode.Value = true;
                        }
                    }).AddTo(disposables);
            }).AddTo(disposables);
            
            CurrentDebugNode.DoWithLast(x =>
                {
                    if (x != null)
                    {
                        var isDebugNode = GetNodeViewModelByGuid(x)?.IsCurrentDebugNode;
                        if (isDebugNode != null) isDebugNode.Value = false;
                    }
                })
                .Subscribe(x =>
            {
                if (x != null)
                {
                    var isDebugNode = GetNodeViewModelByGuid(x)?.IsCurrentDebugNode;
                    if (isDebugNode != null) isDebugNode.Value = true;
                }
            });
        }

        private NodeViewModel GetNodeViewModelByGuid(string guid)
        {
            var node = Nodes.Value.ToList().Find(x => x.guid == guid);
            if (node != null)
            {
                return GetChildViewModel<NodeViewModel>(node);
            }
            return null;
        }
        
        private EdgeViewModel GetEdgeViewModelByGuid(string guid)
        {
            var node = Edges.Value.ToList().Find(x => x.Guid == guid);
            if (node != null)
            {
                return GetChildViewModel<EdgeViewModel>(node);
            }
            return null;
        }
        
        public EdgeViewModel GetEdgeViewModel(NodeViewModel startNode, NodeViewModel endNode){
            var edge = Edges.Value.ToList().Find(x => x.StartNodeGuid == startNode.Guid.Value && x.EndNodeGuid == endNode.Guid.Value);
            if (edge != null)
            {
                return GetChildViewModel<EdgeViewModel>(edge);
            }
            edge = Edges.Value.ToList().Find(x => x.StartNodeGuid == endNode.Guid.Value && x.EndNodeGuid == startNode.Guid.Value);
            if (edge != null)
            {
                Debug.LogWarning("Edge was found but start and end nodes were switched");
                return GetChildViewModel<EdgeViewModel>(edge);
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
            CanvasPath.Value = path;
        }

        public void DeserializeData(string path)
        {
            XMLHelper.DeserializeFromXML(path, out var canvasData);
            LoadData(canvasData);
            CanvasPath.Value = path;
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
        public NodeViewModel AddNode(NodeData nodeData)
        {
            Nodes.Value.Add(nodeData);
            return GetChildViewModel<NodeViewModel>(nodeData);
        }

        public void DeleteNode(NodeData nodeData)
        {
            Nodes.Value.Remove(nodeData);
        }
        
        public EdgeViewModel CreateEdge(NodeViewModel from, NodeViewModel to, PortType startPortType, PortType endPortType)
        {
            EdgeViewModel edgeVm = AddChildViewModel<EdgeViewModel, EdgeData>(new EdgeViewModel(from, to, startPortType, endPortType), Edges);
            return edgeVm;
            //EdgeData edgeData = EdgeViewModel.CreateEdgeData(from.guid, to.guid, startPortType, endPortType);
            //Edges.Value.Add(edgeData);
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
        
        public EdgeViewModel AddEdge(EdgeData edgeData)
        {
            Edges.Value.Add(edgeData);
            return GetChildViewModel<EdgeViewModel>(edgeData);
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
        public void DeleteEdge(EdgeViewModel edgeVm)
        {
            if (edgeVm == null)
            {
                Debug.LogError("Deleting null edge");
                return;
            }
            EdgeData edgeData = edgeVm.DataProperty.Value;
            DeleteEdge(edgeData);
        }

        public void SetCurrentDebugControl(string controlGuid)
        {
            CurrentDebugNode.Value = controlGuid;
        }

        public void SetNextDebugControl(string nextControlGuid, bool active)
        {
            GetNodeViewModelByGuid(nextControlGuid)?.SetNextDebugControl(active);
        }
        
        public void SetDebugBehaviourState(string controlGuid, State? controlRunnerLatestBehaviourState)
        {
            GetNodeViewModelByGuid(controlGuid)?.SetCurrentDebugBehaviourState(controlRunnerLatestBehaviourState);
        }

        public NodeViewModel CreateRoutingNode(NodeViewModel node1, NodeViewModel node2)
        {
            EdgeViewModel oldEdge = GetEdgeViewModel(node1, node2);
            //EdgeData oldEdge = Edges.Value.ToList().Find(x => x.StartNodeGuid == node1.guid && x.EndNodeGuid == node2.guid);
            if (oldEdge != null)
            {
                DeleteEdge(oldEdge);
                NodeViewModel routingNode = CreateNode();
                routingNode.SpecificControl.Value = NodeManager.GetControlInstance(typeof(RoutingControl));
                
                CreateEdge(node1, routingNode, oldEdge.StartPortType.Value, PortType.InOut);
                CreateEdge(routingNode, node2, PortType.InOut, oldEdge.EndPortType.Value);
                return routingNode;
            }
            return null;
        }

        public void NewData()
        {
            LoadData(new CanvasData());
            CanvasPath.Value = "";
        }

    }
}