using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using Unity.VisualScripting;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class Repeater : IBehaviour, IBehaviourRunnerExecuter
    {
        public RepeaterMode mode = RepeaterMode.Loop;
        
        public void OnStart(ControlAgent agentContext)
        {
            
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            return State.Running;
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }

        public ExDirection ReEvaluateDirection(ControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
            State lastCombinedResult)
        {
            if(last == ExDirection.Forward && mode == RepeaterMode.Loop && wrapper.CombinedResultState == State.Running)
            {
                return ExDirection.Backward;
            }
            return last;
        }
        
        public IControl DoForward(ControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
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
                    nextControl = behaviourWrapper.SuccessChild(controlFlow);
                    break;
                default:
                    Debug.LogError($"Unknown state {behaviourWrapper?.CombinedResultState}");
                    throw new ArgumentOutOfRangeException();
            }

            runnerBlackboard.repeaterList.Remove(this);
            return nextControl;
        }


        public IControl DoBackward(ControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard)
        {
            
            
            IControl nextControl = null;
            runnerBlackboard.behaviourStack.Pop();
            if (runnerBlackboard.behaviourStack.TryPeek(out IBehaviour topBehaviour))
            {
                nextControl = topBehaviour;
            }

            if (!runnerBlackboard.behaviourStack.Contains(this))
            {
                if (mode == RepeaterMode.Loop)
                {
                    runnerBlackboard.repeaterList.Add(this);
                }else if(mode == RepeaterMode.Always)
                {
                    runnerBlackboard.repeaterList.Add(this);
                }
                else
                {
                    Debug.LogError($"Unknown mode {mode}");
                }
            }
            
            return nextControl;
        }
    }
    
    public enum RepeaterMode
    {
        Always,
        Loop,
        //UntilSuccess,
        //UntilFailure
    }
}