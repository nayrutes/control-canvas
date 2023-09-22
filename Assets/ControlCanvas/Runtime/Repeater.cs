﻿using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using Unity.VisualScripting;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class Repeater : IBehaviour, IBehaviourRunnerExecuter
    {
        public RepeaterMode mode = RepeaterMode.Loop;
        
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            return State.Running;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }

        public ExDirection ReEvaluateDirection(IControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
            State lastCombinedResult)
        {
            if(last == ExDirection.Forward && mode == RepeaterMode.Loop && wrapper.CombinedResultState == State.Running)
            {
                return ExDirection.Backward;
            }
            return last;
        }
        
        public IControl DoForward(IControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
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
            
            if (runnerBlackboard.behaviourStack.Count(x => x == this) > 1)
            {
                Debug.LogError("Repeater not configured correctly");
                return null;
            }
            
            return nextControl;
        }


        public IControl DoBackward(IControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard)
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