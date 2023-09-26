using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ControlCanvas
{
    [System.Serializable]
    public class Blackboard
    {
        public Vector3 moveToPosition;
        public GameObject moveToObject;
        
        public List<Transform> patrolPoints;
        public string SubFlowPath { get; set; }
        public Subject<Unit> ExitEvent { get; set; } = new();
    }
}