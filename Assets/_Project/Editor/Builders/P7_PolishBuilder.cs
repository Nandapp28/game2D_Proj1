// Alat bantu dosen P7: cahaya gua, game feel battle (VfxPool + guncangan + hit-stop + memudar),
// tombol Keluar, dan pengaturan Player untuk build Windows. Sama dengan langkah di Docs/P07_Polish_Build_Demo.md.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UI = P5_BattleUiBuilder;

public static class P7_PolishBuilder
{
    const string BattlePath = "Assets/_Project/Scenes/Scn_Battle.unity";
    const string TitlePath = "Assets/_Project/Scenes/Scn_Title.unity";
    const string VfxPrefabPath = "Assets/_Project/Prefabs/Fx/Pfb_Vfx_HitSlash.prefab";

    [MenuItem("Pulau Monster/Build/P7 Polish")]
    public static void BuildAll()
    {
        GameObject vfx = BuildVfxPrefab();
        BuildBattle(vfx);
        BuildWorldLight();
        BuildTitleQuit();
        ConfigurePlayer();
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }

    static T Get<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) return;                    // field sudah diganti di versi demo
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------------------------------------------------------- prefab efek tebasan
    public static GameObject BuildVfxPrefab()
    {
        System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/Fx");
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/VFX/VFX_HitSlash_4f_32x32.png")
                                       .OfType<Sprite>()
                                       .OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1)))
                                       .ToArray();
        var go = new GameObject("Pfb_Vfx_HitSlash");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = frames.Length > 0 ? frames[0] : null;
        sr.sortingOrder = 5;
        go.transform.localScale = new Vector3(3, 3, 1);
        var vfx = go.AddComponent<SpriteVfx>();
        var so = new SerializedObject(vfx);
        var fp = so.FindProperty("frames");
        fp.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++) fp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, VfxPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ---------------------------------------------------------------- Scn_Battle: BattleFeel
    public static void BuildBattle(GameObject vfxPrefab)
    {
        Scene scene = EditorSceneManager.OpenScene(BattlePath, OpenSceneMode.Single);
        var old = GameObject.Find("BattleFeel");
        if (old) Object.DestroyImmediate(old);

        var feelGo = new GameObject("BattleFeel");
        var feel = feelGo.AddComponent<BattleFeel>();
        var poolGo = new GameObject("HitVfxPool");
        poolGo.transform.SetParent(feelGo.transform, false);
        var pool = poolGo.AddComponent<VfxPool>();
        Set(pool, "prefab", vfxPrefab.GetComponent<SpriteVfx>());

        Set(feel, "cameraRig", Camera.main.transform);
        Set(feel, "hitVfx", pool);
        Set(feel, "playerBody", GameObject.Find("PlayerMon").GetComponent<SpriteRenderer>());
        Set(feel, "enemyBody", GameObject.Find("EnemyMon").GetComponent<SpriteRenderer>());

        Set(Object.FindAnyObjectByType<BattleInstaller>(), "feel", feel);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Scn_World: cahaya gua
    public static void BuildWorldLight()
    {
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        Light2D global = GameObject.Find("Global Light 2D").GetComponent<Light2D>();
        Transform player = GameObject.Find("Player").transform;

        Transform lampT = player.Find("PlayerLight");
        if (lampT == null)
        {
            lampT = new GameObject("PlayerLight").transform;
            lampT.SetParent(player, false);
        }
        lampT.localPosition = new Vector3(0, 0.5f, 0);
        var lamp = Get<Light2D>(lampT.gameObject);
        lamp.lightType = Light2D.LightType.Point;
        lamp.pointLightInnerRadius = 1f;
        lamp.pointLightOuterRadius = 4.5f;
        lamp.color = new Color32(255, 214, 150, 255);              // hangat seperti obor
        lamp.intensity = 0f;                                       // dinyalakan LightZone

        var gua = GameObject.Find("Area_Gua");
        var zone = Get<LightZone>(gua);
        Set(zone, "globalLight", global);
        Set(zone, "playerLight", lamp);
        var so = new SerializedObject(zone);
        { var __p = so.FindProperty("insideIntensity"); if (__p != null) __p.floatValue = 0.25f; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "insideIntensity"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Scn_Title: tombol Keluar
    public static void BuildTitleQuit()
    {
        Scene scene = EditorSceneManager.OpenScene(TitlePath, OpenSceneMode.Single);
        RectTransform menu = (RectTransform)GameObject.Find("Menu").transform;
        menu.sizeDelta = new Vector2(380, 306);
        var old = menu.Find("Btn_Keluar");
        if (old) Object.DestroyImmediate(old.gameObject);

        RectTransform b = UI.Place(UI.Rect("Btn_Keluar", menu), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -176), new Vector2(320, 64));
        Image img = UI.Img(b, UI.S("UI_Button"), true);
        var btn = b.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.SpriteSwap;
        btn.spriteState = new SpriteState { highlightedSprite = UI.S("UI_Button_Selected"), selectedSprite = UI.S("UI_Button_Selected"), pressedSprite = UI.S("UI_Button_Selected") };
        RectTransform lab = UI.Rect("Label", b);
        lab.anchorMin = Vector2.zero; lab.anchorMax = Vector2.one;
        UI.Txt(lab, "Keluar", 30, UI.Ink, TextAnchor.MiddleCenter);


        Set(Object.FindAnyObjectByType<TitleInstaller>(), "quitButton", btn);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Player Settings untuk build
    public static void ConfigurePlayer()
    {
        PlayerSettings.companyName = "Politeknik Negeri Batam";
        PlayerSettings.productName = "Pulau Monster";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;      // demo di kelas: jendela, bisa diperbesar
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
    }
}
