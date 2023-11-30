using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{

    public class RunInstance
    {
        public Dictionary<Type, IRunnerBase> runnerDict = new ();
        
        public Dictionary<Type, Action<IControl, float, IControl>> updateByType = new ();
        public Dictionary<Type, Func<IControl, IControl, IControl>> nextByType = new ();
        public Dictionary<Type, Func<bool>> checkIfDoneByType = new ();

        public IControl InitControl { get; private set; }
        public IControl LastControl { get; set; }
        public IControl LastToStayIn { get; set; }
        public IControl RunStartControl { get; set; }
        
        public RunInstance(int i, IControl initControl)
        {
            Id = i.ToString();
            InitControl = initControl;
        }

        public string Id { get; set; }
        public IControl CurrentControl { get; set; }

        public bool CheckIfDone(NodeManager nodeManager, CanvasData currentFlow)
        {
            Type executionType = nodeManager.GetExecutionTypeOfNode(LastControl, currentFlow);
            return LastControl == null || checkIfDoneByType[executionType]();
        }
    }
    
    public class ControlRunner
    {
        private FlowManager _flowManager = new ();
        private NodeManager _nodeManager = new ();
        
        //IControl CurrentControl => _flowManager.CurrentFlowTracker.control;
        CanvasData CurrentFlow => _flowManager.CurrentFlowTracker.flow;
        
        public NodeManager NodeManager => _nodeManager;
        
        public Subject<IControl> StepDoneCurrent { get; } = new();
        public Subject<IControl> NextPreview { get; } = new();
        //public Subject<List<IBehaviour>> ClearingBt { get; set; } = new();
        public Subject<Unit> OnCompleteUpdateDone { get; } = new();
        public Subject<Unit> OnStart { get; } = new();

        private IControlAgent agentContext;


        private Queue<RunInstance> _initQueue = new();
        private Dictionary<IControl, RunInstance> _runInstanceDict = new();

        private ReactiveProperty<Mode> mode = new();
        //private IControl nextSuggestedControl;
        //private IControl initInMainFlow;
        private bool stopped = false;
        //private bool startedComplete;
        private bool _autoRestart = true;

        private RunInstance currentRunInstance;
        public State LatestBehaviourState => ((BehaviourRunner)currentRunInstance.runnerDict[typeof(IBehaviour)]).GetLastCombinedResult();
        public IObservable<FlowTracker> ControlFlowChanged => _flowManager.ControlFlowChanged.SkipWhile(_=>_running);

        private float _currentDeltaTimeForSubUpdate;
        int idCounter = 0;

        bool _safetyBreak = false;
        int _safetyCounter = 0;
        bool _running = false;
        //private IControl _lastControl;
        private bool _previewNext;

        private IControl restartInitControl;
        private bool _wasDone = true;

        public void Initialize(string startPath, IControlAgent agentContext)
        {
            this.agentContext = agentContext;
            if (this.agentContext.BlackboardFlowControl == null)
            {
                Debug.LogError("No blackboard-flowControl agent set");
                return;
            }
            
            mode.Value = Mode.CompleteUpdate;
            restartInitControl = InitializeControlFlow(startPath);
            Restart();
        }

        private void AddRunInstanceToQueue(IControl initControl)
        {
            if (initControl == null)
            {
                Debug.LogError("No init control, adding to run queue failed");
                return;
            }
            if (!_runInstanceDict.ContainsKey(initControl))
            {
                CreateRunInstance(initControl);
            }

            if (_initQueue.Contains(_runInstanceDict[initControl]))
            {
                Debug.LogWarning($"Already in queue {initControl}");
                return;
            }
            _initQueue.Enqueue(_runInstanceDict[initControl]);
            // var runInstance = _runInstanceDict[initControl];
            // runInstance.LastControl = runInstance.LastToStayIn;
        }
        private void CreateRunInstance(IControl initControl)
        {
            RunInstance runInstance = new RunInstance(idCounter++, initControl);
            _runInstanceDict.Add(initControl, runInstance);
            runInstance.runnerDict.Add(typeof(IState), new StateRunner(_flowManager, _nodeManager));
            runInstance.runnerDict.Add(typeof(IDecision), new DecisionRunner(_flowManager, _nodeManager));
            runInstance.runnerDict.Add(typeof(IBehaviour), new BehaviourRunner(_flowManager, _nodeManager));
            
            runInstance.updateByType.Add(typeof(IState), RunnerRun<IState>);
            runInstance.updateByType.Add(typeof(IDecision), RunnerRun<IDecision>);
            runInstance.updateByType.Add(typeof(IBehaviour), RunnerRun<IBehaviour>);
            
            runInstance.nextByType.Add(typeof(IState), RunnerGetNext<IState>);
            runInstance.nextByType.Add(typeof(IDecision), RunnerGetNext<IDecision>);
            runInstance.nextByType.Add(typeof(IBehaviour), RunnerGetNext<IBehaviour>);
            
            runInstance.checkIfDoneByType.Add(typeof(IState), RunnerCheckIfDone<IState>);
            runInstance.checkIfDoneByType.Add(typeof(IDecision), RunnerCheckIfDone<IDecision>);
            runInstance.checkIfDoneByType.Add(typeof(IBehaviour), RunnerCheckIfDone<IBehaviour>);
        }



        private IControl InitializeControlFlow(string currentPath)
        {
            //CanvasData initControlFlow = null;
            
            _flowManager.SetCurrentFlow(currentPath);
            IControl initControl = _nodeManager.GetInitControl(CurrentFlow);
            if (initControl == null)
            {
                Debug.LogWarning("No initial control found");
            }
            //nextSuggestedControl = initialControl;
            if (agentContext == null)
            {
                Debug.LogError("No agent context set");
            }
            return initControl;
        }

        private void Restart()
        {
            //Debug.LogWarning("Restarting control flow");
            AddRunInstanceToQueue(restartInitControl);
        }
        
        public void RunningUpdate(float fixedDeltaTime)
        {
            //Debug.Log($"==Running update {fixedDeltaTime}");
            _currentDeltaTimeForSubUpdate += fixedDeltaTime;
            if (!stopped)
            {
                if (mode.Value == Mode.CompleteUpdate)
                    CompleteUpdate();
                else
                    SingleUpdate();
            }
        }

        public void Update()
        {
            
        }

        private void CompleteUpdate()
        {
            //Debug.Log("Starting Complete update");
            _running = true;
            _safetyCounter = 0;
            while (_running)
            {
                try
                {
                    if (_safetyBreak || _safetyCounter++ > 1000)
                    {
                        Debug.LogError("Safety break hit");
                        return;
                    }

                    bool completeUpdateDone = SingleUpdate();
                    if (completeUpdateDone)
                    {
                        _running = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _running = false;
                    stopped = true;
                    throw;
                }
                
            }
            //startedComplete = false;
            PreviewNext();
        }

        private bool SingleUpdate()
        {
            if (_wasDone)
            {
                _wasDone = false;
                OnStart.OnNext(Unit.Default);
            }
            
            IControl next = GetNext();
            //Debug.Log($"Next control {next}");
            if (next == null)
            {
                //Debug.Log("No next control (single update)");
            }
            if (currentRunInstance == null)
            {
                Debug.LogWarning("No current run instance");
                return true;
            }
            SubUpdate(next);
            currentRunInstance.LastControl = next;
            CheckDone(out bool isCompleteDone, out bool isInstanceDone);
            if (isCompleteDone)
            {
                _wasDone = true;
                _currentDeltaTimeForSubUpdate = 0;
                if (_autoRestart)
                {
                    Restart();
                }
            }
            else
            {
            }

            return isCompleteDone;
        }
        
        private IControl GetNext()
        {
            IControl last = null;
            IControl next = null;

            currentRunInstance ??= GetNextRunInstanceFromQueue();
            if (currentRunInstance == null)
            {
                Debug.LogWarning("No current run instance");
                return null;
            }
            last = currentRunInstance.LastControl;
            // if (last != null && last is ICanStayBetweenUpdates)
            // {
            //     currentRunInstance.LastToStayIn = last;
            // }
            IControl lastToStayIn = currentRunInstance.LastToStayIn;
            if (last != null)
            {
                Type executionType = _nodeManager.GetExecutionTypeOfNode(last, CurrentFlow);
                if (executionType != null)
                {
                    next = currentRunInstance.nextByType[executionType](last, lastToStayIn);
                }
            }
            else
            {
                // if (currentRunInstance.InitControl == restartInitControl)
                // {
                //     Debug.LogWarning("Restarting run instance with init control");
                // }
                //Debug.LogWarning("Restarting run instance with run instance start control");
                next = currentRunInstance.RunStartControl;

                if (next == null)
                {
                    //Debug.LogWarning("Restarting run instance with run instance init control");
                    next = currentRunInstance.InitControl;
                }
            }
            //currentRunInstance.CurrentControl = next;
            
            
            
            //currentRunInstance.LastControl = next;
           
            return next;
        }

        private void CheckDone(out bool isCompleteDone, out bool isInstanceDone)
        {
            isCompleteDone = currentRunInstance == null;
            isInstanceDone = false;
            if (!isCompleteDone)
            {
                var last = currentRunInstance.LastControl;
                if (last != null && last is ICanStayBetweenUpdates)
                {
                    currentRunInstance.LastToStayIn = last;
                }
                //isInstanceDone = currentRunInstance.CheckIfDone(_nodeManager, CurrentFlow);
                isInstanceDone = currentRunInstance.LastControl == null ||
                                 currentRunInstance.LastControl == currentRunInstance.LastToStayIn;
                //Debug.Log($"Instance done {isInstanceDone}, last control {currentRunInstance.LastControl}, last to stay in {currentRunInstance.LastToStayIn}");
                if (isInstanceDone)
                {
                    currentRunInstance.RunStartControl = currentRunInstance.LastToStayIn;
                    InstanceUpdateDone(currentRunInstance);
                    currentRunInstance = null;
                    if (_initQueue.Count == 0)
                    {
                        isCompleteDone = true;
                    }
                }
            }
            if(isCompleteDone)
            {
                CompleteUpdateDone();
            }
        }
        
        private RunInstance GetNextRunInstanceFromQueue()
        {
            if (_initQueue.Count == 0)
            {
                return null;
            }

            RunInstance nextRunInstanceFromQueue = _initQueue.Dequeue();
            //nextRunInstanceFromQueue.RunStartControl = nextRunInstanceFromQueue.LastControl;
            //runInstance.LastControl = runInstance.LastToStayIn;
            return nextRunInstanceFromQueue;
        }
      
        private void SubUpdate(IControl current)
        {
            if(current == null || currentRunInstance == null)
                return;
            _flowManager.SetCurrentControlAndFlow(current, _nodeManager);
            
            Type executionType = _nodeManager.GetExecutionTypeOfNode(current, CurrentFlow);
            currentRunInstance.updateByType[executionType](current, _currentDeltaTimeForSubUpdate, currentRunInstance.LastControl);

            //currentRunInstance.runnerDict[executionType].StepDone();
            StepDoneCurrent.OnNext(current);
        }
        
        private void RunnerRun<T>(IControl current, float deltaTime, IControl lastControl) where T : class, IControl
        {
            IRunner<T> runner = currentRunInstance.runnerDict[typeof(T)] as IRunner<T>;
            runner.DoUpdate(current as T, agentContext, deltaTime, lastControl);

            runner.GetParallel(current, CurrentFlow)?
                .ForEach(AddRunInstanceToQueue);
        }

        private IControl RunnerGetNext<T>(IControl current, IControl lastToStayIn) where T : class, IControl
        {
            IRunner<T> runner = currentRunInstance.runnerDict[typeof(T)] as IRunner<T>;
            if (current is ISubFlow subFlow)
            {
                _flowManager.CacheFlow(subFlow.GetSubFlowPath(agentContext));
            }
            IControl next = runner.GetNext(current as T, CurrentFlow, agentContext, lastToStayIn);
            return next;
        }
        
        private bool RunnerCheckIfDone<T>() where T : class, IControl
        {
            IRunner<T> runner = currentRunInstance.runnerDict[typeof(T)] as IRunner<T>;
            return runner.CheckIfDone();
        }
        
        private void InstanceUpdateDone(RunInstance runInstance)
        {
            if(runInstance == null)
                return;
            //Debug.Log($"Complete update done instance {runInstance.Id}");
            foreach (KeyValuePair<Type, IRunnerBase> keyValuePair in runInstance.runnerDict)
            {
                keyValuePair.Value.InstanceUpdateDone(agentContext);
            }
        }
        private void CompleteUpdateDone()
        {
            //Debug.Log("Complete update done");
            OnCompleteUpdateDone.OnNext(Unit.Default);
            foreach (KeyValuePair<IControl,RunInstance> keyValuePair in _runInstanceDict)
            {
                foreach (KeyValuePair<Type, IRunnerBase> keyValuePair2 in keyValuePair.Value.runnerDict)
                {
                    keyValuePair2.Value.InstanceUpdateDone(agentContext);
                }
            }
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
