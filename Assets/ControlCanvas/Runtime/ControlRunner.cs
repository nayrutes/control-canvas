using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace ControlCanvas.Runtime
{

    
    public class ControlRunner : MonoBehaviour
    {
        private FlowManager _flowManager = new FlowManager();
        
        IControl CurrentControl => _flowManager.CurrentFlowTracker.control;
        CanvasData CurrentFlow => _flowManager.CurrentFlowTracker.flow;
        public Subject<IControl> StepDone { get; } = new Subject<IControl>();
        public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        [SerializeField]
        private ControlAgent agentContext;

        [FormerlySerializedAs("path")] [SerializeField]
        private string startPath = "";


        private Dictionary<Type, IRunnerBase> runnerDict = new ();
        
        private Dictionary<Type, Action<float>> updateByType = new ();
        

        private ReactiveProperty<Mode> mode = new ReactiveProperty<Mode>();
        private IControl nextSuggestedControl;
        private IControl initialControl;
        private bool stopped = true;
        private bool startedComplete;
        private bool _autoRestart = true;

        public State LatestBehaviourState => ((BehaviourRunner)runnerDict[typeof(IBehaviour)]).LastCombinedResult;
        public IObservable<FlowTracker> ControlFlowChanged => _flowManager.ControlFlowChanged.SkipWhile(_=>_running);

        private float _currentDeltaTimeForSubUpdate;

        private void Start()
        {
            mode.Value = Mode.CompleteUpdate;
            InitializeControlFlow(startPath);
            runnerDict.Add(typeof(IState), new StateRunner());
            runnerDict.Add(typeof(IDecision), new DecisionRunner());
            runnerDict.Add(typeof(IBehaviour), new BehaviourRunner());
            
            updateByType.Add(typeof(IState), RunRunner<IState>);
            updateByType.Add(typeof(IDecision), RunRunner<IDecision>);
            updateByType.Add(typeof(IBehaviour), RunRunner<IBehaviour>);
        }
        
        private void InitializeControlFlow(string currentPath)
        {
            CanvasData initControlFlow = null;
            
            _flowManager.SetCurrentFlow(currentPath);
            initialControl = NodeManager.Instance.GetInitControl(CurrentFlow);
            nextSuggestedControl = initialControl;
            if (agentContext == null)
            {
                Debug.LogError("No agent context set");
            }
        }

        private void FixedUpdate()
        {
            _currentDeltaTimeForSubUpdate += Time.fixedDeltaTime;
            if (!stopped)
                UpdateControlFlow();
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
            return nextSuggestedControl == CurrentControl || (nextSuggestedControl == initialControl && startedComplete);
        }

        private void SubUpdate()
        {
            if (nextSuggestedControl == null)
            {
                if (_autoRestart)
                {
                    Debug.Log("Restarting control flow because no next suggested control");
                    nextSuggestedControl = initialControl;
                    ResetRunner();
                }
                else
                {
                    Debug.LogError("No next suggested control");
                    return;   
                }
            }
            
            _flowManager.SetCurrentControlAndFlow(nextSuggestedControl);
            nextSuggestedControl = null;
            
            Type executionType = NodeManager.Instance.GetExecutionTypeOfNode(CurrentControl, CurrentFlow);
            updateByType[executionType](_currentDeltaTimeForSubUpdate);
            
            StepDone.OnNext(CurrentControl);
            _currentDeltaTimeForSubUpdate = 0;
        }

        private void RunRunner<T>(float deltaTime) where T : class, IControl
        {
            IRunner<T> runner = runnerDict[typeof(T)] as IRunner<T>;
            if (CurrentControl is ISubFlow subFlow)
            {
                _flowManager.CacheFlow(subFlow.GetSubFlowPath(agentContext));
            }
            
            runner.DoUpdate(CurrentControl as T, agentContext, deltaTime);
            
            IControl next = runner.GetNext(CurrentControl as T, CurrentFlow, agentContext, _flowManager.GetFlow);
            
            nextSuggestedControl = next;
        }

        private void ClearStateRunnerIfNecessary()
        {
            if (NodeManager.Instance.GetExecutionTypeOfNode(nextSuggestedControl, CurrentFlow) != typeof(IState))
            {
                runnerDict[typeof(IState)].ResetRunner(agentContext);
            }
        }
        
        private void ResetRunner()
        {
            runnerDict[typeof(IBehaviour)].ResetRunner(agentContext);
        }
        
        

        // [ContextMenu("AutoNext")]
        // public void AutoNext()
        // {
        //     
        //     nextSuggestedControl = NodeManager.Instance.GetNextForNode(CurrentControl, ControlFlow);
        //     ClearStateRunnerIfNecessary();
        // }

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
