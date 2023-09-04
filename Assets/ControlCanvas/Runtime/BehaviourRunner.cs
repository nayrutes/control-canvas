using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourRunner
    {
        public ReactiveProperty<BehaviourWrapper> currentBehaviour = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        private ControlRunner controlRunner;
        
        private Dictionary<IBehaviour, BehaviourWrapper> behaviourWrappers = new ();
        private Stack<BehaviourWrapper> behaviourStack = new Stack<BehaviourWrapper>();
        
        
        public void Init(IBehaviour initBehaviour, ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            this.controlRunner = controlRunner;
            //currentBehaviour.Value = initBehaviour;
            AgentContext = agent;
            this.controlFlow = controlFlow;
            if (initBehaviour == null)
            {
                Debug.Log($"Initial node {controlFlow.InitialNode} is not a state");
                return;
            }
            
            behaviourStack.Push(GetOrSetWrapper(initBehaviour));
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
            if (behaviourStack.TryPeek(out BehaviourWrapper topBehaviour))
            {
                currentBehaviour.Value = topBehaviour;
            }else
            {
                //Debug.LogWarning($"Behaviour stack is empty");
                return;
            }
            
            currentBehaviour.Value?.Update(AgentContext, Time.deltaTime);
            switch (currentBehaviour.Value?.State)
            {
                case State.Success:
                    if (currentBehaviour.Value.SuccessChild != null)
                    {
                        behaviourStack.Push(GetOrSetWrapper(controlRunner.GetNextBehaviourForNode(currentBehaviour.Value.SuccessChild, controlFlow)));
                        
                    }
                    else
                    {
                        behaviourStack.Pop();
                    }
                    break;
                case State.Failure:
                    if(currentBehaviour.Value.FailureChild != null)
                    {
                        behaviourStack.Push(GetOrSetWrapper(controlRunner.GetNextBehaviourForNode(currentBehaviour.Value.FailureChild, controlFlow)));
                    }
                    else
                    {
                        behaviourStack.Pop();
                    }
                    break;
                case State.Running:
                    
                    break;
                case null:
                    Debug.LogWarning($"state of BT is null");
                    break;
                default:
                    Debug.LogError($"Unknown state {currentBehaviour.Value?.State}");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}