// P1 — membangun ulang scene sandbox persis seperti langkah manual di Docs/P01.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class P1_PlaygroundBuilder
{
    public const string ScenePath = "Assets/_Project/Scenes/Sandbox/Scn_Playground_7312511016.unity";

    [MenuItem("Pulau Monster/Build/P1 Scene Playground")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var c = cam.GetComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 5.625f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(0.36f, 0.62f, 0.32f);
        cam.transform.position = new Vector3(0, 0, -10);
        cam.AddComponent<UniversalAdditionalCameraData>();

        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;

        // Player
        Sprite player0 = LoadSprite("Assets/_Project/Art/Characters/Player_Walk_16x16.png", "Player_Walk_16x16_0");
        var player = new GameObject("Player");
        player.tag = "Player";
        player.AddComponent<SpriteRenderer>().sprite = player0;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var box = player.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.6f, 0.4f);
        box.offset = new Vector2(0f, 0.2f);
        var move = player.AddComponent<PlayerMovement>();
        var so = new SerializedObject(move);
        so.FindProperty("rb").objectReferenceValue = rb;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Tembok batas + rintangan (tugas P1)
        var walls = new GameObject("Walls");
        Wall(walls, "Wall_Top", new Vector2(0, 5.2f), new Vector2(19f, 0.6f));
        Wall(walls, "Wall_Bottom", new Vector2(0, -5.2f), new Vector2(19f, 0.6f));
        Wall(walls, "Wall_Left", new Vector2(-9.2f, 0), new Vector2(0.6f, 10f));
        Wall(walls, "Wall_Right", new Vector2(9.2f, 0), new Vector2(0.6f, 10f));
        Wall(walls, "Wall", new Vector2(2.5f, 0.5f), new Vector2(1f, 3f));
        Wall(walls, "Rock_1", new Vector2(-4f, 2.5f), new Vector2(1.5f, 1f));
        Wall(walls, "Rock_2", new Vector2(-3f, -3f), new Vector2(2f, 1f));

        // NPC diam yang tidak bisa ditembus (tugas P1)
        var npc = new GameObject("NPC_Elder");
        npc.transform.position = new Vector3(5.5f, -2f, 0);
        npc.AddComponent<SpriteRenderer>().sprite = LoadSprite("Assets/_Project/Art/Characters/NPC_Elder_Walk_16x16.png", "NPC_Elder_Walk_16x16_0");
        var nbox = npc.AddComponent<BoxCollider2D>();
        nbox.size = new Vector2(0.8f, 0.5f);
        nbox.offset = new Vector2(0f, 0.25f);

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void Wall(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SquareSprite();
        sr.color = new Color(0.45f, 0.35f, 0.28f);
        go.AddComponent<BoxCollider2D>();
    }

    public static Sprite LoadSprite(string path, string name)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            if (o is Sprite s && s.name == name) return s;
        Debug.LogError("Sprite tidak ditemukan: " + name);
        return null;
    }

    // Sprite persegi putih 1×1 unit (sama fungsinya dengan 2D Object → Sprites → Square).
    public static Sprite SquareSprite()
    {
        const string path = "Assets/_Project/Art/UI/Square.png";
        if (!System.IO.File.Exists(path))
        {
            var t = new Texture2D(16, 16);
            var px = new Color32[256];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            t.SetPixels32(px);
            System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
