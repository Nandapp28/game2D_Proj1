// P4 — 12 MonsterData dari CSV, MonsterDatabase, animasi idle monster (AC + Override), monster liar di Scn_World,
// Canvas_System + Fader di Scn_Bootstrap, dan Scn_Battle placeholder.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class P4_MonsterBuilder
{
    public const string BattlePath = "Assets/_Project/Scenes/Scn_Battle.unity";
    public const string DataFolder = "Assets/_Project/Data/Monsters";
    const string AnimFolder = "Assets/_Project/Art/Monsters/Animations";

    static readonly Dictionary<string, string> Skill = new Dictionary<string, string>
    { { "Pyro", "Semburan Api" }, { "Aqua", "Tembakan Air" }, { "Sprout", "Daun Tajam" }, { "Volt", "Setrum Kilat" } };

    public static Dictionary<string, MonsterData> Data = new Dictionary<string, MonsterData>();
    public static Dictionary<string, AnimatorOverrideController> Aoc = new Dictionary<string, AnimatorOverrideController>();

    [MenuItem("Pulau Monster/Build/P4 Data Monster & Monster Liar")]
    public static void BuildAll()
    {
        BuildData();
        BuildAnimations();
        BuildBootstrapUi();
        BuildBattleScene();
        BuildWildMonsters();
    }

    // ---------------------------------------------------------------- MonsterData dari CSV
    public static MonsterDatabase BuildData()
    {
        Directory.CreateDirectory(DataFolder);
        string[] lines = File.ReadAllLines("Assets/_Project/Data/MonsterDatabase.csv");
        var rows = new List<string[]>();
        for (int i = 1; i < lines.Length; i++) if (lines[i].Trim().Length > 0) rows.Add(lines[i].Split(','));

        Data.Clear();
        foreach (string[] r in rows)
        {
            string path = $"{DataFolder}/Mon_{r[1]}.asset";
            var md = AssetDatabase.LoadAssetAtPath<MonsterData>(path);
            if (md == null) { md = ScriptableObject.CreateInstance<MonsterData>(); AssetDatabase.CreateAsset(md, path); }
            Data[r[1]] = md;
        }
        foreach (string[] r in rows)
        {
            var so = new SerializedObject(Data[r[1]]);
            { var __p = so.FindProperty("id"); if (__p != null) __p.stringValue = r[0]; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "id"); }
            { var __p = so.FindProperty("displayName"); if (__p != null) __p.stringValue = r[1]; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "displayName"); }
            { var __p = so.FindProperty("element"); if (__p != null) __p.enumValueIndex = (int)System.Enum.Parse(typeof(Element), r[2]); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "element"); }
            { var __p = so.FindProperty("baseHP"); if (__p != null) __p.intValue = int.Parse(r[4]); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "baseHP"); }
            { var __p = so.FindProperty("baseATK"); if (__p != null) __p.intValue = int.Parse(r[5]); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "baseATK"); }
            { var __p = so.FindProperty("baseDEF"); if (__p != null) __p.intValue = int.Parse(r[6]); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "baseDEF"); }
            { var __p = so.FindProperty("baseSPD"); if (__p != null) __p.intValue = int.Parse(r[7]); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "baseSPD"); }
            { var __p = so.FindProperty("skillName"); if (__p != null) __p.stringValue = Skill[r[2]]; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "skillName"); }
            { var __p = so.FindProperty("evolvesTo"); if (__p != null) __p.objectReferenceValue = r[8].Length > 0 ? Data[r[8]] : null; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "evolvesTo"); }
            { var __p = so.FindProperty("evolveLevel"); if (__p != null) __p.intValue = r[9].Length > 0 ? int.Parse(r[9]) : 0; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "evolveLevel"); }
            Sprite[] frames = Frames("Assets/_Project/Art/Monsters/" + r[11]);
            SerializedProperty fp = so.FindProperty("frames");
            fp.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) fp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        const string dbPath = "Assets/_Project/Data/MonsterDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(dbPath);
        if (db == null) { db = ScriptableObject.CreateInstance<MonsterDatabase>(); AssetDatabase.CreateAsset(db, dbPath); }
        var dso = new SerializedObject(db);
        SerializedProperty list = dso.FindProperty("monsters");
        list.arraySize = rows.Count;
        for (int i = 0; i < rows.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = Data[rows[i][1]];
        dso.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        return db;
    }

    public static Sprite[] Frames(string sheet)
    {
        string baseName = Path.GetFileNameWithoutExtension(sheet);
        var list = new SortedDictionary<int, Sprite>();
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(sheet))
            if (o is Sprite s) list[int.Parse(s.name.Substring(baseName.Length + 1))] = s;
        return new List<Sprite>(list.Values).ToArray();
    }

    // ---------------------------------------------------------------- Animasi idle: satu AC + 12 Override
    static void BuildAnimations()
    {
        Directory.CreateDirectory(AnimFolder);
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var kv in Data)
            clips[kv.Key] = P3_AnimCamInteractBuilder.MakeClip($"{AnimFolder}/Anim_Mon_{kv.Key}_Idle.anim",
                new[] { kv.Value.Frames[0], kv.Value.Frames[1] }, 3, true);

        string acPath = $"{AnimFolder}/AC_Monster.controller";
        AssetDatabase.DeleteAsset(acPath);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(acPath);
        AnimatorState idle = ac.layers[0].stateMachine.AddState("Idle");
        idle.motion = clips["Pyrit"];
        AssetDatabase.SaveAssets();

        Aoc.Clear();
        foreach (var kv in clips)
        {
            string path = $"{AnimFolder}/AOC_Mon_{kv.Key}.overrideController";
            AssetDatabase.DeleteAsset(path);
            var aoc = new AnimatorOverrideController(ac);
            aoc[clips["Pyrit"]] = kv.Value;
            AssetDatabase.CreateAsset(aoc, path);
            Aoc[kv.Key] = aoc;
        }
        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- Scn_Bootstrap: Canvas_System + Fader + EventSystem
    static void BuildBootstrapUi()
    {
        Scene boot = EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath, OpenSceneMode.Single);
        foreach (string n in new[] { "Canvas_System", "EventSystem" }) { var old = GameObject.Find(n); if (old) Object.DestroyImmediate(old); }

        var canvasGo = new GameObject("Canvas_System", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var faderGo = new GameObject("Fader", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(ScreenFader));
        faderGo.transform.SetParent(canvasGo.transform, false);
        Stretch(faderGo.GetComponent<RectTransform>());
        faderGo.GetComponent<Image>().color = Color.black;
        var group = faderGo.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        var fader = faderGo.GetComponent<ScreenFader>();
        P3_AnimCamInteractBuilder.Set(fader, "group", group);

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
        var so = new SerializedObject(bootstrap);
        { var __p = so.FindProperty("monsterDatabase"); if (__p != null) __p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<MonsterDatabase>("Assets/_Project/Data/MonsterDatabase.asset"); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "monsterDatabase"); }
        { var __p = so.FindProperty("fader"); if (__p != null) __p.objectReferenceValue = fader; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "fader"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(boot);
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // ---------------------------------------------------------------- Scn_Battle (placeholder P4)
    static void BuildBattleScene()
    {
        Scene battle = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGo = new GameObject("Main Camera", typeof(Camera));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5.625f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<UniversalAdditionalCameraData>();
        var ppc = camGo.AddComponent<PixelPerfectCamera>();
        ppc.assetsPPU = 16; ppc.refResolutionX = 320; ppc.refResolutionY = 180;
        new GameObject("Global Light 2D").AddComponent<Light2D>().lightType = Light2D.LightType.Global;

        var bg = new GameObject("Background").AddComponent<SpriteRenderer>();
        bg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Grassland_320x180.png");
        bg.sortingOrder = -10;

        var enemy = new GameObject("EnemyMon").AddComponent<SpriteRenderer>();
        enemy.transform.position = new Vector3(4.5f, 0.3f, 0);
        enemy.transform.localScale = new Vector3(2, 2, 1);

        var installer = new GameObject("BattleInstaller").AddComponent<BattleInstaller>();
        P3_AnimCamInteractBuilder.Set(installer, "enemySprite", enemy);
        EditorSceneManager.SaveScene(battle, BattlePath);

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == BattlePath)) scenes.Add(new EditorBuildSettingsScene(BattlePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------------------------------------------------------- Monster liar di Scn_World
    static void BuildWildMonsters()
    {
        Directory.CreateDirectory("Assets/_Project/Prefabs/Monsters");
        const string prefabPath = "Assets/_Project/Prefabs/Monsters/Pfb_WildMonster.prefab";
        var go = new GameObject("WildMonster");
        go.layer = LayerMask.NameToLayer("Wild");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        go.AddComponent<Animator>();
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.35f; col.offset = new Vector2(0f, 0.3f);
        var wm = go.AddComponent<WildMonster>();
        var so = new SerializedObject(wm);
        { var __p = so.FindProperty("rb"); if (__p != null) __p.objectReferenceValue = rb; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "rb"); }
        { var __p = so.FindProperty("body"); if (__p != null) __p.objectReferenceValue = sr; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "body"); }
        { var __p = so.FindProperty("hitbox"); if (__p != null) __p.objectReferenceValue = col; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "hitbox"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Scene world = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        var old = GameObject.Find("WildMonsters");
        if (old) Object.DestroyImmediate(old);
        var parent = new GameObject("WildMonsters");

        void Spawn(string species, Vector2 pos, int min, int max, bool boss = false)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, world);
            inst.name = (boss ? "Boss_" : "Wild_") + species;
            inst.transform.SetParent(parent.transform);
            inst.transform.position = pos;
            inst.GetComponent<SpriteRenderer>().sprite = Data[species].Frames[0];
            inst.GetComponent<Animator>().runtimeAnimatorController = Aoc[species];
            var s = new SerializedObject(inst.GetComponent<WildMonster>());
            { var __p = s.FindProperty("species"); if (__p != null) __p.objectReferenceValue = Data[species]; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "species"); }
            s.FindProperty("levelRange").vector2IntValue = new Vector2Int(min, max);
            { var __p = s.FindProperty("isBoss"); if (__p != null) __p.boolValue = boss; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "isBoss"); }
            { var __p = s.FindProperty("stationary"); if (__p != null) __p.boolValue = boss; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "stationary"); }
            s.ApplyModifiedPropertiesWithoutUndo();
        }
        Spawn("Sprig", new Vector2(6.5f, 16f), 2, 4);
        Spawn("Zapi", new Vector2(9f, 17.5f), 2, 4);
        Spawn("Blubi", new Vector2(27f, 15.5f), 2, 4);
        Spawn("Pyrit", new Vector2(31f, 17f), 3, 5);
        Spawn("Finnox", new Vector2(17.5f, 28.5f), 5, 7);
        Spawn("Voltcat", new Vector2(26f, 29.5f), 7, 9);
        Spawn("Infernox", new Vector2(30.5f, 31.5f), 12, 12, true);

        var installer = Object.FindAnyObjectByType<WorldInstaller>();
        // P6: debugStarter dihapus dari WorldInstaller (partner diberikan Tetua lewat dialog).
        EditorSceneManager.SaveScene(world);
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }
}
