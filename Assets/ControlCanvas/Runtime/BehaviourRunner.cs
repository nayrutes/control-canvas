﻿using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using Unity.VisualScripting;
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
        private bool _goingBackwards;
        //private IBehaviour _behaviourConnectingToRepeater;
        private Stack<Repeater> _repeaterStack = new();

        public void Init(ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            _controlRunner = controlRunner;
            AgentContext = agent;
            _controlFlow = controlFlow;
        }

        public IControl DoUpdate(IBehaviour behaviour, float deltaTime)
        {
            CurrentBehaviourWrapper.Value = GetOrSetWrapper(behaviour);

            if (!_goingBackwards)
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
                    CurrentBehaviourWrapper.Value.Update(AgentContext, deltaTime);
                }
                else
                {
                    Debug.Log($"Skipping {NodeManager.Instance.GetGuidForControl(CurrentBehaviourWrapper.Value.Behaviour)} " +
                              $"because it is before the repeater {NodeManager.Instance.GetGuidForControl(_repeaterStack.Peek())}");
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
                _goingBackwards = false;
            }
        }

        private IControl HandleResult()
        {
            IControl nextControl = null;

            if (_goingBackwards)
            {
                if (!CurrentBehaviourWrapper.Value.ChoseFailRoute)
                {
                    if (LastCombinedResult == State.Failure && CurrentBehaviourWrapper.Value.FailureChild != null)
                    {
                        _goingBackwards = false;
                        nextControl = CurrentBehaviourWrapper.Value.FailureChild;
                        CurrentBehaviourWrapper.Value.CombinedResultState = State.Failure;
                        CurrentBehaviourWrapper.Value.ChoseFailRoute = true;
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

                _goingBackwards = nextControl == null;
            }

            
            if (!_goingBackwards && nextControl is Repeater repeater)
            {
                if (repeater.mode == RepeaterMode.Loop && _behaviourStack.Contains(repeater))
                {
                    _goingBackwards = true;
                    _repeaterStack.Push(repeater);
                }
            }
            
            if (_goingBackwards)
            {
                
                _behaviourStack.Pop();
                if (_behaviourStack.TryPeek(out IBehaviour topBehaviour))
                {
                    if(CurrentBehaviourWrapper.Value.Behaviour is Repeater repeater3 && repeater3.mode == RepeaterMode.Always)
                    {
                        _repeaterStack.Push(repeater3);
                    }
                    
                    nextControl = topBehaviour;
                }
                else
                {
                    _goingBackwards = false;
                }
            }

            
            return nextControl;
        }

        public void ResetWrappers()
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
}
