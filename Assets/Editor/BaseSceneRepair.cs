#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BaseSceneRepair
{
    private const string BuildingMaterialPath = "Assets/Old building/Materials/Box001_diffuse.mat";
    private const string BlacksmithMaterialPath = "Assets/Assets/PolyRonin/Medieval Blacksmith Props/Materials/medieval-blacksmith-colors.mat";

    [MenuItem("Tools/My First Videogame/Repair Base Materials and Collisions")]
    private static void Repair()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "Base")
        {
            EditorUtility.DisplayDialog("Repair Base", "Abre la escena Base antes de ejecutar esta reparación.", "OK");
            return;
        }

        RepairMaterial(BuildingMaterialPath);
        RepairMaterial(BlacksmithMaterialPath);
        ConfigureDungeonPrompt();
        int added = AddWorldColliders(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"Base reparada: material de la casa convertido a URP y {added} colisiones añadidas.");
        EditorUtility.DisplayDialog("Base reparada", $"Material del edificio actualizado y {added} colisiones añadidas. La entrada conserva su trigger y ahora también bloquea al jugador.", "OK");
    }

    private static void ConfigureDungeonPrompt()
    {
        GameObject cube = GameObject.Find("Cube");
        DungeonEntrance entrance = cube != null ? cube.GetComponent<DungeonEntrance>() : null;
        if (entrance == null) return;
        var serialized = new SerializedObject(entrance);
        SerializedProperty message = serialized.FindProperty("message");
        if (message != null) message.stringValue = "E para ir a la Dungeon";
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(entrance);
        PrefabUtility.RecordPrefabInstancePropertyModifications(entrance);
    }

    private static void RepairMaterial(string materialPath)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (material == null || lit == null)
        {
            Debug.LogWarning("No se encontró el material del edificio o el shader URP/Lit.");
            return;
        }

        Texture albedo = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        Color tint = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        Texture normal = material.HasProperty("_BumpMap") ? material.GetTexture("_BumpMap") : null;
        Texture metallic = material.HasProperty("_MetallicGlossMap") ? material.GetTexture("_MetallicGlossMap") : null;
        Undo.RecordObject(material, "Convert old building material to URP");
        material.shader = lit;
        if (albedo != null) material.SetTexture("_BaseMap", albedo);
        material.SetColor("_BaseColor", tint);
        if (normal != null) material.SetTexture("_BumpMap", normal);
        if (metallic != null) material.SetTexture("_MetallicGlossMap", metallic);
        EditorUtility.SetDirty(material);
    }

    private static int AddWorldColliders(Scene scene)
    {
        int added = 0;
        foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>())
        {
            GameObject obj = filter.gameObject;
            if (obj.scene != scene || filter.sharedMesh == null || obj.name == "Cube") continue;
            if (filter.GetComponent<Renderer>() == null) continue;
            if (obj.GetComponentInParent<CharacterController>() != null ||
                obj.GetComponentInParent<PlayerController>() != null ||
                obj.GetComponentInParent<PlayerUpgrades>() != null) continue;

            Collider[] existing = obj.GetComponents<Collider>();
            bool alreadySolid = false;
            foreach (Collider collider in existing)
                if (!collider.isTrigger) { alreadySolid = true; break; }
            if (alreadySolid) continue;

            MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(obj);
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshCollider);
            added++;
        }

        GameObject entrance = GameObject.Find("Cube");
        if (entrance != null && entrance.scene == scene)
        {
            BoxCollider trigger = entrance.GetComponent<BoxCollider>();
            bool hasSolidBox = false;
            foreach (BoxCollider box in entrance.GetComponents<BoxCollider>())
                if (!box.isTrigger) { hasSolidBox = true; break; }
            if (trigger != null && trigger.isTrigger && !hasSolidBox)
            {
                BoxCollider solid = Undo.AddComponent<BoxCollider>(entrance);
                solid.center = trigger.center;
                solid.size = trigger.size;
                solid.isTrigger = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(solid);
                added++;
            }
        }
        return added;
    }
}
#endif



