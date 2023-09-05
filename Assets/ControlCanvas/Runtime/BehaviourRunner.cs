using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunner
    {
        public ReactiveProperty<BehaviourWrapper> currentBehaviourWrapper = new();
        public ReactiveProperty<IBehaviour> currentBehaviour = new();
        
        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        private ControlRunner controlRunner;
        
        private Dictionary<IBehaviour, BehaviourWrapper> behaviourWrappers = new ();
        private Stack<BehaviourWrapper> behaviourStack = new Stack<BehaviourWrapper>();
        private HashSet<BehaviourWrapper> behaviourTracker = new HashSet<BehaviourWrapper>();
        
        private IBehaviour initBehaviour;
        
        public void Init(IBehaviour initBehaviour, ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            this.controlRunner = controlRunner;
            //currentBehaviour.Value = initBehaviour;
            AgentContext = agent;
            this.controlFlow = controlFlow;
            this.initBehaviour = initBehaviour;
            
            
            //behaviourStack.Push(GetOrSetWrapper(initBehaviour));
            currentBehaviourWrapper.Subscribe(x => currentBehaviour.Value = x?.Behaviour);
        }
        
        private BehaviourWrapper GetOrSetWrapper(IBehaviour behaviour)
        {
            if (behaviourWrappers.TryGetValue(behaviour, out var wrapper))
            {
                return wrapper;
            }
            else
            {
                behaviourWrappers.TryAdd(behaviour, new BehaviourWrapper(behaviour, controlFlow));
                return behaviourWrappers[behaviour];
            }
        }
        
        public void DoUpdate()
        {
            
            if(behaviourStack.Count == 0)
            {
                behaviourTracker.Clear();
                if (initBehaviour == null)
                {
                    Debug.Log($"Initial node {controlFlow.InitialNode} is not a state");
                    return;
                }
                Forward(initBehaviour);
            }
            if (behaviourStack.TryPeek(out BehaviourWrapper topBehaviour))
            {
                currentBehaviourWrapper.Value = topBehaviour;
            }else
            {
                Debug.LogWarning($"Behaviour stack is empty");
                return;
            }
            if(!behaviourTracker.Contains(currentBehaviourWrapper.Value))
            {
                currentBehaviourWrapper.Value?.Update(AgentContext, Time.deltaTime);
            }
            switch (currentBehaviourWrapper.Value?.State)
            {
                case State.Success:
                    behaviourTracker.Add(currentBehaviourWrapper.Value);
                    if (!Forward(currentBehaviourWrapper.Value.SuccessChild))
                    {
                        Backward();
                    }
                    break;
                case State.Failure:
                    behaviourTracker.Add(currentBehaviourWrapper.Value);
                    if (!Forward(currentBehaviourWrapper.Value.FailureChild))
                    {
                        Backward();
                    }
                    break;
                case State.Running:
                    
                    break;
                case null:
                    Debug.LogWarning($"state of BT is null");
                    break;
                default:
                    Debug.LogError($"Unknown state {currentBehaviourWrapper.Value?.State}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool Forward(IControl child)
        {
            if (child == null)
                return false;
            BehaviourWrapper wrapper = GetOrSetWrapper(controlRunner.GetNextBehaviourForNode(child, controlFlow));
            if(!behaviourTracker.Contains(wrapper))
            {
                behaviourStack.Push(wrapper);
                return true;
            }

            return false;
        }
        
        private void Backward()
        {
            behaviourStack.Pop();
            //behaviourTracker.Add();
        }
    }
}