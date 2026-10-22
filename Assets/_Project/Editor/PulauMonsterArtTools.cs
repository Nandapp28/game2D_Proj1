// Alat bantu editor modul Pulau Monster: preset pixel art dan slicing sprite sheet.
// Langkah yang sama bisa dilakukan manual lewat Inspector dan Sprite Editor (lihat Docs/P01).
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class PulauMonsterArtTools
{
    public const string PresetPath = "Assets/_Project/Settings/Presets/Pre_PixelSprite.preset";
    public const string ArtGlob = "glob:\"Assets/_Project/Art/**\"";

    [MenuItem("Pulau Monster/Art/1. Buat Preset Pixel Art")]
    public static void CreatePixelPreset()
    {
        const string tmp = "Assets/_Project/Settings/Presets/_preset_source.png";
        var tex = new Texture2D(16, 16);
        File.WriteAllBytes(tmp, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(tmp);
        var importer = (TextureImporter)AssetImporter.GetAtPath(tmp);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        var preset = new Preset(importer);
        var type = new PresetType(importer);
        AssetDatabase.DeleteAsset(PresetPath);
        AssetDatabase.CreateAsset(preset, PresetPath);
        AssetDatabase.DeleteAsset(tmp);

        var defaults = new List<DefaultPreset>(Preset.GetDefaultPresetsForType(type));
        defaults.RemoveAll(d => d.preset == null || d.filter == ArtGlob);
        defaults.Insert(0, new DefaultPreset(ArtGlob, AssetDatabase.LoadAssetAtPath<Preset>(PresetPath)));
        Preset.SetDefaultPresetsForType(type, defaults.ToArray());
        AssetDatabase.SaveAssets();
        Debug.Log("Pre_PixelSprite dibuat dan dijadikan default untuk " + ArtGlob);
    }

    // Menerapkan preset ke tekstur yang sudah terlanjur diimpor sebelum preset ada.
    public static void ApplyPresetTo(string folder)
    {
        var preset = AssetDatabase.LoadAssetAtPath<Preset>(PresetPath);
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            preset.ApplyTo(importer);
            importer.SaveAndReimport();
        }
    }

    // Sama dengan Sprite Editor → Slice → Grid By Cell Size. Sel kosong (transparan penuh) dilewati.
    public static int SliceGrid(string path, int cellW, int cellH, Vector2 pivot)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var src = new Texture2D(2, 2);
        src.LoadImage(File.ReadAllBytes(path));
        int cols = src.width / cellW, rows = src.height / cellH;
        string baseName = Path.GetFileNameWithoutExtension(path);

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dp = factory.GetSpriteEditorDataProviderFromObject(importer);
        dp.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();
        var pairs = new List<SpriteNameFileIdPair>();
        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int x = c * cellW, y = src.height - (r + 1) * cellH;   // dihitung dari kiri atas
                if (IsEmpty(src, x, y, cellW, cellH)) continue;
                var rect = new SpriteRect
                {
                    name = baseName + "_" + index,
                    rect = new Rect(x, y, cellW, cellH),
                    alignment = SpriteAlignment.Custom,
                    pivot = pivot,
                    spriteID = GUID.Generate(),
                };
                rects.Add(rect);
                pairs.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
                index++;
            }
        }
        dp.SetSpriteRects(rects.ToArray());
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
        dp.Apply();
        importer.SaveAndReimport();
        return rects.Count;
    }

    private static bool IsEmpty(Texture2D t, int x, int y, int w, int h)
    {
        for (int j = y; j < y + h; j++)
            for (int i = x; i < x + w; i++)
                if (t.GetPixel(i, j).a > 0.01f) return false;
        return true;
    }

    [MenuItem("Pulau Monster/Art/2. Slice Semua Sprite Sheet")]
    public static void SliceAll()
    {
        var bottom = new Vector2(0.5f, 0f);
        var center = new Vector2(0.5f, 0.5f);
        int total = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            if (path.Contains("/Characters/")) total += SliceGrid(path, 16, 16, bottom);
            else if (path.Contains("/Monsters/"))
            {
                int n = name.EndsWith("32x32") ? 32 : name.EndsWith("24x24") ? 24 : 16;
                total += SliceGrid(path, n, n, bottom);
            }
            else if (path.Contains("/Tiles/")) total += SliceGrid(path, 16, 16, center);
            else if (path.Contains("/VFX/") && name.Contains("_4f_")) total += SliceGrid(path, 32, 32, center);
            else
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.spriteImportMode = SpriteImportMode.Single;      // background & partikel: satu sprite
                imp.SaveAndReimport();
            }
        }
        Debug.Log($"Slicing selesai: {total} sprite.");
    }
}
