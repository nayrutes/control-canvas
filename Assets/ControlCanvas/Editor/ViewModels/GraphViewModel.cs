using System;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels
{
    public class GraphViewModel : IViewModel
    {
        CanvasViewModel canvasViewModel;

        public GraphViewModel(CanvasViewModel canvasViewModel)
        {
            this.canvasViewModel = canvasViewModel;
        }


        public IViewModel GetChildViewModel(object data)
        {
            return canvasViewModel.GetChildViewModel(data);
        }

        public CanvasViewModel CanvasViewModel => canvasViewModel;
        public ReactiveCollection<EdgeData> Edges => canvasViewModel.Edges.Value;
        public ReactiveCollection<NodeData> Nodes => canvasViewModel.Nodes.Value;

        public void CreateNode(Vector2 mousePosition)
        {
            NodeViewModel nodeVm = canvasViewModel.CreateNode();
            nodeVm.Position.Value = mousePosition;
        }

        public void DeleteNode(NodeViewModel nodeViewModel) =>
            canvasViewModel.DeleteNode(nodeViewModel.DataProperty.Value);

        public void CreateEdge(NodeViewModel startNode, NodeViewModel endNode) =>
            canvasViewModel.CreateEdge(startNode.DataProperty.Value, endNode.DataProperty.Value);

        
        public void CreateEdge(NodeViewModel outputNodeNodeViewModel, NodeViewModel inputNodeNodeViewModel, string outputPortName, string inputPortName)
        {
            canvasViewModel.CreateEdge(outputNodeNodeViewModel.DataProperty.Value, inputNodeNodeViewModel.DataProperty.Value, outputPortName, inputPortName);
        }
        
        public void DeleteEdge(EdgeData edgeData) => canvasViewModel.DeleteEdge(edgeData);

        private void ReleaseUnmanagedResources()
        {
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                canvasViewModel?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GraphViewModel()
        {
            Debug.LogWarning(
                $"Dispose was not called on {this.GetType()}. You should call Dispose on IDisposable objects when you are done using them.");
            Dispose(false);
        }

    }
}