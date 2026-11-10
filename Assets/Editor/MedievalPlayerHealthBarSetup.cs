using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MedievalPlayerHealthBarSetup
{
    private const string DungeonScenePath = "Assets/Scenes/Dungeon.unity";
    private const string FontPath = "Assets/Fonts/Cinzel-Bold SDF.asset";

    [MenuItem("Tools/UI/Style Dungeon Health Bar (Medieval)")]
    private static void StyleHealthBar()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != DungeonScenePath)
        {
            EditorUtility.DisplayDialog("Barra de vida", "Abre la escena Dungeon antes de aplicar el estilo.", "OK");
            return;
        }

        GameObject barObject = GameObject.Find("vida");
        if (barObject == null)
        {
            EditorUtility.DisplayDialog("Barra de vida", "No se encontró el objeto 'vida' en Dungeon.", "OK");
            return;
        }

        RectTransform root = barObject.GetComponent<RectTransform>();
        Slider slider = barObject.GetComponent<Slider>();
        PlayerHealthUI healthUI = barObject.GetComponent<PlayerHealthUI>();
        if (root == null || slider == null || healthUI == null)
        {
            EditorUtility.DisplayDialog("Barra de vida", "El objeto 'vida' no tiene los componentes esperados.", "OK");
            return;
        }

        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        Image frame = barObject.GetComponent<Image>();
        if (frame == null) frame = Undo.AddComponent<Image>(barObject);
        frame.sprite = uiSprite;
        frame.type = Image.Type.Sliced;
        frame.color = new Color(0.58f, 0.34f, 0.12f, 1f);
        frame.raycastTarget = false;

        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = new Vector2(26f, -26f);
        root.sizeDelta = new Vector2(286f, 76f);

        Transform interiorTransform = root.Find("Panel Medieval");
        GameObject interiorObject;
        if (interiorTransform == null)
        {
            interiorObject = new GameObject("Panel Medieval", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(interiorObject, "Crear panel de barra medieval");
            interiorObject.transform.SetParent(root, false);
        }
        else interiorObject = interiorTransform.gameObject;

        RectTransform interior = (RectTransform)interiorObject.transform;
        interior.anchorMin = Vector2.zero;
        interior.anchorMax = Vector2.one;
        interior.offsetMin = new Vector2(5f, 5f);
        interior.offsetMax = new Vector2(-5f, -5f);
        Image interiorImage = interiorObject.GetComponent<Image>();
        interiorImage.sprite = uiSprite;
        interiorImage.type = Image.Type.Sliced;
        interiorImage.color = new Color(0.13f, 0.075f, 0.035f, 0.98f);
        interiorImage.raycastTarget = false;
        interiorObject.transform.SetAsFirstSibling();

        Transform backgroundTransform = root.Find("Background");
        if (backgroundTransform != null)
        {
            RectTransform backgroundRect = backgroundTransform.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.045f, 0.13f);
            backgroundRect.anchorMax = new Vector2(0.955f, 0.48f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image background = backgroundTransform.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0.055f, 0.035f, 0.025f, 1f);
                background.raycastTarget = false;
            }
        }

        Transform fillAreaTransform = root.Find("Fill Area");
        if (fillAreaTransform != null)
        {
            RectTransform fillArea = fillAreaTransform.GetComponent<RectTransform>();
            fillArea.anchorMin = new Vector2(0.06f, 0.17f);
            fillArea.anchorMax = new Vector2(0.94f, 0.44f);
            fillArea.anchoredPosition = Vector2.zero;
            fillArea.sizeDelta = Vector2.zero;
        }

        Transform titleTransform = root.Find("Rotulo Vitalidad");
        TMP_Text title = titleTransform != null ? titleTransform.GetComponent<TMP_Text>() : CreateText(root, "Rotulo Vitalidad", font);
        ConfigureText(title, font, "VITALIDAD", 13f, new Color(0.95f, 0.78f, 0.43f, 1f), TextAlignmentOptions.Left);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(12f, -8f);
        titleRect.sizeDelta = new Vector2(150f, 20f);

        Transform valueTransform = root.Find("Valor Vitalidad");
        TMP_Text value = valueTransform != null ? valueTransform.GetComponent<TMP_Text>() : CreateText(root, "Valor Vitalidad", font);
        ConfigureText(value, font, "100 / 100", 12f, new Color(0.98f, 0.9f, 0.7f, 1f), TextAlignmentOptions.Right);
        RectTransform valueRect = value.rectTransform;
        valueRect.anchorMin = new Vector2(1f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-12f, -8f);
        valueRect.sizeDelta = new Vector2(90f, 20f);

        SerializedObject serializedHealthUI = new SerializedObject(healthUI);
        serializedHealthUI.FindProperty("healthText").objectReferenceValue = value;
        serializedHealthUI.ApplyModifiedProperties();

        ConfigureRivet(root, "Remache TL", new Vector2(5f, -5f));
        ConfigureRivet(root, "Remache TR", new Vector2(-5f, -5f));
        ConfigureRivet(root, "Remache BL", new Vector2(5f, 5f));
        ConfigureRivet(root, "Remache BR", new Vector2(-5f, 5f));

        slider.interactable = false;
        EditorUtility.SetDirty(barObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Barra de vida", "Barra medieval aplicada: marco dorado, rótulo Cinzel y color dinámico según la vida.", "OK");
    }

    private static TMP_Text CreateText(RectTransform parent, string objectName, TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textObject, "Crear texto de vida");
        textObject.transform.SetParent(parent, false);
        return textObject.GetComponent<TextMeshProUGUI>();
    }

    private static void ConfigureText(TMP_Text text, TMP_FontAsset font, string value, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static void ConfigureRivet(RectTransform parent, string objectName, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(objectName);
        GameObject rivetObject;
        if (existing == null)
        {
            rivetObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(rivetObject, "Crear remache decorativo");
            rivetObject.transform.SetParent(parent, false);
        }
        else rivetObject = existing.gameObject;

        RectTransform rivetRect = (RectTransform)rivetObject.transform;
        rivetRect.anchorMin = new Vector2(anchoredPosition.x > 0 ? 0f : 1f, anchoredPosition.y < 0 ? 1f : 0f);
        rivetRect.anchorMax = rivetRect.anchorMin;
        rivetRect.pivot = new Vector2(0.5f, 0.5f);
        rivetRect.anchoredPosition = anchoredPosition;
        rivetRect.sizeDelta = new Vector2(6f, 6f);
        rivetRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image rivet = rivetObject.GetComponent<Image>();
        rivet.color = new Color(0.98f, 0.78f, 0.42f, 1f);
        rivet.raycastTarget = false;
    }
}

