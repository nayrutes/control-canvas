using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DefaultRunnerExecuter : IBehaviourRunnerExecuter
    {
    }
    
    public interface IBehaviourRunnerExecuter
    {
        
        ExDirection ReEvaluateDirection(IControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
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
        
        IControl DoForward(IControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
            BehaviourWrapper behaviourWrapper, CanvasData controlFlow)
        {
            //TODO preferably do this check before entering the behaviour
            if (runnerBlackboard.behaviourStack.Count(x => x == this) > 1)
            {
                Debug.LogError("Loop detected without repeater");
                return null;
            }
            
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

        IControl DoBackward(IControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard)
        {
            runnerBlackboard.behaviourStack.TryPop(out _);
            if (runnerBlackboard.behaviourStack.TryPeek(out IBehaviour topBehaviour))
            {
                return topBehaviour;
            }
            return null;
        }

    }

}