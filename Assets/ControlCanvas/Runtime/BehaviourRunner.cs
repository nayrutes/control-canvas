using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunnerBlackboard
    {
        public readonly Stack<IBehaviour> behaviourStack = new();
        public List<Repeater> repeaterList = new();
    }
    
    public class BehaviourRunner : IRunner<IBehaviour>
    {
        public ReactiveProperty<BehaviourWrapper> CurrentBehaviourWrapper { get; } = new();
        public State LastCombinedResult { get; private set; }

        private ControlRunner _controlRunner;
        //TODO: Warning: wrappers are not specific to agents (yet)
        private readonly Dictionary<IBehaviour, BehaviourWrapper> _behaviourWrappers = new();
        private ExDirection _lastDirection = ExDirection.Forward;
        private readonly BehaviourRunnerBlackboard _blackboard = new ();

        private readonly DefaultRunnerExecuter _behaviourRunnerExecuter = new ();
        private readonly FlowManager _flowManager;
        private readonly NodeManager _nodeManager;
        
        public BehaviourRunner(FlowManager flowManager, NodeManager instance)
        {
            _flowManager = flowManager;
            _nodeManager = instance;
        }
        
        public void DoUpdate(IBehaviour behaviour, ControlAgent agentContext,  float deltaTime)
        {
            CurrentBehaviourWrapper.Value = GetOrSetWrapper(behaviour);

            if (_lastDirection == ExDirection.Forward)
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

                LastCombinedResult = CurrentBehaviourWrapper.Value.CombinedResultState;
            }
        }


        public IControl GetNext(IBehaviour behaviour, CanvasData controlFlow, ControlAgent agentContext)
        {
            IBehaviourRunnerExecuter runnerExecuter = behaviour as IBehaviourRunnerExecuter ?? _behaviourRunnerExecuter;

            IControl nextControl = null;

            ExDirection newDirection = runnerExecuter.ReEvaluateDirection(agentContext, _lastDirection, CurrentBehaviourWrapper.Value, LastCombinedResult);
            
            if (newDirection == ExDirection.Forward)
            {
                nextControl = runnerExecuter.DoForward(agentContext, _blackboard, CurrentBehaviourWrapper.Value, controlFlow);   
            }

            if (nextControl == null || newDirection == ExDirection.Backward)
            {
                newDirection = ExDirection.Backward;
                nextControl = runnerExecuter.DoBackward(agentContext, _blackboard);
            }
            
            _lastDirection = newDirection;
            return nextControl;
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
            if (!_blackboard.behaviourStack.Contains(behaviour))
            {
                _blackboard.behaviourStack.Push(behaviour);
            }
        }

        public void ResetRunner(ControlAgent agentContext)
        {
            foreach (var wrapper in _behaviourWrappers.Values)
            {
                wrapper.Reset();
            }
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

    }
    
    public enum ExDirection
    {
        Forward,
        Backward
    }
}
