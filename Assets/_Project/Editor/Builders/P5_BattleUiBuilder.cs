// Alat bantu dosen P5: menyiapkan sprite UI, UI battle (MVP) di Scn_Battle, dan HUD partner di Scn_World.
// Hasilnya sama dengan langkah manual di Docs/P05_Battle_UI.md.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class P5_BattleUiBuilder
{
    const string UiFolder = "Assets/_Project/Art/UI";
    const string BattlePath = "Assets/_Project/Scenes/Scn_Battle.unity";
    // 1 piksel sprite = 4 unit kanvas (referensi 1280×720): 100 / 16 / 4 = 1,5625
    public const float PixelScale = 1.5625f;

    public static readonly Color Ink = new Color32(28, 21, 64, 255);
    public static readonly Color Cream = new Color32(250, 244, 226, 255);
    public static readonly Color Gold = new Color32(255, 198, 26, 255);
    public static readonly Color ExpBlue = new Color32(84, 160, 232, 255);

    [MenuItem("Pulau Monster/Build/P5 Battle & UI")]
    public static void BuildAll()
    {
        ImportUiSprites();
        BuildBattleScene();
        BuildWorldHud();
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }

    // ---------------------------------------------------------------- Sprite UI: Single, PPU 16, Point, border 9-slice
    public static void ImportUiSprites()
    {
        foreach (string path in Directory.GetFiles(UiFolder, "UI_*.png"))
        {
            string p = path.Replace('\\', '/');
            var ti = (TextureImporter)AssetImporter.GetAtPath(p);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            string n = Path.GetFileNameWithoutExtension(p);
            if (n.StartsWith("UI_Panel") || n.StartsWith("UI_Button")) ti.spriteBorder = new Vector4(4, 4, 4, 4);
            else if (n == "UI_Bar_Frame") ti.spriteBorder = new Vector4(3, 3, 3, 3);
            else ti.spriteBorder = Vector4.zero;
            ti.SaveAndReimport();
        }
    }

    public static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{UiFolder}/{name}.png");

    // ---------------------------------------------------------------- helper UI
    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static Image Img(RectTransform rt, Sprite sprite, bool sliced)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        if (sliced) { img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = PixelScale; }
        return img;
    }

    static Font uiFont;

    public static Text Txt(RectTransform rt, string text, int size, Color color, TextAnchor align)
    {
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var t = rt.gameObject.AddComponent<Text>();
        t.font = uiFont;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Image Bar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
    {
        RectTransform frame = Place(Rect(name, parent), new Vector2(0, 1), new Vector2(0, 1), pos, size);
        Img(frame, S("UI_Bar_Frame"), true);
        RectTransform fillRt = Rect("Fill", frame);
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(6, 6); fillRt.offsetMax = new Vector2(-6, -6);
        var fill = Img(fillRt, S("UI_Bar_Fill"), false);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        fill.color = fillColor;
        return fill;
    }

    public static Canvas MakeCanvas(string name, int order)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    // Panel monster (nama, level, bar HP) + komponen MonsterPanel
    static MonsterPanel MonsterInfo(Transform canvas, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
                                    SpriteRenderer body, bool faceRight)
    {
        RectTransform panel = Place(Rect(name, canvas), anchor, pivot, pos, new Vector2(420, 104));
        Img(panel, S("UI_Panel_Dark"), true);
        var nameText = Txt(Place(Rect("Name", panel), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -14), new Vector2(260, 40)),
                           "Nama", 30, Cream, TextAnchor.MiddleLeft);
        var levelText = Txt(Place(Rect("Level", panel), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -14), new Vector2(120, 40)),
                            "Lv 1", 28, Gold, TextAnchor.MiddleRight);
        Txt(Place(Rect("HpLabel", panel), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -58), new Vector2(52, 30)),
            "HP", 24, Gold, TextAnchor.MiddleLeft);
        Image hp = Bar(panel, "HpBar", new Vector2(76, -58), new Vector2(320, 28), HpColors.High);

        var mp = panel.gameObject.AddComponent<MonsterPanel>();
        var so = new SerializedObject(mp);
        { var __p = so.FindProperty("body"); if (__p != null) __p.objectReferenceValue = body; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "body"); }
        { var __p = so.FindProperty("faceRight"); if (__p != null) __p.boolValue = faceRight; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "faceRight"); }
        { var __p = so.FindProperty("nameText"); if (__p != null) __p.objectReferenceValue = nameText; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "nameText"); }
        { var __p = so.FindProperty("levelText"); if (__p != null) __p.objectReferenceValue = levelText; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "levelText"); }
        { var __p = so.FindProperty("hpFill"); if (__p != null) __p.objectReferenceValue = hp; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "hpFill"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        return mp;
    }

    // ---------------------------------------------------------------- Scn_Battle
    public static void BuildBattleScene()
    {
        Scene scene = EditorSceneManager.OpenScene(BattlePath, OpenSceneMode.Single);
        foreach (string n in new[] { "Canvas_Battle", "BattleSystem", "PlayerMon" })
        {
            var old = GameObject.Find(n);
            if (old) Object.DestroyImmediate(old);
        }

        // posisi monster: di atas lingkaran tanah pada background
        var enemy = GameObject.Find("EnemyMon").GetComponent<SpriteRenderer>();
        enemy.transform.position = new Vector3(4.4f, -1.9f, 0);
        enemy.transform.localScale = new Vector3(3, 3, 1);
        enemy.sortingOrder = 1;
        var player = new GameObject("PlayerMon").AddComponent<SpriteRenderer>();
        player.transform.position = new Vector3(-4.4f, -2.5f, 0);
        player.transform.localScale = new Vector3(3, 3, 1);
        player.sortingOrder = 1;

        var system = new GameObject("BattleSystem").AddComponent<BattleSystem>();

        Canvas canvas = MakeCanvas("Canvas_Battle", 10);
        Transform root = canvas.transform;

        // Kotak pesan
        RectTransform box = Rect("MessageBox", root);
        box.anchorMin = new Vector2(0, 0); box.anchorMax = new Vector2(1, 0); box.pivot = new Vector2(0.5f, 0);
        box.offsetMin = new Vector2(16, 16); box.offsetMax = new Vector2(-16, 186);
        Img(box, S("UI_Panel"), true);
        RectTransform msgRt = Rect("Message", box);
        msgRt.anchorMin = Vector2.zero; msgRt.anchorMax = Vector2.one;
        msgRt.offsetMin = new Vector2(32, 24); msgRt.offsetMax = new Vector2(-620, -24);
        var message = Txt(msgRt, "…", 30, Ink, TextAnchor.UpperLeft);

        // Menu aksi 2 × 2
        RectTransform menu = Place(Rect("Menu", root), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40, 40), new Vector2(560, 138));
        var grid = menu.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(275, 64);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        string[] labels = { "Serang", "Jurus", "Ramuan", "Kabur" };
        string[] icons = { "UI_Icon_Sword", "UI_Icon_Star", "UI_Icon_Potion", "UI_Icon_Run" };
        var buttons = new Button[4];
        var buttonLabels = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            RectTransform b = Rect("Btn_" + labels[i], menu);
            Image bg = Img(b, S("UI_Button"), true);
            var btn = b.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState
            {
                highlightedSprite = S("UI_Button_Selected"),
                selectedSprite = S("UI_Button_Selected"),
                pressedSprite = S("UI_Button_Selected"),
                disabledSprite = S("UI_Button"),
            };
            RectTransform icon = Place(Rect("Icon", b), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(40, 40));
            Img(icon, S(icons[i]), false).preserveAspect = true;
            RectTransform lab = Rect("Label", b);
            lab.anchorMin = Vector2.zero; lab.anchorMax = Vector2.one;
            lab.offsetMin = new Vector2(66, 4); lab.offsetMax = new Vector2(-10, -4);
            buttonLabels[i] = Txt(lab, labels[i], 28, Ink, TextAnchor.MiddleLeft);
            buttons[i] = btn;
        }

        MonsterPanel enemyPanel = MonsterInfo(root, "EnemyPanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -32), enemy, false); // kanan atas, di atas sprite musuh
        MonsterPanel playerPanel = MonsterInfo(root, "PlayerPanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(32, -32), player, true); // kiri atas, di atas sprite pemain

        var view = canvas.gameObject.AddComponent<BattleView>();
        var vso = new SerializedObject(view);
        { var __p = vso.FindProperty("messageText"); if (__p != null) __p.objectReferenceValue = message; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "messageText"); }
        { var __p = vso.FindProperty("menuRoot"); if (__p != null) __p.objectReferenceValue = menu.gameObject; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "menuRoot"); }
        SerializedProperty bp = vso.FindProperty("actionButtons");
        SerializedProperty lp = vso.FindProperty("actionLabels");
        if (bp != null && lp != null)
        {
            bp.arraySize = 4; lp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                bp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                lp.GetArrayElementAtIndex(i).objectReferenceValue = buttonLabels[i];
            }
        }
        else UnityEngine.Debug.LogWarning("[uji] actionButtons/actionLabels hilang");
        { var __p = vso.FindProperty("playerPanel"); if (__p != null) __p.objectReferenceValue = playerPanel; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "playerPanel"); }
        { var __p = vso.FindProperty("enemyPanel"); if (__p != null) __p.objectReferenceValue = enemyPanel; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "enemyPanel"); }
        vso.ApplyModifiedPropertiesWithoutUndo();

        var installer = Object.FindAnyObjectByType<BattleInstaller>();
        var iso = new SerializedObject(installer);
        { var __p = iso.FindProperty("battle"); if (__p != null) __p.objectReferenceValue = system; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "battle"); }
        { var __p = iso.FindProperty("view"); if (__p != null) __p.objectReferenceValue = view; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "view"); }
        iso.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- HUD partner di Scn_World
    public static void BuildWorldHud()
    {
        Scene world = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        var old = GameObject.Find("Canvas_HUD");
        if (old) Object.DestroyImmediate(old);

        Canvas canvas = MakeCanvas("Canvas_HUD", 5);
        Transform root = canvas.transform;
        var hud = canvas.gameObject.AddComponent<PartnerHud>();

        RectTransform panel = Place(Rect("PartnerPanel", root), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(380, 112));
        Img(panel, S("UI_Panel_Dark"), true);
        RectTransform portRt = Place(Rect("Portrait", panel), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(72, 72));
        Image portrait = Img(portRt, null, false);
        portrait.preserveAspect = true;
        var nameText = Txt(Place(Rect("Name", panel), new Vector2(0, 1), new Vector2(0, 1), new Vector2(104, -12), new Vector2(170, 36)),
                           "Partner", 28, Cream, TextAnchor.MiddleLeft);
        var levelText = Txt(Place(Rect("Level", panel), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -12), new Vector2(90, 36)),
                            "Lv 1", 26, Gold, TextAnchor.MiddleRight);
        Image hp = Bar(panel, "HpBar", new Vector2(104, -52), new Vector2(256, 26), HpColors.High);
        Image exp = Bar(panel, "ExpBar", new Vector2(104, -82), new Vector2(256, 18), ExpBlue);

        RectTransform area = Place(Rect("AreaPanel", root), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(300, 56));
        Img(area, S("UI_Panel"), true);
        RectTransform areaTextRt = Rect("AreaName", area);
        areaTextRt.anchorMin = Vector2.zero; areaTextRt.anchorMax = Vector2.one;
        areaTextRt.offsetMin = new Vector2(16, 6); areaTextRt.offsetMax = new Vector2(-16, -6);
        var areaText = Txt(areaTextRt, "Desa Awal", 26, Ink, TextAnchor.MiddleCenter);

        var so = new SerializedObject(hud);
        { var __p = so.FindProperty("partnerRoot"); if (__p != null) __p.objectReferenceValue = panel.gameObject; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "partnerRoot"); }
        { var __p = so.FindProperty("portrait"); if (__p != null) __p.objectReferenceValue = portrait; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "portrait"); }
        { var __p = so.FindProperty("nameText"); if (__p != null) __p.objectReferenceValue = nameText; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "nameText"); }
        { var __p = so.FindProperty("levelText"); if (__p != null) __p.objectReferenceValue = levelText; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "levelText"); }
        { var __p = so.FindProperty("hpFill"); if (__p != null) __p.objectReferenceValue = hp; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "hpFill"); }
        { var __p = so.FindProperty("expFill"); if (__p != null) __p.objectReferenceValue = exp; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "expFill"); }
        { var __p = so.FindProperty("areaText"); if (__p != null) __p.objectReferenceValue = areaText; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "areaText"); }
        so.ApplyModifiedPropertiesWithoutUndo();

        var installer = Object.FindAnyObjectByType<WorldInstaller>();
        P3_AnimCamInteractBuilder.Set(installer, "hud", hud);
        EditorSceneManager.SaveScene(world);
    }
}
