using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DefaultRunnerExecuter : IBehaviourRunnerExecuter
    {
    }
    
    public interface IBehaviourRunnerExecuter
    {
        
        ExDirection ReEvaluateDirection(ControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
            State lastCombinedResult)
        {
            if (last == ExDirection.Backward)
            {
                if (!wrapper.ChoseFailRoute && lastCombinedResult == State.Failure)
                {
                    wrapper.CombinedResultState = State.Failure;
                    return ExDirection.Forward;
                }
            }
            return last;
        }
        
        IControl DoForward(ControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
            BehaviourWrapper behaviourWrapper, CanvasData controlFlow)
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

        IControl DoBackward(ControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard)
        {
            runnerBlackboard.behaviourStack.Pop();
            if (runnerBlackboard.behaviourStack.TryPeek(out IBehaviour topBehaviour))
            {
                return topBehaviour;
            }
            return null;
        }

    }

}