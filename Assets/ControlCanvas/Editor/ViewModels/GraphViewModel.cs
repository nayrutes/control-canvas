using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Serialization;

namespace ControlCanvas.Editor.ViewModels
{
    public class GraphViewModel : IViewModel
    {
        CanvasViewModel canvasViewModel;
        
        public GraphViewModel(CanvasViewModel canvasViewModel)
        {
            this.canvasViewModel = canvasViewModel;
        }

        public void Dispose()
        {
            
        }

        public IViewModel GetChildViewModel(object data)
        {
            return canvasViewModel.GetChildViewModel(data);
        }

        //public List<EdgeViewModel> Edges=> canvasViewModel.EdgeViewModels.ToList();
        //public IEnumerable<NodeViewModel> Nodes => canvasViewModel.NodeViewModels;
        public CanvasViewModel CanvasViewModel => canvasViewModel;
        public IEnumerable<EdgeData> Edges => canvasViewModel.Edges.Value;
        public IEnumerable<NodeData> Nodes => canvasViewModel.Nodes.Value;

        public NodeData CreateNode() => canvasViewModel.CreateNode();
        
        public void DeleteNode(NodeViewModel nodeViewModel) => canvasViewModel.DeleteNode(nodeViewModel.DataProperty.Value);
        
        public EdgeData CreateEdge(NodeViewModel startNode, NodeViewModel endNode) => canvasViewModel.CreateEdge(startNode.DataProperty.Value, endNode.DataProperty.Value);
        
        public void DeleteEdge(EdgeData edgeData) => canvasViewModel.DeleteEdge(edgeData);
    }
}