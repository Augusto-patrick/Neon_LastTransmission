using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float moveSpeed = 2f;

    [Header("Attack")]
    public float damage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Knockback")]
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.15f;

    [Header("Separation")]
    public float separationRange = 1.2f;
    public float separationForce = 2.5f;

    private float nextAttackTime = 0f;
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;

    public bool IsMoving { get; private set; }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (player == null)
            return;

        IsMoving = false;

        if (knockbackTimer > 0)
        {
            transform.position +=
                knockbackDirection *
                knockbackForce *
                Time.deltaTime;

            knockbackTimer -= Time.deltaTime;
            return;
        }

        ApplySeparation();

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance > detectionRange)
            return;

        if (distance > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
        }
    }

    void ApplySeparation()
    {
        Vector3 separation = Vector3.zero;

        for (int i = 0; i < ZombieHealth.All.Count; i++)
        {
            ZombieHealth other = ZombieHealth.All[i];

            if (other == null || other.transform == transform)
                continue;

            Vector3 delta =
                transform.position - other.transform.position;

            delta.y = 0f;

            float distance = delta.magnitude;

            if (distance <= 0.001f || distance > separationRange)
                continue;

            float strength =
                (separationRange - distance) / separationRange;

            separation += delta.normalized * strength;
        }

        if (separation != Vector3.zero)
        {
            transform.position +=
                separation * separationForce * Time.deltaTime;
        }
    }

    void ChasePlayer()
    {
        IsMoving = true;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    8f * Time.deltaTime
                );
        }

        transform.position +=
            transform.forward *
            moveSpeed *
            Time.deltaTime;
    }

    void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime =
                Time.time + attackCooldown;

            PlayerHealth health =
                player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            RobotAnimator animator = GetComponent<RobotAnimator>();

            if (animator != null)
            {
                animator.PlayLunge();
            }

            Debug.Log("Robot attacked player!");
        }
    }

    public void Knockback(Vector3 hitDirection)
    {
        knockbackDirection = hitDirection.normalized;
        knockbackTimer = knockbackDuration;
    }
}