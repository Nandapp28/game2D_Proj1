// P4 — mengatur perpindahan scene: Title/World/End (bergantian) dan Battle (di atas World).
// P6: setelah battle, musik dunia diputar lagi.
// Versi demo: HoldFade (layar tetap hitam untuk narasi pembuka), CurrentScene, IsBusy.
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow
{
    public const string TitleScene = "Scn_Title";
    public const string WorldScene = "Scn_World";
    public const string EndScene = "Scn_End";
    private const string BattleScene = "Scn_Battle";

    private readonly MonoBehaviour runner;
    private readonly ScreenFader fader;
    private readonly GameServices services;
    private string currentScene;
    private bool busy;

    public SceneFlow(MonoBehaviour runner, ScreenFader fader, GameServices services)
    {
        this.runner = runner;
        this.fader = fader;
        this.services = services;
    }

    public string CurrentScene => currentScene;
    public bool IsBusy => busy;
    public bool InBattle { get; private set; }
    public bool InGame => currentScene == WorldScene;

    // Diisi installer saat Install: layar dibiarkan hitam sampai ReleaseFade dipanggil.
    public bool HoldFade { get; set; }

    public void GoTo(string scene)
    {
        if (!busy) runner.StartCoroutine(GoToRoutine(scene));
    }

    public void StartBattle()
    {
        if (!busy) runner.StartCoroutine(StartBattleRoutine());
    }

    public void EndBattle(BattleResult result) => runner.StartCoroutine(EndBattleRoutine(result));

    public IEnumerator ReleaseFade()
    {
        HoldFade = false;
        yield return fader.FadeIn();
        busy = false;
    }

    private IEnumerator GoToRoutine(string scene)
    {
        busy = true;
        yield return fader.FadeOut();
        if (!string.IsNullOrEmpty(currentScene))
            yield return SceneManager.UnloadSceneAsync(currentScene);
        currentScene = scene;
        yield return LoadAndInstall(scene);
        if (HoldFade) yield break;             // installer yang akan memanggil ReleaseFade
        yield return fader.FadeIn();
        busy = false;
    }

    private IEnumerator StartBattleRoutine()
    {
        busy = true;
        InBattle = true;
        services.Input.Lock();
        yield return fader.FadeOut();
        SetRootsActive(currentScene, false);
        yield return LoadAndInstall(BattleScene);
        yield return fader.FadeIn();
    }

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        yield return fader.FadeOut();
        yield return SceneManager.UnloadSceneAsync(BattleScene);
        SetRootsActive(currentScene, true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
        InBattle = false;
        services.Battle.Finish(result);                       // dunia sudah aktif lagi
        if (services.Audio != null) services.Audio.ResumeWorldMusic();   // P6
        services.Input.Unlock();
        yield return fader.FadeIn();
        busy = false;
    }

    private IEnumerator LoadAndInstall(string scene)
    {
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        Scene loaded = SceneManager.GetSceneByName(scene);
        SceneManager.SetActiveScene(loaded);
        foreach (GameObject root in loaded.GetRootGameObjects())
        {
            if (root.TryGetComponent(out SceneInstaller installer))
            {
                installer.Install(services);
                break;
            }
        }
    }

    private static void SetRootsActive(string scene, bool active)
    {
        foreach (GameObject root in SceneManager.GetSceneByName(scene).GetRootGameObjects())
            root.SetActive(active);
    }
}
