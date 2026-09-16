using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Robots")]
    public GameObject zombiePrefab;
    public GameObject[] robotPrefabs;

    [Header("Player")]
    public Transform player;

    [Header("Wave Settings")]
    public int totalWaves = 5;
    public int startingZombies = 5;
    public int additionalZombiesPerWave = 3;
    public float spawnRadius = 15f;
    public float minSpawnRadius = 8f;
    public float nextWaveDelay = 5f;

    [Header("Road Spawning")]
    public float roadCheckHeight = 60f;
    public float maxRoadHeight = 1.5f;
    public float minZombieSpacing = 2f;
    public int spawnAttempts = 30;

    [Header("Pickups")]
    public float pickupSpawnRadius = 8f;
    public int pickupAmmoAmount = 30;
    public float pickupLifetime = 30f;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text zombiesText;

    private int currentWave = 0;
    private int zombiesAlive = 0;
    private float nextWaveTime = 0f;
    private float startTime = 0f;
    private float finishTime = 0f;
    private bool gameFinished = false;
    private bool gameStarted = false;

    private readonly List<GameObject> activePickups =
        new List<GameObject>();

    public int Kills { get; private set; }
    public int CurrentWave
    {
        get { return currentWave; }
    }

    public float SurvivalTime
    {
        get
        {
            if (gameFinished)
                return finishTime;

            return gameStarted ? Time.time - startTime : 0f;
        }
    }

    public int ComputeScore()
    {
        return Kills * 100 +
            currentWave * 500 +
            Mathf.FloorToInt(SurvivalTime) * 10;
    }

    void Start()
    {
        if (robotPrefabs == null || robotPrefabs.Length == 0)
        {
            robotPrefabs = Resources.LoadAll<GameObject>("Robots");
        }

        ScoreboardUI ui = gameObject.GetComponent<ScoreboardUI>();

        if (ui == null)
        {
            ui = gameObject.AddComponent<ScoreboardUI>();
        }

        ui.Initialize(this);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!gameStarted || gameFinished)
        {
            if (zombiesText != null && gameFinished)
            {
                zombiesText.text =
                    "SCORE: " + ComputeScore() +
                    "\nTIME: " + finishTime.ToString("F1") + "s";
            }

            RotatePickups();
            return;
        }

        if (zombiesText != null)
        {
            zombiesText.text = "ROBOTS: " + zombiesAlive;
        }

        RotatePickups();

        if (zombiesAlive <= 0)
        {
            if (currentWave >= totalWaves)
            {
                FinishGame();
                return;
            }

            if (Time.time >= nextWaveTime)
            {
                StartNextWave();
            }
        }
    }

    public void BeginGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;
        startTime = Time.time;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartNextWave();
    }

    void StartNextWave()
    {
        currentWave++;

        int zombiesToSpawn =
            startingZombies +
            (currentWave - 1) * additionalZombiesPerWave;

        Debug.Log(
            "WAVE " + currentWave +
            " - Robots: " + zombiesToSpawn
        );

        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave + " / " + totalWaves;
        }

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
        }

        SpawnPickups(currentWave);

        nextWaveTime = Time.time + nextWaveDelay;
    }

    void SpawnZombie()
    {
        if (!TryFindRoadPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning(
                "No valid road spawn found; using fallback near player."
            );

            Vector2 randomCircle =
                Random.insideUnitCircle.normalized * spawnRadius;

            spawnPosition =
                player.position +
                new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        GameObject prefab =
            (robotPrefabs != null && robotPrefabs.Length > 0)
                ? robotPrefabs[Random.Range(0, robotPrefabs.Length)]
                : zombiePrefab;

        GameObject zombie = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        if (robotPrefabs != null &&
            System.Array.IndexOf(robotPrefabs, prefab) >= 0)
        {
            FitRobot(zombie);

            if (zombie.GetComponent<RobotAnimator>() == null)
            {
                zombie.AddComponent<RobotAnimator>();
            }
        }

        zombiesAlive++;

        ZombieHealth health =
            zombie.GetComponent<ZombieHealth>();

        if (health == null)
        {
            health = zombie.AddComponent<ZombieHealth>();
        }

        ZombieAI ai =
            zombie.GetComponent<ZombieAI>();

        if (ai == null)
        {
            ai = zombie.AddComponent<ZombieAI>();
        }

        ZombieHitEffect hitEffect =
            zombie.GetComponent<ZombieHitEffect>();

        if (hitEffect == null)
        {
            zombie.AddComponent<ZombieHitEffect>();
        }

        CapsuleCollider capsule =
            zombie.GetComponent<CapsuleCollider>();

        if (capsule == null)
        {
            capsule = zombie.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.5f;
            capsule.height = 2f;
            capsule.direction = 1;
            capsule.center = new Vector3(0f, 1f, 0f);
        }
        else
        {
            capsule.isTrigger = true;
        }

        health.maxHealth = 100f;
        health.spawner = this;

        if (ai != null)
        {
            if (ai.player == null && player != null)
            {
                ai.player = player;
            }
        }
    }

    static void FitRobot(GameObject enemy)
    {
        Bounds bounds = GetRenderBounds(enemy);

        if (bounds.size == Vector3.zero)
            return;

        float maxDimension =
            Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));

        if (maxDimension <= 0.001f)
            return;

        float scale = 2.2f / maxDimension;

        enemy.transform.localScale *= scale;

        bounds = GetRenderBounds(enemy);

        enemy.transform.position +=
            new Vector3(0f, GroundHeight(enemy.transform.position) - bounds.min.y, 0f);

        EnsureModelMaterials(enemy);
    }

    static Bounds GetRenderBounds(GameObject enemy)
    {
        Renderer[] renderers =
            enemy.GetComponentsInChildren<Renderer>(true);

        Bounds bounds =
            new Bounds(enemy.transform.position, Vector3.zero);

        bool first = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
                continue;

            if (first)
            {
                bounds = renderers[i].bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;
    }

    static float GroundHeight(Vector3 position)
    {
        Vector3 origin =
            new Vector3(position.x, position.y + 5f, position.z);

        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, 50f))
        {
            return hit.point.y;
        }

        return position.y;
    }

    static void EnsureModelMaterials(GameObject enemy)
    {
        Renderer[] renderers =
            enemy.GetComponentsInChildren<Renderer>(true);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            bool changed = false;

            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] == null ||
                    materials[j].shader == null ||
                    materials[j].shader.name == "Hidden/InternalErrorShader")
                {
                    Material fallback = new Material(shader);
                    fallback.color = new Color(0.42f, 0.48f, 0.52f);
                    materials[j] = fallback;
                    changed = true;
                }
            }

            if (changed)
            {
                renderers[i].sharedMaterials = materials;
            }
        }
    }

    bool TryFindRoadPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            float radius = Random.Range(minSpawnRadius, spawnRadius);
            Vector2 direction =
                Random.insideUnitCircle.normalized * radius;

            Vector3 candidate =
                player.position +
                new Vector3(direction.x, 0f, direction.y);

            if (!IsOnRoad(candidate))
                continue;

            if (TooCloseToZombie(candidate))
                continue;

            result = candidate;
            return true;
        }

        result = player.position;
        return false;
    }

    bool IsOnRoad(Vector3 position)
    {
        Vector3 origin = new Vector3(
            position.x,
            position.y + roadCheckHeight,
            position.z
        );

        RaycastHit hit;

        if (!Physics.Raycast(
                origin,
                Vector3.down,
                out hit,
                roadCheckHeight + 50f
            ))
        {
            return false;
        }

        return hit.point.y <= maxRoadHeight;
    }

    bool TooCloseToZombie(Vector3 position)
    {
        for (int i = 0; i < ZombieHealth.All.Count; i++)
        {
            if (ZombieHealth.All[i] == null)
                continue;

            if (Vector3.Distance(
                    ZombieHealth.All[i].transform.position,
                    position
                ) < minZombieSpacing)
            {
                return true;
            }
        }

        return false;
    }

    void SpawnPickups(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = player.position;

            if (TryFindPickupPosition(out Vector3 roadPosition))
            {
                position = roadPosition;
            }
            else
            {
                Vector2 randomOffset =
                    Random.insideUnitCircle * pickupSpawnRadius;

                position =
                    player.position +
                    new Vector3(
                        randomOffset.x,
                        1f,
                        randomOffset.y
                    );
            }

            GameObject pickup = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );
            pickup.name = "Pickup (Wave " + currentWave + ")";
            pickup.tag = "Untagged";
            pickup.transform.position = position;
            pickup.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            BoxCollider collider = pickup.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            MeshRenderer renderer = pickup.GetComponent<MeshRenderer>();

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = new Color(1f, 0.3f, 0.9f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.8f) * 1.4f);
            renderer.material = material;

            Pickup pickupScript = pickup.AddComponent<Pickup>();
            pickupScript.pickupType = Pickup.PickupType.Ammo;
            pickupScript.ammoAmount = pickupAmmoAmount;

            Destroy(pickup, pickupLifetime);

            activePickups.Add(pickup);
        }
    }

    bool TryFindPickupPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle * pickupSpawnRadius;

            Vector3 candidate =
                player.position +
                new Vector3(randomOffset.x, 1f, randomOffset.y);

            if (!IsOnRoad(candidate))
                continue;

            result = candidate;
            return true;
        }

        result = player.position;
        return false;
    }

    void RotatePickups()
    {
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            GameObject pickup = activePickups[i];

            if (pickup == null)
            {
                activePickups.RemoveAt(i);
                continue;
            }

            pickup.transform.Rotate(
                Vector3.up,
                60f * Time.deltaTime,
                Space.World
            );
        }
    }

    void FinishGame()
    {
        gameFinished = true;
        finishTime = Time.time - startTime;

        if (waveText != null)
        {
            waveText.text = "GAME CLEARED!";
        }

        if (zombiesText != null)
        {
            zombiesText.text =
                "SCORE: " + ComputeScore() +
                "\nTIME: " + finishTime.ToString("F1") + "s";
        }

        Debug.Log(
            "GAME CLEARED - Score: " + ComputeScore() +
            " Time: " + finishTime.ToString("F1") + "s"
        );

        ScoreboardUI ui = gameObject.GetComponent<ScoreboardUI>();

        if (ui != null)
        {
            ui.OnSurvived(this);
        }
    }

    public void ZombieDied()
    {
        if (zombiesAlive > 0)
        {
            zombiesAlive--;
        }

        Kills++;
    }
}