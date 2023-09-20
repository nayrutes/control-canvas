using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DefaultRunnerOverrides : IBehaviourRunnerOverrides
    {
    }
    
    public interface IBehaviourRunnerOverrides
    {
        IControl Forward(BehaviourWrapper behaviourWrapper, CanvasData controlFlow)
        {
            IControl nextControl = null;
            switch (behaviourWrapper.CombinedResultState)
            {
                case State.Success:
                    nextControl = behaviourWrapper.SuccessChild(controlFlow);
                    break;
                case State.Failure:
                    nextControl = behaviourWrapper.FailureChild(controlFlow);
                    behaviourWrapper.ChoseFailRoute = true;
                    break;
                case State.Running:
                    nextControl = behaviourWrapper.Behaviour;
                    break;
                default:
                    Debug.LogError($"Unknown state {behaviourWrapper?.CombinedResultState}");
                    throw new ArgumentOutOfRangeException();
            }
            return nextControl;
        }

        IControl Backward(Stack<IBehaviour> behaviourStack)
        {
            behaviourStack.Pop();
            if (behaviourStack.TryPeek(out IBehaviour topBehaviour))
            {
                return topBehaviour;
            }
            return null;
        }

        bool CheckNextSuggestionValidity(ExDirection direction, BehaviourRunner behaviourRunner,
            out bool changeRequested)
        {
            changeRequested = false;
            return true;
        }
    }
}