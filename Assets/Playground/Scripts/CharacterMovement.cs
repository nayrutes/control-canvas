using System;
using UnityEngine;
using UnityEngine.AI;

namespace Playground.Scripts
{
    public class CharacterMovement : MonoBehaviour
    {
        public Animator animator;
        public NavMeshAgent agent;
        public SpriteRenderer spriteRenderer;
        
        public bool flipX;

        private void Start()
        {
            //agent = GetComponent<NavMeshAgent>();
            //animator = GetComponentInChildren<Animator>();
            //spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        private void Update()
        {
            animator.SetFloat("Horizontal", agent.velocity.x);
            animator.SetFloat("Vertical", agent.velocity.y);
            animator.SetFloat("Speed", agent.velocity.sqrMagnitude);

            if (agent.velocity.x < -0.01 && flipX)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }



        public void SetDestination(Vector3 position)
        {
            agent.SetDestination(position);
        }
    }
}
