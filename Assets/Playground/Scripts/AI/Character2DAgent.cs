using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Playground.Scripts.AI
{
    public class Character2DAgent : ControlAgentMonoBase
    {
        public EntityTypes EntityType;
        [SerializeField] private PoiSpot HomePoi;
        [SerializeField] private float _interactionRange = 2;

        private void Start()
        {
            var bb = new SensorBlackboard();
            AddBlackboard(bb);
            if (HomePoi != null)
            {
                bb.HomePoi = HomePoi;   
            }
            else
            {
                List<PoiSpot> homeSpots = FindObjectsByType<PoiSpot>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(x => x.name.Contains("House")).ToList();
                if (homeSpots.Count > 0)
                {
                    bb.HomePoi = homeSpots[Random.Range(0, homeSpots.Count)];
                }
            }
        }

        private void Update()
        {
            var bb = GetBlackboard<WorldEntityBlackboard>();
            EntityTypes enemyType = EntityType == EntityTypes.Forester ? EntityTypes.Townsfolk : EntityTypes.Forester;
            var go = bb.GetNearestEntityOfType(enemyType, transform.position, out float neDistance);
            SensorBlackboard sensorBlackboard = GetBlackboard<SensorBlackboard>();
            if (go != null)
            {
                sensorBlackboard.IsNearEnemyEventRp.Value = neDistance < 5f;
                sensorBlackboard.Target = go.transform;
            }
            else
            {
                sensorBlackboard.Target = null;
            }
            sensorBlackboard.lastPosition = transform.position;
        }

        public GameObject GetNearestEnemy(out float neDistance)
        {
            var bb = GetBlackboard<WorldEntityBlackboard>();
            EntityTypes enemyType = EntityType == EntityTypes.Forester ? EntityTypes.Townsfolk : EntityTypes.Forester;
            return bb.GetNearestEntityOfType(enemyType, transform.position, out neDistance);
        }
        
        public void PickupClosestItem()
        {
            GetComponent<Hero>()?.PickupClosestItem();
        }

        public bool Interact()
        {
            var bb = GetBlackboard<WorldInfo>();
            bb.GetNearestInteractable(transform.position, out IInteractable nearestInteractable, out float nearestDistance);
            if(nearestInteractable != null && nearestDistance < _interactionRange && nearestInteractable.CanInteract)
            {
                return nearestInteractable.Interact();
                
            }
            return false;
        }

        public void MakeInVisibleAndFixed(bool b)
        {
            GetComponent<NavMeshAgent>().enabled = !b;
            GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => x.enabled = !b);
        }
    }
}