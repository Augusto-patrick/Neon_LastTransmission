using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public static readonly List<ZombieHealth> All = new List<ZombieHealth>();

    public float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;

    public ZombieSpawner spawner;

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log("Robot Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Robot destroyed");

        if (spawner != null)
        {
            spawner.ZombieDied();
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        ZombieAI ai = GetComponent<ZombieAI>();

        if (ai != null)
        {
            ai.enabled = false;
        }

        RobotAnimator animator = GetComponent<RobotAnimator>();

        if (animator != null)
        {
            animator.enabled = false;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            transform.position = Vector3.Lerp(
                startPosition,
                startPosition + Vector3.down * 0.5f,
                progress
            );

            Vector3 euler = startRotation.eulerAngles;

            transform.rotation = Quaternion.Euler(
                euler.x + Mathf.Sin(progress * Mathf.PI * 0.5f) * 80f,
                euler.y,
                euler.z
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}