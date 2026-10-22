// Alat dokumentasi: memotret jendela Editor (Inspector, Tile Palette, Animator, ...) ke file PNG.
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorShot
{
    public static string Capture(EditorWindow window, string fileName)
    {
        window.Focus();
        window.Repaint();
        Rect r = window.position;
        float scale = EditorGUIUtility.pixelsPerPoint;
        int w = Mathf.RoundToInt(r.width * scale), h = Mathf.RoundToInt(r.height * scale);
        Color[] px = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(new Vector2(r.x * scale, r.y * scale), w, h);
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.SetPixels(px);
        tex.Apply();
        string path = Path.Combine(Application.dataPath, "..", "Docs", "screenshots", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        return $"{w}x{h} -> {path}";
    }

    public static string LastResult = "";

    // Memotret setelah beberapa frame editor, agar jendela sempat menggambar ulang isinya.
    public static void CaptureLater(EditorWindow window, string fileName, int frames = 20)
    {
        LastResult = "pending";
        window.Focus();
        int left = frames;
        EditorApplication.CallbackFunction tick = null;
        tick = () =>
        {
            window.Repaint();
            if (--left > 0) return;
            EditorApplication.update -= tick;
            LastResult = Capture(window, fileName);
        };
        EditorApplication.update += tick;
    }

    public static EditorWindow Open(string typeName)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t != null) return EditorWindow.GetWindow(t);
        }
        return null;
    }

    // Pindahkan hasil capture_game_view (Assets/_Shots/<P>) ke Docs/screenshots/<P>, lalu hapus folder sementara.
    public static string CollectShots(string meeting)
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string src = Path.Combine(root, "Assets/_Shots", meeting);
        string dst = Path.Combine(root, "Docs/screenshots", meeting);
        Directory.CreateDirectory(dst);
        if (Directory.Exists(src))
            foreach (string f in Directory.GetFiles(src, "*.png"))
                File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
        AssetDatabase.DeleteAsset("Assets/_Shots");
        return string.Join(", ", System.Array.ConvertAll(Directory.GetFiles(dst), Path.GetFileName));
    }

    public static void DeleteFile(string relativeToProject)
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativeToProject));
        if (File.Exists(path)) File.Delete(path);
    }
}
