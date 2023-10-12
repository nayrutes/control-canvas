using UnityEngine;

namespace Playground.Scripts
{
    public class CharacterHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;

        public void TakeDamage(float attackDamage)
        {
            currentHealth -= attackDamage;
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }
    }
}