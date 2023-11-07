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
        
        [SerializeField]
        private Transform SwordSpot;
        public Vector3 SwordSpotPosition => SwordSpot.position;
        public bool IsSwordAvailable => SwordSpot.gameObject.activeSelf;

        [SerializeField]
        private PoiSpot _stand1BuyerSpot;
        public PoiSpot Stand1BuyerSpot => _stand1BuyerSpot;
    }
}