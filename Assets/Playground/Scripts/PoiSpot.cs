using System;
using System.Collections.Generic;
using ControlCanvas.Runtime;
using Playground.Scripts.AI;
using UnityEngine;

namespace Playground.Scripts
{
    public class PoiSpot : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private int subSpots = 1;
        [SerializeField]
        private float subSpotRadius = 1f;
        [SerializeField]
        private bool horizontalFirst = true;
        
        private List<Vector3> subSpotPositions = new ();
        private List<bool> subSpotOccupied = new ();
        private List<Character2DAgent> agentsOnSpot = new ();
        
        private int freeSpots = 1;
        [SerializeField] private bool _interactionPossible;


        private void Start()
        {
            subSpotPositions.AddRange(CalculateSubSpots());
            for (var i = 0; i < subSpots; i++)
            {
                subSpotOccupied.Add(false);
                agentsOnSpot.Add(null);
            }
        }

        private void Update()
        {
            for (int i = 0; i < agentsOnSpot.Count; i++)
            {
                if(agentsOnSpot[i] == null)
                    continue;
                if (Vector2.Distance(subSpotPositions[i], agentsOnSpot[i].transform.position) > 2f)
                {
                    agentsOnSpot[i] = null;
                    subSpotOccupied[i] = false;
                }
            }
        }

        public bool HasFreeSpot()
        {
            return freeSpots > 0;
        }

        public bool IsSpotFree(Vector2 position)
        {
            var index = subSpotPositions.IndexOf(position);
            if (index == -1)
            {
                throw new ArgumentException("Position is not a sub spot");
            }

            return !subSpotOccupied[index];
        }
        
        public bool GetFreeSpotPosition(out Vector2 position)
        {
            var index = GetFreeSpotIndex();
            if (index == -1)
            {
                position = Vector2.zero;
                return false;
            }

            position = subSpotPositions[index];
            return true;
        }
        
        private int GetFreeSpotIndex()
        {
            for (var i = 0; i < subSpotOccupied.Count; i++)
            {
                if (!subSpotOccupied[i])
                {
                    return i;
                }
            }

            return -1;
        }
        
        private Vector3[] CalculateSubSpots()
        {
            if(subSpots == 1)
                return new []{transform.position};
            
            var positions = new Vector3[subSpots];
            
            var angle = 360f / subSpots;
            var currentAngle = 0f;
            if(horizontalFirst)
            {
                currentAngle = 90f;
            }
            for (var i = 0; i < subSpots; i++)
            {
                var position = Vector3.zero;
                var radians = currentAngle * Mathf.Deg2Rad;
                position.x = Mathf.Sin(radians) * subSpotRadius;
                position.y = Mathf.Cos(radians) * subSpotRadius;
                positions[i] = transform.position + position;
                currentAngle += angle;
            }

            return positions;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, subSpotRadius);
            foreach (var position in CalculateSubSpots())
            {
                Gizmos.DrawWireSphere(position, 0.3f);
            }
        }

        public void OccupySpot(Vector2 spotPosition, Character2DAgent agent)
        {
            var index = subSpotPositions.IndexOf(spotPosition);
            if (index == -1)
            {
                throw new ArgumentException("Position is not a sub spot");
            }
            if(agent == null)
                throw new ArgumentException("Agent is null");

            if (!subSpotOccupied[index])
            {
                subSpotOccupied[index] = true;
                agentsOnSpot[index] = agent;
                freeSpots--;
            }
        }

        public bool CanInteract  => _interactionPossible;
        public bool Interact()
        {
            return true;
        }
    }
}