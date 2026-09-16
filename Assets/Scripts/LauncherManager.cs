using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[System.Serializable]
public class GameEntry
{
    public string gameName;

    // Path to the .exe RELATIVE to the "Games" folder that sits next to
    // the Launcher's own .exe. Example:
    //   GameRoomHub/Games/BeatSaber/BeatSaber.exe  ->  "BeatSaber/BeatSaber.exe"
    public string exeRelativePath;

    public Sprite icon;

    [TextArea]
    public string description;
}

public class LauncherManager : MonoBehaviour
{
    [Header("Game List")]
    public List<GameEntry> games = new List<GameEntry>();

    [Header("UI References")]
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    [Header("Error Popup")]
    public GameObject errorPanel;
    public Text errorText;

    public void PopulateLibrary()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (GameEntry game in games)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            GameButtonUI ui = btnObj.GetComponent<GameButtonUI>();
            if (ui != null)
                ui.Setup(game, this);
            else
                Debug.LogError("buttonPrefab is missing a GameButtonUI component.");
        }
    }

    public void LaunchGame(GameEntry game)
    {
        string launcherRoot = Directory.GetParent(Application.dataPath).FullName;
        string fullPath = Path.Combine(launcherRoot, "Games", game.exeRelativePath);

        if (!File.Exists(fullPath))
        {
            ShowError("Could not find:\n" + fullPath +
                      "\n\nCheck the exeRelativePath for \"" + game.gameName + "\".");
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fullPath)
            {
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                UseShellExecute = true
            };
            Process process = Process.Start(startInfo);
            StartCoroutine(WaitForGameExitThenReportScore(process, game));
        }
        catch (System.Exception e)
        {
            ShowError("Failed to launch " + game.gameName + ":\n" + e.Message);
        }
    }

    private IEnumerator WaitForGameExitThenReportScore(Process process, GameEntry game)
    {
        while (process != null && !process.HasExited)
            yield return new WaitForSeconds(1f);

        string gameFolder = Path.GetDirectoryName(game.exeRelativePath);
        string launcherRoot = Directory.GetParent(Application.dataPath).FullName;
        string scorePath = Path.Combine(launcherRoot, "Games", gameFolder, "score.txt");

        if (!File.Exists(scorePath))
        {
            Debug.Log("No score.txt found for " + game.gameName + " (game may not report scores, or player quit early).");
            yield break;
        }

        string raw = File.ReadAllText(scorePath).Trim();
        if (int.TryParse(raw, out int score) && AuthManager.CurrentUser != null)
        {
            ScoreSyncManager.Instance.ReportScore(AuthManager.CurrentUser.mobile, game.gameName, score);
        }
    }

    private void ShowError(string message)
    {
        Debug.LogError(message);
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
            if (errorText != null) errorText.text = message;
        }
    }

    public void HideError()
    {
        if (errorPanel != null) errorPanel.SetActive(false);
    }
}
