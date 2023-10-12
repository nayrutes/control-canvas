//using Demo;

using System;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlAgent : MonoBehaviour, IControlAgent
    {
        public DebugBlackboard DebugBlackboardAgent { get; set; } = new();
        public BlackboardFlowControl BlackboardFlowControl { get; set; } = new();
        public string Name { get; set; }
        public IBlackboard GetBlackboard(Type blackboardType)
        {
            return DebugBlackboardAgent;
        }

        public T GetBlackboard<T>() where T : IBlackboard
        {
            return (T)(IBlackboard)DebugBlackboardAgent;
        }

        public bool testBool;
        public bool testBoolSecond;
        public State testState;

        public bool CheckTransitionCondition()
        {
            return false;
        }
        
        public void SetDestination(Vector3 destination)
        {
            //GetComponent<NpcController>().SetDestination(destination);
        }

        public bool CheckDestinationReached(Vector3 destination)
        {
            return true;
            //return GetComponent<NpcController>().CheckDestinationReached(destination);
        }

        public bool CheckDestinationReachable( )
        {
            return true;
            //return GetComponent<NpcController>().CheckDestinationReachable();
        }
        

        [ContextMenu("Trigger Exit event")]
        public void TriggerExitEvent()
        {
            DebugBlackboardAgent.ExitEvent.OnNext(Unit.Default);
        }
    }
}