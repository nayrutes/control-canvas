using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Serialization;
using UniRx;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class RoutingNodeView : Node, IView<NodeViewModel>, IVisualNode
    {
        NodeViewModel nodeViewModel;
        public Port inOutPort;
        private CompositeDisposable disposables = new CompositeDisposable();

        public RoutingNodeView() : base("Assets/ControlCanvas/Editor/RoutingNodeUXML.uxml")
        {
            title = "Routing";
        }

        public void SetViewModel(NodeViewModel nodeViewModel)
        {
            UnbindViewFromViewModel();
            UnbindViewModelFromView();
            
            this.nodeViewModel = nodeViewModel;
            CreatePorts();
            BindViewToViewModel();
            BindViewModelToView();
        }
        
        public void CreatePorts()
        {
            inOutPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inOutPort.portName = "In/Out";
            inOutPort.name = "In/Out";
            mainContainer.Q<VisualElement>("in-out-port").Add(inOutPort);
        }

        private void BindViewToViewModel()
        {
            nodeViewModel.Position.CombineLatest(nodeViewModel.Size, (position, size) => new Rect(position, size))
                .Subscribe(rect => this.SetPosition(rect)).AddTo(disposables);
        }

        private void UnbindViewFromViewModel()
        {
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void BindViewModelToView()
        {
            RegisterCallback((GeometryChangedEvent evt) => OnGeometryChanged(evt));
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            nodeViewModel.Position.Value = evt.newRect.position;
            nodeViewModel.Size.Value = evt.newRect.size;
        }
        
        private void UnbindViewModelFromView()
        {
            UnregisterCallback((GeometryChangedEvent evt) => OnGeometryChanged(evt));
        }
        
        public string GetVmGuid()
        {
            return nodeViewModel.Guid.Value;
        }

        public Port GetPort(PortType portType)
        {
            return inOutPort;
        }

        public NodeViewModel GetViewModel()
        {
            return nodeViewModel;
        }

        public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            collectedElementSet.UnionWith(inOutPort.connections);
        }
    }
}