using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlRunner : MonoBehaviour
    {
        public ReactiveProperty<IControl> CurrentControl { get; private set; } = new ReactiveProperty<IControl>();
        public Subject<IControl> StepDone { get; } = new Subject<IControl>();

        public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        [SerializeField]
        private ControlAgent agentContext;

        [SerializeField]
        private string path = "Assets/ControlFlows/StateFlowEx4.xml";

        private CanvasData controlFlow;

        private StateRunner stateRunner = new StateRunner();
        private DecisionRunner<IControl> decisionRunner = new DecisionRunner<IControl>();
        private BehaviourRunner behaviourRunner = new BehaviourRunner();

        private ReactiveProperty<Mode> mode = new ReactiveProperty<Mode>();
        private IControl nextSuggestedControl;
        private IControl initialControl;
        private bool stopped = true;
        private bool startedComplete;
        private bool _autoRestart = true;
        private List<IBehaviour> _btTracker = new();

        public State LatestBehaviourState => behaviourRunner.LastCombinedResult;

        //public IControl LatestPop => behaviourRunner.LatestPop;

        private void Start()
        {
            InitializeControlFlow();
            InitializeRunners();
        }

        private void FixedUpdate()
        {
            if (!stopped)
                UpdateControlFlow();
        }

        private void InitializeControlFlow()
        {
            XMLHelper.DeserializeFromXML(path, out controlFlow);
            if (controlFlow.InitialNode == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            initialControl = NodeManager.Instance.GetControlForNode(controlFlow.InitialNode, controlFlow);
            nextSuggestedControl = initialControl;
            mode.Value = Mode.CompleteUpdate;
            if (agentContext == null)
            {
                Debug.LogError("No agent context set");
            }
        }

        private void InitializeRunners()
        {
            stateRunner.Init(agentContext, controlFlow, this);
            decisionRunner.Init(agentContext, controlFlow);
            behaviourRunner.Init(agentContext, controlFlow, this);
        }

        private void UpdateControlFlow()
        {
            if (mode.Value == Mode.CompleteUpdate)
                CompleteUpdate();
            else
                SubUpdate();
        }

        bool _safetyBreak = false;
        bool _running = false;
        private void CompleteUpdate()
        {
            _running = true;
            while (_running)
            {
                if (_safetyBreak)
                    return;
                
                if (IsUpdateComplete())
                {
                    _running = false;
                    
                }
                SubUpdate();

                startedComplete = true;
            }
            startedComplete = false;
        }

        private bool IsUpdateComplete()
        {
            return nextSuggestedControl == CurrentControl.Value || (nextSuggestedControl == initialControl && startedComplete);
        }

        private void SubUpdate()
        {
            if (nextSuggestedControl == null)
            {
                if (_autoRestart)
                {
                    Debug.Log("Restarting control flow because no next suggested control");
                    nextSuggestedControl = initialControl;
                    ClearBtMarker();
                }
                else
                {
                    Debug.LogError("No next suggested control");
                    return;   
                }
            }
            CurrentControl.Value = nextSuggestedControl;
            nextSuggestedControl = null;
            UpdateBasedOnControlType();
            StepDone.OnNext(CurrentControl.Value);
        }

        private void UpdateBasedOnControlType()
        {
            switch (CurrentControl.Value)
            {
                case IState state:
                    nextSuggestedControl = stateRunner.DoUpdate(state);
                    break;
                case IDecision decision:
                    nextSuggestedControl = decisionRunner.DoUpdate(decision);
                    ClearDecisionTrackerIfNecessary();
                    break;
                case IBehaviour behaviour:
                    _btTracker.Add(behaviour);
                    nextSuggestedControl = behaviourRunner.DoUpdate(behaviour);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearDecisionTrackerIfNecessary()
        {
            if (nextSuggestedControl is not IDecision)
            {
                decisionRunner.ClearTracker();
            }
        }

        private void ClearBtMarker()
        {
            ClearingBt.OnNext(_btTracker);
            _btTracker.Clear();
        }
        
        public IControl GetNextForNode(NodeData nodeData, CanvasData controlFlow)
        {
            return NodeManager.Instance.GetControlForNode(nodeData.guid, controlFlow);
        }

        [ContextMenu("AutoNext")]
        public void AutoNext()
        {
            EdgeData edgeData = controlFlow.Edges.First(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(CurrentControl.Value));
            NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            nextSuggestedControl = GetNextForNode(nodeData, controlFlow);
        }

        public void Play()
        {
            stopped = false;
            mode.Value = Mode.CompleteUpdate;
        }

        public void Stop()
        {
            stopped = true;
        }

        public void Step()
        {
            stopped = true;
            mode.Value = Mode.SubUpdate;
            SubUpdate();
        }
    }

    public enum Mode
    {
        CompleteUpdate,
        SubUpdate
    }
}
