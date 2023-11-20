using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public class NodeViewModel : BaseViewModel<NodeData>
    {
        //Reactive properties with equivalent field in NodeData
        public ReactiveProperty<string> Name { get; } = new ();
        public ReactiveProperty<string> Guid { get; } = new ();
        public ReactiveProperty<SerializableVector2> Position { get; set; } = new ();
        public ReactiveProperty<SerializableVector2> Size { get; set; } = new ();
        public ReactiveProperty<IControl> SpecificControl { get; } = new();
        
        //Reactive properties for internal functions
        public ReactiveProperty<bool> IsInitialNode { get; private set; } = new();
        public ReactiveProperty<string> ClassName { get;} = new();
        public ReactiveProperty<bool> CoreDebugging { get; private set; } = new();
        public ReactiveProperty<bool> ExpandContent { get; private set; } = new(true);
        
        //Reactive properties for debugging
        public ReactiveProperty<bool> IsCurrentDebugNode { get; private set; } = new();
        public ReactiveProperty<bool> IsNextDebugNode { get; private set; } = new();
        public ReactiveProperty<bool> IsCurrentState { get; private set; } = new();
        public ReactiveProperty<State?> CurrentDebugBehaviourState { get; private set; } = new();
        
        //Commands
        public ReactiveCommand<NodeViewModel> MakeStartNodeCommand { get; set; } = new();

        //Internal properties
        public List<string> ClassChoices { get;} = new();
        


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
            
            SpecificControl.Subscribe(control =>
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
                    SpecificControl.Value = null;
                }
                else if(SpecificControl.Value == null || SpecificControl.Value.GetType().Name != className)
                {
                    if (NodeManager.TryGetInstance(className, out var instance))
                    {
                        SpecificControl.Value = instance;
                    }
                    else
                    {
                        Debug.LogError($"Could not get instance of {className}");
                        SpecificControl.Value = null;
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

        public void SetDebugLastState(bool active)
        {
            IsCurrentState.Value = active;
        }
    }
}