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
            return mousePosition = evt.mousePosition;
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
                    if (element is VisualNodeView node)
                    {
                        //viewModel.Nodes.ToList().Find(x => x.Guid.Value == node.nodeViewModel.Guid.Value).Position.Value = node.GetPosition().position;
                    }
                }
            }

            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    if (edge.input.node is VisualNodeView inputNode && edge.output.node is VisualNodeView outputNode)
                    {
                        _ignoreAddEdge = true;
                        viewModel.CreateEdge(outputNode.nodeViewModel, inputNode.nodeViewModel, edge.output.portName, edge.input.portName);
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
                    if (element is VisualNodeView node)
                    {
                        viewModel.DeleteNode(node.nodeViewModel);
                    }
                    else if (element is UnityEditor.Experimental.GraphView.Edge edge)
                    {
                        //RemoveVisualEdge(edge);
                        if (edge.input.node is VisualNodeView inputNode &&
                            edge.output.node is VisualNodeView outputNode)
                        {
                            viewModel.DeleteEdge(viewModel.Edges.ToList().Find(x =>
                                x.StartNodeGuid == outputNode.nodeViewModel.Guid.Value &&
                                (x.StartPortName == null || x.StartPortName == edge.output.portName) &&
                                x.EndNodeGuid == inputNode.nodeViewModel.Guid.Value &&
                                (x.EndPortName == null || x.EndPortName == edge.input.portName)));

                        }
                    }
                }
            }

            return graphviewchange;
        }

        void CreateVisualNode(NodeData nodeData)
        {
            VisualNodeView visualNodeView = new VisualNodeView();
            NodeViewModel nodeViewModel = (NodeViewModel)viewModel.GetChildViewModel(nodeData);
            visualNodeView.SetViewModel(nodeViewModel);
            AddElement(visualNodeView);
        }

        private void RemoveVisualNode(NodeData nodeData)
        {
        }

        private void CreateVisualEdge(EdgeData edgeData)
        {
            if (_ignoreAddEdge)
                return;

            var startNode = nodes.ToList().Find(x =>
                x is VisualNodeView node && node.nodeViewModel.Guid.Value == edgeData.StartNodeGuid);
            var endNode = nodes.ToList().Find(x =>
                x is VisualNodeView node && node.nodeViewModel.Guid.Value == edgeData.EndNodeGuid);
            if (startNode != null && endNode != null)
            {
                var edgeGV = new UnityEditor.Experimental.GraphView.Edge();
                Port portIn = endNode.inputContainer.Q<Port>();
                Port portOut = startNode.outputContainer.Q<Port>();
                if (edgeData.StartPortName is not (null or ""))
                {
                    portOut = startNode.Q<Port>(edgeData.StartPortName);
                }
                if (edgeData.EndPortName is not (null or ""))
                {
                    portIn = endNode.Q<Port>(edgeData.EndPortName);
                }
                edgeGV.input = portIn;
                edgeGV.output = portOut;
                
                //edgeGV.capabilities &= ~Capabilities.Deletable;

                AddElement(edgeGV);
            }
        }

        private void RemoveVisualEdge(EdgeData edgeData)
        {
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