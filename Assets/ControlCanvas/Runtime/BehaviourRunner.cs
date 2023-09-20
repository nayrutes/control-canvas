using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunner : IRunner<IBehaviour>
    {
        
        public ReactiveProperty<BehaviourWrapper> CurrentBehaviourWrapper { get; } = new();
        //public ControlAgent AgentContext { get; set; }
        public State LastCombinedResult { get; private set; }
        public Stack<IBehaviour> BehaviourStack => _behaviourStack;
        public Stack<Repeater> RepeaterStack => _repeaterStack;


        //private CanvasData _controlFlow;
        private ControlRunner _controlRunner;
        private readonly Dictionary<IBehaviour, BehaviourWrapper> _behaviourWrappers = new();
        private readonly Stack<IBehaviour> _behaviourStack = new();
        private readonly HashSet<IControl> _behaviourTracker = new();
        private ExDirection _executionDirection = ExDirection.Forward;
        //private IBehaviour _behaviourConnectingToRepeater;
        private Stack<Repeater> _repeaterStack = new();

        // public void InitRunner(ControlAgent agentContext, CanvasData controlFlow)
        // {
        //     //_controlRunner = controlRunner;
        //     //AgentContext = agent;
        //     //_controlFlow = controlFlow;
        // }

        public void DoUpdate(IBehaviour behaviour, ControlAgent agentContext,  float deltaTime)
        {
            CurrentBehaviourWrapper.Value = GetOrSetWrapper(behaviour);

            if (_executionDirection == ExDirection.Forward)
            {
                TryAddToStack(CurrentBehaviourWrapper.Value.Behaviour);
                
                //skip all behaviours that are before the repeater that is currently running
                bool skipExecute = false;
                if(_repeaterStack.Count > 0)
                {
                    skipExecute = true;
                    if(_repeaterStack.Peek() == CurrentBehaviourWrapper.Value.Behaviour)
                    {
                        skipExecute = false;
                        _repeaterStack.Pop();
                    }
                }
                
                if (!skipExecute)
                {
                    CurrentBehaviourWrapper.Value.Update(agentContext, deltaTime);
                }
                else
                {
                    Debug.Log($"Skipping {NodeManager.Instance.GetGuidForControl(CurrentBehaviourWrapper.Value.Behaviour)} " +
                              $"because it is before the repeater {NodeManager.Instance.GetGuidForControl(_repeaterStack.Peek())}");
                }

                LastCombinedResult = CurrentBehaviourWrapper.Value.CombinedResultState;
            }

            //return HandleResult();
        }
        DefaultRunnerOverrides _behaviourRunnerOverrides = new DefaultRunnerOverrides();

        public IControl GetNext(IBehaviour behaviour, CanvasData controlFlow, ControlAgent agentContext,
            Func<string, CanvasData> getFlow)
        {
            IBehaviourRunnerOverrides currentOverrides = behaviour as IBehaviourRunnerOverrides ?? _behaviourRunnerOverrides;

            IControl nextControl = null;
            List<IControl> declinedControls = new List<IControl>();
            bool searchingNext = true;
            ExDirection newDirection = _executionDirection;
            bool changeRequested = false;
            while (searchingNext)
            {
                newDirection = EvaluateDirection(newDirection, currentOverrides, changeRequested);
                nextControl = EvaluateNextSuggestion(newDirection, controlFlow, currentOverrides, declinedControls);
                var nextFound = CheckNextSuggestionValidity(nextControl, newDirection, out changeRequested);
                if(nextFound)
                {
                    searchingNext = false;
                }
                else
                {
                    if (declinedControls.Contains(nextControl))
                    {
                        searchingNext = false;
                        nextControl = null;
                    }
                    else
                    {
                        declinedControls.Add(nextControl);
                    }
                }
            }
            
            
            // if(behaviour is IBehaviourRunnerOverrides overrides)
            // {
            //     nextControl = overrides.GetNext(CurrentBehaviourWrapper.Value.Behaviour, controlFlow, agentContext, getFlow, ref _goingBackwards);
            //     if(nextControl != null)
            //         return nextControl;
            // }
            
            //Pre pass (backwards)
            // if (_goingBackwards)
            // {
            //     ReEvaluateCombinedResult(out bool routeChosen);
            //     _goingBackwards = !routeChosen;
            // }
            //
            // //First pass (forward)
            // if (!_goingBackwards)
            // {
            //     DefaultForward(controlFlow, agentContext, getFlow, out nextControl);
            //     _goingBackwards = nextControl == null;
            // }
            //
            // //Second pass (forward)
            // if(!_goingBackwards && nextControl is IBehaviourRunnerOverrides overrides2)
            // {
            //     overrides2.CheckForwardAsNext(ref _goingBackwards, _behaviourStack, _repeaterStack);
            // }
            //
            // //Third pass (backwards)
            // if (_goingBackwards)
            // {
            //     _behaviourStack.Pop();
            //     if (_behaviourStack.TryPeek(out IBehaviour topBehaviour))
            //     {
            //         nextControl = topBehaviour;
            //     }
            //     else
            //     {
            //         _goingBackwards = false;
            //     }
            //     
            //     if(nextControl is IBehaviourRunnerOverrides overrides3)
            //     {
            //         overrides3.CheckBackwardAsNext(_repeaterStack);
            //     }
            // }
            return nextControl;
        }

        private bool CheckNextSuggestionValidity(IControl nextControl, ExDirection direction, out bool changeRequested)
        {
            changeRequested = false;
            if (nextControl == null)
                return false;
            IBehaviourRunnerOverrides nextRunnerOverrides = nextControl as IBehaviourRunnerOverrides;
            nextRunnerOverrides ??= _behaviourRunnerOverrides;
            return nextRunnerOverrides.CheckNextSuggestionValidity(direction, this, out changeRequested);
        }

        private ExDirection EvaluateDirection(ExDirection executionDirection,
            IBehaviourRunnerOverrides behaviourRunnerOverrides, bool changeRequested)
        {
            if (executionDirection == ExDirection.Backward)
            {
                return ReEvaluateCombinedResult(executionDirection);
            }
            if(changeRequested)
            {
                executionDirection = executionDirection == ExDirection.Forward ? ExDirection.Backward : ExDirection.Forward;
            }
            return executionDirection;
        }

        private IControl EvaluateNextSuggestion(ExDirection lastExecutionDirection, CanvasData controlFlow,
            IBehaviourRunnerOverrides behaviourRunnerOverrides, List<IControl> declinedControls)
        {
            return lastExecutionDirection == ExDirection.Forward ? 
                behaviourRunnerOverrides.Forward(CurrentBehaviourWrapper.Value, controlFlow) 
                : behaviourRunnerOverrides.Backward(_behaviourStack);
        }

        private ExDirection ReEvaluateCombinedResult(ExDirection last)
        {
            if (!CurrentBehaviourWrapper.Value.ChoseFailRoute && LastCombinedResult == State.Failure)
            {
                CurrentBehaviourWrapper.Value.CombinedResultState = State.Failure;
                return ExDirection.Forward;
            }
            return last;
        }
        
        private BehaviourWrapper GetOrSetWrapper(IBehaviour behaviour)
        {
            if (!_behaviourWrappers.TryGetValue(behaviour, out var wrapper))
            {
                wrapper = new BehaviourWrapper(behaviour);
                _behaviourWrappers[behaviour] = wrapper;
            }
            return wrapper;
        }

        private void TryAddToStack(IBehaviour behaviour)
        {
            if (!_behaviourStack.Contains(behaviour))
            {
                _behaviourStack.Push(behaviour);
                //_goingBackwards = false;
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
