using System;
using System.Linq;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class Repeater : IBehaviour, IBehaviourRunnerExecuter
    {
        public RepeaterMode mode = RepeaterMode.Loop;
        
        private struct RepeaterRuntimeData
        {
            public bool firstHit;
            public bool secondHit;
        }
        
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

        
        private RepeaterRuntimeData GetRuntimeData(IControlAgent agentContext)
        {
            RepeaterRuntimeData data = agentContext.BlackboardFlowControl.SetIfNeededWithFunctionAndGet(this, () => new RepeaterRuntimeData());
            return data;
        }
        
        private void SetRuntimeData(IControlAgent agentContext, RepeaterRuntimeData data)
        {
            agentContext.BlackboardFlowControl.Set(this, data);
        }
        
        public ExDirection ReEvaluateDirection(IControlAgent agentContext,
            BehaviourRunnerBlackboard blackboard, BehaviourWrapper wrapper)
        {
            RepeaterRuntimeData data = GetRuntimeData(agentContext);
            if(blackboard.LastDirection == ExDirection.Forward)
            {
                if (!data.firstHit)
                {
                    data.firstHit = true;
                }
                else
                {
                    data.secondHit = true;
                }
                SetRuntimeData(agentContext, data);
            }
            
            if(blackboard.LastDirection == ExDirection.Forward && mode == RepeaterMode.Loop && data.secondHit)
            {
                return ExDirection.Backward;
            }
            return blackboard.LastDirection;
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
            RepeaterRuntimeData data = GetRuntimeData(agentContext);
            
            IControl nextControl = null;
            runnerBlackboard.behaviourStack.Pop();
            if (runnerBlackboard.behaviourStack.TryPeek(out IBehaviour topBehaviour))
            {
                nextControl = topBehaviour;
            }

            if (!runnerBlackboard.behaviourStack.Contains(this))
            {
                if (mode == RepeaterMode.Loop && data.secondHit)
                {
                    runnerBlackboard.repeaterList.Add(this);
                }else if(mode == RepeaterMode.Always)
                {
                    runnerBlackboard.repeaterList.Add(this);
                }
                
                data.firstHit = false;
                data.secondHit = false;
                SetRuntimeData(agentContext, data);
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