using UnityEngine;
using UnityEngine.UI;

// Attach this to your button prefab (the one that gets duplicated per game).
public class GameButtonUI : MonoBehaviour
{
    [Header("Assign these from the prefab's children")]
    public Text nameText;       // swap for TMP_Text if you're using TextMeshPro
    public Image iconImage;     // optional, can leave empty
    public Button button;       // the Button component on this same object

    private GameEntry gameData;
    private LauncherManager manager;

    public void Setup(GameEntry game, LauncherManager mgr)
    {
        gameData = game;
        manager = mgr;

        if (nameText != null)
            nameText.text = game.gameName;

        if (iconImage != null && game.icon != null)
            iconImage.sprite = game.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.LaunchGame(gameData));
    }
}
