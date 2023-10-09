using System;
using Playground.Scripts.AI;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace Playground.Scripts
{
    public class CharacterMovement : MonoBehaviour
    {
        public Animator animator;
        public NavMeshAgent agent;
        public SpriteRenderer spriteRenderer;
        
        public bool flipX;

        public MovementBlackboard MovementBlackboard { get; set; } = new();
        
        private void Start()
        {
            //agent = GetComponent<NavMeshAgent>();
            //animator = GetComponentInChildren<Animator>();
            //spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            GetComponent<Character2DAgent>().AddBlackboard(MovementBlackboard);
            MovementBlackboard.TargetPosition.SkipLatestValueOnSubscribe().Subscribe(target =>
            {
                agent.SetDestination(target);
            }).AddTo(this);
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
            MovementBlackboard.CurrentPosition = transform.position;
        }



        public void SetDestination(Vector3 position)
        {
            agent.SetDestination(position);
        }
    }
}
