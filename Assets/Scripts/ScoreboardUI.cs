using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    public static ScoreboardUI Instance { get; private set; }

    public static bool MenuOpen
    {
        get { return Instance != null && Instance.IsMenuOpen; }
    }

    public ZombieSpawner Spawner { get; private set; }
    public bool IsMenuOpen { get; private set; }

    public bool GameStarted { get; private set; }

    private Canvas canvas;
    private GameObject menuPanel;
    private GameObject gameOverPanel;
    private GameObject rankingsPanel;

    private InputField menuNameInput;
    private Text gameOverTitle;
    private InputField gameOverNameInput;
    private Text scoreText;
    private Text rankText;
    private Text contestantNameText;
    private Text listText;

    private RankingEntry currentEntry;
    private bool entrySaved;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Initialize(ZombieSpawner spawner)
    {
        Spawner = spawner;
        CreateCanvas();
        BuildMenuPanel();
        BuildGameOverPanel();
        BuildRankingsPanel();
        ShowMenu();
    }

    void CreateCanvas()
    {
        GameObject canvasGo = new GameObject(
            "ScoreboardCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler =
            canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    void BuildMenuPanel()
    {
        menuPanel = UIBuilder.CreatePanel(
            "MenuPanel",
            canvas.transform,
            new Color(0.04f, 0.04f, 0.05f, 0.92f),
            1920f,
            1080f
        ).gameObject;

        Text title = UIBuilder.CreateText(
            "Title",
            menuPanel.transform,
            "NEON LAST TRANSMISSION",
            64
        );
        title.rectTransform.anchoredPosition = new Vector2(0f, 260f);

        Text subtitle = UIBuilder.CreateText(
            "Subtitle",
            menuPanel.transform,
            "SURVIVE THE MACHINE UPRISING",
            22
        );
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, 205f);
        subtitle.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        Text nameLabel = UIBuilder.CreateText(
            "NameLabel",
            menuPanel.transform,
            "Contestant name:",
            18
        );
        nameLabel.alignment = TextAnchor.MiddleLeft;
        nameLabel.rectTransform.anchoredPosition = new Vector2(-230f, 130f);

        menuNameInput = UIBuilder.CreateInputField(
            "NameInput",
            menuPanel.transform,
            "Enter your name",
            420f,
            46f
        );
        ((RectTransform)menuNameInput.transform).anchoredPosition =
            new Vector2(10f, 130f);

        string saved = PlayerPrefs.GetString(
            "ContestantName",
            ""
        );

        if (string.IsNullOrEmpty(saved))
        {
            saved = "Contestant";
        }

        menuNameInput.text = saved;

        Button startBtn = UIBuilder.CreateButton(
            "StartBtn",
            menuPanel.transform,
            "START SURVIVAL",
            StartGame,
            380f,
            64f
        );
        startBtn.transform.localPosition = new Vector3(0f, 0f, 0f);

        Button ranksBtn = UIBuilder.CreateButton(
            "RanksBtn",
            menuPanel.transform,
            "VIEW RANKINGS",
            ShowRankings,
            380f,
            52f
        );
        ranksBtn.transform.localPosition = new Vector3(0f, -110f, 0f);

        Text controls = UIBuilder.CreateText(
            "Controls",
            menuPanel.transform,
            "WASD - Move    Mouse - Look    Left Mouse - Shoot    R - Reload",
            18
        );
        controls.rectTransform.anchoredPosition = new Vector2(0f, -300f);
    }

    void BuildGameOverPanel()
    {
        gameOverPanel = UIBuilder.CreatePanel(
            "GameOverPanel",
            canvas.transform,
            new Color(0.08f, 0.04f, 0.04f, 0.95f),
            1920f,
            1080f
        ).gameObject;

        gameOverTitle = UIBuilder.CreateText(
            "Title",
            gameOverPanel.transform,
            "YOU DIED",
            72
        );
        gameOverTitle.color = new Color(0.9f, 0.15f, 0.15f, 1f);
        gameOverTitle.rectTransform.anchoredPosition =
            new Vector2(0f, 240f);

        contestantNameText = UIBuilder.CreateText(
            "ContName",
            gameOverPanel.transform,
            "",
            24
        );
        contestantNameText.rectTransform.anchoredPosition =
            new Vector2(0f, 170f);

        Text nameLbl = UIBuilder.CreateText(
            "NameLbl",
            gameOverPanel.transform,
            "Save as:",
            16
        );
        nameLbl.alignment = TextAnchor.MiddleLeft;
        nameLbl.rectTransform.anchoredPosition =
            new Vector2(-210f, 115f);

        gameOverNameInput = UIBuilder.CreateInputField(
            "NameInput",
            gameOverPanel.transform,
            "Contestant name",
            440f,
            44f
        );
        ((RectTransform)gameOverNameInput.transform).anchoredPosition =
            new Vector2(10f, 115f);

        scoreText = UIBuilder.CreateText(
            "Score",
            gameOverPanel.transform,
            "",
            22
        );
        scoreText.alignment = TextAnchor.MiddleLeft;
        scoreText.rectTransform.anchoredPosition =
            new Vector2(-210f, 40f);

        rankText = UIBuilder.CreateText(
            "Rank",
            gameOverPanel.transform,
            "",
            26
        );
        rankText.alignment = TextAnchor.MiddleLeft;
        rankText.rectTransform.anchoredPosition =
            new Vector2(-210f, -10f);
        rankText.color = new Color(1f, 0.9f, 0.5f, 1f);

        Button saveBtn = UIBuilder.CreateButton(
            "SaveBtn",
            gameOverPanel.transform,
            "SAVE RANKING",
            SaveEntry,
            300f,
            50f
        );
        saveBtn.transform.localPosition = new Vector3(-260f, -120f, 0f);

        Button ranksBtn = UIBuilder.CreateButton(
            "RanksBtn",
            gameOverPanel.transform,
            "VIEW RANKINGS",
            delegate
            {
                SaveEntry();
                ShowRankings();
            },
            300f,
            50f
        );
        ranksBtn.transform.localPosition = new Vector3(130f, -120f, 0f);

        Button retryBtn = UIBuilder.CreateButton(
            "RetryBtn",
            gameOverPanel.transform,
            "RETRY",
            RestartGame,
            200f,
            50f
        );
        retryBtn.transform.localPosition = new Vector3(-130f, -210f, 0f);

        Button menuBtn = UIBuilder.CreateButton(
            "MenuBtn",
            gameOverPanel.transform,
            "MAIN MENU",
            ReturnToMenu,
            200f,
            50f
        );
        menuBtn.transform.localPosition = new Vector3(130f, -210f, 0f);

        gameOverPanel.SetActive(false);
    }

    void BuildRankingsPanel()
    {
        rankingsPanel = UIBuilder.CreatePanel(
            "RankingsPanel",
            canvas.transform,
            new Color(0.04f, 0.04f, 0.05f, 0.95f),
            1920f,
            1080f
        ).gameObject;

        Text title = UIBuilder.CreateText(
            "Title",
            rankingsPanel.transform,
            "RANKINGS",
            56
        );
        title.rectTransform.anchoredPosition = new Vector2(0f, 260f);

        listText = UIBuilder.CreateText(
            "List",
            rankingsPanel.transform,
            "",
            20
        );
        listText.alignment = TextAnchor.UpperCenter;

        RectTransform listRt = listText.rectTransform;
        listRt.anchorMin = new Vector2(0.5f, 0.5f);
        listRt.anchorMax = new Vector2(0.5f, 0.5f);
        listRt.sizeDelta = new Vector2(900f, 560f);
        listRt.anchoredPosition = new Vector2(0f, -40f);

        Button backBtn = UIBuilder.CreateButton(
            "BackBtn",
            rankingsPanel.transform,
            "BACK",
            ReturnFromRankings,
            240f,
            52f
        );
        backBtn.transform.localPosition = new Vector3(0f, -340f, 0f);

        rankingsPanel.SetActive(false);
    }

    public void ShowMenu()
    {
        IsMenuOpen = true;
        GameStarted = false;
        entrySaved = false;

        menuPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        rankingsPanel.SetActive(false);

        string saved = PlayerPrefs.GetString(
            "ContestantName",
            "Contestant"
        );

        if (menuNameInput != null)
        {
            menuNameInput.text = saved;
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        SaveMenuName();

        IsMenuOpen = false;
        GameStarted = true;

        menuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        rankingsPanel.SetActive(false);

        if (Spawner != null)
        {
            Spawner.BeginGame();
        }
    }

    void SaveMenuName()
    {
        string name = menuNameInput != null
            ? menuNameInput.text.Trim()
            : "";

        if (string.IsNullOrEmpty(name))
        {
            name = "Contestant";
        }

        PlayerPrefs.SetString("ContestantName", name);
        PlayerPrefs.Save();
    }

    public void OnPlayerDied(ZombieSpawner spawner)
    {
        Spawner = spawner;
        BuildEntry("YOU DIED");
        ShowGameOver();
    }

    public void OnSurvived(ZombieSpawner spawner)
    {
        Spawner = spawner;
        BuildEntry("SURVIVED");
        ShowGameOver();
    }

    void BuildEntry(string title)
    {
        string name = PlayerPrefs.GetString(
            "ContestantName",
            "Contestant"
        );

        currentEntry = new RankingEntry();
        currentEntry.name = name;
        currentEntry.score = Spawner.ComputeScore();
        currentEntry.kills = Spawner.Kills;
        currentEntry.wave = Spawner.CurrentWave;
        currentEntry.survivalTime = Spawner.SurvivalTime;
        currentEntry.finishedAt =
            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        int rank = RankSaver.Insert(currentEntry);
        currentEntry.rank = rank;

        entrySaved = true;

        if (gameOverTitle != null)
        {
            gameOverTitle.text = title;

            if (title == "SURVIVED")
            {
                gameOverTitle.color =
                    new Color(0.3f, 0.9f, 0.5f, 1f);
            }
            else
            {
                gameOverTitle.color =
                    new Color(0.9f, 0.15f, 0.15f, 1f);
            }
        }
    }

    void ShowGameOver()
    {
        IsMenuOpen = true;
        GameStarted = false;

        menuPanel.SetActive(false);
        rankingsPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (contestantNameText != null)
        {
            contestantNameText.text =
                "Contestant: " + PlayerPrefs.GetString(
                    "ContestantName",
                    "Contestant"
                );
        }

        if (gameOverNameInput != null)
        {
            gameOverNameInput.text = PlayerPrefs.GetString(
                "ContestantName",
                "Contestant"
            );
        }

        if (scoreText != null && currentEntry != null)
        {
            scoreText.text =
                "Score: " + currentEntry.score +
                "  |  Kills: " + currentEntry.kills +
                "  |  Wave: " + currentEntry.wave;
        }

        if (rankText != null && currentEntry != null)
        {
            rankText.text = "RANK: #" + currentEntry.rank;
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SaveEntry()
    {
        if (currentEntry == null)
            return;

        string name = gameOverNameInput != null
            ? gameOverNameInput.text.Trim()
            : "";

        if (string.IsNullOrEmpty(name))
        {
            name = "Contestant";
        }

        PlayerPrefs.SetString("ContestantName", name);
        PlayerPrefs.Save();

        currentEntry.name = name;
        entrySaved = true;

        RankSaver.Save();

        if (rankText != null)
        {
            rankText.text = "RANK: #" + currentEntry.rank;
        }

        if (contestantNameText != null)
        {
            contestantNameText.text = "Contestant: " + name;
        }
    }

    void ShowRankings()
    {
        rankingsPanel.SetActive(true);
        RefreshRankings();
    }

    void ReturnFromRankings()
    {
        rankingsPanel.SetActive(false);

        if (gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            menuPanel.SetActive(true);
        }
    }

    void RefreshRankings()
    {
        if (listText == null)
            return;

        var entries = RankSaver.GetTop(10);

        StringBuilder sb = new StringBuilder();

        if (entries.Count == 0)
        {
            sb.AppendLine("No finished contestants yet.");
        }
        else
        {
            sb.AppendLine("#\tNAME\t\t\t\tSCORE\tKILLS\tWAVE\tTIME");
            sb.AppendLine();

            for (int i = 0; i < entries.Count; i++)
            {
                RankingEntry e = entries[i];

                int m = Mathf.FloorToInt(e.survivalTime / 60f);
                int s = Mathf.FloorToInt(e.survivalTime % 60f);

                string name = e.name.Length > 14
                    ? e.name.Substring(0, 14)
                    : e.name;

                sb.AppendLine(string.Format(
                    "{0}\t{1}\t{2}\t{3}\t{4}\t{5:00}:{6:00}",
                    e.rank,
                    name,
                    e.score,
                    e.kills,
                    e.wave,
                    m,
                    s
                ));
            }
        }

        listText.text = sb.ToString();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}