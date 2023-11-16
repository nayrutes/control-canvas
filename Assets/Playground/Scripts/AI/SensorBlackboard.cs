using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class SensorBlackboard : IBlackboard
    {
        public Subject<Unit> IsNearEnemyEvent { get; } = new();
        public ReactiveProperty<bool> IsNearEnemyEventRp { get; } = new();
        public bool IsNearEnemyEventBool { get; private set; } = new();

        public Transform Target { get; set; } = null;

        public Subject<Unit> AreaClearedEvent { get; } = new();
        
        public Subject<Unit> ExitHomeEvent { get; } = new();
        
        public SensorBlackboard()
        {
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEvent.OnNext(Unit.Default));
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEventBool = x);
        }
        
        public Vector3 lastPosition = Vector3.zero;
        public PoiSpot GetNearestPoiSpot()
        {
            var poiSpots = Object.FindObjectsOfType<PoiSpot>();
            PoiSpot nearestSpot = null;
            float nearestDistance = float.MaxValue;
            foreach (var poiSpot in poiSpots)
            {
                float distance = Vector3.Distance(lastPosition, poiSpot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpot = poiSpot;
                }
            }

            return nearestSpot;
        }

        public PoiSpot NearestPoiSpot
        {
            get
            {
                return GetNearestPoiSpot();
            }
        }
        
        public PoiSpot HomePoi { get; set; } = null;

        public Vector3 NearestOffLanternLightPos => NearestOffLanternLight.transform.position;
        public LanternLight NearestOffLanternLight
        {
            get
            {
                return GetNearestLanternLight(false);
            }
        }

        private LanternLight GetNearestLanternLight(bool isOn)
        {
            var lanternLights = Object.FindObjectsOfType<LanternLight>();
            LanternLight nearestLight = null;
            float nearestDistance = float.MaxValue;
            foreach (var lanternLight in lanternLights)
            {
                if (lanternLight.IsOn == isOn)
                {
                    float distance = Vector3.Distance(lastPosition, lanternLight.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestLight = lanternLight;
                    }
                }
            }

            return nearestLight;
        }
    }
}