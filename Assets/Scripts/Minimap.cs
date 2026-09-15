using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [Header("Camera")]
    public float height = 40f;
    public float orthoSize = 20f;
    public int textureSize = 256;
    public float farClipPlane = 120f;

    [Header("UI")]
    public float mapSize = 190f;
    public float mapMargin = 14f;
    public Color mapTint = new Color(1f, 1f, 1f, 1f);
    public Color borderColor = new Color(0.1f, 1f, 1f, 0.6f);
    public Color playerColor = new Color(0.2f, 1f, 1f, 1f);
    public Color zombieColor = new Color(1f, 0.2f, 0.25f, 1f);
    public float playerRadius = 7f;
    public float zombieRadius = 6f;

    private Camera mapCamera;
    private RenderTexture renderTexture;
    private RawImage mapImage;
    private Sprite dotSprite;
    private RectTransform playerMarker;
    private readonly List<RectTransform> zombieMarkers = new List<RectTransform>();
    private GameObject hudRoot;
    private GameObject cameraObject;

    void Start()
    {
        BuildCamera();
        BuildUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMinimap();
        }
    }

    void ToggleMinimap()
    {
        bool visible =
            hudRoot != null && hudRoot.activeSelf;

        if (hudRoot != null)
        {
            hudRoot.SetActive(!visible);
        }

        if (cameraObject != null)
        {
            cameraObject.SetActive(!visible);
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            if (mapCamera != null)
                mapCamera.targetTexture = null;

            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (mapCamera != null)
            Destroy(mapCamera.gameObject);
    }

    void LateUpdate()
    {
        if (mapCamera == null || dotSprite == null)
            return;

        Vector3 playerPosition = transform.position;

        mapCamera.transform.position = new Vector3(
            playerPosition.x,
            height,
            playerPosition.z
        );
        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        PositionMarker(playerMarker, playerPosition, playerColor);

        int zombieCount = ZombieHealth.All.Count;

        while (zombieMarkers.Count < zombieCount)
        {
            zombieMarkers.Add(
                CreateMarker(
                    mapImage.transform,
                    dotSprite,
                    zombieColor,
                    zombieRadius
                )
            );
        }

        for (int i = 0; i < zombieCount; i++)
        {
            ZombieHealth zombie = ZombieHealth.All[i];

            if (zombie == null)
            {
                zombieMarkers[i].gameObject.SetActive(false);
                continue;
            }

            zombieMarkers[i].gameObject.SetActive(true);
            PositionMarker(
                zombieMarkers[i],
                zombie.transform.position,
                zombieColor
            );
        }

        for (int i = zombieCount; i < zombieMarkers.Count; i++)
        {
            zombieMarkers[i].gameObject.SetActive(false);
        }
    }

    void BuildCamera()
    {
        GameObject camObject = new GameObject("MinimapCamera");
        cameraObject = camObject;

        mapCamera = camObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = orthoSize;
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(0.02f, 0.03f, 0.06f, 1f);
        mapCamera.nearClipPlane = 0.3f;
        mapCamera.farClipPlane = farClipPlane;
        mapCamera.depth = -5;

        renderTexture = new RenderTexture(
            textureSize,
            textureSize,
            16,
            RenderTextureFormat.ARGB32
        );
        mapCamera.targetTexture = renderTexture;

        UniversalAdditionalCameraData additionalData =
            mapCamera.GetUniversalAdditionalCameraData();
        additionalData.renderShadows = false;
        additionalData.renderPostProcessing = false;
        additionalData.volumeLayerMask = 0;
    }

    void BuildUI()
    {
        GameObject canvasObject = new GameObject(
            "MinimapHUD",
            typeof(Canvas),
            typeof(CanvasScaler)
        );
        canvasObject.transform.SetParent(null);
        hudRoot = canvasObject;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject borderObject = new GameObject("MinimapBorder");
        borderObject.transform.SetParent(canvasObject.transform, false);
        Image border = borderObject.AddComponent<Image>();
        border.color = borderColor;
        RectTransform borderRect = border.rectTransform;
        borderRect.anchorMin = new Vector2(0f, 0f);
        borderRect.anchorMax = new Vector2(0f, 0f);
        borderRect.pivot = new Vector2(0f, 0f);
        borderRect.anchoredPosition = new Vector2(
            mapMargin - 3f,
            mapMargin - 3f
        );
        borderRect.sizeDelta = new Vector2(mapSize + 6f, mapSize + 6f);

        GameObject mapObject = new GameObject("Minimap");
        mapObject.transform.SetParent(canvasObject.transform, false);
        mapImage = mapObject.AddComponent<RawImage>();
        mapImage.texture = renderTexture;
        mapImage.color = mapTint;
        RectTransform mapRect = mapImage.rectTransform;
        mapRect.anchorMin = new Vector2(0f, 0f);
        mapRect.anchorMax = new Vector2(0f, 0f);
        mapRect.pivot = new Vector2(0f, 0f);
        mapRect.anchoredPosition = new Vector2(mapMargin, mapMargin);
        mapRect.sizeDelta = new Vector2(mapSize, mapSize);

        dotSprite = CreateDotSprite();

        playerMarker = CreateMarker(
            mapObject.transform,
            dotSprite,
            playerColor,
            playerRadius
        );
    }

    Sprite CreateDotSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float radius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(radius - dist);
                alpha = Mathf.Clamp01(alpha * 2f);

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );
    }

    RectTransform CreateMarker(
        Transform parent,
        Sprite sprite,
        Color color,
        float radius
    )
    {
        GameObject markerObject = new GameObject("Marker");
        markerObject.transform.SetParent(parent, false);

        Image image = markerObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.sizeDelta = new Vector2(radius * 2f, radius * 2f);

        return rectTransform;
    }

    void PositionMarker(
        RectTransform marker,
        Vector3 worldPosition,
        Color color
    )
    {
        if (marker == null)
            return;

        Vector3 viewport = mapCamera.WorldToViewportPoint(worldPosition);

        if (viewport.z < 0f)
        {
            marker.gameObject.SetActive(false);
            return;
        }

        viewport.x = Mathf.Clamp01(viewport.x);
        viewport.y = Mathf.Clamp01(viewport.y);

        marker.gameObject.SetActive(true);
        marker.GetComponent<Image>().color = color;

        float x = (viewport.x - 0.5f) * mapSize;
        float y = (viewport.y - 0.5f) * mapSize;

        marker.anchoredPosition = new Vector2(x, y);
    }
}