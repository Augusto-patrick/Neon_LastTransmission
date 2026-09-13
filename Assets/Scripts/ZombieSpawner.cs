using UnityEngine;
using TMPro;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie")]
    public GameObject zombiePrefab;

    [Header("Player")]
    public Transform player;

    [Header("Wave Settings")]
    public int startingZombies = 5;
    public int additionalZombiesPerWave = 3;
    public float spawnRadius = 15f;
    public float nextWaveDelay = 5f;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text zombiesText;

    private int currentWave = 0;
    private int zombiesAlive = 0;
    private float nextWaveTime = 0f;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        if (zombiesText != null)
        {
            zombiesText.text = "ZOMBIES: " + zombiesAlive;
        }

        if (zombiesAlive <= 0)
        {
            if (Time.time >= nextWaveTime)
            {
                StartNextWave();
            }
        }
    }

    void StartNextWave()
    {
        currentWave++;

        int zombiesToSpawn =
            startingZombies +
            (currentWave - 1) * additionalZombiesPerWave;

        Debug.Log(
            "WAVE " + currentWave +
            " - Zombies: " + zombiesToSpawn
        );

        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave;
        }

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
        }

        nextWaveTime = Time.time + nextWaveDelay;
    }

    void SpawnZombie()
    {
        Vector2 randomCircle =
            Random.insideUnitCircle.normalized * spawnRadius;

        Vector3 spawnPosition =
            player.position +
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            );

        GameObject zombie = Instantiate(
            zombiePrefab,
            spawnPosition,
            Quaternion.identity
        );

        zombiesAlive++;

        ZombieHealth health =
            zombie.GetComponent<ZombieHealth>();

        if (health != null)
        {
            health.spawner = this;
        }
    }

    public void ZombieDied()
    {
        zombiesAlive--;
    }
}