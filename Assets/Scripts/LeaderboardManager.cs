using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public Transform perGameContainer;
    public Transform cumulativeContainer;
    public GameObject rowPrefab;   // simple prefab: an object with one Text component
    public Text statusText;

    [Header("Fill this from your LauncherManager's game name list")]
    public List<string> gameNames;

    public void OpenLeaderboard()
    {
        statusText.text = "Loading...";
        FirebaseRunner.Instance.StartCoroutine(FirebaseRestClient.Get("users", OnUsersLoaded));
    }

    private void OnUsersLoaded(bool success, string json)
    {
        foreach (Transform t in perGameContainer) Destroy(t.gameObject);
        foreach (Transform t in cumulativeContainer) Destroy(t.gameObject);

        if (!success)
        {
            statusText.text = "Couldn't load leaderboard — check WiFi.";
            return;
        }
        if (json == "null")
        {
            statusText.text = "No scores yet.";
            return;
        }

        statusText.text = "";
        Dictionary<string, UserProfile> users =
            JsonConvert.DeserializeObject<Dictionary<string, UserProfile>>(json);

        var topOverall = users.Values.OrderByDescending(u => u.cumulativeScore).Take(10);
        int rank = 1;
        foreach (var u in topOverall)
        {
            SpawnRow(cumulativeContainer, rank + ". " + u.name + " — " + u.cumulativeScore);
            rank++;
        }

        foreach (string game in gameNames)
        {
            SpawnRow(perGameContainer, "— " + game + " —");
            var topForGame = users.Values
                .Where(u => u.scores != null && u.scores.ContainsKey(game))
                .OrderByDescending(u => u.scores[game])
                .Take(5);
            int gRank = 1;
            foreach (var u in topForGame)
            {
                SpawnRow(perGameContainer, gRank + ". " + u.name + " — " + u.scores[game]);
                gRank++;
            }
        }
    }

    private void SpawnRow(Transform parent, string text)
    {
        GameObject row = Instantiate(rowPrefab, parent);
        Text t = row.GetComponentInChildren<Text>();
        if (t != null) t.text = text;
    }
}
