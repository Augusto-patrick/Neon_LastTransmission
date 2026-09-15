using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class UIBuilder
{
    public static Font GetFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    public static RectTransform CreatePanel(
        string name,
        Transform parent,
        Color color,
        float width,
        float height)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        Image img = go.GetComponent<Image>();
        img.color = color;

        return rt;
    }

    public static Text CreateText(
        string name,
        Transform parent,
        string content,
        int fontSize,
        HorizontalWrapMode wrap = HorizontalWrapMode.Overflow)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text)
        );

        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = GetFont();
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400f, 40f);

        return text;
    }

    public static Button CreateButton(
        string name,
        Transform parent,
        string label,
        UnityAction onClick,
        float width,
        float height,
        Color? bgColor = null)
    {
        Color bg = bgColor ??
            new Color(0.16f, 0.16f, 0.18f, 1f);

        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.color = bg;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        Text text = CreateText(
            "Label",
            go.transform,
            label,
            Mathf.RoundToInt(height * 0.35f)
        );

        RectTransform textRt = text.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.highlightedColor =
            new Color(0.3f, 0.3f, 0.34f, 1f);
        colors.pressedColor = new Color(0.1f, 0.12f, 0.6f, 1f);
        button.colors = colors;

        return button;
    }

    public static InputField CreateInputField(
        string name,
        Transform parent,
        string placeholder,
        float width,
        float height)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(InputField)
        );

        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.12f, 1f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        Text placeholderText = CreateText(
            "Placeholder",
            go.transform,
            placeholder,
            18
        );

        RectTransform phRt = placeholderText.rectTransform;
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(10f, 0f);
        phRt.offsetMax = new Vector2(-10f, 0f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject textGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(Text)
        );

        textGo.transform.SetParent(go.transform, false);

        RectTransform txtRt = textGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(10f, 0f);
        txtRt.offsetMax = new Vector2(-10f, 0f);

        Text text = textGo.GetComponent<Text>();
        text.font = GetFont();
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        InputField input = go.GetComponent<InputField>();
        input.targetGraphic = img;
        input.textComponent = text;
        input.placeholder = placeholderText;

        return input;
    }
}