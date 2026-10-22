// Alat bantu dosen — "Versi Demo": menyamakan proyek dengan demo HTML (Pulau Monster Demo Playthrough).
// Menu: Pulau Monster → Build → Versi Demo. Jalankan SETELAH P1–P7.
// Semua koordinat UI ditulis dalam piksel demo (layar 320 × 180, y ke bawah); 1 piksel demo = 4 unit kanvas (1280 × 720).
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UI = P5_BattleUiBuilder;

public static class DemoBuilder
{
    const string TitlePath = "Assets/_Project/Scenes/Scn_Title.unity";
    const string BattlePath = "Assets/_Project/Scenes/Scn_Battle.unity";
    const string EndPath = "Assets/_Project/Scenes/Scn_End.unity";
    const string FontFolder = "Assets/_Project/Art/Fonts";
    const string TitleBgPath = "Assets/_Project/Art/Backgrounds/BG_Title_World.png";

    static readonly Color Ink = new Color32(18, 16, 39, 255);       // #121027
    static readonly Color Edge = new Color32(11, 9, 24, 255);       // #0B0918
    static readonly Color Navy = new Color32(28, 21, 64, 255);      // #1C1540
    static readonly Color Gold = new Color32(255, 198, 26, 255);
    static readonly Color Cream = new Color32(237, 233, 247, 255);
    static readonly Color Muted = new Color32(169, 163, 201, 255);
    static readonly Color Shade = new Color32(18, 15, 38, 255);
    static readonly Color ExpBlue = new Color32(111, 155, 255, 255);

    static Font regular, bold;

    [MenuItem("Pulau Monster/Build/Versi Demo")]
    public static void BuildAll()
    {
        ImportAssets();
        UpdateMonsterData();
        BuildBootstrap();
        BuildWorld();
        BuildBattle();
        RenderTitleWorld();
        BuildTitle();
        BuildEnd();
        SetBuildScenes();
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }

    // ================================================================ helper umum
    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    static AudioClip Sfx(string n) => Load<AudioClip>($"Assets/_Project/Audio/SFX/{n}.wav");
    static AudioClip Bgm(string n) => Load<AudioClip>($"Assets/_Project/Audio/BGM/{n}.wav");
    static MonsterData Mon(string n) => Load<MonsterData>($"{P4_MonsterBuilder.DataFolder}/Mon_{n}.asset");

    static T Get<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var __q = so.FindProperty(field); if (__q == null) { UnityEngine.Debug.LogWarning("[uji] field hilang: " + field); return; } __q.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(Object target, string field, IList<Object> values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null) { UnityEngine.Debug.LogWarning("[uji] array hilang: " + field); return; }
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetValues(Object target, System.Action<SerializedObject> edit)
    {
        var so = new SerializedObject(target);
        edit(so);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Kill(string name)
    {
        var go = GameObject.Find(name);
        if (go) Object.DestroyImmediate(go);
    }

    // ================================================================ helper UI (piksel demo)
    // Kotak di posisi (x, y) dari kiri-atas induk, ukuran w × h piksel demo.
    static RectTransform Px(string name, Transform parent, float x, float y, float w, float h)
    {
        RectTransform rt = UI.Rect(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x * 4f, -y * 4f);
        rt.sizeDelta = new Vector2(w * 4f, h * 4f);
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Panel seperti fungsi panel() demo: tepi gelap 1 px di luar, isi, garis tepi 1 px di dalam.
    static Image Panel(string name, Transform parent, float x, float y, float w, float h, string sprite = "UI_Panel_Navy")
    {
        RectTransform rt = Px(name, parent, x - 1, y - 1, w + 2, h + 2);
        Image img = UI.Img(rt, UI.S(sprite), true);
        img.raycastTarget = false;
        return img;
    }

    // Teks seperti drawText() demo: (x, y) = pojok kiri-atas (align kiri), tengah (center) atau kanan (right).
    static Text Label(string name, Transform parent, string text, float x, float y, float size, Color color,
                      bool isBold = false, TextAnchor align = TextAnchor.UpperLeft, float wrapWidth = 0f)
    {
        float w = wrapWidth > 0 ? wrapWidth : 300f;
        float left = align == TextAnchor.UpperCenter ? x - w / 2 : align == TextAnchor.UpperRight ? x - w : x;
        RectTransform rt = Px(name, parent, left, y - size * 0.15f, w, size * 1.6f);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = isBold ? bold : regular;
        t.fontStyle = FontStyle.Normal;
        t.fontSize = Mathf.RoundToInt(size * 4f);
        t.text = text;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = wrapWidth > 0 ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 0.95f;
        t.raycastTarget = false;
        var sh = rt.gameObject.AddComponent<Shadow>();
        sh.effectColor = Shade;
        sh.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    static Image Solid(string name, Transform parent, Color color)
    {
        RectTransform rt = UI.Rect(name, parent);
        Stretch(rt);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = UI.S("UI_Pixel");
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    // Bar seperti bar() demo: latar gelap + isi 1 px ke dalam (Image Filled).
    static Image Bar(string name, Transform parent, float x, float y, float w, float h, Color color)
    {
        RectTransform frame = Px(name, parent, x, y, w, h);
        var bg = frame.gameObject.AddComponent<Image>();
        bg.sprite = UI.S("UI_Pixel");
        bg.color = Edge;
        bg.raycastTarget = false;
        RectTransform fillRt = UI.Rect("Fill", frame);
        Stretch(fillRt);
        fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
        var fill = fillRt.gameObject.AddComponent<Image>();
        fill.sprite = UI.S("UI_Pixel");
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = color;
        fill.raycastTarget = false;
        return fill;
    }

    // Baris pilihan TextMenu: Text + Button (klik) di posisi piksel demo.
    static Text MenuRow(string name, Transform parent, float x, float y, float w, float size, bool isBold = true)
    {
        Text t = Label(name, parent, "Pilihan", x, y, size, Cream, isBold, TextAnchor.UpperLeft);
        t.rectTransform.sizeDelta = new Vector2(w * 4f, size * 4f * 1.5f);
        t.raycastTarget = true;
        var b = t.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav;
        return t;
    }

    static TextMenu Menu(GameObject host, IList<Text> rows, int columns = 1)
    {
        var menu = host.AddComponent<TextMenu>();
        var objs = new List<Object>();
        foreach (Text r in rows) objs.Add(r);
        SetArray(menu, "rows", objs);
        SetValues(menu, so =>
        {
            { var __p = so.FindProperty("columns"); if (__p != null) __p.intValue = columns; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "columns"); }
            { var __p = so.FindProperty("moveClip"); if (__p != null) __p.objectReferenceValue = Sfx("SFX_UI_Move"); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "moveClip"); }
            { var __p = so.FindProperty("okClip"); if (__p != null) __p.objectReferenceValue = Sfx("SFX_UI_Confirm"); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "okClip"); }
            { var __p = so.FindProperty("cancelClip"); if (__p != null) __p.objectReferenceValue = Sfx("SFX_UI_Cancel"); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "cancelClip"); }
        });
        return menu;
    }

    static Button ClickCatcher(Transform parent)
    {
        RectTransform rt = UI.Rect("ClickCatcher", parent);
        Stretch(rt);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        var b = rt.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav;
        return b;
    }

    // ================================================================ 1. aset: font OFL + sprite UI baru
    public static void ImportAssets()
    {
        AssetDatabase.Refresh();
        foreach (string f in new[] { "PixelifySans-Regular.ttf", "PixelifySans-Bold.ttf" })
        {
            var fi = (TrueTypeFontImporter)AssetImporter.GetAtPath($"{FontFolder}/{f}");
            fi.fontTextureCase = FontTextureCase.Dynamic;
            fi.fontRenderingMode = FontRenderingMode.HintedRaster;      // tepi tajam, cocok untuk font piksel
            fi.SaveAndReimport();
        }
        regular = Load<Font>($"{FontFolder}/PixelifySans-Regular.ttf");
        bold = Load<Font>($"{FontFolder}/PixelifySans-Bold.ttf");

        foreach (string n in new[] { "UI_Panel_Navy", "UI_Panel_Gold", "UI_Panel_Narr", "UI_Pixel", "UI_Ring", "UI_Alert" })
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath($"Assets/_Project/Art/UI/{n}.png");
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.spriteBorder = n.StartsWith("UI_Panel") ? new Vector4(4, 4, 4, 4) : Vector4.zero;
            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)(n == "UI_Alert" ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            ti.SetTextureSettings(s);
            ti.SaveAndReimport();
        }
    }

    static void EnsureFonts()
    {
        if (regular == null) regular = Load<Font>($"{FontFolder}/PixelifySans-Regular.ttf");
        if (bold == null) bold = Load<Font>($"{FontFolder}/PixelifySans-Bold.ttf");
    }

    // ================================================================ 2. data monster: evolusi Lv 5 / Lv 9, nama jurus demo
    public static void UpdateMonsterData()
    {
        var skills = new Dictionary<Element, string> {
            { Element.Pyro, "Semburan Api" }, { Element.Aqua, "Tembakan Air" },
            { Element.Sprout, "Daun Tajam" }, { Element.Volt, "Setrum Kilat" } };
        foreach (string guid in AssetDatabase.FindAssets("t:MonsterData", new[] { P4_MonsterBuilder.DataFolder }))
        {
            var md = Load<MonsterData>(AssetDatabase.GUIDToAssetPath(guid));
            SetValues(md, so =>
            {
                { var __p = so.FindProperty("skillName"); if (__p != null) __p.stringValue = skills[md.Element]; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "skillName"); }
                if (md.EvolvesTo == null) { var __p = so.FindProperty("evolveLevel"); if (__p != null) __p.intValue = 0; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "evolveLevel"); }
                else { var __p = so.FindProperty("evolveLevel"); if (__p != null) __p.intValue = md.EvolvesTo.EvolvesTo != null ? 5 : 9; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "evolveLevel"); }   // tahap 1 → Lv 5, tahap 2 → Lv 9
            });
            EditorUtility.SetDirty(md);
        }
        // CSV ikut disamakan (kolom evolveLevel)
        string csv = "Assets/_Project/Data/MonsterDatabase.csv";
        var lines = File.ReadAllLines(csv);
        for (int i = 1; i < lines.Length; i++)
        {
            var c = lines[i].Split(',');
            if (c.Length > 9 && c[9] == "10") c[9] = "5";
            if (c.Length > 9 && c[9] == "25") c[9] = "9";
            lines[i] = string.Join(",", c);
        }
        File.WriteAllLines(csv, lines);
        AssetDatabase.SaveAssets();
    }

    // ================================================================ 3. Scn_Bootstrap: dialog, toast, menu jeda
    public static void BuildBootstrap()
    {
        EnsureFonts();
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath, OpenSceneMode.Single);
        foreach (string n in new[] { "Canvas_Dialogue", "Canvas_Toast", "Canvas_Pause" }) Kill(n);

        // --- Dialog
        Canvas canvas = UI.MakeCanvas("Canvas_Dialogue", 20);
        var view = canvas.gameObject.AddComponent<DialogueView>();
        var runner = canvas.gameObject.AddComponent<DialogueRunner>();
        RectTransform root = UI.Rect("DialogueRoot", canvas.transform);
        Stretch(root);
        Button catcher = ClickCatcher(root);

        Image box = Panel("Box", root, 6, 128, 308, 46);
        RectTransform bodyRt = UI.Rect("Body", box.transform);
        Stretch(bodyRt);
        bodyRt.offsetMin = new Vector2(40, 8); bodyRt.offsetMax = new Vector2(-48, -32);
        var body = bodyRt.gameObject.AddComponent<Text>();
        body.font = regular; body.fontSize = 32; body.color = Cream; body.lineSpacing = 1.1f;
        body.alignment = TextAnchor.UpperLeft; body.raycastTarget = false;
        body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Overflow;
        var bsh = bodyRt.gameObject.AddComponent<Shadow>(); bsh.effectColor = Shade; bsh.effectDistance = new Vector2(2, -2);

        Image tag = Panel("SpeakerTag", root, 12, 117, 70, 13, "UI_Panel_Gold");
        Text speaker = Label("Speaker", tag.transform, "Nama", 7, 2, 8, Navy, true);
        speaker.GetComponent<Shadow>().enabled = false;

        Text hint = Label("ContinueHint", root, "▼", 302, 162, 8, Gold);

        Image choicesPanel = Panel("Choices", root, 204, 90, 108, 34);
        RectTransform cp = choicesPanel.rectTransform;
        cp.pivot = new Vector2(0, 0);
        cp.anchoredPosition = new Vector2(203 * 4, -125 * 4);              // tepi bawah tetap 4 px di atas kotak
        var rows = new List<Text>();
        for (int i = 0; i < 3; i++) rows.Add(MenuRow("Choice_" + i, cp, 9, 6 + i * 13, 100, 8, false));
        TextMenu choiceMenu = Menu(choicesPanel.gameObject, rows);
        choicesPanel.gameObject.SetActive(false);

        Set(view, "canvas", canvas);
        Set(view, "root", root.gameObject);
        Set(view, "clickCatcher", catcher);
        Set(view, "box", box);
        Set(view, "normalPanel", UI.S("UI_Panel_Navy"));
        Set(view, "narrationPanel", UI.S("UI_Panel_Narr"));
        Set(view, "speakerTag", tag.gameObject);
        Set(view, "speakerTagRect", tag.rectTransform);
        Set(view, "speakerText", speaker);
        Set(view, "bodyText", body);
        Set(view, "continueHint", hint);
        Set(view, "choicesPanel", cp);
        Set(view, "choices", choiceMenu);
        Set(runner, "view", view);
        Set(runner, "blip", Sfx("SFX_Dialog_Blip"));
        SetValues(runner, so => { var __p = so.FindProperty("charsPerSecond"); if (__p != null) __p.floatValue = 55f; });
        root.gameObject.SetActive(false);

        // --- Toast (pesan singkat)
        Canvas toastCanvas = UI.MakeCanvas("Canvas_Toast", 30);
        var toast = toastCanvas.gameObject.AddComponent<ToastView>();
        RectTransform tp = UI.Rect("Panel", toastCanvas.transform);
        tp.anchorMin = tp.anchorMax = new Vector2(0.5f, 1); tp.pivot = new Vector2(0.5f, 1);
        tp.anchoredPosition = new Vector2(0, -41 * 4); tp.sizeDelta = new Vector2(400, 18 * 4);
        UI.Img(tp, UI.S("UI_Panel_Gold"), true).raycastTarget = false;
        RectTransform tl = UI.Rect("Label", tp);
        Stretch(tl);
        var tText = tl.gameObject.AddComponent<Text>();
        tText.font = bold; tText.fontSize = 32; tText.color = Navy; tText.alignment = TextAnchor.MiddleCenter;
        tText.horizontalOverflow = HorizontalWrapMode.Overflow; tText.raycastTarget = false;
        Set(toast, "panel", tp);
        Set(toast, "label", tText);

        // --- Menu jeda (Esc)
        Canvas pauseCanvas = UI.MakeCanvas("Canvas_Pause", 40);
        var pause = pauseCanvas.gameObject.AddComponent<PauseMenu>();
        RectTransform proot = UI.Rect("PauseRoot", pauseCanvas.transform);
        Stretch(proot);
        Solid("Dim", proot, new Color(10 / 255f, 8 / 255f, 24 / 255f, 0.5f)).raycastTarget = true;
        Image pp = Panel("Panel", proot, 104, 28, 112, 62);
        Label("Title", proot, "MENU", 160, 32, 8, Gold, true, TextAnchor.UpperCenter);
        RectTransform pm = Px("Menu", proot, 0, 0, 320, 180);
        var prow = new List<Text>();
        string[] pl = { "Lanjut", "Simpan sekarang", "Kembali ke judul" };
        for (int i = 0; i < 3; i++) prow.Add(MenuRow("Item_" + i, pm, 112, 46 + i * 14, 100, 8, false));
        TextMenu pauseMenu = Menu(pm.gameObject, prow);
        Set(pause, "root", proot.gameObject);
        Set(pause, "menu", pauseMenu);
        proot.gameObject.SetActive(false);

        var boot = Object.FindAnyObjectByType<GameBootstrap>();
        Set(boot, "dialogueRunner", runner);
        Set(boot, "toast", toast);
        Set(boot, "pauseMenu", pause);
        EditorSceneManager.SaveScene(scene);
    }

    // ================================================================ 4. Scn_World
    static void ReplaceNpc<T>(string objectName, string prefabName, string displayName) where T : Npc
    {
        GameObject go = GameObject.Find(objectName);
        if (PrefabUtility.IsPartOfPrefabInstance(go))
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        foreach (Npc old in go.GetComponents<Npc>()) if (!(old is T)) Object.DestroyImmediate(old, true);
        T npc = Get<T>(go);
        var src = Load<GameObject>($"Assets/_Project/Prefabs/NPC/{prefabName}.prefab").GetComponent<Npc>();
        var from = new SerializedObject(src);
        SetValues(npc, so =>
        {
            { var __p = so.FindProperty("npcName"); if (__p != null) __p.stringValue = displayName; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "npcName"); }
            { var __p = so.FindProperty("body"); if (__p != null) __p.objectReferenceValue = go.GetComponent<SpriteRenderer>(); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "body"); }
            foreach (string f in new[] { "faceDown", "faceLeft", "faceRight", "faceUp" })
                so.FindProperty(f).objectReferenceValue = from.FindProperty(f).objectReferenceValue;
        });
    }

    static DialogueData Dlg(string name, string[,] lines)
    {
        string path = $"Assets/_Project/Data/Dialogue/{name}.asset";
        var d = Load<DialogueData>(path);
        if (d == null) { d = ScriptableObject.CreateInstance<DialogueData>(); AssetDatabase.CreateAsset(d, path); }
        SetValues(d, so =>
        {
            var lp = so.FindProperty("lines");
            lp.arraySize = lines.GetLength(0);
            for (int i = 0; i < lines.GetLength(0); i++)
            {
                lp.GetArrayElementAtIndex(i).FindPropertyRelative("speaker").stringValue = lines[i, 0];
                lp.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue = lines[i, 1];
            }
            so.FindProperty("choices").arraySize = 0;
        });
        EditorUtility.SetDirty(d);
        return d;
    }

    public static void BuildWorld()
    {
        EnsureFonts();
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        GameObject player = GameObject.Find("Player");
        var movement = player.GetComponent<PlayerMovement>();
        int charLayer = SortingLayer.NameToID("Characters");

        // --- NPC (nama dan perilaku demo)
        ReplaceNpc<StarterGiver>("Pfb_Npc_Elder", "Pfb_Npc_Elder", "Tetua Rahmat");
        ReplaceNpc<GuardNpc>("Pfb_Npc_Guard", "Pfb_Npc_Guard", "Penjaga Batas");
        ReplaceNpc<MerchantNpc>("Pfb_Npc_Merchant", "Pfb_Npc_Merchant", "Pak Dagang");
        var sari = GameObject.Find("Pfb_Npc_Girl").GetComponent<Npc>();
        DialogueData dlgSari = Dlg("Dlg_Sari", new[,] {
            { "Sari", "Pyro kuat melawan Sprout. Sprout kuat melawan Aqua. Aqua kuat melawan Pyro." },
            { "Sari", "Volt juga kuat melawan Aqua. Jurus elemen yang tepat bisa memberi damage ×1,5!" } });
        SetValues(sari, so =>
        {
            { var __p = so.FindProperty("dialogue"); if (__p != null) __p.objectReferenceValue = dlgSari; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "dialogue"); }
            { var __p = so.FindProperty("dialogueAfterFlag"); if (__p != null) __p.objectReferenceValue = null; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "dialogueAfterFlag"); }
            { var __p = so.FindProperty("flagKey"); if (__p != null) __p.stringValue = ""; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "flagKey"); }
        });

        GameObject elder = GameObject.Find("Pfb_Npc_Elder");
        var giver = elder.GetComponent<StarterGiver>();
        SetArray(giver, "starters", new List<Object> { Mon("Pyrit"), Mon("Blubi"), Mon("Sprig") });
        SetValues(giver, so => { var __p = so.FindProperty("starterLevel"); if (__p != null) __p.intValue = 3; });
        Set(giver, "receiveClip", Sfx("SFX_LevelUp"));
        Set(GameObject.Find("Pfb_Npc_Merchant").GetComponent<MerchantNpc>(), "giftClip", Sfx("SFX_Heal"));

        // --- Penjaga di celah pagar + palang tak terlihat
        Tilemap blocking = GameObject.Find("TM_Blocking").GetComponent<Tilemap>();
        GameObject guard = GameObject.Find("Pfb_Npc_Guard");
        guard.transform.position = new Vector3(20f, 11.4f, 0f);
        Vector2 aside = blocking.HasTile(new Vector3Int(22, 10, 0)) ? new Vector2(17.5f, 10.4f) : new Vector2(22.5f, 10.4f);
        SetValues(guard.GetComponent<GuardNpc>(), so => { var __p = so.FindProperty("asidePosition"); if (__p != null) __p.vector2Value = aside; });

        Tile fence = Load<Tile>($"{P2_WorldBuilder.TileFolder}/T_Fence_H.asset");
        for (int x = 1; x <= 38; x++)
        {
            if (x == 19 || x == 20) continue;
            var cell = new Vector3Int(x, 11, 0);
            if (!blocking.HasTile(cell)) blocking.SetTile(cell, fence);
        }
        Kill("Gate_Hutan");
        var gate = new GameObject("Gate_Hutan");
        gate.transform.position = new Vector3(20f, 11.5f, 0f);
        gate.AddComponent<BoxCollider2D>().size = new Vector2(2f, 1f);
        var gateToggle = gate.AddComponent<FlagToggle>();
        SetArray(gateToggle, "hideWhenSet", new List<Object> { gate });
        SetArray(gateToggle, "showWhenSet", new List<Object>());

        // --- Partikel, partner pengikut, daun di rumput tinggi
        Kill("PixelBurst"); Kill("Partner");
        var burstGo = new GameObject("PixelBurst");
        var burst = burstGo.AddComponent<PixelBurst>();
        Set(burst, "pixel", UI.S("UI_Pixel"));

        var partnerGo = new GameObject("Partner");
        var psr = partnerGo.AddComponent<SpriteRenderer>();
        psr.sortingLayerID = charLayer;
        psr.spriteSortPoint = SpriteSortPoint.Pivot;
        var follower = partnerGo.AddComponent<PartnerFollower>();
        Set(follower, "body", psr);

        var rustle = Get<GrassRustle>(player);
        Set(rustle, "tallGrass", GameObject.Find("TM_TallGrass").GetComponent<Tilemap>());
        Set(rustle, "movement", movement);
        Set(rustle, "burst", burst);

        // --- Monster liar sesuai demo: barat Lv 2–4 (tahap 1), timur Lv 5–7 (tahap 2), boss Raijugar Lv 9
        Kill("WildMonsters");
        var parent = new GameObject("WildMonsters");
        var prefab = Load<GameObject>("Assets/_Project/Prefabs/Monsters/Pfb_WildMonster.prefab");
        var crystalLights = new List<Object>();
        void Spawn(string species, Vector2 pos, int min, int max, bool boss = false)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            inst.name = (boss ? "Boss_" : "Wild_") + species;
            inst.transform.SetParent(parent.transform);
            inst.transform.position = pos;
            MonsterData d = Mon(species);
            inst.GetComponent<SpriteRenderer>().sprite = d.Frames[0];
            inst.GetComponent<Animator>().runtimeAnimatorController =
                Load<RuntimeAnimatorController>($"Assets/_Project/Art/Monsters/Animations/AOC_Mon_{species}.overrideController");
            var alert = new GameObject("Alert");
            alert.transform.SetParent(inst.transform, false);
            alert.transform.localPosition = new Vector3(0, boss ? 2.2f : 1.3f, 0);
            var asr = alert.AddComponent<SpriteRenderer>();
            asr.sprite = UI.S("UI_Alert");
            asr.sortingLayerID = charLayer;
            asr.sortingOrder = 50;
            alert.SetActive(false);
            SetValues(inst.GetComponent<WildMonster>(), so =>
            {
                { var __p = so.FindProperty("species"); if (__p != null) __p.objectReferenceValue = d; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "species"); }
                so.FindProperty("levelRange").vector2IntValue = new Vector2Int(min, max);
                { var __p = so.FindProperty("isBoss"); if (__p != null) __p.boolValue = boss; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "isBoss"); }
                { var __p = so.FindProperty("stationary"); if (__p != null) __p.boolValue = boss; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "stationary"); }
                { var __p = so.FindProperty("wanderSpeed"); if (__p != null) __p.floatValue = 1.125f; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "wanderSpeed"); }
                { var __p = so.FindProperty("chaseSpeed"); if (__p != null) __p.floatValue = 2.5f; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "chaseSpeed"); }
                { var __p = so.FindProperty("sightRadius"); if (__p != null) __p.floatValue = 3.25f; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "sightRadius"); }
                { var __p = so.FindProperty("giveUpRadius"); if (__p != null) __p.floatValue = 6.25f; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "giveUpRadius"); }
                { var __p = so.FindProperty("alert"); if (__p != null) __p.objectReferenceValue = alert; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "alert"); }
            });
            if (boss)
            {
                var glow = new GameObject("BossLight");
                glow.transform.SetParent(inst.transform, false);
                glow.transform.localPosition = new Vector3(0, 0.9f, 0);
                var l = glow.AddComponent<Light2D>();
                l.lightType = Light2D.LightType.Point;
                l.pointLightInnerRadius = 0.3f; l.pointLightOuterRadius = 1.8f;
                l.color = new Color32(255, 224, 102, 255);
                l.intensity = 0f;
                crystalLights.Add(l);
            }
        }
        Spawn("Sprig", new Vector2(5.5f, 15.5f), 2, 4);
        Spawn("Zapi", new Vector2(8.5f, 17.5f), 2, 4);
        Spawn("Blubi", new Vector2(7f, 14.5f), 2, 4);
        Spawn("Pyrit", new Vector2(9.5f, 16f), 2, 4);
        Spawn("Pyrago", new Vector2(26.5f, 15.5f), 5, 7);
        Spawn("Finnox", new Vector2(30f, 17.5f), 5, 7);
        Spawn("Voltcat", new Vector2(28f, 14.5f), 5, 7);
        Spawn("Bloomlet", new Vector2(31.5f, 15.5f), 5, 7);
        Spawn("Raijugar", new Vector2(30.5f, 31.5f), 9, 9, true);

        // --- Gua: lebih gelap (0,15), cahaya pemain radius 58 px, kristal bercahaya
        Kill("CaveLights");
        var caveLights = new GameObject("CaveLights");
        foreach (var c in new[] { new Vector2(24, 27), new Vector2(36, 28), new Vector2(26, 32), new Vector2(35, 32), new Vector2(33, 27) })
        {
            var go = new GameObject($"Crystal_{c.x}_{c.y}");
            go.transform.SetParent(caveLights.transform);
            go.transform.position = new Vector3(c.x + 0.5f, c.y + 0.5f, 0);
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.pointLightInnerRadius = 0.3f; l.pointLightOuterRadius = 1.9f;
            l.color = new Color32(255, 242, 168, 255);
            l.intensity = 0f;
            crystalLights.Add(l);
        }
        var lamp = player.transform.Find("PlayerLight").GetComponent<Light2D>();
        lamp.pointLightInnerRadius = 1.2f; lamp.pointLightOuterRadius = 3.6f;
        lamp.color = new Color32(255, 240, 210, 255);
        var zone = GameObject.Find("Area_Gua").GetComponent<LightZone>();
        SetArray(zone, "extraLights", crystalLights);
        SetValues(zone, so => { var __p = so.FindProperty("insideIntensity"); if (__p != null) __p.floatValue = 0.15f; });

        // --- HUD gaya demo
        Kill("Canvas_HUD");
        Canvas canvas = UI.MakeCanvas("Canvas_HUD", 5);
        Transform root = canvas.transform;
        var hud = canvas.gameObject.AddComponent<PartnerHud>();
        Panel("AreaPanel", root, 226, 4, 90, 24);
        Text areaText = Label("AreaName", root, "Desa Awal", 232, 8, 8, Gold, true);
        Text potionText = Label("Potions", root, "Ramuan ×3", 232, 17, 7, Cream);
        Image partner = Panel("PartnerPanel", root, 4, 4, 112, 34);
        Transform pr = partner.transform;                       // anak panel: koordinat relatif (panel mulai di 3,3)
        RectTransform portRt = Px("Portrait", pr, 5, 6, 24, 24);
        var portrait = portRt.gameObject.AddComponent<Image>();
        portrait.preserveAspect = true; portrait.raycastTarget = false;
        Text nameText = Label("Name", pr, "Partner", 33, 4, 8, Cream, true);
        Text levelText = Label("Level", pr, "Lv1", 107, 4, 8, Gold, true, TextAnchor.UpperRight, 40);
        Image hp = Bar("HpBar", pr, 33, 15, 74, 6, HpColors.High);
        Text hpText = Label("HpText", pr, "0/0", 107, 22, 6, Muted, false, TextAnchor.UpperRight, 60);
        Image exp = Bar("ExpBar", pr, 33, 29, 74, 3, ExpBlue);
        Set(hud, "partnerRoot", partner.gameObject);
        Set(hud, "portrait", portrait);
        Set(hud, "nameText", nameText);
        Set(hud, "levelText", levelText);
        Set(hud, "hpFill", hp);
        Set(hud, "hpText", hpText);
        Set(hud, "expFill", exp);
        Set(hud, "areaText", areaText);
        Set(hud, "potionText", potionText);

        var installer = Object.FindAnyObjectByType<WorldInstaller>();
        Set(installer, "hud", hud);
        Set(installer, "follower", follower);
        Set(installer, "burst", burst);
        foreach (var stone in Object.FindObjectsByType<SaveStone>(FindObjectsSortMode.None)) Set(stone, "saveClip", Sfx("SFX_Save"));
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
    }

    // ================================================================ 5. Scn_Battle (tata letak demo 320 × 180)
    public static void BuildBattle()
    {
        EnsureFonts();
        Scene scene = EditorSceneManager.OpenScene(BattlePath, OpenSceneMode.Single);
        Kill("Canvas_Battle"); Kill("PixelBurst");

        var enemy = GameObject.Find("EnemyMon").GetComponent<SpriteRenderer>();
        var player = GameObject.Find("PlayerMon").GetComponent<SpriteRenderer>();
        enemy.transform.position = new Vector3(4.5f, -0.125f, 0);       // cx 232, kaki di y 92
        player.transform.position = new Vector3(-4.5f, -2.375f, 0);     // cx 88, kaki di y 128

        Canvas canvas = UI.MakeCanvas("Canvas_Battle", 10);
        Transform root = canvas.transform;

        // panel lawan
        Image foe = Panel("EnemyPanel", root, 8, 8, 128, 30);
        Transform fr = foe.transform;                                    // koordinat relatif (panel mulai di 7,7)
        Text foeName = Label("Name", fr, "Nama", 7, 5, 8, Cream, true);
        Text foeLv = Label("Level", fr, "Lv1", 123, 5, 8, Gold, true, TextAnchor.UpperRight, 40);
        Text foeEl = Label("Element", fr, "Volt", 7, 18, 6, Muted);
        Image foeHp = Bar("HpBar", fr, 33, 17, 90, 6, HpColors.High);
        var enemyPanel = foe.gameObject.AddComponent<MonsterPanel>();
        Set(enemyPanel, "body", enemy);
        Set(enemyPanel, "nameText", foeName);
        Set(enemyPanel, "levelText", foeLv);
        Set(enemyPanel, "elementText", foeEl);
        Set(enemyPanel, "hpFill", foeHp);

        // panel pemain
        Image me = Panel("PlayerPanel", root, 186, 92, 128, 36);
        Transform mr = me.transform;                                     // panel mulai di 185,91
        Text meName = Label("Name", mr, "Nama", 7, 5, 8, Cream, true);
        Text meLv = Label("Level", mr, "Lv1", 123, 5, 8, Gold, true, TextAnchor.UpperRight, 40);
        Image meHp = Bar("HpBar", mr, 7, 17, 116, 6, HpColors.High);
        Text meHpText = Label("HpText", mr, "0/0", 123, 24, 6, Muted, false, TextAnchor.UpperRight, 60);
        Image meExp = Bar("ExpBar", mr, 7, 32, 116, 3, ExpBlue);
        var playerPanel = me.gameObject.AddComponent<MonsterPanel>();
        Set(playerPanel, "body", player);
        SetValues(playerPanel, so => { var __p = so.FindProperty("faceRight"); if (__p != null) __p.boolValue = true; });
        Set(playerPanel, "nameText", meName);
        Set(playerPanel, "levelText", meLv);
        Set(playerPanel, "hpText", meHpText);
        Set(playerPanel, "hpFill", meHp);
        Set(playerPanel, "expFill", meExp);

        // pesan + menu
        Panel("MessagePanel", root, 0, 132, 196, 48);
        Panel("MenuPanel", root, 196, 132, 124, 48);
        Text message = Label("Message", root, "…", 9, 140, 8, Cream, false, TextAnchor.UpperLeft, 180);
        Text hint = Label("SkillHint", root, "Jurus elemen: -", 9, 158, 7, Muted);
        RectTransform menuRt = Px("Menu", root, 0, 0, 320, 180);
        var rows = new List<Text>();
        for (int i = 0; i < 4; i++) rows.Add(MenuRow("Btn_" + i, menuRt, 204 + (i % 2) * 58, 140 + (i / 2) * 20, 56, 8));
        TextMenu menu = Menu(menuRt.gameObject, rows, 2);

        // efek layar: cincin evolusi, kilat putih, tirai pembuka
        var rings = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            RectTransform r = Px("Ring_" + i, root, 88, 100, 10, 10);
            r.pivot = new Vector2(0.5f, 0.5f);
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = UI.S("UI_Ring"); img.color = new Color(1f, 0.88f, 0.4f, 0.8f); img.raycastTarget = false;
            rings.Add(r);
        }
        Image flash = Solid("Flash", root, new Color(1, 1, 1, 0));
        RectTransform Wipe(string n, float anchorY)
        {
            Image w = Solid(n, root, Edge);
            RectTransform rt = w.rectTransform;
            rt.anchorMin = new Vector2(0, anchorY); rt.anchorMax = new Vector2(1, anchorY); rt.pivot = new Vector2(0.5f, anchorY);
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(0, 0);
            return rt;
        }
        RectTransform wipeTop = Wipe("WipeTop", 1f);
        RectTransform wipeBottom = Wipe("WipeBottom", 0f);

        var view = Get<BattleView>(canvas.gameObject);
        Set(view, "messageText", message);
        Set(view, "hintText", hint);
        Set(view, "menu", menu);
        Set(view, "playerPanel", playerPanel);
        Set(view, "enemyPanel", enemyPanel);
        Set(view, "wipeTop", wipeTop);
        Set(view, "wipeBottom", wipeBottom);
        Set(view, "flash", flash);
        SetArray(view, "rings", rings);

        // partikel + feel + audio + latar
        var burstGo = new GameObject("PixelBurst");
        var burst = burstGo.AddComponent<PixelBurst>();
        Set(burst, "pixel", UI.S("UI_Pixel"));
        SetValues(burst, so => { { var __p = so.FindProperty("sortingLayer"); if (__p != null) __p.stringValue = "Default"; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "sortingLayer"); } { var __p = so.FindProperty("sortingOrder"); if (__p != null) __p.intValue = 10; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "sortingOrder"); } });
        var feel = Object.FindAnyObjectByType<BattleFeel>();
        Set(feel, "burst", burst);

        var installer = Object.FindAnyObjectByType<BattleInstaller>();
        Set(installer, "view", view);
        Set(installer, "background", GameObject.Find("Background").GetComponent<SpriteRenderer>());
        Set(installer, "forestBackground", Load<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Forest_320x180.png"));
        Set(installer, "caveBackground", Load<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Cave_320x180.png"));
        var audio = installer.GetComponent<BattleAudio>();
        Set(audio, "criticalClip", Sfx("SFX_Hit_Critical"));
        Set(audio, "missClip", Sfx("SFX_Miss"));
        Set(audio, "levelClip", Sfx("SFX_LevelUp"));
        EditorSceneManager.SaveScene(scene);
    }

    // ================================================================ 6. gambar peta untuk latar layar judul
    public static void RenderTitleWorld()
    {
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);
        const int W = 440, H = 180;                              // 320 px layar + 2 × 60 px geser
        var camGo = new GameObject("TitleShotCamera");
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<UniversalAdditionalCameraData>();
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Ink;
        cam.aspect = W / (float)H;
        camGo.transform.position = new Vector3(20f, 30.375f, -10f);   // bagian atas peta: danau + gua
        var rt = new RenderTexture(W, H, 24) { filterMode = FilterMode.Point };
        var request = new RenderPipeline.StandardRequest { destination = rt };
        if (RenderPipeline.SupportsRenderRequest(cam, request)) RenderPipeline.SubmitRenderRequest(cam, request);
        else { cam.targetTexture = rt; cam.Render(); cam.targetTexture = null; }

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        File.WriteAllBytes(TitleBgPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(camGo);
        EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);   // buang perubahan (tidak disimpan)

        AssetDatabase.ImportAsset(TitleBgPath);
        var ti = (TextureImporter)AssetImporter.GetAtPath(TitleBgPath);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 16;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.SaveAndReimport();
    }

    // ================================================================ 7. Scn_Title
    public static void BuildTitle()
    {
        EnsureFonts();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var installer = new GameObject("TitleInstaller").AddComponent<TitleInstaller>();
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Ink;
        camGo.transform.position = new Vector3(0, 0, -10);

        Canvas canvas = UI.MakeCanvas("Canvas_Title", 0);
        Transform root = canvas.transform;
        RectTransform bg = UI.Rect("WorldBackground", root);
        bg.anchorMin = bg.anchorMax = bg.pivot = new Vector2(0.5f, 0.5f);
        bg.sizeDelta = new Vector2(440 * 4, 180 * 4);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.sprite = Load<Sprite>(TitleBgPath);
        bgImg.raycastTarget = false;
        Solid("Overlay", root, new Color(18 / 255f, 16 / 255f, 39 / 255f, 0.55f));

        Label("Pulau", root, "PULAU", 160, 30, 16, Cream, true, TextAnchor.UpperCenter);
        Label("Monster", root, "MONSTER", 160, 46, 32, Gold, true, TextAnchor.UpperCenter);

        string[] starters = { "Pyrit", "Blubi", "Sprig", "Zapi" };
        var imgs = new List<Object>(); var data = new List<Object>();
        for (int i = 0; i < 4; i++)
        {
            RectTransform s = Px("Starter_" + starters[i], root, 160 - 70 + i * 36, 86, 32, 32);
            var img = s.gameObject.AddComponent<Image>();
            img.sprite = Mon(starters[i]).Frames[0];
            img.raycastTarget = false;
            imgs.Add(img); data.Add(Mon(starters[i]));
        }

        RectTransform menuRt = Px("Menu", root, 0, 0, 320, 180);
        var rows = new List<Text>();
        for (int i = 0; i < 3; i++) rows.Add(MenuRow("Option_" + i, menuRt, 130, 124 + i * 14, 80, 9));
        TextMenu menu = Menu(menuRt.gameObject, rows);
        Label("Hint", root, "Enter / Spasi untuk mulai · klik juga bisa", 160, 164, 7, Muted, false, TextAnchor.UpperCenter);

        Set(installer, "menu", menu);
        Set(installer, "worldBackground", bg);
        SetArray(installer, "starterImages", imgs);
        SetArray(installer, "starterData", data);
        Set(installer, "titleMusic", Bgm("BGM_Title"));
        EditorSceneManager.SaveScene(scene, TitlePath);
    }

    // ================================================================ 8. Scn_End ("DEMO SELESAI")
    public static void BuildEnd()
    {
        EnsureFonts();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var installer = new GameObject("EndInstaller").AddComponent<EndInstaller>();
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Ink;
        camGo.transform.position = new Vector3(0, 0, -10);

        Canvas canvas = UI.MakeCanvas("Canvas_End", 0);
        Transform root = canvas.transform;
        Solid("Background", root, Ink);
        Label("Title", root, "DEMO SELESAI", 160, 22, 20, Gold, true, TextAnchor.UpperCenter);
        RectTransform pRt = Px("Partner", root, 128, 50, 64, 64);
        var partner = pRt.gameObject.AddComponent<Image>();
        partner.raycastTarget = false;
        Text name = Label("PartnerText", root, "Nama  ·  Lv 1", 160, 120, 9, Cream, true, TextAnchor.UpperCenter);
        Text time = Label("Playtime", root, "Waktu bermain 0 menit 0 detik", 160, 134, 8, Muted, false, TextAnchor.UpperCenter);
        Label("Hint", root, "Tekan Enter untuk kembali ke judul", 160, 158, 8, Cream, false, TextAnchor.UpperCenter);
        Button catcher = ClickCatcher(root);

        Set(installer, "partnerImage", partner);
        Set(installer, "partnerText", name);
        Set(installer, "timeText", time);
        Set(installer, "clickCatcher", catcher);
        EditorSceneManager.SaveScene(scene, EndPath);
    }

    static void SetBuildScenes()
    {
        string[] order = { P2_WorldBuilder.BootstrapPath, TitlePath, P2_WorldBuilder.WorldPath, BattlePath, EndPath };
        var list = new List<EditorBuildSettingsScene>();
        foreach (string p in order) list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();
        EditorSceneManager.playModeStartScene = Load<SceneAsset>(P2_WorldBuilder.BootstrapPath);
    }
}
