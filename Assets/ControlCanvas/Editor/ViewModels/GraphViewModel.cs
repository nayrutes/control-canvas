using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Serialization;
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
        public IEnumerable<EdgeData> Edges => canvasViewModel.Edges.Value;
        public IEnumerable<NodeData> Nodes => canvasViewModel.Nodes.Value;

        public NodeData CreateNode() => canvasViewModel.CreateNode();
        
        public void DeleteNode(NodeViewModel nodeViewModel) => canvasViewModel.DeleteNode(nodeViewModel.DataProperty.Value);
        
        public EdgeData CreateEdge(NodeViewModel startNode, NodeViewModel endNode) => canvasViewModel.CreateEdge(startNode.DataProperty.Value, endNode.DataProperty.Value);
        
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
            Debug.LogWarning($"Dispose was not called on {this.GetType()}. You should call Dispose on IDisposable objects when you are done using them.");
            Dispose(false);
        }
    }
}