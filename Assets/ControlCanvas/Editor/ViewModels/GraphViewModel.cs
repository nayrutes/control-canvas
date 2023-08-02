using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.ViewModels.Base;

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
        
        public List<EdgeViewModel> Edges=> canvasViewModel.EdgeViewModels.ToList();
        public IEnumerable<NodeViewModel> Nodes => canvasViewModel.NodeViewModels;
        public CanvasViewModel CanvasViewModel => canvasViewModel;

        public NodeViewModel CreateNode() => canvasViewModel.CreateNode();
        
        public void DeleteNode(NodeViewModel nodeViewModel) => canvasViewModel.DeleteNode(nodeViewModel);
        
        public EdgeViewModel CreateEdge(NodeViewModel startNode, NodeViewModel endNode) => canvasViewModel.CreateEdge(startNode, endNode);
        
        public void DeleteEdge(EdgeViewModel edgeViewModel) => canvasViewModel.DeleteEdge(edgeViewModel);
    }
}