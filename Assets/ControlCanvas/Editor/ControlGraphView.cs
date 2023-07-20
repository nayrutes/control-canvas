using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor
{
    public class ControlGraphView : GraphView
    {
        private ControlCanvasSO currentCanvas;
        private bool _ignoreChanges;
        
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

            this.viewDataKey = "ControlGraphView";
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ControlCanvas/Editor/ControlCanvasEditorWindow.uss");
            styleSheets.Add(styleSheet);
            graphViewChanged += OnGraphViewChanged;
            OnSelectionChanged += OnSelectionChangedHandler;
        }

        private void OnSelectionChangedHandler(SelectedChangedArgs obj)
        {
            //Debug log
            foreach (var element in obj.Selectables)
            {
                Debug.Log(element);
            }
        }

        ~ControlGraphView()
        {
            graphViewChanged -= OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
        {
            if (_ignoreChanges)
                return graphviewchange;
            
            if (graphviewchange.movedElements != null)
            {
                foreach (var element in graphviewchange.movedElements)
                {
                    // if (element is VisualNode node)
                    // {
                    //     currentCanvas.NodesCC.Find(x => x.Guid == node.node.Guid).Position = node.GetPosition().position;
                    // }
                }
            }

            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    if (edge.input.node is VisualNode inputNode && edge.output.node is VisualNode outputNode)
                    {
                        currentCanvas.CreateEdge(inputNode.node, outputNode.node);
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
                    if (element is VisualNode node)
                    {
                        currentCanvas.DeleteNode(node.node);
                    }
                    else if (element is UnityEditor.Experimental.GraphView.Edge edge)
                    {
                        if (edge.input.node is VisualNode inputNode && edge.output.node is VisualNode outputNode)
                        {
                            currentCanvas.DeleteEdge(currentCanvas.EdgesCC.Find(x => x.StartNodeGuid == inputNode.node.Guid && x.EndNodeGuid == outputNode.node.Guid));
                        }
                    }
                }
            }

            return graphviewchange;
        }

        public void PopulateView(ControlCanvasSO mControlCanvasSo)
        {
            currentCanvas = mControlCanvasSo;
            ClerView();
            if(currentCanvas == null)
                return;
            foreach (var node in mControlCanvasSo.NodesCC)
            {
                CreateVisualNode(node);
            }

            foreach (var edge in mControlCanvasSo.EdgesCC)
            {
                var startNode = nodes.ToList().Find(x => x is VisualNode node && node.node.Guid == edge.StartNodeGuid);
                var endNode = nodes.ToList().Find(x => x is VisualNode node && node.node.Guid == edge.EndNodeGuid);
                if (startNode != null && endNode != null)
                {
                    var edgeGV = new UnityEditor.Experimental.GraphView.Edge();
                    edgeGV.input = startNode.inputContainer.Q<Port>();
                    edgeGV.output = endNode.outputContainer.Q<Port>();
                    //edgeGV.capabilities &= ~Capabilities.Deletable;
                    
                    AddElement(edgeGV);
                }
            }
        }

        void CreateVisualNode(Node node)
        {
            VisualNode visualNode = new VisualNode(node);
            AddElement(visualNode);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);
            if (currentCanvas == null)
            {
                evt.menu.AppendAction("No Canvas selected", a => {}, DropdownMenuAction.AlwaysDisabled);
                return;
            }
            evt.menu.AppendAction("Create Node", (a) => CreateNode(), DropdownMenuAction.AlwaysEnabled);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(
                port => port.direction != startPort.direction && 
                        port.node != startPort.node).ToList();
        }

        private void CreateNode()
        {
            var node = currentCanvas.CreateNode();
            CreateVisualNode(node);
        }

        public void ClerView()
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