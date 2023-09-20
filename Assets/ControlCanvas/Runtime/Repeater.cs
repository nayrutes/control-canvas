using System.Collections.Generic;

namespace ControlCanvas.Runtime
{
    public class Repeater : IBehaviour, IBehaviourRunnerOverrides
    {
        public RepeaterMode mode = RepeaterMode.Loop;
        
        public void OnStart(ControlAgent agentContext)
        {
            
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            return State.Success;
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }

        public bool CheckNextSuggestionValidity(ExDirection direction, BehaviourRunner behaviourRunner,
            out bool changeRequested)
        {
            changeRequested = false;
            if(direction == ExDirection.Forward)
            {
                if (behaviourRunner.BehaviourStack.Contains(this))
                {
                    if (mode == RepeaterMode.Loop)
                    {
                        behaviourRunner.RepeaterStack.Push(this);   
                    }
                    changeRequested = true;
                    return false;
                }
            }
            else
            {
                if(mode == RepeaterMode.Always)
                {
                    behaviourRunner.RepeaterStack.Push(this);
                }
            }
            return true;
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