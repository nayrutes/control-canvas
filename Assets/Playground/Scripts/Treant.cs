using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Playground.Scripts
{
    public class Treant : MonoBehaviour
    {
        CharacterMovement cm;

        public List<Transform> targets;
        private int currentTargetIndex = 0;
        
        private void Start()
        {
            cm = GetComponent<CharacterMovement>();
            if (cm == null)
            {
                Debug.LogError("CharacterMovement not found");
            }
            //cm.SetDestination(targets[currentTargetIndex].position);
            cm.MovementBlackboard.TargetPositions = targets.ConvertAll(t => t.position);
        }

        // private void Update()
        // {
        //     if (IsAtTarget())
        //     {
        //         currentTargetIndex = (currentTargetIndex + 1) % targets.Count;
        //         cm.SetDestination(targets[currentTargetIndex].position);
        //     }
        // }
        
        private bool IsAtTarget()
        {
            return Vector3.Distance(transform.position, targets[currentTargetIndex].position) < 0.1f;
        }
    }
}