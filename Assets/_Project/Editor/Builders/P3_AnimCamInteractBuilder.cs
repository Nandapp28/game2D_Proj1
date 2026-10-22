// P3 — animasi Player (8 clip + Blend Tree), kamera Cinemachine + Confiner 2D, layer, NPC prefab & varian, AreaZone.
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class P3_AnimCamInteractBuilder
{
    const string AnimFolder = "Assets/_Project/Art/Characters/Animations/Player";
    const string PlayerSheet = "Assets/_Project/Art/Characters/Player_Walk_16x16.png";
    static readonly string[] Dirs = { "Down", "Left", "Right", "Up" };
    static readonly Vector2[] DirVec = { Vector2.down, Vector2.left, Vector2.right, Vector2.up };

    [MenuItem("Pulau Monster/Build/P3 Animasi, Kamera & Interaksi")]
    public static void BuildAll()
    {
        Layers();
        AnimatorController ac = BuildPlayerAnimator();
        GameObject npcBase = BuildNpcPrefabs(out GameObject[] variants);

        Scene world = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        GameObject player = GameObject.Find("Player");
        player.layer = LayerMask.NameToLayer("Player");

        // Animator + PlayerAnimator
        var animator = GetOrAdd<Animator>(player);
        animator.runtimeAnimatorController = ac;
        var pa = GetOrAdd<PlayerAnimator>(player);
        Set(pa, "movement", player.GetComponent<PlayerMovement>());

        // PlayerInteractor
        var pi = GetOrAdd<PlayerInteractor>(player);
        Set(pi, "movement", player.GetComponent<PlayerMovement>());
        var so = new SerializedObject(pi);
        so.FindProperty("interactableMask").intValue = 1 << LayerMask.NameToLayer("Interactable");
        so.ApplyModifiedPropertiesWithoutUndo();

        var installer = Object.FindAnyObjectByType<WorldInstaller>();
        Set(installer, "interactor", pi);

        // Kamera: Brain di Main Camera, CM_Follow (Position Composer + Pixel Perfect + Confiner 2D)
        GameObject mainCam = GameObject.Find("Main Camera");
        if (!mainCam.GetComponent<CinemachineBrain>()) mainCam.AddComponent<CinemachineBrain>();

        var bounds = new GameObject("CameraBounds");
        bounds.layer = LayerMask.NameToLayer("CameraBounds");
        var poly = bounds.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.pathCount = 1;
        poly.SetPath(0, new[] { new Vector2(0, 0), new Vector2(40, 0), new Vector2(40, 36), new Vector2(0, 36) });

        var cmGo = new GameObject("CM_Follow");
        var cm = cmGo.AddComponent<CinemachineCamera>();
        cm.Target.TrackingTarget = player.transform;
        cm.Lens.OrthographicSize = 5.625f;
        cmGo.transform.position = player.transform.position + new Vector3(0, 0, -10);
        var composer = cmGo.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10f;
        composer.Damping = new Vector3(0.5f, 0.5f, 0f);
        cmGo.AddComponent<CinemachinePixelPerfect>();
        var confiner = cmGo.AddComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = poly;

        // NPC warga desa (varian prefab)
        var npcs = new GameObject("NPCs");
        Vector3[] spots = { new Vector3(25.5f, 7f), new Vector3(7f, 7f), new Vector3(20f, 12.2f), new Vector3(13f, 4f) };
        for (int i = 0; i < variants.Length; i++)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(variants[i], world);
            inst.transform.SetParent(npcs.transform);
            inst.transform.position = spots[i];
        }

        // Area
        var areas = new GameObject("Areas");
        Zone(areas, "Area_Desa", "Desa Awal", new Vector2(20f, 6f), new Vector2(38f, 12f));
        Zone(areas, "Area_Hutan", "Hutan Rimbun", new Vector2(20f, 17f), new Vector2(38f, 10f));
        Zone(areas, "Area_Danau", "Danau Sunyi", new Vector2(9.5f, 29f), new Vector2(17f, 14f));
        Zone(areas, "Area_Gua", "Gua Petir", new Vector2(30f, 30f), new Vector2(18f, 12f));

        EditorSceneManager.SaveScene(world);
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }

    static void Zone(GameObject parent, string name, string area, Vector2 center, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = center;
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;
        var zone = go.AddComponent<AreaZone>();
        var so = new SerializedObject(zone);
        so.FindProperty("areaName").stringValue = area;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    public static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) return;                    // (uji) field sudah diganti di versi demo
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------------------------------------------------------- Layer & Collision Matrix
    public static void Layers()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tm.FindProperty("layers");
        string[] wanted = { "Player", "Interactable", "Wild", "CameraBounds" };
        foreach (string name in wanted)
        {
            bool exists = false;
            for (int i = 0; i < 32; i++) if (layers.GetArrayElementAtIndex(i).stringValue == name) exists = true;
            if (exists) continue;
            for (int i = 6; i < 32; i++)
                if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                { layers.GetArrayElementAtIndex(i).stringValue = name; break; }
        }
        tm.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        int cb = LayerMask.NameToLayer("CameraBounds");
        for (int i = 0; i < 32; i++) Physics2D.IgnoreLayerCollision(cb, i, true);
    }

    // ---------------------------------------------------------------- Animasi
    static Sprite[] Frames(string sheet)
    {
        var list = new Sprite[12];
        string baseName = System.IO.Path.GetFileNameWithoutExtension(sheet);
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(sheet))
            if (o is Sprite s) list[int.Parse(s.name.Substring(baseName.Length + 1))] = s;
        return list;
    }

    public static AnimationClip MakeClip(string path, Sprite[] seq, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[seq.Length + (seq.Length > 1 ? 1 : 0)];
        for (int i = 0; i < seq.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = seq[i] };
        if (seq.Length > 1) keys[seq.Length] = new ObjectReferenceKeyframe { time = seq.Length / fps, value = seq[seq.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static AnimatorController BuildPlayerAnimator()
    {
        System.IO.Directory.CreateDirectory(AnimFolder);
        Sprite[] f = Frames(PlayerSheet);
        var idle = new AnimationClip[4];
        var walk = new AnimationClip[4];
        for (int d = 0; d < 4; d++)
        {
            int b = d * 3;
            idle[d] = MakeClip($"{AnimFolder}/Anim_Player_Idle_{Dirs[d]}.anim", new[] { f[b] }, 8, true);
            walk[d] = MakeClip($"{AnimFolder}/Anim_Player_Walk_{Dirs[d]}.anim", new[] { f[b], f[b + 1], f[b], f[b + 2] }, 8, true);
        }

        string acPath = $"{AnimFolder}/AC_Player.controller";
        AssetDatabase.DeleteAsset(acPath);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(acPath);
        ac.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        ac.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        ac.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        AnimatorState idleState = ac.CreateBlendTreeInController("Idle", out BlendTree idleTree, 0);
        AnimatorState walkState = ac.CreateBlendTreeInController("Walk", out BlendTree walkTree, 0);
        foreach (var (tree, clips) in new[] { (idleTree, idle), (walkTree, walk) })
        {
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            for (int d = 0; d < 4; d++) tree.AddChild(clips[d], DirVec[d]);
        }
        var sm = ac.layers[0].stateMachine;
        sm.defaultState = idleState;
        var toWalk = idleState.AddTransition(walkState);
        toWalk.hasExitTime = false; toWalk.duration = 0f;
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        var toIdle = walkState.AddTransition(idleState);
        toIdle.hasExitTime = false; toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        AssetDatabase.SaveAssets();
        return ac;
    }

    // ---------------------------------------------------------------- NPC prefab & varian
    static GameObject BuildNpcPrefabs(out GameObject[] variants)
    {
        System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/NPC");
        const string basePath = "Assets/_Project/Prefabs/NPC/Pfb_Npc.prefab";
        Sprite[] guard = Frames("Assets/_Project/Art/Characters/NPC_Guard_Walk_16x16.png");

        var go = new GameObject("Npc");
        go.layer = LayerMask.NameToLayer("Interactable");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        sr.sprite = guard[0];
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.8f, 0.5f);
        box.offset = new Vector2(0f, 0.25f);
        var npc = go.AddComponent<Npc>();
        Set(npc, "body", sr);
        GameObject basePrefab = PrefabUtility.SaveAsPrefabAsset(go, basePath);
        Object.DestroyImmediate(go);

        string[] who = { "Elder", "Girl", "Guard", "Merchant" };
        string[] names = { "Tetua Rahmat", "Sari", "Penjaga Bayu", "Pedagang Ucok" };
        variants = new GameObject[who.Length];
        for (int i = 0; i < who.Length; i++)
        {
            Sprite[] fr = Frames($"Assets/_Project/Art/Characters/NPC_{who[i]}_Walk_16x16.png");
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            inst.name = "Npc_" + who[i];
            inst.GetComponent<SpriteRenderer>().sprite = fr[0];
            var so = new SerializedObject(inst.GetComponent<Npc>());
            so.FindProperty("npcName").stringValue = names[i];
            so.FindProperty("faceDown").objectReferenceValue = fr[0];
            so.FindProperty("faceLeft").objectReferenceValue = fr[3];
            so.FindProperty("faceRight").objectReferenceValue = fr[6];
            so.FindProperty("faceUp").objectReferenceValue = fr[9];
            so.ApplyModifiedPropertiesWithoutUndo();
            variants[i] = PrefabUtility.SaveAsPrefabAsset(inst, $"Assets/_Project/Prefabs/NPC/Pfb_Npc_{who[i]}.prefab");
            Object.DestroyImmediate(inst);
        }
        return basePrefab;
    }
}
