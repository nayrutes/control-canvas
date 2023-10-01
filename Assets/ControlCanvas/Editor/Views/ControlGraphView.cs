using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Serialization;
using UniRx;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class ControlGraphView : GraphView, IView<GraphViewModel>
    {
        //private ControlCanvasSO currentCanvas;
        private bool _ignoreChanges;
        private GraphViewModel viewModel;

        Vector2 mousePosition = new Vector2();
        private CompositeDisposable disposables = new();
        private bool _ignoreAddEdge;
        private Dictionary<EdgeData,Edge> _visualEdgeMap = new();

        //Selected Objects changed
        public event Action<SelectedChangedArgs> OnSelectionChanged;

        public new class UxmlFactory : UxmlFactory<ControlGraphView, GraphView.UxmlTraits>
        {
        }

        public ControlGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            RegisterCallback<MouseMoveEvent>(evt => EvtMousePosition(evt));

            this.viewDataKey = "ControlGraphView";

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ControlCanvas/Editor/ControlCanvasEditorWindow.uss");
            styleSheets.Add(styleSheet);
        }

        private Vector2 EvtMousePosition(MouseMoveEvent evt)
        {
            //mouse position in the graph view
            return mousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
        }

        private void OnSelectionChangedHandler(SelectedChangedArgs obj)
        {
            //Debug log
            foreach (var element in obj.Selectables)
            {
                Debug.Log(element);
            }
        }

        public void SetViewModel(GraphViewModel viewModel)
        {
            this.viewModel = viewModel;

            graphViewChanged += OnGraphViewChanged;
            OnSelectionChanged += OnSelectionChangedHandler;
            viewTransformChanged += OnViewTransformChanged;
            viewDataKey = "ControlGraphView";

            this.viewModel.CanvasViewModel.DataProperty.DoWithLast(x =>
            {
                ClearView();
                UnbindViewFromVM();
            }).Subscribe(x =>
            {
                if (x != null)
                {
                    BindViewToVM();
                }
            });
        }

        private void BindViewToVM()
        {
            viewModel.Nodes.SubscribeAndProcessExisting(x => CreateVisualNode(x)).AddTo(disposables);
            viewModel.Nodes.ObserveRemove().Subscribe(x => RemoveVisualNode(x.Value)).AddTo(disposables);
            viewModel.Edges.SubscribeAndProcessExisting(x => CreateVisualEdge(x)).AddTo(disposables);
            viewModel.Edges.ObserveRemove().Subscribe(x => RemoveVisualEdge(x.Value)).AddTo(disposables);
        }

        private void UnbindViewFromVM()
        {
            disposables.Clear();
        }

        ~ControlGraphView()
        {
            graphViewChanged -= OnGraphViewChanged;
            OnSelectionChanged -= OnSelectionChangedHandler;
            viewTransformChanged -= OnViewTransformChanged;
            UnregisterCallback<MouseMoveEvent>(evt => EvtMousePosition(evt));
        }

        private void OnViewTransformChanged(GraphView graphview)
        {
            //This is saved by unity with viewDataKey = "ControlGraphView";
            //viewModel.SetViewTransform(graphview.viewTransform.position, graphview.viewTransform.scale);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
        {
            if (_ignoreChanges)
                return graphviewchange;

            if (graphviewchange.movedElements != null)
            {
                foreach (var element in graphviewchange.movedElements)
                {
                    if (element is IVisualNode node)
                    {
                        //viewModel.Nodes.ToList().Find(x => x.Guid.Value == node.nodeViewModel.Guid.Value).Position.Value = node.GetPosition().position;
                    }
                }
            }

            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    if (edge.input.node is IVisualNode inputNode && edge.output.node is IVisualNode outputNode)
                    {
                        _ignoreAddEdge = true;
                        CreateEdge(outputNode.GetViewModel(), inputNode.GetViewModel(), edge.output, edge.input);
                        _ignoreAddEdge = false;
                    }
                }
            }

            if (graphviewchange.moveDelta != Vector2.zero)
            {
            }

            if (graphviewchange.elementsToRemove != null)
            {
                foreach (var element in graphviewchange.elementsToRemove)
                {
                    if (element is IVisualNode node)
                    {
                        viewModel.DeleteNode(node.GetViewModel());
                    }
                    else if (element is Edge edge)
                    {
                        //RemoveVisualEdge(edge);
                        if (edge.input.node is IVisualNode inputNode &&
                            edge.output.node is IVisualNode outputNode)
                        {

                            viewModel.DeleteEdge(viewModel.CanvasViewModel.GetEdgeViewModel(inputNode.GetViewModel(),
                                outputNode.GetViewModel()));
                                
                            // viewModel.DeleteEdge(viewModel.Edges.ToList().Find(x =>
                            //     x.StartNodeGuid == outputNode.GetVmGuid() &&
                            //     (x.StartPortName == null || x.StartPortName == edge.output.portName) &&
                            //     x.EndNodeGuid == inputNode.GetVmGuid() &&
                            //     (x.EndPortName == null || x.EndPortName == edge.input.portName)));
                        
                        }
                    }
                }
            }

            return graphviewchange;
        }

        private void CreateEdge(NodeViewModel getViewModel, NodeViewModel nodeViewModel, Port outputPort, Port inputPort)
        {
            PortType outputPortType = VisualNodeView.PortNameToType(outputPort.name);
            PortType inputPortType = VisualNodeView.PortNameToType(inputPort.name);
            viewModel.CreateEdge(getViewModel, nodeViewModel, outputPortType, inputPortType);
        }

        private void CreateVisualNode(NodeData nodeData)
        {
            NodeViewModel nodeViewModel = (NodeViewModel)viewModel.GetChildViewModel(nodeData);
            if (nodeData.nodeType == NodeType.Routing)
            {
                RoutingNodeView routingNodeView = new RoutingNodeView();
                routingNodeView.SetViewModel(nodeViewModel);
                AddElement(routingNodeView);
            }
            else
            {
                VisualNodeView visualNodeView = new VisualNodeView();
                visualNodeView.SetViewModel(nodeViewModel);
                AddElement(visualNodeView);
            }
        }

        private void RemoveVisualNode(NodeData nodeData)
        {
        }

        private void CreateVisualEdge(EdgeData edgeData)
        {
            if (_ignoreAddEdge)
                return;

            EdgeViewModel edgeViewModel = (EdgeViewModel)viewModel.GetChildViewModel(edgeData);
            Edge edgeView = new Edge();
            SetEdgeViewModel(edgeView, edgeViewModel);
            AddElement(edgeView);
        }

        private void SetEdgeViewModel(Edge edgeView, EdgeViewModel edgeViewModel)
        {
            //edgeView.output = 
            
            IVisualNode startNode = nodes.ToList().Find(x =>
                x is IVisualNode node && node.GetVmGuid() == edgeViewModel.StartNodeGuid.Value) as IVisualNode;
            IVisualNode endNode = nodes.ToList().Find(x =>
                x is IVisualNode node && node.GetVmGuid() == edgeViewModel.EndNodeGuid.Value) as IVisualNode;
            if (startNode != null && endNode != null)
            {
                //var edgeGV = new Edge();
                PortType portTypeEnd = edgeViewModel.EndPortType.Value;
                edgeView.input = endNode.GetPort(portTypeEnd);
                
                PortType portTypeStart = edgeViewModel.StartPortType.Value;
                edgeView.output = startNode.GetPort(portTypeStart);
                
                //edgeGV.capabilities &= ~Capabilities.Deletable;

                //AddElement(edgeGV);
                //_visualEdgeMap.Add(edgeData, edgeGV);
            }
            else
            {
                if (startNode == null)
                {
                    Debug.LogWarning($"Could not find start node {edgeViewModel.StartNodeGuid.Value} for edge {edgeViewModel.Guid.Value} and end node {edgeViewModel.EndNodeGuid.Value}");
                }else if(endNode == null)
                {
                    Debug.LogWarning($"Could not find end node {edgeViewModel.EndNodeGuid.Value} for edge {edgeViewModel.Guid.Value} and start node {edgeViewModel.StartNodeGuid.Value}");
                }
            }
        }


        private void RemoveVisualEdge(EdgeData edgeData)
        {
            // if (_visualEdgeMap.ContainsKey(edgeData))
            // {
            //     RemoveElement(_visualEdgeMap[edgeData]);
            //     _visualEdgeMap.Remove(edgeData);
            // }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);
            // if (currentCanvas == null)
            // {
            //     evt.menu.AppendAction("No Canvas selected", a => {}, DropdownMenuAction.AlwaysDisabled);
            //     return;
            // }
            evt.menu.AppendAction("Create Node", (a) => viewModel.CreateNode(mousePosition),
                DropdownMenuAction.AlwaysEnabled);
            if (selection.All(x => x is Edge))
            {
                evt.menu.AppendAction("CreateRoutingNode", (a) =>
                    {
                        List<ISelectable> selectablesCopy = new List<ISelectable>(selection);
                        foreach (var selectable in selectablesCopy)
                        {
                            var edge = (Edge)selectable;
                            CreateRoutingNode(edge);
                        }
                    },
                    DropdownMenuAction.AlwaysEnabled);
            }
        }

        private void CreateRoutingNode(Edge edge)
        {
            if (edge.output.node is IVisualNode startNode && edge.input.node is IVisualNode endNode)
            {
                Vector2 pos = edge.edgeControl.controlPoints[0] + (edge.edgeControl.controlPoints[^1] - edge.edgeControl.controlPoints[0]) / 2;
                viewModel.CreateRoutingNode(startNode.GetViewModel(), endNode.GetViewModel(), pos);
                // CreateVisualNode(routingNode);
                // viewModel.CreateEdge(startNode.nodeViewModel, routingNode);
                // viewModel.CreateEdge(routingNode, endNode.nodeViewModel);
                // viewModel.DeleteEdge(viewModel.Edges.ToList().Find(x =>
                //     x.StartNodeGuid == startNode.nodeViewModel.Guid.Value &&
                //     (x.StartPortName == null || x.StartPortName == edge.output.portName) &&
                //     x.EndNodeGuid == endNode.nodeViewModel.Guid.Value &&
                //     (x.EndPortName == null || x.EndPortName == edge.input.portName)));
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(
                port => port.direction != startPort.direction &&
                        port.node != startPort.node).ToList();
        }

        // private void CreateNode()
        // {
        //     var node = ;
        //     //CreateVisualNode(node);
        // }

        public void ClearView()
        {
            _ignoreChanges = true;
            DeleteElements(graphElements);
            _ignoreChanges = false;
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            OnSelectionChanged?.Invoke(new SelectedChangedArgs()
            {
                Type = SelectedChangedArgs.ChangeType.Added,
                Selectables = selection.ToList(),
                CurrentChange = selectable as ISelectable
            });
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChanged?.Invoke(new SelectedChangedArgs()
            {
                Type = SelectedChangedArgs.ChangeType.Removed,
                Selectables = selection.ToList(),
                CurrentChange = selectable as ISelectable
            });
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnSelectionChanged?.Invoke(new SelectedChangedArgs()
            {
                Type = SelectedChangedArgs.ChangeType.Cleared,
                Selectables = selection.ToList(),
                CurrentChange = null
            });
        }
    }

    public class SelectedChangedArgs
    {
        public enum ChangeType
        {
            Added,
            Removed,
            Cleared
        }

        public ChangeType Type;
        public List<ISelectable> Selectables;
        public ISelectable CurrentChange;
    }
}