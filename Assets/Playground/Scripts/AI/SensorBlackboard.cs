using System.Collections.Generic;
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
        
        public Subject<Unit> ForceExitStateEvent { get; } = new();

        private Dictionary<string, bool> sayLines = new();
        
        public SensorBlackboard()
        {
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEvent.OnNext(Unit.Default));
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEventBool = x);
            
            AssignNewRandomPoiSpot();
            RandomPoiSpotChangeEvent.Subscribe(x => AssignNewRandomPoiSpot());
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

        //public Vector3 NearestOffLanternLightPos => NearestOffLanternLight.transform.position;
        public PoiSpot NearestOffLanternLightPos => NearestOffLanternLight != null ? NearestOffLanternLight.GetComponent<PoiSpot>() : null;

        public bool LightOffNearby
        {
            get
            {
                GetNearestLanternLight(false, out float distance);
                return distance < 7f;
            }
        }

        public LanternLight NearestOffLanternLight
        {
            get
            {
                return GetNearestLanternLight(false,out _);
            }
        }

        private LanternLight GetNearestLanternLight(bool isOn, out float nearestDistance)
        {
            var lanternLights = Object.FindObjectsOfType<LanternLight>();
            LanternLight nearestLight = null;
            nearestDistance = float.MaxValue;
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

        public PoiSpot RandomPoiSpot { get; set; }
        public Subject<Unit> RandomPoiSpotChangeEvent { get; } = new();
        public void AssignNewRandomPoiSpot()
        {
            RandomPoiSpot = GetRandomPoiSpot();
        }
        private static PoiSpot GetRandomPoiSpot()
        {
            var poiSpots = Object.FindObjectsOfType<PoiSpot>();
            if (poiSpots.Length == 0)
            {
                return null;
            }

            return poiSpots[Random.Range(0, poiSpots.Length)];
        }

        public bool IsSayLineCompleted(string textToDisplay)
        {
            return sayLines.ContainsKey(textToDisplay) && sayLines[textToDisplay];
        }

        public void SayLineCompleted(string textToDisplay)
        {
            if (!sayLines.ContainsKey(textToDisplay))
            {
                sayLines.Add(textToDisplay, true);
            }
            else
            {
                sayLines[textToDisplay] = true;
            }
        }
        
        public void ResetSayLine(string textToDisplay)
        {
            if (sayLines.ContainsKey(textToDisplay))
            {
                sayLines[textToDisplay] = false;
            }
        }
    }
}