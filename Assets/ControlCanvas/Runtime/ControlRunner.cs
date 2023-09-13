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

        private Stack<CanvasData> _controlFlowStack = new ();
        private CanvasData controlFlow => _controlFlowStack.Peek();

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
            InitializeControlFlow(path);
            runnerDict.Add(typeof(IState), new StateRunner());
            runnerDict.Add(typeof(IDecision), new DecisionRunner());
            runnerDict.Add(typeof(IBehaviour), new BehaviourRunner());
            //InitializeRunners();
            
            updateByType.Add(typeof(IState), RunRunner<IState>);
            updateByType.Add(typeof(IDecision), RunRunner<IDecision>);
            updateByType.Add(typeof(IBehaviour), RunRunner<IBehaviour>);
        }

        private void FixedUpdate()
        {
            _currentDeltaTimeForSubUpdate += Time.fixedDeltaTime;
            if (!stopped)
                UpdateControlFlow();
        }

        private void InitializeControlFlow(string currentPath)
        {
            CanvasData initControlFlow = null;
            XMLHelper.DeserializeFromXML(currentPath, out initControlFlow);
            if (initControlFlow == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            _controlFlowStack.Push(initControlFlow);
            initialControl = NodeManager.Instance.GetControlForNode(controlFlow.InitialNode, controlFlow);
            nextSuggestedControl = initialControl;
            //mode.Value = Mode.CompleteUpdate;
            if (agentContext == null)
            {
                Debug.LogError("No agent context set");
            }
        }

        // private void InitializeRunners()
        // {
        //     // stateRunner.Init(agentContext, controlFlow, this);
        //     // decisionRunner.Init(agentContext, controlFlow);
        //     // behaviourRunner.Init(agentContext, controlFlow, this);
        // }

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
                    ResetRunner();
                }
                else
                {
                    Debug.LogError("No next suggested control");
                    return;   
                }
            }
            CurrentControl.Value = nextSuggestedControl;
            nextSuggestedControl = null;
            
            Type executionType = NodeManager.Instance.GetExecutionTypeOfNode(CurrentControl.Value, controlFlow);
            updateByType[executionType](_currentDeltaTimeForSubUpdate);
            
            StepDone.OnNext(CurrentControl.Value);
            _currentDeltaTimeForSubUpdate = 0;
        }

        private void RunRunner<T>(float deltaTime) where T : class, IControl
        {
            IRunner<T> runner = runnerDict[typeof(T)] as IRunner<T>;
            runner.DoUpdate(CurrentControl.Value as T, agentContext, deltaTime);
            nextSuggestedControl = runner.GetNext(CurrentControl.Value as T, controlFlow);
        }

        private void ClearStateRunnerIfNecessary()
        {
            if (NodeManager.Instance.GetExecutionTypeOfNode(nextSuggestedControl, controlFlow) != typeof(IState))
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
            
            nextSuggestedControl = NodeManager.Instance.GetNextForNode(CurrentControl.Value, controlFlow);
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
