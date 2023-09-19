using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace ControlCanvas.Runtime
{
    public class FlowTracker
    {
        public CanvasData currentFlow;
        public IControl currentControl;
        public string currentPath;
    }
    
    public class ControlRunner : MonoBehaviour
    {
        private Stack<FlowTracker> _controlFlowStack = new ();
        private CanvasData ControlFlow => _controlFlowStack.Peek().currentFlow;
        public IControl CurrentControl
        {
            get => _controlFlowStack.Peek().currentControl;
            set => _controlFlowStack.Peek().currentControl = value;
        }

        public Subject<IControl> StepDone { get; } = new Subject<IControl>();
        public Subject<FlowTracker> ControlFlowChanged { get; } = new Subject<FlowTracker>();
        public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        [SerializeField]
        private ControlAgent agentContext;

        [FormerlySerializedAs("path")] [SerializeField]
        private string startPath = "";


        private Dictionary<Type, IRunnerBase> runnerDict = new ();
        
        private Dictionary<Type, Action<float>> updateByType = new ();
        
        //private StateRunner stateRunner = new StateRunner();
        //private DecisionRunner decisionRunner = new DecisionRunner();
        //private BehaviourRunner behaviourRunner = new BehaviourRunner();

        private ReactiveProperty<Mode> mode = new ReactiveProperty<Mode>();
        private IControl nextSuggestedControl;
        private IControl initialControl;
        private bool stopped = true;
        private bool startedComplete;
        private bool _autoRestart = true;
        //private List<IBehaviour> _btTracker = new();

        public State LatestBehaviourState => ((BehaviourRunner)runnerDict[typeof(IBehaviour)]).LastCombinedResult;

        private float _currentDeltaTimeForSubUpdate;
        //public IControl LatestPop => behaviourRunner.LatestPop;

        private void Start()
        {
            mode.Value = Mode.CompleteUpdate;
            InitializeControlFlow(startPath);
            runnerDict.Add(typeof(IState), new StateRunner());
            runnerDict.Add(typeof(IDecision), new DecisionRunner());
            runnerDict.Add(typeof(IBehaviour), new BehaviourRunner());
            //InitializeRunners();
            
            updateByType.Add(typeof(IState), RunRunner<IState>);
            updateByType.Add(typeof(IDecision), RunRunner<IDecision>);
            updateByType.Add(typeof(IBehaviour), RunRunner<IBehaviour>);
            updateByType.Add(typeof(ISubFlow), EnterSubFlow);
        }

        private void EnterSubFlow(float deltaTime)
        {
            ISubFlow subFlow = CurrentControl as ISubFlow;
            string subFlowPath = subFlow.GetSubFlowPath(agentContext);
            InitializeControlFlow(subFlowPath);
        }
        private void InitializeControlFlow(string currentPath)
        {
            CanvasData initControlFlow = null;
            XMLHelper.DeserializeFromXML(currentPath, out initControlFlow);
            if (initControlFlow == null)
            {
                Debug.LogError($"No canvasData for path {currentPath}");
                return;
            }
            InitializeControlFlow(initControlFlow, currentPath);
        }

        private void InitializeControlFlow(CanvasData canvasData, string path = null)
        {
            _controlFlowStack.Push(new FlowTracker
            {
                currentFlow = canvasData,
                currentControl = null,
                currentPath = path
            });
            ControlFlowChanged.OnNext(_controlFlowStack.Peek());
            initialControl = NodeManager.Instance.GetControlForNode(ControlFlow.InitialNode, ControlFlow);
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
                if(_controlFlowStack.Count > 1)
                {
                    _controlFlowStack.Pop();
                    ControlFlowChanged.OnNext(_controlFlowStack.Peek());
                    nextSuggestedControl = NodeManager.Instance.GetNextForNode(CurrentControl, ControlFlow);
                    return;
                }
                
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
            CurrentControl = nextSuggestedControl;
            nextSuggestedControl = null;
            
            Type executionType = NodeManager.Instance.GetExecutionTypeOfNode(CurrentControl, ControlFlow);
            updateByType[executionType](_currentDeltaTimeForSubUpdate);
            
            StepDone.OnNext(CurrentControl);
            _currentDeltaTimeForSubUpdate = 0;
        }

        private void RunRunner<T>(float deltaTime) where T : class, IControl
        {
            IRunner<T> runner = runnerDict[typeof(T)] as IRunner<T>;
            runner.DoUpdate(CurrentControl as T, agentContext, deltaTime);
            nextSuggestedControl = runner.GetNext(CurrentControl as T, ControlFlow);
        }

        private void ClearStateRunnerIfNecessary()
        {
            if (NodeManager.Instance.GetExecutionTypeOfNode(nextSuggestedControl, ControlFlow) != typeof(IState))
            {
                runnerDict[typeof(IState)].ResetRunner(agentContext);
            }
        }
        
        private void ResetRunner()
        {
            runnerDict[typeof(IBehaviour)].ResetRunner(agentContext);
            //behaviourRunner.ResetRunner();
            //ClearingBt.OnNext(_btTracker);
            //_btTracker.Clear();
        }
        
        

        [ContextMenu("AutoNext")]
        public void AutoNext()
        {
            
            nextSuggestedControl = NodeManager.Instance.GetNextForNode(CurrentControl, ControlFlow);
            ClearStateRunnerIfNecessary();
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
