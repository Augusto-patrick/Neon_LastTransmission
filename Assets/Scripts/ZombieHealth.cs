using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    private float currentHealth;

    public ZombieSpawner spawner;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log("Zombie Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Zombie Died");

        if (spawner != null)
        {
            spawner.ZombieDied();
        }

        Destroy(gameObject);
    }
}