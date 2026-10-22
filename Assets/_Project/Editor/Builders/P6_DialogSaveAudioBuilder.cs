// Alat bantu dosen P6: dialog, story flag, Batu Simpan, layar judul, dan audio.
// Hasilnya sama dengan langkah manual di Docs/P06_Dialog_Save_Audio.md.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UI = P5_BattleUiBuilder;

public static class P6_DialogSaveAudioBuilder
{
    const string DlgFolder = "Assets/_Project/Data/Dialogue";
    const string TitlePath = "Assets/_Project/Scenes/Scn_Title.unity";
    const string BattlePath = "Assets/_Project/Scenes/Scn_Battle.unity";

    [MenuItem("Pulau Monster/Build/P6 Dialog, Save & Audio")]
    public static void BuildAll()
    {
        ImportSprites();
        ImportAudio();
        CreateDialogues();
        BuildBootstrap();
        BuildTitle();
        BuildWorld();
        BuildBattle();
        SetBuildScenes();
        EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath);
    }

    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    static AudioClip Bgm(string n) => Load<AudioClip>($"Assets/_Project/Audio/BGM/{n}.wav");
    static AudioClip Sfx(string n) => Load<AudioClip>($"Assets/_Project/Audio/SFX/{n}.wav");

    // GetComponent di Editor bisa mengembalikan "fake null": jangan memakai operator null-coalescing
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

    static void SetArray(Object target, string field, IList<Object> values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null) { UnityEngine.Debug.LogWarning("[uji] array hilang: " + field); return; }
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------------------------------------------------------- aset
    public static void ImportSprites()
    {
        UI.ImportUiSprites();                                       // UI_Exclaim, UI_Arrow_Down ikut
        var ti = (TextureImporter)AssetImporter.GetAtPath("Assets/_Project/Art/UI/Prop_SaveStone.png");
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 16;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        ti.SetTextureSettings(settings);
        ti.SaveAndReimport();
    }

    // BGM: Streaming + Vorbis; SFX: Decompress On Load (pendek, sering diputar)
    public static void ImportAudio()
    {
        foreach (string dir in new[] { "BGM", "SFX" })
            foreach (string f in Directory.GetFiles($"Assets/_Project/Audio/{dir}", "*.wav"))
            {
                var ai = (AudioImporter)AssetImporter.GetAtPath(f.Replace('\\', '/'));
                var s = ai.defaultSampleSettings;
                s.loadType = dir == "BGM" ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                s.compressionFormat = dir == "BGM" ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
                s.quality = 0.6f;
                ai.defaultSampleSettings = s;
                ai.forceToMono = true;
                ai.SaveAndReimport();
            }
    }

    static DialogueData Dlg(string name, string[,] lines, params string[] choices)
    {
        string path = $"{DlgFolder}/{name}.asset";
        var d = Load<DialogueData>(path);
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(d, path);
        }
        var so = new SerializedObject(d);
        var lp = so.FindProperty("lines");
        lp.arraySize = lines.GetLength(0);
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            lp.GetArrayElementAtIndex(i).FindPropertyRelative("speaker").stringValue = lines[i, 0];
            lp.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue = lines[i, 1];
        }
        var cp = so.FindProperty("choices");
        cp.arraySize = choices.Length;
        for (int i = 0; i < choices.Length; i++) cp.GetArrayElementAtIndex(i).stringValue = choices[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(d);
        return d;
    }

    public static void CreateDialogues()
    {
        Directory.CreateDirectory(DlgFolder);
        const string T = "Tetua Rahmat";
        Dlg("Dlg_Tetua_Starter", new[,] {
            { T, "Selamat datang di Pulau Monster, anak muda." },
            { T, "Monster liar tidak akan menantangmu selama kamu belum punya partner." },
            { T, "Pilihlah satu partner pertamamu." } },
            "Pyrit (api)", "Blubi (air)", "Sprig (daun)");
        Dlg("Dlg_Tetua_After", new[,] {
            { T, "Rawat partnermu baik-baik." },
            { T, "Batu bercahaya di samping rumahku memulihkan partner dan menyimpan perjalananmu." } });
        Dlg("Dlg_Sari", new[,] { { "Sari", "Hutan Rimbun di utara penuh monster liar. Kata Tetua, jangan ke sana sendirian!" } });
        Dlg("Dlg_Sari_After", new[,] { { "Sari", "Wah, partnermu lucu sekali! Coba latih di rumput tinggi." } });
        Dlg("Dlg_Penjaga", new[,] { { "Penjaga Bayu", "Tanpa partner, lebih aman tetap di desa." } });
        Dlg("Dlg_Penjaga_After", new[,] {
            { "Penjaga Bayu", "Gua Petir di timur laut dijaga Infernox." },
            { "Penjaga Bayu", "Kuatkan dulu partnermu sebelum ke sana." } });
        Dlg("Dlg_Pedagang", new[,] { { "Pedagang Ucok", "Ramuan? Stokku sedang kosong. Datang lagi nanti, ya!" } });
        Dlg("Dlg_Save_Ok", new[,] { { "", "Batu itu bersinar. Partner pulih dan perjalanan tersimpan." } });
        Dlg("Dlg_Save_Fail", new[,] { { "", "Batu itu redup... Penyimpanan gagal." } });
        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- Scn_Bootstrap: dialog + audio
    public static void BuildBootstrap()
    {
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.BootstrapPath, OpenSceneMode.Single);
        foreach (string n in new[] { "Canvas_Dialogue", "AudioService" })
        {
            var old = GameObject.Find(n);
            if (old) Object.DestroyImmediate(old);
        }

        // --- Kotak dialog
        Canvas canvas = UI.MakeCanvas("Canvas_Dialogue", 20);
        Transform root = canvas.transform;
        var view = canvas.gameObject.AddComponent<DialogueView>();
        var runner = canvas.gameObject.AddComponent<DialogueRunner>();

        RectTransform box = UI.Rect("DialogueRoot", root);
        box.anchorMin = new Vector2(0, 0); box.anchorMax = new Vector2(1, 0); box.pivot = new Vector2(0.5f, 0);
        box.offsetMin = new Vector2(24, 20); box.offsetMax = new Vector2(-24, 200);
        Image bg = UI.Img(box, UI.S("UI_Panel"), true);
        var boxButton = box.gameObject.AddComponent<Button>();
        boxButton.targetGraphic = bg;
        boxButton.transition = Selectable.Transition.None;
        var nav = boxButton.navigation; nav.mode = Navigation.Mode.None; boxButton.navigation = nav;

        RectTransform tag = UI.Place(UI.Rect("SpeakerTag", box), new Vector2(0, 1), new Vector2(0, 0), new Vector2(28, -12), new Vector2(300, 52));
        UI.Img(tag, UI.S("UI_Panel_Dark"), true);
        RectTransform tagText = UI.Rect("Speaker", tag);
        tagText.anchorMin = Vector2.zero; tagText.anchorMax = Vector2.one;
        tagText.offsetMin = new Vector2(16, 4); tagText.offsetMax = new Vector2(-16, -4);
        Text speaker = UI.Txt(tagText, "Nama", 26, UI.Gold, TextAnchor.MiddleLeft);

        RectTransform bodyRt = UI.Rect("Body", box);
        bodyRt.anchorMin = Vector2.zero; bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(36, 24); bodyRt.offsetMax = new Vector2(-36, -44);
        Text body = UI.Txt(bodyRt, "…", 30, UI.Ink, TextAnchor.UpperLeft);
        body.fontStyle = FontStyle.Normal;

        RectTransform hint = UI.Place(UI.Rect("ContinueHint", box), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-28, 18), new Vector2(28, 20));
        UI.Img(hint, UI.S("UI_Arrow_Down"), false);

        // 3 tombol pilihan di kanan atas kotak
        RectTransform choices = UI.Place(UI.Rect("Choices", box), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 12), new Vector2(320, 3 * 60 + 2 * 8));
        var layout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8; layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = true;
        var buttons = new List<Object>(); var labels = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            RectTransform b = UI.Rect("Choice_" + i, choices);
            Image img = UI.Img(b, UI.S("UI_Button"), true);
            var btn = b.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState { highlightedSprite = UI.S("UI_Button_Selected"), selectedSprite = UI.S("UI_Button_Selected"), pressedSprite = UI.S("UI_Button_Selected") };
            RectTransform lab = UI.Rect("Label", b);
            lab.anchorMin = Vector2.zero; lab.anchorMax = Vector2.one;
            lab.offsetMin = new Vector2(20, 4); lab.offsetMax = new Vector2(-12, -4);
            labels.Add(UI.Txt(lab, "Pilihan", 26, UI.Ink, TextAnchor.MiddleLeft));
            buttons.Add(btn);
            b.gameObject.SetActive(false);
        }

        Set(view, "root", box.gameObject);
        Set(view, "speakerTag", tag.gameObject);
        Set(view, "speakerText", speaker);
        Set(view, "bodyText", body);
        Set(view, "continueHint", hint.gameObject);
        Set(view, "boxButton", boxButton);
        SetArray(view, "choiceButtons", buttons);
        SetArray(view, "choiceLabels", labels);
        Set(runner, "view", view);
        Set(runner, "blip", Sfx("SFX_Dialog_Blip"));
        box.gameObject.SetActive(false);

        // --- AudioService + AudioListener (satu-satunya di game)
        var audioGo = new GameObject("AudioService");
        audioGo.AddComponent<AudioListener>();
        var service = audioGo.AddComponent<AudioService>();
        AudioSource Src(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(audioGo.transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }
        Set(service, "musicA", Src("MusicA", true));
        Set(service, "musicB", Src("MusicB", true));
        Set(service, "sfx", Src("Sfx", false));

        var boot = Object.FindAnyObjectByType<GameBootstrap>();
        var so = new SerializedObject(boot);
        { var __p = so.FindProperty("firstScene"); if (__p != null) __p.stringValue = "Scn_Title"; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "firstScene"); }
        { var __p = so.FindProperty("dialogueRunner"); if (__p != null) __p.objectReferenceValue = runner; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "dialogueRunner"); }
        { var __p = so.FindProperty("audioService"); if (__p != null) __p.objectReferenceValue = service; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "audioService"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Scn_Title
    public static void BuildTitle()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var installerGo = new GameObject("TitleInstaller");
        var installer = installerGo.AddComponent<TitleInstaller>();

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = UI.Ink;
        camGo.transform.position = new Vector3(0, 0, -10);

        Canvas canvas = UI.MakeCanvas("Canvas_Title", 0);
        Transform root = canvas.transform;
        RectTransform bgRt = UI.Rect("Background", root);
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bgRt.gameObject.AddComponent<Image>();
        bgImg.sprite = Load<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Grassland_320x180.png");
        bgImg.raycastTarget = false;

        RectTransform title = UI.Place(UI.Rect("Title", root), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(900, 120));
        Text titleText = UI.Txt(title, "PULAU MONSTER", 84, UI.Gold, TextAnchor.MiddleCenter);
        var outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = UI.Ink; outline.effectDistance = new Vector2(5, -5);

        RectTransform menu = UI.Place(UI.Rect("Menu", root), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220, -60), new Vector2(380, 230));
        UI.Img(menu, UI.S("UI_Panel_Dark"), true);
        Button MakeButton(string name, string label, float y)
        {
            RectTransform b = UI.Place(UI.Rect(name, menu), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(320, 64));
            Image img = UI.Img(b, UI.S("UI_Button"), true);
            var btn = b.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState { highlightedSprite = UI.S("UI_Button_Selected"), selectedSprite = UI.S("UI_Button_Selected"), pressedSprite = UI.S("UI_Button_Selected") };
            RectTransform lab = UI.Rect("Label", b);
            lab.anchorMin = Vector2.zero; lab.anchorMax = Vector2.one;
            UI.Txt(lab, label, 30, UI.Ink, TextAnchor.MiddleCenter);
            return btn;
        }
        Button cont = MakeButton("Btn_Lanjutkan", "Lanjutkan", -24);
        Button newGame = MakeButton("Btn_MulaiBaru", "Mulai Baru", -100);
        RectTransform infoRt = UI.Place(UI.Rect("SaveInfo", menu), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(340, 36));
        Text info = UI.Txt(infoRt, "Belum ada simpanan", 22, UI.Cream, TextAnchor.MiddleCenter);
        info.fontStyle = FontStyle.Normal;

        // Versi demo: panel slider volume dihapus (tombol M = suara mati/nyala). Lihat DemoBuilder.

        Set(installer, "continueButton", cont);
        Set(installer, "newGameButton", newGame);
        Set(installer, "saveInfo", info);
        Set(installer, "titleMusic", Bgm("BGM_Title"));
        EditorSceneManager.SaveScene(scene, TitlePath);
    }

    // ---------------------------------------------------------------- Scn_World
    public static void BuildWorld()
    {
        Scene scene = EditorSceneManager.OpenScene(P2_WorldBuilder.WorldPath, OpenSceneMode.Single);

        // AudioListener hanya di AudioService (Bootstrap)
        var listener = Object.FindAnyObjectByType<AudioListener>();
        if (listener) Object.DestroyImmediate(listener);

        DialogueData D(string n) => Load<DialogueData>($"{DlgFolder}/{n}.asset");
        void NpcDialog(string go, string first, string after)
        {
            var npc = GameObject.Find(go).GetComponent<Npc>();
            var so = new SerializedObject(npc);
            { var __p = so.FindProperty("dialogue"); if (__p != null) __p.objectReferenceValue = D(first); else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "dialogue"); }
            { var __p = so.FindProperty("dialogueAfterFlag"); if (__p != null) __p.objectReferenceValue = after != null ? D(after) : null; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "dialogueAfterFlag"); }
            { var __p = so.FindProperty("flagKey"); if (__p != null) __p.stringValue = after != null ? StoryFlags.HasStarter : ""; else UnityEngine.Debug.LogWarning("[uji] field hilang: " + "flagKey"); }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        NpcDialog("Pfb_Npc_Girl", "Dlg_Sari", "Dlg_Sari_After");
        NpcDialog("Pfb_Npc_Guard", "Dlg_Penjaga", "Dlg_Penjaga_After");
        NpcDialog("Pfb_Npc_Merchant", "Dlg_Pedagang", null);

        // Tetua: Npc diganti StarterGiver + tanda "!" yang hilang setelah has_starter
        GameObject elder = GameObject.Find("Pfb_Npc_Elder");
        if (PrefabUtility.IsPartOfPrefabInstance(elder))          // komponen Npc akan dibuang: lepas dari prefab
            PrefabUtility.UnpackPrefabInstance(elder, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        var oldNpc = elder.GetComponent<Npc>();
        if (oldNpc) Object.DestroyImmediate(oldNpc, true);
        var giver = Get<StarterGiver>(elder);
        Set(giver, "offerDialogue", D("Dlg_Tetua_Starter"));
        Set(giver, "afterDialogue", D("Dlg_Tetua_After"));
        SetArray(giver, "starters", new List<Object> {
            Load<MonsterData>("Assets/_Project/Data/Monsters/Mon_Pyrit.asset"),
            Load<MonsterData>("Assets/_Project/Data/Monsters/Mon_Blubi.asset"),
            Load<MonsterData>("Assets/_Project/Data/Monsters/Mon_Sprig.asset") });

        Transform marker = elder.transform.Find("Marker");
        if (marker == null)
        {
            marker = new GameObject("Marker").transform;
            marker.SetParent(elder.transform, false);
        }
        marker.localPosition = new Vector3(0, 1.2f, 0);
        var mr = Get<SpriteRenderer>(marker.gameObject);
        mr.sprite = UI.S("UI_Exclaim");
        mr.sortingLayerID = elder.GetComponent<SpriteRenderer>().sortingLayerID;   // layer Characters, bukan Default
        mr.sortingOrder = 50;
        var toggle = Get<FlagToggle>(elder);
        SetArray(toggle, "hideWhenSet", new List<Object> { marker.gameObject });
        SetArray(toggle, "showWhenSet", new List<Object>());

        // Batu Simpan di samping rumah Tetua
        var oldStone = GameObject.Find("SaveStone");
        if (oldStone) Object.DestroyImmediate(oldStone);
        var stone = new GameObject("SaveStone");
        stone.layer = elder.layer;                                  // Interactable
        stone.transform.position = new Vector3(22.5f, 7f, 0);
        var sr = stone.AddComponent<SpriteRenderer>();
        sr.sprite = Load<Sprite>("Assets/_Project/Art/UI/Prop_SaveStone.png");
        var elderSr = elder.GetComponent<SpriteRenderer>();
        sr.sortingLayerID = elderSr.sortingLayerID;
        sr.spriteSortPoint = elderSr.spriteSortPoint;
        var col = stone.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.75f, 0.5f); col.offset = new Vector2(0, 0.25f);
        var save = stone.AddComponent<SaveStone>();
        Set(save, "savedDialogue", D("Dlg_Save_Ok"));
        Set(save, "failedDialogue", D("Dlg_Save_Fail"));
        Set(save, "saveClip", Sfx("SFX_Save"));
        stone.transform.SetParent(GameObject.Find("NPCs").transform, true);

        // Musik area
        var music = new Dictionary<string, string> {
            { "Area_Desa", "BGM_Village" }, { "Area_Hutan", "BGM_Forest" }, { "Area_Danau", "BGM_Forest" }, { "Area_Gua", "BGM_Forest" } };
        foreach (var kv in music) Set(GameObject.Find(kv.Key).GetComponent<AreaZone>(), "music", Bgm(kv.Value));

        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Scn_Battle: audio
    public static void BuildBattle()
    {
        Scene scene = EditorSceneManager.OpenScene(BattlePath, OpenSceneMode.Single);
        var installer = Object.FindAnyObjectByType<BattleInstaller>();
        var audio = Get<BattleAudio>(installer.gameObject);
        Set(audio, "hitClip", Sfx("SFX_Hit"));
        Set(audio, "healClip", Sfx("SFX_Heal"));
        Set(audio, "evolveClip", Sfx("SFX_Evolve"));
        Set(audio, "victoryClip", Sfx("SFX_Victory_Jingle"));
        Set(audio, "faintClip", Sfx("SFX_Faint"));
        Set(installer, "battleAudio", audio);
        Set(installer, "normalMusic", Bgm("BGM_Battle"));
        Set(installer, "bossMusic", Bgm("BGM_Boss"));
        EditorSceneManager.SaveScene(scene);
    }

    static void SetBuildScenes()
    {
        string[] order = { P2_WorldBuilder.BootstrapPath, TitlePath, P2_WorldBuilder.WorldPath, BattlePath };
        var list = new List<EditorBuildSettingsScene>();
        foreach (string p in order) list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
