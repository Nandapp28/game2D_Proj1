// Versi demo — layar "DEMO SELESAI" setelah Raijugar kalah: partner, level, dan waktu bermain.
// Enter / Spasi / E atau klik → kembali ke layar judul.
using UnityEngine;
using UnityEngine.UI;

public class EndInstaller : SceneInstaller
{
    [SerializeField] private Image partnerImage;
    [SerializeField] private Text partnerText;      // "Raijugar  ·  Lv 10"
    [SerializeField] private Text timeText;         // "Waktu bermain 12 menit 5 detik"
    [SerializeField] private Button clickCatcher;

    private GameServices services;
    private MonsterInstance partner;
    private bool leaving;
    private float ready;

    public override void Install(GameServices s)
    {
        services = s;
        partner = s.Party.Partner;
        int total = Mathf.FloorToInt(s.Party.Playtime);
        partnerText.text = partner != null ? $"{partner.Data.DisplayName}  ·  Lv {partner.Level}" : "";
        timeText.text = $"Waktu bermain {total / 60} menit {total % 60} detik";
        partnerImage.enabled = partner != null;
        clickCatcher.onClick.AddListener(Leave);
        ready = Time.unscaledTime + 0.5f;           // tombol dari dialog sebelumnya tidak langsung menutup layar
    }

    void Update()
    {
        if (partner != null)
            partnerImage.sprite = partner.Data.Frames[Mathf.FloorToInt(Time.unscaledTime / 0.35f) % 2];
        if (MenuInput.Confirm) Leave();
    }

    private void Leave()
    {
        if (leaving || services == null || Time.unscaledTime < ready) return;
        leaving = true;
        services.Scenes.GoTo(SceneFlow.TitleScene);
    }
}
