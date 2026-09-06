using UnityEngine;
using TMPro;
public class ZombieSpawner : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text zombiesText;

    [Header("Zombie")]
    public GameObject zombiePrefab;

    [Header("Player")]
    public Transform player;

    [Header("Wave Settings")]
    public int startingZombies = 5;
    public int additionalZombiesPerWave = 3;
    public float spawnRadius = 15f;
    public float nextWaveDelay = 5f;

    private int currentWave = 0;
    private int zombiesAlive = 0;
    private float nextWaveTime = 0f;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        // Start next wave when all zombies are dead
        if (zombiesAlive <= 0 && Time.time >= nextWaveTime)
        {
            StartNextWave();
        }
        if (zombiesText != null)
        {
            zombiesText.text = "ZOMBIES: " + zombiesAlive;
        }
    }

    void StartNextWave()
    {
        currentWave++;
        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave;
        }
        int zombiesToSpawn =
            startingZombies +
            (currentWave - 1) * additionalZombiesPerWave;

        Debug.Log("WAVE " + currentWave +
                  " - Zombies: " + zombiesToSpawn);

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
            new Vector3(randomCircle.x, 0f, randomCircle.y);

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