using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlAgent : MonoBehaviour
    {
        public Blackboard blackboardAgent;
        public string Name { get; set; }
        
        public bool CheckTransitionCondition()
        {
            return false;
        }
    }
}