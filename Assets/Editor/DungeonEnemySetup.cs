using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class DungeonEnemySetup
{
    private const string PrefabPath = "Assets/prefabs/enemi_standar.prefab";
    private const string ModelPrefabPath = "Assets/Assets/Mini Legion Lich PBR HP Polyart/Prefabs/FreeLichHP.prefab";
    private const string ControllerPath = "Assets/Animations/DungeonEnemy.controller";
    private const string MaterialPath = "Assets/Textures/DungeonEnemy/DungeonLich.mat";
    private const string TexturePath = "Assets/Assets/Mini Legion Lich PBR HP Polyart/Textures/PolyartStandard.png";
    private const string ClipFolder = "Assets/Assets/Mini Legion Lich PBR HP Polyart/Animations/Lich/";

    [MenuItem("Tools/Dungeon/Build Animated Dungeon Enemy")]
    private static void Build()
    {
        AnimationClip idle = LoadClip("Idle.fbx");
        AnimationClip run = LoadClip("Run.fbx");
        AnimationClip attack = LoadClip("Attack01.fbx");
        if (idle == null || run == null || attack == null)
        {
            EditorUtility.DisplayDialog("Dungeon enemy", "No se encontraron los clips Idle, Run y Attack01 del Lich.", "OK");
            return;
        }

        AnimatorController controller = CreateController(idle, run, attack);
        Material material = CreateMaterial();
        BuildPrefab(controller, material);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Dungeon enemy", "El enemigo de Dungeon ahora tiene textura, animación de movimiento y animación de ataque.", "OK");
    }

    private static AnimationClip LoadClip(string fileName)
    {
        string path = ClipFolder + fileName;
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(clip => !clip.name.StartsWith("__preview__"));
    }

    private static AnimatorController CreateController(AnimationClip idle, AnimationClip run, AnimationClip attack)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller != null) AssetDatabase.DeleteAsset(ControllerPath);
        controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        machine.states = new ChildAnimatorState[0];
        AnimatorState idleState = machine.AddState("Idle");
        AnimatorState runState = machine.AddState("Run");
        AnimatorState attackState = machine.AddState("Attack");
        idleState.motion = idle;
        runState.motion = run;
        attackState.motion = attack;
        machine.defaultState = idleState;

        AnimatorStateTransition startRun = idleState.AddTransition(runState);
        startRun.hasExitTime = false;
        startRun.duration = 0.12f;
        startRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition stopRun = runState.AddTransition(idleState);
        stopRun.hasExitTime = false;
        stopRun.duration = 0.12f;
        stopRun.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        AnimatorStateTransition attackTransition = machine.AddAnyStateTransition(attackState);
        attackTransition.hasExitTime = false;
        attackTransition.duration = 0.08f;
        attackTransition.canTransitionToSelf = false;
        attackTransition.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        AnimatorStateTransition finishAttack = attackState.AddTransition(idleState);
        finishAttack.hasExitTime = true;
        finishAttack.exitTime = 0.95f;
        finishAttack.duration = 0.1f;
        return controller;
    }

    private static Material CreateMaterial()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null) throw new System.Exception("No se encontró la textura PolyartStandard.png.");

        string folder = "Assets/Textures/DungeonEnemy";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Textures", "DungeonEnemy");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else material.shader = shader;

        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(0.82f, 0.86f, 0.92f, 1f));
        if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(0.82f, 0.86f, 0.92f, 1f));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void BuildPrefab(AnimatorController controller, Material material)
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath);
        if (sourcePrefab == null || modelPrefab == null)
            throw new System.Exception("No se encontró el prefab de enemigo o el modelo Lich.");

        GameObject root = new GameObject("enemi_standar");
        root.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) root.layer = enemyLayer;

        CopyComponent(sourcePrefab.GetComponent<Health>(), root.AddComponent<Health>());
        CopyComponent(sourcePrefab.GetComponent<EnemyClose>(), root.AddComponent<EnemyClose>());
        CopyComponent(sourcePrefab.GetComponent<EnemyDrop>(), root.AddComponent<EnemyDrop>());
        CopyComponent(sourcePrefab.GetComponent<EnemyHealthBar>(), root.AddComponent<EnemyHealthBar>());

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 1f, 0f);
        collider.radius = 0.45f;
        collider.height = 2f;

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        model.name = "DungeonLich";
        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        Animator animator = model.GetComponentInChildren<Animator>();
        if (animator == null) animator = model.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterial = material;
        }

        SetLayerRecursively(root.transform, root.layer);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CopyComponent(Component source, Component destination)
    {
        if (source != null && destination != null) EditorUtility.CopySerialized(source, destination);
    }

    private static void SetLayerRecursively(Transform item, int layer)
    {
        item.gameObject.layer = layer;
        foreach (Transform child in item) SetLayerRecursively(child, layer);
    }
}
