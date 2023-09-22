using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{

    
    public class ControlRunner
    {
        private FlowManager _flowManager = new FlowManager();
        
        IControl CurrentControl => _flowManager.CurrentFlowTracker.control;
        CanvasData CurrentFlow => _flowManager.CurrentFlowTracker.flow;
        public Subject<IControl> StepDoneCurrent { get; } = new Subject<IControl>();
        public Subject<IControl> StepDoneNext { get; } = new Subject<IControl>();
        public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        
        private IControlAgent agentContext;

        private Dictionary<Type, IRunnerBase> runnerDict = new ();
        
        private Dictionary<Type, Action<float>> updateByType = new ();
        

        private ReactiveProperty<Mode> mode = new ReactiveProperty<Mode>();
        private IControl nextSuggestedControl;
        private IControl initialControl;
        private bool stopped = false;
        private bool startedComplete;
        private bool _autoRestart = true;

        public State LatestBehaviourState => ((BehaviourRunner)runnerDict[typeof(IBehaviour)]).LastCombinedResult;
        public IObservable<FlowTracker> ControlFlowChanged => _flowManager.ControlFlowChanged.SkipWhile(_=>_running);

        private float _currentDeltaTimeForSubUpdate;

        public void Initialize(string startPath, IControlAgent agentContext)
        {
            this.agentContext = agentContext;
            mode.Value = Mode.CompleteUpdate;
            InitializeControlFlow(startPath);
            runnerDict.Add(typeof(IState), new StateRunner(_flowManager, NodeManager.Instance));
            runnerDict.Add(typeof(IDecision), new DecisionRunner(_flowManager, NodeManager.Instance));
            runnerDict.Add(typeof(IBehaviour), new BehaviourRunner(_flowManager, NodeManager.Instance));
            
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

        public void RunningUpdate(float fixedDeltaTime)
        {
            _currentDeltaTimeForSubUpdate += fixedDeltaTime;
            if (!stopped)
                SingleUpdate();
        }
        public void SingleUpdate()
        {
            nextSuggestedControl ??= initialControl;

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
                    break;
                }
                SubUpdate();

                startedComplete = true;
            }
            startedComplete = false;
        }

        private bool IsUpdateComplete()
        {
            return nextSuggestedControl == null || nextSuggestedControl == CurrentControl;
            //return nextSuggestedControl == CurrentControl || (nextSuggestedControl == initialControl && startedComplete);
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
            
            StepDoneCurrent.OnNext(CurrentControl);
            StepDoneNext.OnNext(nextSuggestedControl);
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
            
            IControl next = runner.GetNext(CurrentControl as T, CurrentFlow, agentContext);
            
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

        public IControl GetNextSuggestion()
        {
            return nextSuggestedControl;
        }
    }

    public enum Mode
    {
        CompleteUpdate,
        SubUpdate
    }
}
