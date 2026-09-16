using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public Gun gun;
    public PlayerHealth playerHealth;

    public TMP_Text ammoText;
    public TMP_Text healthText;

    [Header("Ammo Colors")]
    public float ammoLowPercent = 0.25f;
    public Color ammoNormalColor = new Color(0.85f, 0.95f, 1f);
    public Color ammoLowColor = new Color(1f, 0.75f, 0.15f);
    public Color ammoEmptyColor = new Color(1f, 0.15f, 0.1f);

    [Header("Health Colors")]
    public float healthLowPercent = 0.25f;
    public Color healthFullColor = new Color(0.35f, 1f, 0.5f);
    public Color healthMidColor = new Color(1f, 0.8f, 0.15f);
    public Color healthCriticalColor = new Color(1f, 0.2f, 0.1f);

    [Header("Reload Warning")]
    public TMP_Text reloadWarning;
    public Image reloadWarningBg;
    public Color reloadWarningColorStart = new Color(1f, 0.85f, 0.1f);
    public Color reloadWarningColorEnd = Color.red;

    private Color ammoDisplayColor = Color.white;
    private Color healthDisplayColor = Color.white;

    const float SmoothSpeed = 10f;

    void Start()
    {
        if (reloadWarning == null)
        {
            CreateReloadWarning();
        }
    }

    void Update()
    {
        if (gun != null && ammoText != null)
        {
            ammoText.text =
                gun.currentAmmo + " / " + gun.reserveAmmo;
            UpdateAmmoStyle();
        }

        if (playerHealth != null && healthText != null)
        {
            healthText.text =
                "HP: " + Mathf.CeilToInt(playerHealth.currentHealth);
            UpdateHealthStyle();
        }

        UpdateReloadWarning();
    }

    void UpdateAmmoStyle()
    {
        bool empty = gun.currentAmmo <= 0;
        bool low = !empty &&
            gun.currentAmmo <=
                gun.magazineSize * ammoLowPercent;

        Color target = empty
            ? ammoEmptyColor
            : low ? ammoLowColor : ammoNormalColor;

        ammoDisplayColor = Color.Lerp(
            ammoDisplayColor,
            target,
            Time.deltaTime * SmoothSpeed
        );

        ammoText.color = ammoDisplayColor;

        if (empty || low)
        {
            float speed = empty ? 8f : 5f;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
            float scale = empty
                ? 1f + 0.15f * pulse
                : 1f + 0.07f * pulse;

            ammoText.rectTransform.localScale =
                Vector3.one * scale;
        }
        else
        {
            ammoText.rectTransform.localScale = Vector3.one;
        }
    }

    void UpdateHealthStyle()
    {
        float ratio = playerHealth.maxHealth > 0
            ? playerHealth.currentHealth / playerHealth.maxHealth
            : 0f;

        bool critical = ratio <= healthLowPercent;
        bool low = !critical &&
            ratio <= healthLowPercent * 2f;

        Color target = critical
            ? healthCriticalColor
            : low ? healthMidColor : healthFullColor;

        healthDisplayColor = Color.Lerp(
            healthDisplayColor,
            target,
            Time.deltaTime * SmoothSpeed
        );

        healthText.color = healthDisplayColor;

        if (critical)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 8f);
            healthText.rectTransform.localScale =
                Vector3.one * (1f + 0.15f * pulse);
        }
        else
        {
            healthText.rectTransform.localScale = Vector3.one;
        }
    }

    void UpdateReloadWarning()
    {
        if (reloadWarning == null)
            return;

        bool show = gun != null && gun.currentAmmo <= 0;

        if (show)
        {
            float blink = 0.5f + 0.5f * Mathf.Sin(Time.time * 7f);

            Color c = Color.Lerp(
                reloadWarningColorStart,
                reloadWarningColorEnd,
                blink
            );

            c.a = 0.65f + 0.35f * blink;
            reloadWarning.color = c;

            float scale = 1f + 0.1f * blink;
            reloadWarning.rectTransform.localScale =
                Vector3.one * scale;

            if (reloadWarningBg != null)
            {
                Color bg = reloadWarningBg.color;
                bg.a = 0.5f + 0.3f * blink;
                reloadWarningBg.color = bg;
            }
        }
        else
        {
            reloadWarning.alpha = 0f;

            if (reloadWarningBg != null)
            {
                Color bg = reloadWarningBg.color;
                bg.a = 0f;
                reloadWarningBg.color = bg;
            }
        }
    }

    void CreateReloadWarning()
    {
        if (ammoText == null)
            return;

        Transform parent = ammoText.transform.parent;

        GameObject rootGo = new GameObject(
            "ReloadWarning",
            typeof(RectTransform),
            typeof(Image)
        );

        rootGo.transform.SetParent(parent, false);
        rootGo.transform.SetSiblingIndex(
            ammoText.transform.GetSiblingIndex()
        );

        RectTransform rootRt =
            rootGo.GetComponent<RectTransform>();

        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0f);
        rootRt.sizeDelta = new Vector2(340f, 55f);
        rootRt.anchoredPosition =
            ammoText.rectTransform.anchoredPosition +
            new Vector2(0f, -52f);

        reloadWarningBg = rootGo.GetComponent<Image>();
        reloadWarningBg.color =
            new Color(0.45f, 0.05f, 0.05f, 0f);
        reloadWarningBg.raycastTarget = false;

        GameObject textGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textGo.transform.SetParent(rootGo.transform, false);

        RectTransform textRt =
            textGo.GetComponent<RectTransform>();

        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        reloadWarning =
            textGo.GetComponent<TextMeshProUGUI>();

        reloadWarning.font = ammoText.font;
        reloadWarning.fontSharedMaterial =
            ammoText.fontSharedMaterial;
        reloadWarning.fontSize = 28;
        reloadWarning.fontStyle = FontStyles.Bold;
        reloadWarning.alignment = TextAlignmentOptions.Center;
        reloadWarning.text = "PRESS [R] TO RELOAD";
        reloadWarning.raycastTarget = false;
        reloadWarning.alpha = 0f;
    }
}