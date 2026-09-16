using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Talks to Firebase Realtime Database over plain REST — no Firebase SDK import needed.
// Works fine on flaky WiFi: each call just fails cleanly and the caller decides what to do.
public static class FirebaseRestClient
{
    // Set this to your project's Realtime Database URL, e.g.:
    // "https://gameroomhub-default-rtdb.asia-southeast1.firebasedatabase.app"
    public static string DatabaseUrl = "https://rhythia-7d94d-default-rtdb.asia-southeast1.firebasedatabase.app/";

    public static IEnumerator Get(string path, Action<bool, string> callback)
    {
        string url = DatabaseUrl + "/" + path + ".json";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 8;
            yield return req.SendWebRequest();
            bool ok = req.result == UnityWebRequest.Result.Success;
            callback(ok, ok ? req.downloadHandler.text : req.error);
        }
    }

    public static IEnumerator Put(string path, string json, Action<bool, string> callback)
    {
        yield return SendBody(path, "PUT", json, callback);
    }

    public static IEnumerator Patch(string path, string json, Action<bool, string> callback)
    {
        yield return SendBody(path, "PATCH", json, callback);
    }

    private static IEnumerator SendBody(string path, string method, string json, Action<bool, string> callback)
    {
        string url = DatabaseUrl + "/" + path + ".json";
        byte[] body = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest req = new UnityWebRequest(url, method))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 8;
            yield return req.SendWebRequest();
            bool ok = req.result == UnityWebRequest.Result.Success;
            callback(ok, ok ? req.downloadHandler.text : req.error);
        }
    }
}

// Runs the coroutines above from a persistent, always-available object,
// since the static methods themselves can't run coroutines.
public class FirebaseRunner : MonoBehaviour
{
    private static FirebaseRunner _instance;
    public static FirebaseRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("FirebaseRunner");
                _instance = go.AddComponent<FirebaseRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
