using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ScoreSyncManager : MonoBehaviour
{
    private static ScoreSyncManager _instance;
    public static ScoreSyncManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ScoreSyncManager");
                _instance = go.AddComponent<ScoreSyncManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private string PendingQueuePath =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, "pending_scores.json");

    void Start()
    {
        // Retries anything that failed to sync earlier, e.g. during a WiFi drop.
        InvokeRepeating(nameof(FlushPendingQueue), 5f, 30f);
    }

    public void ReportScore(string mobile, string gameName, int score)
    {
        StartCoroutine(FetchAndUpdate(mobile, gameName, score));
    }

    private IEnumerator FetchAndUpdate(string mobile, string gameName, int score)
    {
        bool done = false, ok = false;
        string userJson = null;

        yield return FirebaseRestClient.Get("users/" + mobile, (success, resp) =>
        {
            ok = success; userJson = resp; done = true;
        });
        while (!done) yield return null;

        if (!ok || userJson == "null")
        {
            QueuePendingScore(mobile, gameName, score);
            yield break;
        }

        UserProfile profile = JsonConvert.DeserializeObject<UserProfile>(userJson);
        if (profile.scores == null) profile.scores = new Dictionary<string, int>();

        int existing = profile.scores.ContainsKey(gameName) ? profile.scores[gameName] : 0;
        profile.scores[gameName] = Mathf.Max(existing, score);

        int cumulative = 0;
        foreach (var kv in profile.scores) cumulative += kv.Value;

        string patchJson = JsonConvert.SerializeObject(new
        {
            scores = profile.scores,
            cumulativeScore = cumulative
        });

        bool patchOk = false;
        yield return FirebaseRestClient.Patch("users/" + mobile, patchJson, (success, resp) => patchOk = success);

        if (!patchOk)
            QueuePendingScore(mobile, gameName, score);
    }

    private void QueuePendingScore(string mobile, string gameName, int score)
    {
        List<PendingScore> queue = LoadQueue();
        queue.Add(new PendingScore { mobile = mobile, gameName = gameName, score = score });
        File.WriteAllText(PendingQueuePath, JsonConvert.SerializeObject(queue));
        Debug.LogWarning("No network right now — queued score for later sync: " + gameName + " = " + score);
    }

    private List<PendingScore> LoadQueue()
    {
        if (!File.Exists(PendingQueuePath)) return new List<PendingScore>();
        try
        {
            return JsonConvert.DeserializeObject<List<PendingScore>>(File.ReadAllText(PendingQueuePath))
                   ?? new List<PendingScore>();
        }
        catch
        {
            return new List<PendingScore>();
        }
    }

    private void FlushPendingQueue()
    {
        List<PendingScore> queue = LoadQueue();
        if (queue.Count == 0) return;

        // Clear first, then re-report — if a report fails again it'll re-queue itself.
        File.WriteAllText(PendingQueuePath, JsonConvert.SerializeObject(new List<PendingScore>()));
        foreach (var p in queue)
            ReportScore(p.mobile, p.gameName, p.score);
    }

    [System.Serializable]
    private class PendingScore
    {
        public string mobile;
        public string gameName;
        public int score;
    }
}
