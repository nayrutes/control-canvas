using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Guid { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ReactiveProperty<SerializableVector2>();
        public ReactiveCommand<NodeViewModel> MakeStartNodeCommand { get; set; } = new();

        public ReactiveProperty<string> ClassName { get;} = new();
        public List<string> ClassChoices { get;} = new();

        public ReactiveProperty<IControl> specificControl { get; } = new();
        public ReactiveProperty<bool> IsInitialNode { get; private set; } = new();
        
        public ReactiveProperty<bool> IsCurrentDebugNode { get; private set; } = new();
        public ReactiveProperty<bool> IsNextDebugNode { get; private set; } = new();

        public ReactiveProperty<State?> CurrentDebugBehaviourState { get; private set; } = new();
        
        //[NonSerialized]
        public Blackboard blackboardCanvas;

        //[NonSerialized]
        public ControlAgent controlAgent;

        //public ReactiveProperty<NodeType> NodeType = new();

        public NodeViewModel(NodeData nodeData) : base(nodeData)
        {
            InitNode();
        }
        
        public NodeViewModel() : base()
        {
            InitNode();
        }
        
        public NodeViewModel(NodeData data, bool autobind) : base(data, autobind)
        {
            InitNode();
        }

        void InitNode()
        {
            ClassChoices.Clear();
            ClassChoices.Add("None");
            ClassChoices.AddRange(NodeManager.GetSpecificTypes());
            
            specificControl.Subscribe(control =>
            {
                if (control != null)
                {
                    ClassName.Value = control.GetType().Name;
                }
                
            }).AddTo(disposables);
            
            ClassName.Subscribe(className =>
            {
                if (className == null || className == "None")
                {
                    specificControl.Value = null;
                }
                else if(specificControl.Value == null || specificControl.Value.GetType().Name != className)
                {
                    if (NodeManager.TryGetInstance(className, out var instance))
                    {
                        specificControl.Value = instance;
                    }
                    else
                    {
                        Debug.LogError($"Could not get instance of {className}");
                        specificControl.Value = null;
                    }
                }
            }).AddTo(disposables);
        }
        
        protected override NodeData CreateData()
        {
            return CreateNodeData();
        }
        
        public static NodeData CreateNodeData()
        {
            NodeData newData = new();
            newData.name = "New Node";
            newData.guid = System.Guid.NewGuid().ToString();
            return newData;
        }

        public void SetCurrentDebugBehaviourState(State? controlRunnerLatestBehaviourState)
        {
            CurrentDebugBehaviourState.Value = controlRunnerLatestBehaviourState;
        }

        public void SetNextDebugControl(bool active)
        {
            IsNextDebugNode.Value = active;
        }
    }
}