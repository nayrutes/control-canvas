﻿using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToPointBase
    {
        public bool overrideCurrentTarget = false;
        
        protected Vector3 TargetPosition { get; set; }
        public void OnStartBase(IControlAgent agentContext, IControl control)
        {
            // var bb = agentContext.GetBlackboard<MovementBlackboard>();
            // if ((!bb.IsAgentMoving || overrideCurrentTarget) && bb.TargetPosition.Value != TargetPosition && !agentContext.BlackboardFlowControl.Get<bool>(control, false))
            // {
            //     bb.TargetPosition.Value = TargetPosition;
            //     //bb.NoTargetSet = false;
            // }
            // else
            // {
            //     
            // }
        }

        public State OnUpdateBase(IControlAgent agentContext, float deltaTime, IControl control)
        {
            if (agentContext.BlackboardFlowControl.Get(control, false))
            {
                return State.Success;
            }
            
            var bb = agentContext.GetBlackboard<MovementBlackboard>();
            if ((!bb.IsAgentMoving || overrideCurrentTarget) && bb.TargetPosition.Value != TargetPosition && !agentContext.BlackboardFlowControl.Get<bool>(control, false))
            {
                bb.TargetPosition.Value = TargetPosition;
            }
            
            if (Vector3.Distance(TargetPosition, bb.CurrentPosition) < 1f)
            {
                agentContext.BlackboardFlowControl.Set(control,true);
                return State.Success;
            }
            // else if (bb.TargetUnreachabel)
            // {
            //     return State.Failure;
            // }
            else
            {
                return State.Running;
            }
        }
        
        public void OnStop(IControlAgent agentContext)
        {
            //var bb = agentContext.GetBlackboard<MovementBlackboard>();
            //bb.IsAgentMoving = false;
        }
        public void OnResetBase(IControlAgent agentContext, State blackboardLastCombinedResult, IControl control)
        {
            if (blackboardLastCombinedResult != State.Running)
            {
                agentContext.BlackboardFlowControl.Set(control, false);
            }
        }
    }
}