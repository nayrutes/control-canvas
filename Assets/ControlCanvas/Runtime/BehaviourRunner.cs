using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunner
    {
        
        public ReactiveProperty<BehaviourWrapper> CurrentBehaviourWrapper { get; } = new();
        public ControlAgent AgentContext { get; set; }
        public State LastCombinedResult { get; private set; }

        
        private CanvasData _controlFlow;
        private ControlRunner _controlRunner;
        private readonly Dictionary<IBehaviour, BehaviourWrapper> _behaviourWrappers = new();
        private readonly Stack<IBehaviour> _behaviourStack = new();
        private readonly HashSet<IControl> _behaviourTracker = new();
        private bool _endReached;

        public void Init(ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            _controlRunner = controlRunner;
            AgentContext = agent;
            _controlFlow = controlFlow;
        }

        public IControl DoUpdate(IBehaviour behaviour)
        {
            CurrentBehaviourWrapper.Value = GetOrSetWrapper(behaviour);

            if (!_endReached)
            {
                TryAddToStack(CurrentBehaviourWrapper.Value.Behaviour);
                if (!_behaviourTracker.Contains(CurrentBehaviourWrapper.Value.Behaviour))
                {
                    CurrentBehaviourWrapper.Value.Update(AgentContext, Time.deltaTime);
                }

                LastCombinedResult = CurrentBehaviourWrapper.Value.CombinedResultState;
            }

            return HandleResult();
        }

        private BehaviourWrapper GetOrSetWrapper(IBehaviour behaviour)
        {
            if (!_behaviourWrappers.TryGetValue(behaviour, out var wrapper))
            {
                wrapper = new BehaviourWrapper(behaviour, _controlFlow);
                _behaviourWrappers[behaviour] = wrapper;
            }
            return wrapper;
        }

        private void TryAddToStack(IBehaviour behaviour)
        {
            if (!_behaviourStack.Contains(behaviour))
            {
                _behaviourStack.Push(behaviour);
                _endReached = false;
            }
        }

        private IControl HandleResult()
        {
            IControl nextControl = null;

            if (_endReached)
            {
                if (!CurrentBehaviourWrapper.Value.ChoseFailRoute)
                {
                    if (LastCombinedResult == State.Failure)
                    {
                        _endReached = false;
                        nextControl = CurrentBehaviourWrapper.Value.FailureChild;
                        CurrentBehaviourWrapper.Value.CombinedResultState = State.Failure;
                        //CurrentBehaviourWrapper.Value.ChoseFailRoute = true;
                    }
                    
                }
            }
            else
            {
                switch (CurrentBehaviourWrapper.Value.CombinedResultState)
                {
                    case State.Success:
                        nextControl = CurrentBehaviourWrapper.Value.SuccessChild;
                        break;
                    case State.Failure:
                        nextControl = CurrentBehaviourWrapper.Value.FailureChild;
                        CurrentBehaviourWrapper.Value.ChoseFailRoute = true;
                        break;
                    case State.Running:
                        nextControl = CurrentBehaviourWrapper.Value.Behaviour;
                        break;
                    default:
                        Debug.LogError($"Unknown state {CurrentBehaviourWrapper.Value?.CombinedResultState}");
                        throw new ArgumentOutOfRangeException();
                }

                _endReached = nextControl == null;
            }

            if (_endReached)
            {
                _behaviourStack.Pop();
                if (_behaviourStack.TryPeek(out IBehaviour topBehaviour))
                {
                    nextControl = topBehaviour;
                }
                else
                {
                    _endReached = false;
                }
            }

            return nextControl;
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
}
