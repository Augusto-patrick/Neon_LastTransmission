using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Converts robot/structure FBX models into enemy prefabs and wires them
/// into the ZombieSpawner used by the MainGame scene.
///
/// Usage:
///   1. Drop any robot FBX (SpiderBot, Assault Mech, Niko robot, ...)
///      somewhere under Assets.
///   2. In the Unity Editor menu: Tools > Robot Apocalypse >
///      Convert Robot Models to Enemies.
///
/// The tool auto-detects the models, generates a ready-to-spawn prefab per
/// model, and assigns them to ZombieSpawner.robotPrefabs in MainGame.unity.
/// </summary>
public static class RobotEnemyBuilder
{
    private const string ROBOT_PREFAB_DIR = "Assets/Prefabs/Robots";
    private const string SCENE_PATH = "Assets/Scenes/MainGame.unity";

    private static readonly string[] RobotKeywords =
    {
        "robot", "mech", "spider", "assault", "niko", "drone", "bot"
    };

    private const float TargetSize = 1.7f;

    [MenuItem("Tools/Robot Apocalypse/Convert Robot Models to Enemies")]
    public static void BuildAll()
    {
        if (File.Exists(SCENE_PATH))
        {
            EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        EnsurePrefabFolder();
        CleanOldRobotPrefabs();

        List<GameObject> robotPrefabs = new List<GameObject>();

        string[] fbxPaths = FindRobotModels();

        foreach (string fbxPath in fbxPaths)
        {
            GameObject prefab = BuildRobotPrefab(fbxPath);

            if (prefab != null)
            {
                robotPrefabs.Add(prefab);
            }
        }

        if (robotPrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Robot Apocalypse",
                "No robot FBX models were found.\n\n" +
                "Drop robot models (SpiderBot, Assault Mech, " +
                "Niko robot, etc.) anywhere under Assets and try again.",
                "OK"
            );

            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UpdateSpawner(robotPrefabs);

        string list = string.Empty;

        for (int i = 0; i < robotPrefabs.Count; i++)
        {
            list += "  - " + AssetDatabase.GetAssetPath(robotPrefabs[i]) + "\n";
        }

        EditorUtility.DisplayDialog(
            "Robots Ready",
            "Generated " + robotPrefabs.Count + " robot enemies:\n\n" +
            list +
            "\nMainGame scene updated. Models can now spawn as enemies.",
            "OK"
        );
    }

    static void EnsurePrefabFolder()
    {
        if (!AssetDatabase.IsValidFolder(ROBOT_PREFAB_DIR))
        {
            AssetDatabase.CreateFolder(
                "Assets/Prefabs",
                "Robots"
            );
        }
    }

    static void CleanOldRobotPrefabs()
    {
        string[] prefabGuids =
            AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { ROBOT_PREFAB_DIR }
            );

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(prefabGuids[i]);

            AssetDatabase.DeleteAsset(path);
        }
    }

    static string[] FindRobotModels()
    {
        string[] modelGuids =
            AssetDatabase.FindAssets(
                "t:Model",
                new[] { "Assets" }
            );

        List<string> results = new List<string>();

        for (int i = 0; i < modelGuids.Length; i++)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(modelGuids[i]);

            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string lower = path.ToLowerInvariant();

            if (lower.Contains("zombie") ||
                lower.Contains("/low poly shotgun") ||
                lower.Contains("materials/source"))
            {
                continue;
            }

            string fileName =
                Path.GetFileNameWithoutExtension(path)
                    .ToLowerInvariant();

            if (!ContainsRobotKeyword(fileName))
            {
                continue;
            }

            results.Add(path);
        }

        return results.ToArray();
    }

    static bool ContainsRobotKeyword(string fileName)
    {
        for (int i = 0; i < RobotKeywords.Length; i++)
        {
            if (fileName.Contains(RobotKeywords[i]))
            {
                return true;
            }
        }

        return false;
    }

    static GameObject BuildRobotPrefab(string fbxPath)
    {
        GameObject fbx =
            AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

        if (fbx == null)
        {
            Debug.LogWarning("Could not load model: " + fbxPath);
            return null;
        }

        string modelName =
            Path.GetFileNameWithoutExtension(fbxPath);

        GameObject rig =
            new GameObject("Robot_" + modelName);

        rig.transform.position = Vector3.zero;

        GameObject model =
            (GameObject)PrefabUtility.InstantiatePrefab(fbx);

        model.transform.SetParent(rig.transform, false);

        Renderer[] renderers =
            model.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            Debug.LogWarning(
                "No renderers found in: " + fbxPath
            );

            Object.DestroyImmediate(rig);
            return null;
        }

        Bounds bounds = GetBounds(model);

        float currentSize =
            Mathf.Max(
                bounds.size.x,
                Mathf.Max(bounds.size.y, bounds.size.z)
            );

        if (currentSize > 0.0001f)
        {
            model.transform.localScale *=
                TargetSize / currentSize;
        }

        bounds = GetBounds(model);

        model.transform.localPosition -=
            new Vector3(0f, bounds.min.y, 0f);

        bounds = GetBounds(model);

        AddCollider(rig, bounds);

        rig.AddComponent<ZombieHealth>();
        rig.AddComponent<ZombieAI>();
        rig.AddComponent<ZombieHitEffect>();

        EnsureMaterials(model);

        string prefabPath =
            ROBOT_PREFAB_DIR + "/Robot_" + modelName + ".prefab";

        GameObject prefab =
            PrefabUtility.SaveAsPrefabAsset(rig, prefabPath);

        Object.DestroyImmediate(rig);

        return prefab;
    }

    static Bounds GetBounds(GameObject model)
    {
        Renderer[] renderers =
            model.GetComponentsInChildren<Renderer>(true);

        Bounds bounds =
            new Bounds(model.transform.position, Vector3.zero);

        bool first = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled)
            {
                continue;
            }

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

    static void AddCollider(GameObject rig, Bounds bounds)
    {
        BoxCollider box =
            rig.AddComponent<BoxCollider>();

        box.isTrigger = false;

        box.size = new Vector3(
            Mathf.Max(bounds.size.x, 0.3f),
            Mathf.Max(bounds.size.y, 0.3f),
            Mathf.Max(bounds.size.z, 0.3f)
        );

        box.center = new Vector3(
            0f,
            box.size.y * 0.5f,
            0f
        );
    }

    static void EnsureMaterials(GameObject model)
    {
        Renderer[] renderers =
            model.GetComponentsInChildren<Renderer>(true);

        bool hasMissing = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].sharedMaterials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j] == null)
                {
                    hasMissing = true;
                    break;
                }
            }

            if (hasMissing)
            {
                break;
            }
        }

        if (!hasMissing)
        {
            return;
        }

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return;
        }

        Material fallback = new Material(shader);
        fallback.color = new Color(0.4f, 0.45f, 0.5f);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].sharedMaterials;
            bool changed = false;

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j] == null)
                {
                    mats[j] = fallback;
                    changed = true;
                }
            }

            if (changed)
            {
                renderers[i].sharedMaterials = mats;
            }
        }
    }

    static void UpdateSpawner(List<GameObject> robotPrefabs)
    {
        if (!File.Exists(SCENE_PATH))
        {
            Debug.LogWarning("Scene not found: " + SCENE_PATH);
            return;
        }

        EditorSceneManager.OpenScene(
            SCENE_PATH,
            OpenSceneMode.Single
        );

        ZombieSpawner spawner =
            Object.FindObjectOfType<ZombieSpawner>();

        if (spawner == null)
        {
            Debug.LogWarning(
                "No ZombieSpawner found in " + SCENE_PATH
            );

            return;
        }

        if (spawner.zombiePrefab == null &&
            robotPrefabs.Count > 0)
        {
            spawner.zombiePrefab = robotPrefabs[0];
        }

        spawner.robotPrefabs = robotPrefabs.ToArray();

        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene()
        );

        Debug.Log(
            "ZombieSpawner.robotPrefabs updated with " +
            robotPrefabs.Count + " robots."
        );
    }
}