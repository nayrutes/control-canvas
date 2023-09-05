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
        public ReactiveProperty<IBehaviour> CurrentBehaviour { get; } = new();
        public ControlAgent AgentContext { get; set; }

        private CanvasData _controlFlow;
        private ControlRunner _controlRunner;
        private readonly Dictionary<IBehaviour, BehaviourWrapper> _behaviourWrappers = new();
        private readonly Stack<IBehaviour> _behaviourStack = new();
        private readonly HashSet<IControl> _behaviourTracker = new();
        private IBehaviour _initBehaviour;
        public State ResultState { get; private set; }
        private bool _endReached;
        public IBehaviour LatestPop;

        public void Init(ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            _controlRunner = controlRunner;
            AgentContext = agent;
            _controlFlow = controlFlow;
            CurrentBehaviourWrapper.Subscribe(x => CurrentBehaviour.Value = x?.Behaviour);
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

        public IControl DoUpdate(IBehaviour behaviour)
        {
            if (!_behaviourStack.Contains(behaviour))
            {
                _behaviourStack.Push(behaviour);
                return DoSubUpdate();
            }
            else
            {
                if (_behaviourStack.Count == 1 && _behaviourStack.Peek() == behaviour)
                {
                    return DoSubUpdate();
                }
                
                LatestPop = _behaviourStack.Pop();
                return _behaviourStack.Peek();
            }

        }

        private IControl DoSubUpdate()
        {
            if (!_behaviourStack.TryPeek(out var topBehaviour))
            {
                Debug.LogWarning("Behaviour stack is empty");
                return default;
            }

            CurrentBehaviourWrapper.Value = GetOrSetWrapper(topBehaviour);

            if (!_behaviourTracker.Contains(CurrentBehaviourWrapper.Value.Behaviour))
            {
                CurrentBehaviourWrapper.Value?.Update(AgentContext, Time.deltaTime);
            }

            ResultState = CurrentBehaviourWrapper.Value?.State ?? State.Failure;
            HandleResult(ResultState, out var nextControl);
            return nextControl;
        }

        private void HandleResult(State result, out IControl nextControl)
        {
            switch (result)
            {
                case State.Success:
                    HandleState(CurrentBehaviourWrapper.Value.SuccessChild, out nextControl);
                    break;
                case State.Failure:
                    HandleState(CurrentBehaviourWrapper.Value.FailureChild, out nextControl);
                    break;
                case State.Running:
                    nextControl = CurrentBehaviourWrapper.Value.Behaviour;
                    break;
                default:
                    Debug.LogError($"Unknown state {CurrentBehaviourWrapper.Value?.State}");
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void HandleState(IControl child, out IControl nextControl)
        {
            if (CurrentBehaviourWrapper.Value.SuccessChild == null)
            {
                //nextControl = CurrentBehaviourWrapper.Value.Behaviour;
                if(_behaviourStack.Count == 1)
                    nextControl = CurrentBehaviourWrapper.Value.Behaviour;
                else
                {
                    LatestPop = _behaviourStack.Pop();
                    nextControl = _behaviourStack.Peek();
                }
            }
            else
            {
                nextControl = child;
            }
        }
        //
        // private bool Forward(IControl child)
        // {
        //     if (child == null)
        //         return false;
        //
        //     if (child is IBehaviour)
        //     {
        //         var wrapper = GetOrSetWrapper(child as IBehaviour);
        //         if (!_behaviourTracker.Contains(wrapper))
        //         {
        //             _behaviourStack.Push(wrapper);
        //             return true;
        //         }
        //         return false;
        //     }
        //     //var wrapper = GetOrSetWrapper(_controlRunner.GetNextBehaviourForNode(child, _controlFlow));
        //     
        // }
        //
        // private void Backward()
        // {
        //     _behaviourStack.Pop();
        // }
    }
}
