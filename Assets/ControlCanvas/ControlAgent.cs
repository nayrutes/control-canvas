using UnityEngine;

namespace ControlCanvas
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