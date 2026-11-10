using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MedievalDungeonHudSetup
{
    private const string DungeonScenePath = "Assets/Scenes/Dungeon.unity";
    private const string FontPath = "Assets/Fonts/Cinzel-Bold SDF.asset";

    [MenuItem("Tools/UI/Style Death Screen and Dungeon Map")]
    private static void Apply()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != DungeonScenePath)
        {
            EditorUtility.DisplayDialog("Interfaz de mazmorra", "Abre la escena Dungeon antes de aplicar el estilo.", "OK");
            return;
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Canvas canvasComponent = Resources.FindObjectsOfTypeAll<Canvas>().FirstOrDefault(item => item.name == "Canvas" && item.gameObject.scene == scene);
        GameObject canvasObject = canvasComponent != null ? canvasComponent.gameObject : null;
        Transform deathTransform = canvasObject != null ? canvasObject.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == "Death") : null;
        GameObject deathObject = deathTransform != null ? deathTransform.gameObject : null;
        if (canvasObject == null || deathObject == null)
        {
            EditorUtility.DisplayDialog("Interfaz de mazmorra", "No se encontró el Canvas o el panel Death.", "OK");
            return;
        }

        StyleDeath(deathObject, font, uiSprite);
        StyleMap(canvasObject.transform, font, uiSprite);
        deathObject.transform.SetAsLastSibling();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Interfaz de mazmorra", "Pantalla de muerte y minimapa medieval aplicados. El marcador sigue la posición del jugador en el mapa.", "OK");
    }

    private static void StyleDeath(GameObject deathObject, TMP_FontAsset font, Sprite uiSprite)
    {
        Image overlay = deathObject.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.color = new Color(0.045f, 0.018f, 0.012f, 0.82f);
            overlay.raycastTarget = false;
        }

        RectTransform death = deathObject.GetComponent<RectTransform>();
        Transform oldFrame = death.Find("Marco Muerte Medieval");
        GameObject frameObject = oldFrame == null
            ? new GameObject("Marco Muerte Medieval", typeof(RectTransform), typeof(Image))
            : oldFrame.gameObject;
        if (oldFrame == null) frameObject.transform.SetParent(death, false);
        RectTransform frame = (RectTransform)frameObject.transform;
        frame.anchorMin = frame.anchorMax = new Vector2(0.5f, 0.5f);
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.anchoredPosition = Vector2.zero;
        frame.sizeDelta = new Vector2(500f, 340f);
        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.sprite = uiSprite;
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.66f, 0.42f, 0.17f, 1f);
        frameImage.raycastTarget = false;

        Transform innerTransform = frame.Find("Panel Interior Muerte");
        GameObject innerObject = innerTransform == null
            ? new GameObject("Panel Interior Muerte", typeof(RectTransform), typeof(Image))
            : innerTransform.gameObject;
        if (innerTransform == null) innerObject.transform.SetParent(frame, false);
        RectTransform inner = (RectTransform)innerObject.transform;
        inner.anchorMin = Vector2.zero;
        inner.anchorMax = Vector2.one;
        inner.offsetMin = new Vector2(6f, 6f);
        inner.offsetMax = new Vector2(-6f, -6f);
        Image innerImage = innerObject.GetComponent<Image>();
        innerImage.sprite = uiSprite;
        innerImage.type = Image.Type.Sliced;
        innerImage.color = new Color(0.105f, 0.055f, 0.027f, 1f);
        innerImage.raycastTarget = false;
        inner.SetAsFirstSibling();

        TMP_Text title = GetOrCreateText(frame, "Titulo Muerte", font);
        ConfigureText(title, font, "HAS CAÍDO", 32f, new Color(0.92f, 0.68f, 0.3f, 1f), TextAlignmentOptions.Center);
        PlaceCentered(title.rectTransform, new Vector2(0f, 112f), new Vector2(440f, 56f));

        TMP_Text subtitle = GetOrCreateText(frame, "Subtitulo Muerte", font);
        ConfigureText(subtitle, font, "La mazmorra reclama otra alma", 15f, new Color(0.84f, 0.77f, 0.65f, 1f), TextAlignmentOptions.Center);
        PlaceCentered(subtitle.rectTransform, new Vector2(0f, 72f), new Vector2(440f, 30f));

        StyleButton(FindDescendant(death, "retry"), frame, "REINTENTAR", new Vector2(0f, -35f), font, uiSprite);
        StyleButton(FindDescendant(death, "Base"), frame, "VOLVER A LA BASE", new Vector2(0f, -112f), font, uiSprite);
        frame.SetAsLastSibling();
    }

    private static void StyleButton(Transform buttonTransform, RectTransform frame, string label, Vector2 position, TMP_FontAsset font, Sprite uiSprite)
    {
        if (buttonTransform == null) return;
        buttonTransform.SetParent(frame, false);
        RectTransform rect = buttonTransform.GetComponent<RectTransform>();
        PlaceCentered(rect, position, new Vector2(300f, 58f));
        Image image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = uiSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.31f, 0.17f, 0.075f, 1f);
            image.raycastTarget = true;
        }
        Button button = buttonTransform.GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.82f, 0.62f, 0.32f, 1f);
            colors.highlightedColor = new Color(1f, 0.84f, 0.48f, 1f);
            colors.pressedColor = new Color(0.55f, 0.34f, 0.15f, 1f);
            button.colors = colors;
        }
        TMP_Text labelText = buttonTransform.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null) ConfigureText(labelText, font, label, 15f, new Color(0.98f, 0.88f, 0.66f, 1f), TextAlignmentOptions.Center);
    }

    private static void StyleMap(Transform canvas, TMP_FontAsset font, Sprite uiSprite)
    {
        RawImage map = null;
        Camera mapCamera = null;
        foreach (RawImage candidate in canvas.GetComponentsInChildren<RawImage>(true))
        {
            if (candidate.texture != null && candidate.texture.name.ToLowerInvariant().Contains("minimap")) { map = candidate; break; }
        }
        GameObject cameraObject = GameObject.Find("minimap");
        if (cameraObject != null) mapCamera = cameraObject.GetComponent<Camera>();
        if (map == null || mapCamera == null) return;

        Transform existing = canvas.Find("Marco Minimap Medieval");
        GameObject frameObject = existing == null
            ? new GameObject("Marco Minimap Medieval", typeof(RectTransform), typeof(Image))
            : existing.gameObject;
        if (existing == null) frameObject.transform.SetParent(canvas, false);
        RectTransform frame = (RectTransform)frameObject.transform;
        frame.anchorMin = frame.anchorMax = new Vector2(1f, 1f);
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.anchoredPosition = new Vector2(-112f, -120f);
        frame.sizeDelta = new Vector2(184f, 204f);
        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.sprite = uiSprite;
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.58f, 0.36f, 0.14f, 1f);
        frameImage.raycastTarget = false;

        Transform panelTransform = frame.Find("Interior Minimap");
        GameObject panelObject = panelTransform == null
            ? new GameObject("Interior Minimap", typeof(RectTransform), typeof(Image))
            : panelTransform.gameObject;
        if (panelTransform == null) panelObject.transform.SetParent(frame, false);
        RectTransform panel = (RectTransform)panelObject.transform;
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = new Vector2(5f, 5f);
        panel.offsetMax = new Vector2(-5f, -5f);
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.sprite = uiSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(0.09f, 0.055f, 0.03f, 1f);
        panelImage.raycastTarget = false;
        panel.SetAsFirstSibling();

        map.transform.SetParent(frame, false);
        RectTransform mapRect = map.rectTransform;
        mapRect.anchorMin = mapRect.anchorMax = new Vector2(0.5f, 0f);
        mapRect.pivot = new Vector2(0.5f, 0f);
        mapRect.anchoredPosition = new Vector2(0f, 8f);
        mapRect.sizeDelta = new Vector2(164f, 164f);
        mapRect.localScale = Vector3.one;
        mapRect.localRotation = Quaternion.identity;
        map.raycastTarget = false;

        TMP_Text label = GetOrCreateText(frame, "Titulo Minimap", font);
        ConfigureText(label, font, "MAPA DE LA MAZMORRA", 11f, new Color(0.96f, 0.79f, 0.48f, 1f), TextAlignmentOptions.Center);
        PlaceCentered(label.rectTransform, new Vector2(0f, 88f), new Vector2(174f, 22f));

        Transform oldMarker = map.transform.Find("Marcador Jugador");
        GameObject markerObject = oldMarker == null
            ? new GameObject("Marcador Jugador", typeof(RectTransform), typeof(Image), typeof(Outline))
            : oldMarker.gameObject;
        if (oldMarker == null) markerObject.transform.SetParent(map.transform, false);
        RectTransform marker = (RectTransform)markerObject.transform;
        marker.anchorMin = marker.anchorMax = new Vector2(0.5f, 0.5f);
        marker.pivot = new Vector2(0.5f, 0.5f);
        marker.sizeDelta = new Vector2(18f, 18f);
        marker.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image markerImage = markerObject.GetComponent<Image>();
        markerImage.sprite = uiSprite;
        markerImage.color = new Color(0.12f, 0.075f, 0.035f, 1f);
        markerImage.raycastTarget = false;
        Outline outline = markerObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.04f, 0.025f, 0.012f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        Transform centerTransform = marker.Find("Centro");
        GameObject centerObject = centerTransform == null
            ? new GameObject("Centro", typeof(RectTransform), typeof(Image))
            : centerTransform.gameObject;
        if (centerTransform == null) centerObject.transform.SetParent(marker, false);
        RectTransform center = (RectTransform)centerObject.transform;
        center.anchorMin = center.anchorMax = new Vector2(0.5f, 0.5f);
        center.anchoredPosition = Vector2.zero;
        center.sizeDelta = new Vector2(7f, 7f);
        center.localRotation = Quaternion.identity;
        Image centerImage = centerObject.GetComponent<Image>();
        centerImage.sprite = uiSprite;
        centerImage.color = new Color(1f, 0.79f, 0.34f, 1f);
        centerImage.raycastTarget = false;

        DungeonMinimapPlayerMarker tracker = map.GetComponent<DungeonMinimapPlayerMarker>();
        if (tracker == null) tracker = Undo.AddComponent<DungeonMinimapPlayerMarker>(map.gameObject);
        SerializedObject serializedTracker = new SerializedObject(tracker);
        serializedTracker.FindProperty("mapCamera").objectReferenceValue = mapCamera;
        serializedTracker.FindProperty("mapRect").objectReferenceValue = mapRect;
        serializedTracker.FindProperty("marker").objectReferenceValue = marker;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) serializedTracker.FindProperty("player").objectReferenceValue = player.transform;
        serializedTracker.ApplyModifiedProperties();

        frame.SetAsLastSibling();
    }

    private static Transform FindDescendant(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == objectName) return child;
        return null;
    }
    private static TMP_Text GetOrCreateText(RectTransform parent, string objectName, TMP_FontAsset font)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null) return existing.GetComponent<TMP_Text>();
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textObject, "Crear texto medieval");
        textObject.transform.SetParent(parent, false);
        return textObject.GetComponent<TMP_Text>();
    }

    private static void ConfigureText(TMP_Text text, TMP_FontAsset font, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}








