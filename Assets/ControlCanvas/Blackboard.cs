using System.Collections.Generic;
using UnityEngine;

namespace ControlCanvas
{
    [System.Serializable]
    public class Blackboard
    {
        public Vector3 moveToPosition;
        public GameObject moveToObject;
        
        public List<Transform> patrolPoints;
    }
}