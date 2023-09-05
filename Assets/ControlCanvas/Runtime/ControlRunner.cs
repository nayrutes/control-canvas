using System;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlRunner : MonoBehaviour
    {
        public ReactiveProperty<IControl> currentControl = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        public string path = "Assets/ControlFlows/StateFlowEx4.xml";
        
        StateRunner stateRunner = new ();
        DecisionRunner<IControl> decisionRunner = new();
        BehaviourRunner behaviourRunner = new();
        private ReactiveProperty<Mode> _mode = new();
        private bool _waitFrameRequested;
        private IControl _nextSuggestedControl;

        private void Start()
        {
            XMLHelper.DeserializeFromXML(path, out controlFlow);
            //AgentContext = GetComponent<ControlAgent>();
            if (controlFlow.InitialNode == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            IControl initialControl = NodeManager.Instance.GetControlForNode(controlFlow.InitialNode, controlFlow);
            _nextSuggestedControl = initialControl;
            _mode.Value = Mode.SubUpdate;
            stateRunner.Init(AgentContext, controlFlow, this);
            //stateRunner.currentState.Subscribe(x => currentControl.Value = x);
            
            decisionRunner.Init(AgentContext, controlFlow);
            //decisionRunner.CurrentDecision.Subscribe(x => currentControl.Value = x);
           // _mode.Subscribe(x => decisionRunner.SetMode(x));
            
            behaviourRunner.Init(AgentContext, controlFlow, this);
            //behaviourRunner.CurrentBehaviour.Subscribe(x => currentControl.Value = x);
            //_mode.Subscribe(x => behaviourRunner.SetMode(x));
        }

        private void FixedUpdate()
        {
            //stateRunner.DoUpdate();
            //behaviourRunner.DoUpdate();
        }

        [ContextMenu("DoUpdate")]
        private void DoUpdate()
        {
            if(_mode.Value == Mode.CompleteUpdate)
                DoCompleteUpdate();
            else
                DoSubUpdate();
        }
        private void DoCompleteUpdate()
        {
            while (_nextSuggestedControl != currentControl.Value)
            {
                DoSubUpdate();
            }
        }
        
        private void DoSubUpdate()
        {
            if (_nextSuggestedControl == null)
            {
                Debug.LogError("No next suggested control");
                return;
            }
            currentControl.Value = _nextSuggestedControl;
            _nextSuggestedControl = null;
            if (currentControl.Value is not IDecision)
            {
                decisionRunner.ClearTracker();
            }
            switch (currentControl.Value)
            {
                case IState state:
                    _nextSuggestedControl = stateRunner.DoUpdate(state);
                    break;
                case IDecision decision:
                    _nextSuggestedControl = decisionRunner.DoUpdate(decision);
                    break;
                case IBehaviour behaviour:
                    _nextSuggestedControl = behaviourRunner.DoUpdate(behaviour);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public IControl GetNextForNode(NodeData nodeData, CanvasData controlFlow)
        {
            IControl control = NodeManager.Instance.GetControlForNode(nodeData.guid, controlFlow);
            return control;
        }
        
        [ContextMenu("AutoNext")]
        public void AutoNext()
        { 
            EdgeData edgeData = controlFlow.Edges.First(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(currentControl.Value));
            NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            //IState state = nodeData.specificState;
            currentControl.Value = GetNextForNode(nodeData, controlFlow);
        }
        

        [ContextMenu("SetModeCompleteUpdate")]
        public void SetModeCompleteUpdate()
        {
            _mode.Value = Mode.CompleteUpdate;
        }
        
        [ContextMenu("SetModeSubUpdate")]
        public void SetModeSubUpdate()
        {
            _mode.Value = Mode.SubUpdate;
        }

    }

    public enum Mode
    {
        CompleteUpdate,
        SubUpdate
    }
}