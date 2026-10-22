// P6 — layar judul: Lanjutkan (jika ada save) atau Mulai baru, plus musik judul.
// P7 — pilihan Keluar untuk versi build.
// Versi demo: latar peta yang bergeser pelan, 4 monster starter beranimasi, menu teks "▶ Mulai baru"
// (panah/WASD + Enter/Spasi/E, atau klik).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleInstaller : SceneInstaller
{
    [SerializeField] private TextMenu menu;
    [SerializeField] private RectTransform worldBackground;     // BG_Title_World, lebih lebar dari layar
    [SerializeField] private float panDistance = 240f;          // 60 px demo × 4
    [SerializeField] private Image[] starterImages;             // 4 monster tahap 1
    [SerializeField] private MonsterData[] starterData;
    [SerializeField] private AudioClip titleMusic;

    private GameServices services;
    private readonly List<string> options = new List<string>();

    public override void Install(GameServices s)
    {
        services = s;
        options.Clear();
        if (s.Save.HasSave()) options.Add("Lanjutkan");
        options.Add("Mulai baru");
        options.Add("Keluar");                                  // P7: hanya berguna di versi build
        menu.Picked += OnPicked;
        menu.Open(options.ToArray());
        if (s.Audio != null) s.Audio.PlayWorldMusic(titleMusic);
    }

    void Update()
    {
        float t = Time.unscaledTime;
        worldBackground.anchoredPosition = new Vector2(Mathf.Sin(t / 6f) * panDistance, 0f);
        for (int i = 0; i < starterImages.Length; i++)
            starterImages[i].sprite = starterData[i].Frames[Mathf.FloorToInt(t / 0.35f + i) % 2];
    }

    private void OnPicked(int index)
    {
        switch (options[index])
        {
            case "Lanjutkan": Continue(); break;
            case "Mulai baru": NewGame(); break;
            default: Quit(); break;
        }
    }

    private void Continue()
    {
        SaveData data = services.Save.Load();
        if (data == null) { NewGame(); return; }
        SaveMapper.Apply(data, services);
        services.Save.PendingLoad = data;
        menu.Close();
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void NewGame()
    {
        services.Party.Reset();
        services.Flags.Load(new List<string>());
        menu.Close();
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // di Editor: hentikan Play Mode
#else
        Application.Quit();
#endif
    }
}
