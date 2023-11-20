using System;
using System.Linq;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace Playground.Scripts
{
    public class WorldInfo : MonoBehaviour, IBlackboard
    {
        private IInteractable[] interactables;
        
        public ReactiveProperty<bool> Night { get; } = new();
        public Subject<Unit> NightEvent { get; } = new();
        public ReactiveProperty<bool> Day { get; } = new();
        public Subject<Unit> DayEvent { get; } = new();
        public ReactiveProperty<bool> Dawn { get; } = new();
        public Subject<Unit> DawnEvent { get; } = new();
        public ReactiveProperty<bool> Dusk { get; } = new();
        public Subject<Unit> DuskEvent { get; } = new();
        private void Start()
        {
            interactables = FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>().ToArray();
            
            Night.Where(x=> x).Subscribe(x => NightEvent.OnNext(Unit.Default)).AddTo(this);
            Day.Where(x=> x).Subscribe(x => DayEvent.OnNext(Unit.Default)).AddTo(this);
            Dawn.Where(x=> x).Subscribe(x => DawnEvent.OnNext(Unit.Default)).AddTo(this);
            Dusk.Where(x=> x).Subscribe(x => DuskEvent.OnNext(Unit.Default)).AddTo(this);
        }

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

        public void GetNearestInteractable(Vector3 transformPosition, out IInteractable nearestInteractable, out float nearestDistance)
        {
            nearestDistance = float.MaxValue;
            nearestInteractable = null;
            foreach (var interactable in interactables)
            {
                if(!interactable.CanInteract)
                    continue;
                var distance = Vector3.Distance(((MonoBehaviour)interactable).transform.position, transformPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractable = interactable;
                }
            }
        }
    }
}