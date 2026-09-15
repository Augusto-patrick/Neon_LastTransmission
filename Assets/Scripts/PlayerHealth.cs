using UnityEngine;

public class PlayerHealth : MonoBehaviour

{
    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Feedback")]
    public DamageFlash damageFlash;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("PLAYER DIED");

        if (ScoreboardUI.Instance != null)
        {
            ZombieSpawner spawner =
                FindObjectOfType<ZombieSpawner>();

            ScoreboardUI.Instance.OnPlayerDied(spawner);

            return;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }
}