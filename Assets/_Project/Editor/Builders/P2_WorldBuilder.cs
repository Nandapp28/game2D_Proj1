// P2 — membangun Scn_Bootstrap dan Scn_World (Tilemap berlapis, Rule Tile, kolisi, sorting, Pixel Perfect).
// Setiap langkah di sini sama dengan langkah manual di Docs/P02.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class P2_WorldBuilder
{
    public const string TilesetPath = "Assets/_Project/Art/Tiles/Tiles_Overworld_16x16.png";
    public const string TileFolder = "Assets/_Project/Art/Tiles/Palette";
    public const string BootstrapPath = "Assets/_Project/Scenes/Scn_Bootstrap.unity";
    public const string WorldPath = "Assets/_Project/Scenes/Scn_World.unity";
    public const int W = 40, H = 36;

    // Nama tile sesuai indeks slicing (baris × 12 + kolom), sama dengan Tiles_Overworld_index.txt.
    public static readonly string[] Names =
    {
        "Path_TL","Path_T","Path_TR","Path_iTL","Path_iTR","Water_TL","Water_T","Water_TR","Water_iTL","Water_iTR","Grass_A","Grass_B",
        "Path_L","Path_C","Path_R","Path_iBL","Path_iBR","Water_L","Water_C","Water_R","Water_iBL","Water_iBR","Grass_Flower","Sand",
        "Path_BL","Path_B","Path_BR","TallGrass_A","TallGrass_B","Water_BL","Water_B","Water_BR","Flowers","SmallRock","Sign","SaveStone",
        "Tree_Round","Tree_Pine","Bush","BigRock","Stump","Fence_H","Fence_V","Cave_Floor_A","Cave_Floor_B","Cave_Wall","Cave_Entrance","Crystal",
        "Roof_L","Roof_M","Roof_R","Wall_L","Wall_Window","Wall_Door","Wall_R","Wall_M",
    };

    static readonly HashSet<string> Solid = new HashSet<string>
    {
        "Tree_Round","Tree_Pine","Bush","BigRock","Stump","Fence_H","Fence_V","Cave_Wall","Crystal","SaveStone",
        "Roof_L","Roof_M","Roof_R","Wall_L","Wall_Window","Wall_Door","Wall_R","Wall_M",
    };

    static Dictionary<string, Tile> tiles;
    static RuleTile rtPath, rtWater;

    [MenuItem("Pulau Monster/Build/P2 Tiles, Rule Tile & Palette")]
    public static void BuildTiles()
    {
        System.IO.Directory.CreateDirectory(TileFolder);
        tiles = new Dictionary<string, Tile>();
        var sprites = new Dictionary<string, Sprite>();
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(TilesetPath))
            if (o is Sprite s) sprites[s.name] = s;

        for (int i = 0; i < Names.Length; i++)
        {
            string path = $"{TileFolder}/T_{Names[i]}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprites[$"Tiles_Overworld_16x16_{i}"];
            tile.colliderType = Solid.Contains(Names[i]) ? Tile.ColliderType.Grid : Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            tiles[Names[i]] = tile;
        }

        rtPath = MakeRuleTile("RT_Path", "Path", sprites, false);
        rtWater = MakeRuleTile("RT_Water", "Water", sprites, true);
        AssetDatabase.SaveAssets();
        BuildPalette();
    }

    // Tabel aturan P2: ✓ = This, ✗ = NotThis, hanya 4 tetangga orthogonal + sudut dalam (diagonal ✗).
    static RuleTile MakeRuleTile(string name, string prefix, Dictionary<string, Sprite> sprites, bool solid)
    {
        string path = $"Assets/_Project/Art/Tiles/{name}.asset";
        var rt = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
        if (rt == null)
        {
            rt = ScriptableObject.CreateInstance<RuleTile>();
            AssetDatabase.CreateAsset(rt, path);
        }
        Sprite S(string n) => sprites[$"Tiles_Overworld_16x16_{System.Array.IndexOf(Names, prefix + "_" + n)}"];
        var col = solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        rt.m_DefaultSprite = S("C");
        rt.m_DefaultColliderType = col;
        rt.m_TilingRules = new List<RuleTile.TilingRule>();
        const int T = RuleTile.TilingRuleOutput.Neighbor.This, N = RuleTile.TilingRuleOutput.Neighbor.NotThis;
        var up = new Vector3Int(0, 1, 0); var down = new Vector3Int(0, -1, 0);
        var left = new Vector3Int(-1, 0, 0); var right = new Vector3Int(1, 0, 0);
        void Rule(string sprite, int u, int d, int l, int r, Vector3Int? diag = null)
        {
            var rule = new RuleTile.TilingRule();
            rule.m_Sprites = new[] { S(sprite) };
            rule.m_ColliderType = col;
            rule.m_NeighborPositions = new List<Vector3Int> { up, down, left, right };
            rule.m_Neighbors = new List<int> { u, d, l, r };
            if (diag.HasValue) { rule.m_NeighborPositions.Add(diag.Value); rule.m_Neighbors.Add(N); }
            rt.m_TilingRules.Add(rule);
        }
        Rule("TL", N, T, N, T); Rule("T", N, T, T, T); Rule("TR", N, T, T, N);
        Rule("L", T, T, N, T); Rule("R", T, T, T, N);
        Rule("BL", T, N, N, T); Rule("B", T, N, T, T); Rule("BR", T, N, T, N);
        Rule("iTL", T, T, T, T, new Vector3Int(-1, 1, 0)); Rule("iTR", T, T, T, T, new Vector3Int(1, 1, 0));
        Rule("iBL", T, T, T, T, new Vector3Int(-1, -1, 0)); Rule("iBR", T, T, T, T, new Vector3Int(1, -1, 0));
        EditorUtility.SetDirty(rt);
        return rt;
    }

    static void BuildPalette()
    {
        const string palPath = "Assets/_Project/Art/Tiles/Pal_Overworld.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(palPath) != null) AssetDatabase.DeleteAsset(palPath);
        GameObject pal = GridPaletteUtility.CreateNewPalette("Assets/_Project/Art/Tiles", "Pal_Overworld",
            GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);
        string path = AssetDatabase.GetAssetPath(pal);
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Tilemap tm = root.GetComponentInChildren<Tilemap>();
        for (int i = 0; i < Names.Length; i++)
            tm.SetTile(new Vector3Int(i % 12, -(i / 12), 0), tiles[Names[i]]);
        tm.SetTile(new Vector3Int(13, 0, 0), rtPath);
        tm.SetTile(new Vector3Int(13, -1, 0), rtWater);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    [MenuItem("Pulau Monster/Build/P2 Project Settings (Sorting & Renderer)")]
    public static void ProjectSettings()
    {
        // Sorting Layers: Ground, Decor, Characters
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        foreach (string name in new[] { "Ground", "Decor", "Characters" })
        {
            bool exists = false;
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name) exists = true;
            if (exists) continue;
            layers.InsertArrayElementAtIndex(layers.arraySize);
            SerializedProperty e = layers.GetArrayElementAtIndex(layers.arraySize - 1);
            e.FindPropertyRelative("name").stringValue = name;
            e.FindPropertyRelative("uniqueID").intValue = (int)(name.GetHashCode() & 0x7fffffff);
        }
        tagManager.ApplyModifiedProperties();

        // Renderer 2D: Transparency Sort Mode = Custom Axis (0, 1, 0)
        var renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/Renderer2D.asset");
        var so = new SerializedObject(renderer);
        so.FindProperty("m_TransparencySortMode").intValue = (int)TransparencySortMode.CustomAxis;
        so.FindProperty("m_TransparencySortAxis").vector3Value = new Vector3(0, 1, 0);
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Pulau Monster/Build/P2 Scenes (Bootstrap & World)")]
    public static void BuildScenes()
    {
        if (tiles == null) BuildTiles();
        ProjectSettings();

        // Cfg_Player
        const string cfgPath = "Assets/_Project/Data/Config/Cfg_Player.asset";
        var cfg = AssetDatabase.LoadAssetAtPath<PlayerConfig>(cfgPath);
        if (cfg == null) { cfg = ScriptableObject.CreateInstance<PlayerConfig>(); AssetDatabase.CreateAsset(cfg, cfgPath); }

        // ---------- Scn_Bootstrap: tanpa kamera
        Scene boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        EditorSceneManager.SaveScene(boot, BootstrapPath);

        // ---------- Scn_World
        Scene world = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.06f, 0.16f);
        camGo.transform.position = new Vector3(18f, 7f, -10f);
        camGo.AddComponent<UniversalAdditionalCameraData>();
        var ppc = camGo.AddComponent<PixelPerfectCamera>();
        ppc.assetsPPU = 16;
        ppc.refResolutionX = 320;
        ppc.refResolutionY = 180;

        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;

        var grid = new GameObject("Grid", typeof(Grid));
        Tilemap ground = NewTilemap(grid, "TM_Ground", "Ground", 0, false);
        Tilemap decor = NewTilemap(grid, "TM_Decor", "Decor", 0, false);
        Tilemap blocking = NewTilemap(grid, "TM_Blocking", "Characters", 0, true);
        Tilemap tall = NewTilemap(grid, "TM_TallGrass", "Decor", 1, false);

        PaintWorld(ground, decor, blocking, tall);

        // Kolisi TM_Blocking: Tilemap Collider 2D (Merge) + Composite Collider 2D (Static)
        var tc = blocking.gameObject.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        var brb = blocking.gameObject.AddComponent<Rigidbody2D>();
        brb.bodyType = RigidbodyType2D.Static;
        blocking.gameObject.AddComponent<CompositeCollider2D>();
        blocking.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;

        // Rumput tinggi: trigger (dipakai efek daun di P7)
        var tallCol = tall.gameObject.AddComponent<TilemapCollider2D>();
        tallCol.isTrigger = true;
        foreach (Vector3Int p in tall.cellBounds.allPositionsWithin)
            if (tall.HasTile(p)) tall.SetColliderType(p, Tile.ColliderType.Grid);

        // Player (dari scene sandbox P1, kini memakai PlayerMovement v4)
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(16f, 5f, 0f);
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = P1_PlaygroundBuilder.LoadSprite("Assets/_Project/Art/Characters/Player_Walk_16x16.png", "Player_Walk_16x16_0");
        sr.sortingLayerName = "Characters";
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var box = player.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.6f, 0.4f);
        box.offset = new Vector2(0f, 0.2f);
        var move = player.AddComponent<PlayerMovement>();
        var mso = new SerializedObject(move);
        mso.FindProperty("rb").objectReferenceValue = rb;
        mso.FindProperty("config").objectReferenceValue = cfg;
        mso.ApplyModifiedPropertiesWithoutUndo();

        var installer = new GameObject("WorldInstaller").AddComponent<WorldInstaller>();
        var iso = new SerializedObject(installer);
        iso.FindProperty("player").objectReferenceValue = move;
        iso.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(world, WorldPath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootstrapPath, true),
            new EditorBuildSettingsScene(WorldPath, true),
        };
        EditorSceneManager.OpenScene(BootstrapPath);   // Play selalu dari Scn_Bootstrap
    }

    static Tilemap NewTilemap(GameObject grid, string name, string sortingLayer, int order, bool blocking)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid.transform);
        var r = go.GetComponent<TilemapRenderer>();
        r.sortingLayerName = sortingLayer;
        r.sortingOrder = order;
        return go.GetComponent<Tilemap>();
    }

    // ------------------------------------------------------------------ peta 40 × 36
    static void PaintWorld(Tilemap ground, Tilemap decor, Tilemap blocking, Tilemap tall)
    {
        var rnd = new System.Random(7);
        bool InCave(int x, int y) => x >= 22 && x <= 38 && y >= 25 && y <= 34;

        // Tanah: rumput (variasi) dan lantai gua
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                string n = InCave(x, y) ? (rnd.Next(4) == 0 ? "Cave_Floor_B" : "Cave_Floor_A")
                         : rnd.Next(10) == 0 ? "Grass_B" : rnd.Next(25) == 0 ? "Grass_Flower" : "Grass_A";
                ground.SetTile(new Vector3Int(x, y, 0), tiles[n]);
            }

        var path = new HashSet<Vector2Int>();
        void PathRect(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) path.Add(new Vector2Int(x, y));
        }
        PathRect(2, 5, 37, 6);      // jalan utama desa
        PathRect(19, 7, 20, 23);    // jalan ke utara melewati hutan
        PathRect(6, 22, 34, 23);    // jalan utara: danau ↔ gua
        PathRect(30, 24, 31, 25);   // pintu gua
        PathRect(8, 24, 9, 25);     // ke tepi danau
        foreach (var p in path) ground.SetTile(new Vector3Int(p.x, p.y, 0), rtPath);

        void Water(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) blocking.SetTile(new Vector3Int(x, y, 0), rtWater);
        }
        Water(31, 8, 35, 11);      // kolam desa
        Water(2, 26, 15, 33);      // Danau Sunyi
        Water(4, 34, 12, 34);

        void Put(Tilemap tm, int x, int y, string n) => tm.SetTile(new Vector3Int(x, y, 0), tiles[n]);
        bool Free(int x, int y) => !path.Contains(new Vector2Int(x, y)) && !blocking.HasTile(new Vector3Int(x, y, 0))
                                   && !tall.HasTile(new Vector3Int(x, y, 0));

        // Rumah: dinding (bawah) + atap (atas), pintu menghadap jalan
        void House(int x, int y)
        {
            Put(blocking, x, y, "Wall_L"); Put(blocking, x + 1, y, "Wall_Door"); Put(blocking, x + 2, y, "Wall_R");
            Put(blocking, x, y + 1, "Roof_L"); Put(blocking, x + 1, y + 1, "Roof_M"); Put(blocking, x + 2, y + 1, "Roof_R");
        }
        House(4, 8); House(9, 8); House(24, 8);
        for (int x = 2; x <= 14; x++) Put(blocking, x, 3, "Fence_H");
        Put(decor, 17, 7, "Sign");
        Put(decor, 22, 4, "Sign");

        // Rumput tinggi (dua petak di Hutan Rimbun)
        for (int x = 4; x <= 10; x++) for (int y = 14; y <= 18; y++) Put(tall, x, y, (x + y) % 2 == 0 ? "TallGrass_A" : "TallGrass_B");
        for (int x = 25; x <= 32; x++) for (int y = 14; y <= 18; y++) Put(tall, x, y, (x + y) % 2 == 0 ? "TallGrass_A" : "TallGrass_B");

        // Gua: dinding keliling, pintu masuk, kristal
        for (int x = 22; x <= 38; x++) { if (x != 30 && x != 31) Put(blocking, x, 25, "Cave_Wall"); Put(blocking, x, 34, "Cave_Wall"); }
        for (int y = 25; y <= 34; y++) { Put(blocking, 22, y, "Cave_Wall"); Put(blocking, 38, y, "Cave_Wall"); }
        Put(decor, 30, 25, "Cave_Entrance"); Put(decor, 31, 25, "Cave_Entrance");
        foreach (var c in new[] { new Vector2Int(24, 27), new Vector2Int(36, 28), new Vector2Int(26, 32), new Vector2Int(35, 32), new Vector2Int(33, 27) })
            Put(blocking, c.x, c.y, "Crystal");

        // Pohon pembatas peta
        for (int x = 0; x < W; x++) { Put(blocking, x, 0, x % 2 == 0 ? "Tree_Pine" : "Tree_Round"); if (!InCave(x, 35)) Put(blocking, x, 35, x % 2 == 0 ? "Tree_Round" : "Tree_Pine"); }
        for (int y = 1; y < H - 1; y++) { if (!blocking.HasTile(new Vector3Int(0, y, 0))) Put(blocking, 0, y, y % 2 == 0 ? "Tree_Round" : "Tree_Pine");
                                          if (!blocking.HasTile(new Vector3Int(39, y, 0))) Put(blocking, 39, y, y % 2 == 0 ? "Tree_Pine" : "Tree_Round"); }
        for (int x = 22; x < W; x++) Put(blocking, x, 35, "Tree_Pine");

        // Hutan Rimbun: pohon tersebar (y 12–21), jalan dan rumput tinggi tetap bersih
        for (int y = 12; y <= 21; y++)
            for (int x = 1; x < W - 1; x++)
                if (Free(x, y) && Mathf.Abs(x - 19.5f) > 2.5f && rnd.Next(100) < 22)
                    Put(blocking, x, y, rnd.Next(3) == 0 ? "Tree_Round" : "Tree_Pine");
        // Desa dan sekitar danau: dekorasi
        for (int i = 0; i < 40; i++)
        {
            int x = rnd.Next(1, W - 1), y = rnd.Next(1, 25);
            if (y >= 12 && y <= 21) continue;
            if (Free(x, y) && !decor.HasTile(new Vector3Int(x, y, 0)) && Mathf.Abs(x - 16) + Mathf.Abs(y - 5) > 2)
                Put(decor, x, y, rnd.Next(3) == 0 ? "SmallRock" : "Flowers");
        }
        foreach (var b in new[] { new Vector2Int(17, 9), new Vector2Int(22, 9), new Vector2Int(28, 3), new Vector2Int(36, 4), new Vector2Int(14, 26), new Vector2Int(17, 30) })
            if (Free(b.x, b.y)) Put(blocking, b.x, b.y, "Bush");
        foreach (var b in new[] { new Vector2Int(17, 33), new Vector2Int(19, 28), new Vector2Int(36, 14), new Vector2Int(2, 20) })
            if (Free(b.x, b.y)) Put(blocking, b.x, b.y, b.x % 2 == 0 ? "BigRock" : "Stump");
    }
}
