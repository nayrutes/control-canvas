namespace Playground.Scripts
{
    public class CharacterHealth
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
        }
    }
}