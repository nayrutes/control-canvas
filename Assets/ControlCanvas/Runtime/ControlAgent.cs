﻿//using Demo;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlAgent : MonoBehaviour
    {
        public Blackboard blackboardAgent;
        public BlackboardFlowControl blackboardFlowControl = new();
        public string Name { get; set; }

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
    }
}