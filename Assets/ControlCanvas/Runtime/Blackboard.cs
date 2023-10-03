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
        public Subject<Unit> OtherExitEvent { get; set; } = new();

        public List<IObservable<Unit>> GetExitEvents()
        {
            return new List<IObservable<Unit>>()
            {
                ExitEvent,
                OtherExitEvent
            };
        }
        
        public static List<string> GetExitEventNames()
        {
            return new List<string>()
            {
                "Exit Event 1",
                "Exit Event 2"
            };
        }
    }
}