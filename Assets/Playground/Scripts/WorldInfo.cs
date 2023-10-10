using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts
{
    public class WorldInfo : MonoBehaviour, IBlackboard
    {
        [SerializeField]
        private Transform _lakeSpot;
        public Vector3 LakeSpotPosition => _lakeSpot.position;
        
        [SerializeField]
        private Transform _forestSpot;
        public Vector3 ForestSpotPosition => _forestSpot.position;
        
        [SerializeField]
        private Transform _townSpot;
        public Vector3 TownSpotPosition => _townSpot.position;
        
    }
}