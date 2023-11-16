using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunnerBlackboard
    {
        public State LastCombinedResult
        {
            get { return _lastCombinedResult; }
            set
            {
                //Debug.Log($"Setting last combined result to {value}");
                _lastCombinedResult = value;
            }
        }

        public ExDirection LastDirection
        {
            get => _lastDirection;
            set
            {
                //Debug.Log($"Setting last direction to {value}");
                _lastDirection = value;
            }
        }

        public readonly Stack<IBehaviour> behaviourStack = new();
        public readonly List<Repeater> repeaterList = new();
        private State _lastCombinedResult;
        private ExDirection _lastDirection = ExDirection.Forward;
        public readonly Dictionary<IControl, bool> parallelStarted = new();

        public BehaviourRunnerBlackboard()
        {
        }
        
        public BehaviourRunnerBlackboard(BehaviourRunnerBlackboard blackboard)
        {
            LastCombinedResult = blackboard.LastCombinedResult;
            LastDirection = blackboard.LastDirection;
            behaviourStack = new Stack<IBehaviour>(blackboard.behaviourStack);
            repeaterList = new List<Repeater>(blackboard.repeaterList);
        }
    }
    
    public class BehaviourRunner : IRunner<IBehaviour>
    {
        public ReactiveProperty<BehaviourWrapper> CurrentBehaviourWrapper { get; } = new();

        private ControlRunner _controlRunner;
        //TODO: Warning: wrappers are not specific to agents (yet)
        private readonly Dictionary<IBehaviour, BehaviourWrapper> _behaviourWrappers = new();
        private BehaviourRunnerBlackboard _blackboard = new ();

        private readonly DefaultRunnerExecuter _behaviourRunnerExecuter = new ();
        private readonly FlowManager _flowManager;
        private readonly NodeManager _nodeManager;
        //private IControl _controlBeforeBehaviour;

        //private BehaviourRunnerBlackboard _tmpBlackboard = new();

        public BehaviourRunner(FlowManager flowManager, NodeManager instance)
        {
            _flowManager = flowManager;
            _nodeManager = instance;
        }
        
        public void DoUpdate(IBehaviour behaviour, IControlAgent agentContext, float deltaTime, IControl lastControl)
        {
            CurrentBehaviourWrapper.Value = GetOrSetWrapper(behaviour);
            //_blackboard = _tmpBlackboard;

            // if (lastControl is not IBehaviour)
            // {
            //     _controlBeforeBehaviour = lastControl;
            // }
            
            if (_blackboard.LastDirection == ExDirection.Forward)
            {
                TryAddToStack(CurrentBehaviourWrapper.Value.Behaviour);
                
                if (_blackboard.repeaterList.Count <= 0)
                {
                    CurrentBehaviourWrapper.Value.Update(agentContext, deltaTime);
                }
                else
                {
                    Debug.Log($"Skipping {_nodeManager.GetGuidForControl(CurrentBehaviourWrapper.Value.Behaviour)} ");
                }

                _blackboard.LastCombinedResult = CurrentBehaviourWrapper.Value.CombinedResultState;
            }
            
            IBehaviourRunnerExecuter runnerExecuter = behaviour as IBehaviourRunnerExecuter ?? _behaviourRunnerExecuter;
            _blackboard.LastDirection = runnerExecuter.ReEvaluateDirection(agentContext, _blackboard, CurrentBehaviourWrapper.Value);
        }

        //TODO: split this up to have a GetNext which changes nothing of the flow and a one that does if needed
        public IControl GetNext(IBehaviour behaviour, CanvasData controlFlow, IControlAgent agentContext, IControl lastToStayIn)
        {
            if (CurrentBehaviourWrapper.Value == null)
            {
                return behaviour;
            }
            IBehaviourRunnerExecuter runnerExecuter = behaviour as IBehaviourRunnerExecuter ?? _behaviourRunnerExecuter;

            IControl nextControl = null;

            ExDirection newDirection = _blackboard.LastDirection;
            
            if (newDirection == ExDirection.Forward)
            {
                nextControl = runnerExecuter.DoForward(agentContext, _blackboard, CurrentBehaviourWrapper.Value, controlFlow);   
            }

            if (nextControl == null || newDirection == ExDirection.Backward)
            {
                newDirection = ExDirection.Backward;
                nextControl = runnerExecuter.DoBackward(agentContext, _blackboard);
            }
            
            _blackboard.LastDirection = newDirection;
            if (newDirection == ExDirection.Backward)
            {
                CurrentBehaviourWrapper.Value.Behaviour.OnReset(agentContext, _blackboard.LastCombinedResult);
                //Remember to start at the behaviour group again if it was running 
                if (nextControl == null && _blackboard.LastCombinedResult == State.Running)
                {
                    nextControl = CurrentBehaviourWrapper.Value.Behaviour;
                }
            }

            if (nextControl == null)
            {
                return lastToStayIn;
            }
            return nextControl;
        }

        public List<IControl> GetParallel(IControl current, CanvasData currentFlow)
        {
            if (_blackboard.LastDirection == ExDirection.Forward && !_blackboard.parallelStarted.ContainsKey(current))
            {
                List<IControl> parallelForNode = _nodeManager.GetParallelForNode(current, currentFlow);
                _blackboard.parallelStarted[current] = true;
                return parallelForNode;
            }
            return null;
        }


        // private bool CheckNextSuggestionValidity(IControl nextControl, ExDirection direction, out bool changeRequested)
        // {
        //     changeRequested = false;
        //     if (nextControl == null)
        //         return false;
        //     IBehaviourRunnerExecuter nextRunnerExecuter = nextControl as IBehaviourRunnerExecuter;
        //     nextRunnerExecuter ??= _behaviourRunnerExecuter;
        //     return nextRunnerExecuter.CheckNextSuggestionValidity(direction, this, out changeRequested);
        // }
        //
        // private ExDirection EvaluateDirection(ExDirection executionDirection,
        //     IBehaviourRunnerExecuter behaviourRunnerExecuter, bool changeRequested)
        // {
        //     if (executionDirection == ExDirection.Backward)
        //     {
        //         return ReEvaluateDirection(executionDirection);
        //     }
        //     if(changeRequested)
        //     {
        //         executionDirection = executionDirection == ExDirection.Forward ? ExDirection.Backward : ExDirection.Forward;
        //     }
        //     return executionDirection;
        // }
        //
        // private IControl EvaluateNextSuggestion(ExDirection lastExecutionDirection, CanvasData controlFlow,
        //     IBehaviourRunnerExecuter behaviourRunnerExecuter, List<IControl> declinedControls)
        // {
        //     return lastExecutionDirection == ExDirection.Forward ? 
        //         behaviourRunnerExecuter.DoForward(CurrentBehaviourWrapper.Value, controlFlow) 
        //         : behaviourRunnerExecuter.DoBackward(_behaviourStack);
        // }

        
        private BehaviourWrapper GetOrSetWrapper(IBehaviour behaviour)
        {
            if (!_behaviourWrappers.TryGetValue(behaviour, out var wrapper))
            {
                wrapper = new BehaviourWrapper(behaviour, _flowManager, _nodeManager);
                _behaviourWrappers[behaviour] = wrapper;
            }
            return wrapper;
        }

        private void TryAddToStack(IBehaviour behaviour)
        {
            _blackboard.behaviourStack.Push(behaviour);
            // if (!_blackboard.behaviourStack.Contains(behaviour))
            // {
            //     _blackboard.behaviourStack.Push(behaviour);
            // }
        }

        public bool CheckIfDone()
        {
            return _blackboard.LastDirection == ExDirection.Backward && _blackboard.behaviourStack.Count == 0;
        }
        
        public void InstanceUpdateDone(IControlAgent agentContext)
        {
            CurrentBehaviourWrapper.Value = null;
            foreach (var wrapper in _behaviourWrappers.Values)
            {
                wrapper.Reset();
            }
            _blackboard.behaviourStack.Clear();
            //_blackboard.repeaterList.Clear();
            _blackboard.LastDirection = ExDirection.Forward;
            _blackboard.LastCombinedResult = State.Success;
            _blackboard.parallelStarted.Clear();
            //_controlBeforeBehaviour = null;
        }


        // private void CombineResults()
        // {
        //     State currentResult = CurrentBehaviourWrapper.Value.CombinedResultState;
        //     if (!CurrentBehaviourWrapper.Value.ChoseFailRoute)
        //     {
        //         CurrentBehaviourWrapper.Value.CombinedResultState = (currentResult == State.Failure || LastCombinedResult == State.Failure)
        //     }
        //     
        //     currentResult = (currentResult == State.Failure || LastCombinedResult == State.Failure) ? State.Failure : currentResult;
        //
        //     CurrentBehaviourWrapper.Value.CombinedResultState = currentResult;
        // }

        public State GetLastCombinedResult()
        {
            return _blackboard.LastCombinedResult;
        }
    }
    
    public enum ExDirection
    {
        Forward,
        Backward
    }
}
