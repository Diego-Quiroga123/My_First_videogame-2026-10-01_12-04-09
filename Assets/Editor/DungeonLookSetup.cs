using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonLookSetup
{
    private const string DirtTexturePath = "Assets/Textures/DungeonLook/DungeonDirt.png";
    private const string WallTexturePath = "Assets/Textures/DungeonLook/SandstoneBricks.png";
    private const string DirtMaterialPath = "Assets/Textures/DungeonLook/DungeonDirt.mat";
    private const string WallMaterialPath = "Assets/Textures/DungeonLook/SandstoneBricks.mat";
    private const string DirtLayerPath = "Assets/Textures/DungeonLook/DungeonDirt.terrainlayer";
    private const string TerrainDataPath = "Assets/New Terrain 1.asset";

    [MenuItem("Tools/Dungeon/Apply Dirt Floor and Sandstone Walls")]
    private static void Apply()
    {
        if (SceneManager.GetActiveScene().path != "Assets/Scenes/Dungeon.unity")
        {
            EditorUtility.DisplayDialog("Dungeon materials", "Open Assets/Scenes/Dungeon.unity first, then run this command again.", "OK");
            return;
        }

        ConfigureTexture(DirtTexturePath);
        ConfigureTexture(WallTexturePath);
        var dirtTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DirtTexturePath);
        var wallTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WallTexturePath);
        if (dirtTexture == null || wallTexture == null)
        {
            EditorUtility.DisplayDialog("Dungeon materials", "Unity has not imported both textures yet. Wait for import, then run this command again.", "OK");
            return;
        }

        var dirtMaterial = LoadOrCreateMaterial(DirtMaterialPath, dirtTexture);
        var wallMaterial = LoadOrCreateMaterial(WallMaterialPath, wallTexture);
        var dirtLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(DirtLayerPath);
        if (dirtLayer == null)
        {
            dirtLayer = new TerrainLayer();
            AssetDatabase.CreateAsset(dirtLayer, DirtLayerPath);
        }
        dirtLayer.diffuseTexture = dirtTexture;
        dirtLayer.tileSize = new Vector2(3f, 3f);
        dirtLayer.metallic = 0f;
        dirtLayer.smoothness = 0f;
        EditorUtility.SetDirty(dirtLayer);

        var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
        if (terrainData != null)
        {
            terrainData.terrainLayers = new[] { dirtLayer };
            EditorUtility.SetDirty(terrainData);
        }
        foreach (var terrain in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
        {
            if (terrainData != null && terrain.terrainData == terrainData)
                EditorUtility.SetDirty(terrain);
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/prefabs" });
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            var changed = false;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!HasWallsAncestor(renderer.transform))
                    continue;
                renderer.sharedMaterial = wallMaterial;
                changed = true;
            }
            if (changed)
                PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (HasWallsAncestor(renderer.transform) || renderer.gameObject.name.StartsWith("Cube"))
                renderer.sharedMaterial = wallMaterial;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Dungeon materials", "Dirt and sandstone materials have been applied to the Dungeon scene and room prefabs.", "OK");
    }

    private static void ConfigureTexture(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.maxTextureSize = 1024;
        importer.SaveAndReimport();
    }

    private static Material LoadOrCreateMaterial(string path, Texture2D texture)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", Color.white);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static bool HasWallsAncestor(Transform current)
    {
        while (current != null)
        {
            if (current.name.StartsWith("Walls"))
                return true;
            current = current.parent;
        }
        return false;
    }
}
