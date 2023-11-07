using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Editor.ViewModels.UndoRedo;
using ControlCanvas.Runtime;
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

            BindViewCallbacks();
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

        private void BindViewCallbacks()
        {
            graphViewChanged += OnGraphViewChanged;
            OnSelectionChanged += OnSelectionChangedHandler;
            viewTransformChanged += OnViewTransformChanged;
            
            
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.C && evt.ctrlKey)
                {
                    CopySelection();
                }
                else if (evt.keyCode == KeyCode.V && evt.ctrlKey)
                {
                    Paste();
                }else if (evt.keyCode == KeyCode.D && evt.ctrlKey)
                {
                    DuplicateSelection();
                }else if (evt.keyCode == KeyCode.X)
                {
                    CutSelection();
                }
                
                else if (evt.keyCode == KeyCode.Z && evt.ctrlKey)
                {
                    CommandManager.Undo();
                }else if (evt.keyCode == KeyCode.Y && evt.ctrlKey)
                {
                    CommandManager.Redo();
                }
                
            });
        }


        private void CopySelection()
        {
            CanvasData canvasDataSelection = new CanvasData();
            selection.ToList().ForEach(x =>
            {
                if (x is IVisualNode node)
                {
                    canvasDataSelection.Nodes.Add(node.GetViewModel().DataProperty.Value);
                }
                else if (x is Edge edge)
                {
                    if (edge.input.node is IVisualNode inputNode && edge.output.node is IVisualNode outputNode)
                    {
                        canvasDataSelection.Edges.Add(viewModel.CanvasViewModel.GetEdgeViewModel(inputNode.GetViewModel(),
                            outputNode.GetViewModel()).DataProperty.Value);
                    }
                }
            });
            string selectionXML = XMLHelper.SerializeToXML(canvasDataSelection);
            EditorGUIUtility.systemCopyBuffer = selectionXML;
        }
        private void Paste(bool changeGuid = true, Vector2 generalOffset = default)
        {
            string selectionXML = EditorGUIUtility.systemCopyBuffer;
            CanvasData canvasDataSelection = XMLHelper.DeserializeFromXML(selectionXML);
            if (changeGuid)
            {
                canvasDataSelection.ReassignGuids();
            }
            float centerX = canvasDataSelection.Nodes.Average(x => x.position.x);
            float centerY = canvasDataSelection.Nodes.Average(x => x.position.y);
            Vector2 average = new Vector2(centerX, centerY);
            List<IViewModel> viewModels = new List<IViewModel>();
            canvasDataSelection.Nodes.ForEach(nodeData =>
            {
                Vector2 localOffset = nodeData.position - average;
                viewModels.Add(viewModel.AddNode(nodeData, mousePosition, localOffset + generalOffset));
            });
            canvasDataSelection.Edges.ForEach(edgeData =>
            {
                viewModels.Add(viewModel.AddEdge(edgeData));
            });
            
            ClearSelection();
            foreach (IViewModel model in viewModels)
            {
                if (model is NodeViewModel nodeViewModel)
                {
                    AddToSelection(FindVisualNode(nodeViewModel) as Node);
                }else if (model is EdgeViewModel edgeViewModel)
                {
                    AddToSelection(FindVisualEdge(edgeViewModel) as Edge);
                }
            }
        }
        private void CutSelection()
        {
            CopySelection();
            DeleteSelection();
        }

        private void DuplicateSelection()
        {
            CopySelection();
            Paste(true, new Vector2(10,-10));
        }


        public void UnsetViewModel()
        {
            
        }

        private void BindViewToVM()
        {
            viewModel.Nodes.SubscribeAndProcessExisting(x =>
            {
                if (viewModel.GetChildViewModel(x) is NodeViewModel nodeViewModel)
                {
                    CreateVisualNode(nodeViewModel.DataProperty.Value);
                    nodeViewModel.SpecificControl.Pairwise().Subscribe(pair =>
                    {
                        bool typeChange = pair.Previous is IRouting ^ pair.Current is IRouting;

                        if (typeChange)
                        {
                            RemoveVisualNode(FindVisualNode(nodeViewModel));
                            CreateVisualNode(nodeViewModel.DataProperty.Value);
                        }
                    }).AddTo(disposables);
                }
                
                
            }).AddTo(disposables);
            viewModel.Nodes.ObserveRemove().Subscribe(x => RemoveVisualNode(FindVisualNode(x.Value))).AddTo(disposables);
            
            viewModel.Edges.SubscribeAndProcessExisting(x => CreateVisualEdge(x)).AddTo(disposables);
            viewModel.Edges.ObserveRemove().Subscribe(x => RemoveVisualEdge(x.Value)).AddTo(disposables);
        }

        private void UnbindViewFromVM()
        {
            disposables.Clear();
        }

        //Override to disable to allow for own implementation
        protected override bool canPaste { get; } = false;
        protected override bool canCopySelection { get; } = false;
        protected override bool canCutSelection { get; } = false;
        //protected override bool canDeleteSelection { get; } = false;
        protected override bool canDuplicateSelection { get; } = false;

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
                        var edgeVm = CreateEdge(outputNode.GetViewModel(), inputNode.GetViewModel(), edge.output, edge.input);
                        _visualEdgeMap.Add(edgeVm.DataProperty.Value, edge);
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

        private EdgeViewModel CreateEdge(NodeViewModel getViewModel, NodeViewModel nodeViewModel, Port outputPort, Port inputPort)
        {
            PortType outputPortType = VisualNodeView.PortNameToType(outputPort.name);
            PortType inputPortType = VisualNodeView.PortNameToType(inputPort.name);
            return viewModel.CreateEdge(getViewModel, nodeViewModel, outputPortType, inputPortType);
        }

        
        private void CreateVisualNode(NodeData nodeData)
        {
            NodeViewModel nodeViewModel = (NodeViewModel)viewModel.GetChildViewModel(nodeData);
            if (nodeViewModel.SpecificControl.Value is IRouting)
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

        private IVisualNode FindVisualNode(NodeViewModel nodeViewModel)
        {
            return nodes.ToList().Find(x => x is IVisualNode node && node.GetViewModel() == nodeViewModel) as IVisualNode;
        }
        
        private IVisualNode FindVisualNode(NodeData nodeData)
        {
            return nodes.ToList().Find(x => x is IVisualNode node && node.GetVmGuid() == nodeData.guid) as IVisualNode;
        }
        
        private void RemoveVisualNode(IVisualNode iNode)
        {
            if (iNode != null && iNode is Node node)
            {
                IView<NodeViewModel> visualNode = node as IView<NodeViewModel>;
                if (visualNode == null)
                {
                    Debug.LogWarning($"Could not find node {iNode.GetVmGuid()} as visual node");
                }
                else
                {
                    visualNode.UnsetViewModel();
                }
                RemoveElement(node);
            }
        }

        private void CreateVisualEdge(EdgeData edgeData)
        {
            if (_ignoreAddEdge)
                return;

            EdgeViewModel edgeViewModel = (EdgeViewModel)viewModel.GetChildViewModel(edgeData);
            Edge edgeView = new Edge();
            SetEdgeViewModel(edgeView, edgeViewModel);
            _visualEdgeMap.Add(edgeData, edgeView);
            AddElement(edgeView);
        }

        private Edge FindVisualEdge(EdgeViewModel edgeViewModel)
        {
            return FindVisualEdge(edgeViewModel.DataProperty.Value);
        }
        
        private Edge FindVisualEdge(EdgeData edgeData)
        {
            return _visualEdgeMap.TryGetValue(edgeData, out Edge edge) ? edge : null;
        }
        
        private void SetEdgeViewModel(Edge edgeView, EdgeViewModel edgeViewModel)
        {
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
            if (_visualEdgeMap.ContainsKey(edgeData))
            {
                RemoveElement(_visualEdgeMap[edgeData]);
                _visualEdgeMap.Remove(edgeData);
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

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
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