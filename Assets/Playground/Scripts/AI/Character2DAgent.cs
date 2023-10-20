using System;
using System.Collections.Generic;
using ControlCanvas;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class Character2DAgent : ControlAgentMonoBase
    {
        public EntityTypes EntityType;

        private void Start()
        {
            var bb = new SensorBlackboard();
            AddBlackboard(bb);
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
    }
}