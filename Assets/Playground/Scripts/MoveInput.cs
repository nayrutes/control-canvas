using System;
using UnityEngine;

namespace Playground.Scripts
{
    public class MoveInput : MonoBehaviour
    {
        CharacterMovement cm;
        // public float Speed = 5f;
        // public Rigidbody2D rb;
        // public Animator animator;


        private Vector2 _movement;
        private void Start()
        {
            cm = GetComponent<CharacterMovement>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.z = 0;
                cm.SetDestination(target);
            }
        }

        // private void Update()
        // {
        //     _movement.x = Input.GetAxisRaw("Horizontal");
        //     _movement.y = Input.GetAxisRaw("Vertical");
        //     _movement.Normalize();
        //     animator.SetFloat("Horizontal", _movement.x);
        //     animator.SetFloat("Vertical", _movement.y);
        //     animator.SetFloat("Speed", _movement.sqrMagnitude);
        // }
        // void FixedUpdate()
        // {
        //     rb.MovePosition(rb.position + _movement * (Speed * Time.fixedDeltaTime));
        // }
    }
}