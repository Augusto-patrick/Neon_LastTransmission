using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FactoryMapBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame.unity";
    private const string FactoryScenePath = "Assets/TirgamesAssets/Factory/Scenes/FactoryDay.unity";
    private const string BackupScenePath = "Assets/Scenes/MainGame_PreFactory.unity";

    private static readonly string[] FactoryRootsToSkip =
    {
        "PostProcess", "Camera"
    };

    private static readonly string[] GameplayRootsToSkip =
    {
        "CityGround", "Directional Light"
    };

    [MenuItem("Tools/Robot Apocalypse/Rebuild Scene with Abandoned Factory")]
    public static void RebuildScene()
    {
        try
        {
            if (!File.Exists(FactoryScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Abandoned Factory",
                    "Factory scene not found at:\n" + FactoryScenePath,
                    "OK"
                );
                return;
            }

            if (File.Exists(BackupScenePath))
            {
                File.Delete(BackupScenePath);
            }

            if (File.Exists(BackupScenePath + ".meta"))
            {
                File.Delete(BackupScenePath + ".meta");
            }

            if (File.Exists(GameScenePath))
            {
                File.Copy(GameScenePath, BackupScenePath);
            }

            Scene target =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            CopyFactoryEnvironment(target);
            CopyGameplaySystems(target);

            PolishScene(target);

            EditorSceneManager.SaveScene(target, GameScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(GameScenePath, true)
            };

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Abandoned Factory",
                "MainGame.unity rebuilt with the Abandoned Factory map.\n\n" +
                "Backup saved to:\n" + BackupScenePath,
                "OK"
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog(
                "Abandoned Factory - Error",
                ex.Message,
                "OK"
            );
        }
    }

    static void CopyFactoryEnvironment(Scene target)
    {
        Scene factory =
            EditorSceneManager.OpenScene(
                FactoryScenePath,
                OpenSceneMode.Additive
            );

        GameObject[] roots = factory.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];

            if (ShouldSkipFactoryRoot(root.name))
                continue;

            GameObject clone = Object.Instantiate(root);

            SceneManager.MoveGameObjectToScene(clone, target);

            clone.name = StripCloneSuffix(clone.name);
        }

        EditorSceneManager.CloseScene(factory, true);
        EditorSceneManager.SetActiveScene(target);
    }

    static bool ShouldSkipFactoryRoot(string name)
    {
        for (int i = 0; i < FactoryRootsToSkip.Length; i++)
        {
            if (name == FactoryRootsToSkip[i])
                return true;
        }

        return false;
    }

    static void CopyGameplaySystems(Scene target)
    {
        Scene source =
            EditorSceneManager.OpenScene(
                GameScenePath,
                OpenSceneMode.Additive
            );

        GameObject pool = new GameObject("_GameplayPool");
        SceneManager.MoveGameObjectToScene(pool, source);

        GameObject[] roots = source.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];

            if (ShouldSkipGameplayRoot(root.name))
                continue;

            root.transform.SetParent(pool.transform, false);
        }

        GameObject poolClone = Object.Instantiate(pool);

        SceneManager.MoveGameObjectToScene(poolClone, target);

        for (int i = poolClone.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = poolClone.transform.GetChild(i);

            child.SetParent(null);
            child.name = StripCloneSuffix(child.name);
        }

        Object.DestroyImmediate(poolClone);

        EditorSceneManager.CloseScene(source, true);
        EditorSceneManager.SetActiveScene(target);
    }

    static bool ShouldSkipGameplayRoot(string name)
    {
        if (name.StartsWith("Neon"))
            return true;

        for (int i = 0; i < GameplayRootsToSkip.Length; i++)
        {
            if (name == GameplayRootsToSkip[i])
                return true;
        }

        return false;
    }

    static string StripCloneSuffix(string name)
    {
        int index = name.IndexOf("(Clone)", System.StringComparison.Ordinal);

        if (index >= 0)
        {
            return name.Substring(0, index).TrimEnd();
        }

        return name;
    }

    static void PolishScene(Scene target)
    {
        GameObject[] roots = target.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            foreach (Transform t in roots[i].GetComponentsInChildren<Transform>(true))
            {
                EnsureColliders(t);
            }
        }

        ZombieSpawner spawner =
            Object.FindObjectOfType<ZombieSpawner>();

        if (spawner == null)
        {
            Debug.LogWarning("No ZombieSpawner found in rebuilt scene.");
            return;
        }

        if (spawner.player == null)
        {
            PlayerController controller =
                Object.FindObjectOfType<PlayerController>();

            if (controller != null)
            {
                spawner.player = controller.transform;
            }
        }

        PlacePlayerOnFloor();
    }

    static void PlacePlayerOnFloor()
    {
        PlayerController controller =
            Object.FindObjectOfType<PlayerController>();

        if (controller == null)
            return;

        Physics.SyncTransforms();

        RaycastHit hit;

        Vector3 origin =
            new Vector3(
                controller.transform.position.x,
                controller.transform.position.y + 20f,
                controller.transform.position.z
            );

        if (Physics.Raycast(origin, Vector3.down, out hit, 60f))
        {
            controller.transform.position = hit.point;
        }
        else
        {
            Physics.SyncTransforms();

            if (Physics.Raycast(new Vector3(0f, 20f, 0f), Vector3.down, out hit, 60f))
            {
                controller.transform.position = hit.point;
            }
            else
            {
                controller.transform.position = new Vector3(0f, 2f, 0f);
            }
        }
    }

    static void EnsureColliders(Transform t)
    {
        MeshRenderer renderer = t.GetComponent<MeshRenderer>();

        if (renderer == null || !renderer.enabled)
            return;

        if (renderer.GetComponentInParent<Collider>() != null)
            return;

        MeshFilter filter = t.GetComponent<MeshFilter>();

        if (filter == null || filter.sharedMesh == null)
            return;

        MeshCollider collider = t.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.sharedMesh;
        collider.convex = false;
    }
}