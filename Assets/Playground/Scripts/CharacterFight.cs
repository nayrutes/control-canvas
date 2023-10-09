using UnityEngine;

namespace Playground.Scripts
{
    public class CharacterFight : MonoBehaviour
    {
        public Animator animator;
        public float attackRange = 0.5f;
        public float attackRate = 2f;
        public float attackDamage = 10f;
        
        private float _cooldown = 0f;
        
        public bool CanAttack()
        {
            return _cooldown <= 0f;
        }
        
        public void Attack()
        {
            if(!CanAttack())
                return;
            _cooldown = 1f / attackRate;
            animator.SetTrigger("Attack");
            OnAttack();
        }
        
        private void Update()
        {
            _cooldown -= Time.deltaTime;
        }
        
        public void OnAttack()
        {
            var results = new Collider2D[10];
            var size = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, results);
            for (var i = 0; i < size; i++)
            {
                var collider = results[i];
                if (collider == null || collider.gameObject == gameObject)
                {
                    continue;
                }
                var health = collider.GetComponent<CharacterHealth>();
                health?.TakeDamage(attackDamage);
            }
        }
    }
}