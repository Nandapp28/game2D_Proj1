// Versi demo — tombol Play di Editor SELALU mulai dari Scn_Bootstrap, scene mana pun yang sedang dibuka.
// Tanpa ini, menekan Play saat Scn_Title terbuka membuat EventSystem dan service (audio, save, dialog)
// tidak pernah dibuat, sehingga klik dan tombol tidak bereaksi.
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    const string BootstrapPath = "Assets/_Project/Scenes/Scn_Bootstrap.unity";

    static PlayFromBootstrap()
    {
        EditorApplication.delayCall += Apply;
    }

    static void Apply()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath);
    }
}
