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
        private NodeManager _nodeManager = new NodeManager();
        
        //IControl CurrentControl => _flowManager.CurrentFlowTracker.control;
        CanvasData CurrentFlow => _flowManager.CurrentFlowTracker.flow;
        
        public NodeManager NodeManager => _nodeManager;
        
        public Subject<IControl> StepDoneCurrent { get; } = new Subject<IControl>();
        public Subject<IControl> NextPreview { get; } = new Subject<IControl>();
        public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        
        private IControlAgent agentContext;

        private Dictionary<Type, IRunnerBase> runnerDict = new ();
        
        private Dictionary<Type, Action<IControl, float>> updateByType = new ();
        private Dictionary<Type, Func<IControl, IControl>> nextByType = new ();
        

        private ReactiveProperty<Mode> mode = new ReactiveProperty<Mode>();
        //private IControl nextSuggestedControl;
        private IControl initialControl;
        private bool stopped = false;
        private bool startedComplete;
        private bool _autoRestart = true;

        public State LatestBehaviourState => ((BehaviourRunner)runnerDict[typeof(IBehaviour)]).GetLastCombinedResult();
        public IObservable<FlowTracker> ControlFlowChanged => _flowManager.ControlFlowChanged.SkipWhile(_=>_running);

        private float _currentDeltaTimeForSubUpdate;

        public void Initialize(string startPath, IControlAgent agentContext)
        {
            this.agentContext = agentContext;
            if (this.agentContext.BlackboardAgent == null)
            {
                Debug.LogError("No blackboard agent set");
                return;
            }
            if (this.agentContext.BlackboardFlowControl == null)
            {
                Debug.LogError("No blackboard-flowControl agent set");
                return;
            }
            
            mode.Value = Mode.CompleteUpdate;
            InitializeControlFlow(startPath);
            runnerDict.Add(typeof(IState), new StateRunner(_flowManager, _nodeManager));
            runnerDict.Add(typeof(IDecision), new DecisionRunner(_flowManager, _nodeManager));
            runnerDict.Add(typeof(IBehaviour), new BehaviourRunner(_flowManager, _nodeManager));
            
            updateByType.Add(typeof(IState), RunnerRun<IState>);
            updateByType.Add(typeof(IDecision), RunnerRun<IDecision>);
            updateByType.Add(typeof(IBehaviour), RunnerRun<IBehaviour>);
            
            nextByType.Add(typeof(IState), RunnerGetNext<IState>);
            nextByType.Add(typeof(IDecision), RunnerGetNext<IDecision>);
            nextByType.Add(typeof(IBehaviour), RunnerGetNext<IBehaviour>);
        }
        
        private void InitializeControlFlow(string currentPath)
        {
            CanvasData initControlFlow = null;
            
            _flowManager.SetCurrentFlow(currentPath);
            initialControl = _nodeManager.GetInitControl(CurrentFlow);
            if (initialControl == null)
            {
                Debug.LogWarning("No initial control found");
            }
            //nextSuggestedControl = initialControl;
            if (agentContext == null)
            {
                Debug.LogError("No agent context set");
            }
        }

        public void RunningUpdate(float fixedDeltaTime)
        {
            _currentDeltaTimeForSubUpdate += fixedDeltaTime;
            if (!stopped)
            {
                if (mode.Value == Mode.CompleteUpdate)
                    CompleteUpdate();
                else
                    SingleUpdate();
            }
            _currentDeltaTimeForSubUpdate = 0;
        }

        bool _safetyBreak = false;
        int _safetyCounter = 0;
        bool _running = false;
        private IControl _lastControl;
        private bool _previewNext;

        private void CompleteUpdate()
        {
            _running = true;
            _safetyCounter = 0;
            while (_running)
            {
                if (_safetyBreak || _safetyCounter++ > 1000)
                {
                    Debug.LogError("Safety break hit");
                    return;
                }
                IControl next = GetNext(_lastControl, !startedComplete);
                if (next == null)
                {
                    _running = false;
                    _lastControl = null;
                    break;
                }
                if (next == _lastControl && startedComplete)
                {
                    _running = false;
                    break;
                }
                SubUpdate(next);
                _lastControl = next;
                startedComplete = true;
            }
            startedComplete = false;
            PreviewNext();
        }

        void SingleUpdate()
        {
            IControl next = GetNext(_lastControl, true);
            SubUpdate(next);
            _lastControl = next;
        }
        
        private IControl GetNext(IControl last, bool allowRestart)
        {
            IControl next = null;
            Type executionType = _nodeManager.GetExecutionTypeOfNode(last, CurrentFlow);
            if (executionType != null)
            {
                next = nextByType[executionType](last);
            }
            
            if (next == null && allowRestart)
            {
                if (_autoRestart)
                {
                    Debug.Log("Restarting control flow because no next suggested control");
                    next = initialControl;
                    ResetRunner();
                }
                else
                {
                    Debug.LogError("No next suggested control");
                    return null;   
                }
            }
            return next;
        }
        
        private void SubUpdate(IControl current)
        {
            _flowManager.SetCurrentControlAndFlow(current, _nodeManager);
            
            Type executionType = _nodeManager.GetExecutionTypeOfNode(current, CurrentFlow);
            updateByType[executionType](current, _currentDeltaTimeForSubUpdate);
            
            StepDoneCurrent.OnNext(current);
        }

        private void RunnerRun<T>(IControl current, float deltaTime) where T : class, IControl
        {
            IRunner<T> runner = runnerDict[typeof(T)] as IRunner<T>;
            runner.DoUpdate(current as T, agentContext, deltaTime);
        }

        private IControl RunnerGetNext<T>(IControl current) where T : class, IControl
        {
            IRunner<T> runner = runnerDict[typeof(T)] as IRunner<T>;
            if (current is ISubFlow subFlow)
            {
                _flowManager.CacheFlow(subFlow.GetSubFlowPath(agentContext));
            }
            IControl next = runner.GetNext(current as T, CurrentFlow, agentContext);
            return next;
        }
        private void ResetRunner()
        {
            runnerDict[typeof(IBehaviour)].ResetRunner(agentContext);
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
            SingleUpdate();
            PreviewNext();
        }

        private void PreviewNext()
        {
            //TODO: not working because GetNext changes the flow when called
            // if (_previewNext)
            // {
            //     IControl next = GetNext(_lastControl, false);
            //     NextPreview.OnNext(next);
            // }
        }

        // public IControl GetNextSuggestion()
        // {
        //     return nextSuggestedControl;
        // }
        public void EnablePreview(bool b)
        {
            _previewNext = b;
        }
    }

    public enum Mode
    {
        CompleteUpdate,
        SubUpdate
    }
}
