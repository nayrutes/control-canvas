using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
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


        public Subject<Unit> OnDispose { get; }

        public IViewModel GetChildViewModel(object data)
        {
            return canvasViewModel.GetChildViewModel(data);
        }

        public Dictionary<string, IDisposable> GetAllReactiveProperties()
        {
            return canvasViewModel.GetAllReactiveProperties();
        }

        public IDisposable GetReactiveProperty(string fieldName)
        {
            return canvasViewModel.GetReactiveProperty(fieldName);
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

        // public void CreateEdge(NodeViewModel startNode, NodeViewModel endNode) =>
        //     canvasViewModel.CreateEdge(startNode.DataProperty.Value, endNode.DataProperty.Value);

        
        public EdgeViewModel CreateEdge(NodeViewModel outputNodeNodeViewModel, NodeViewModel inputNodeNodeViewModel, PortType outputPortType, PortType inputPortType)
        {
            return canvasViewModel.CreateEdge(outputNodeNodeViewModel, inputNodeNodeViewModel, outputPortType, inputPortType);
        }
        
        public void CreateRoutingNode(NodeViewModel startNodeNodeViewModel, NodeViewModel endNodeNodeViewModel,
            Vector2 vector2)
        {
            NodeViewModel nvm = canvasViewModel.CreateRoutingNode(startNodeNodeViewModel, endNodeNodeViewModel);
            nvm.Position.Value = vector2;
        }
        public void DeleteEdge(EdgeData edgeData) => canvasViewModel.DeleteEdge(edgeData);
        public void DeleteEdge(EdgeViewModel edgeViewModel) => canvasViewModel.DeleteEdge(edgeViewModel);

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