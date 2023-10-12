using System;
using System.Collections.Generic;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace ControlCanvas
{
    [System.Serializable]
    public class DebugBlackboard : IBlackboard
    {
        public Vector3 moveToPosition;
        public GameObject moveToObject;
        
        public List<Transform> patrolPoints;
        public string SubFlowPath { get; set; }
        public Subject<Unit> ExitEvent { get; set; } = new();
        public Subject<Unit> OtherExitEvent { get; set; } = new();

        public string TestString { get; set; } = "Test";
        public int TestInt { get; set; } = 42;
        public float TestFloat { get; set; }= 4.2f;
        public bool TestBool { get; set; }= true;
        
        public List<IObservable<object>> GetExitEvents()
        {
            return new List<IObservable<object>>()
            {
                ExitEvent.Select(x => (object)x),
                OtherExitEvent.Select(x => (object)x)
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