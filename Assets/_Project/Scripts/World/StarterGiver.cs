// P6 — Tetua Rahmat: menawarkan 3 starter, lalu menandai flag has_starter.
// Versi demo: turunan Npc. Pilihan dikonfirmasi ("Yakin?" → Ya / Pilih lagi) dan dialog berubah
// setelah boss Raijugar kalah.
using System.Collections;
using UnityEngine;

public class StarterGiver : Npc
{
    [SerializeField] private MonsterData[] starters;           // Pyrit, Blubi, Sprig
    [SerializeField] private int starterLevel = 3;
    [SerializeField] private AudioClip receiveClip;

    private PlayerParty party;
    private ToastView toast;
    private AudioService audioService;
    private PixelBurst burst;
    private Transform player;

    public void ConstructStarter(PlayerParty playerParty, ToastView toastView, AudioService audio,
                                 PixelBurst pixelBurst, Transform playerTransform)
    {
        party = playerParty;
        toast = toastView;
        audioService = audio;
        burst = pixelBurst;
        player = playerTransform;
    }

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.HasStarter))
        {
            yield return GiveStarter();
        }
        else if (Flags.Has(StoryFlags.BossDefeated))
        {
            yield return Dialogue.Say(NpcName, "Kau menaklukkan Raijugar di Gua Petir? Luar biasa!");
            yield return Dialogue.Say(NpcName, $"Desa ini aman berkat kamu dan {party.Partner.Data.DisplayName}.");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Monster di hutan utara lebih kuat. Di timur laut ada Gua Petir, sarang Raijugar.");
            yield return Dialogue.Say(NpcName, "Kalau partnermu sudah level 9, mungkin kalian siap menantangnya.");
        }
    }

    private IEnumerator GiveStarter()
    {
        yield return Dialogue.Say(NpcName, "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        yield return Dialogue.Say(NpcName, "Tidak ada yang boleh masuk hutan tanpa partner. Pilih satu dari tiga ini.");

        var options = new string[starters.Length];
        for (int i = 0; i < starters.Length; i++)
            options[i] = $"{starters[i].DisplayName} ({starters[i].Element})";

        MonsterData pick = null;
        while (pick == null)                                    // ulangi sampai pemain memilih "Ya"
        {
            int choice = 0, sure = 0;
            yield return Dialogue.Choose(NpcName, "Siapa yang akan menjadi partnermu?", options, c => choice = c);
            MonsterData m = starters[choice];
            yield return Dialogue.Choose(NpcName, $"{m.DisplayName} tipe {m.Element}. Yakin?", new[] { "Ya", "Pilih lagi" }, c => sure = c);
            if (sure == 0) pick = m;
        }

        party.SetPartner(new MonsterInstance(pick, starterLevel));
        Flags.Set(StoryFlags.HasStarter);
        if (audioService != null) audioService.PlaySfx(receiveClip, 0f);
        if (burst != null) burst.Play(player.position + Vector3.up * 0.5f, ElementColor(pick.Element), 24, 70f, 0.8f);

        yield return Dialogue.Say("", $"Kamu mendapat {pick.DisplayName}!");
        yield return Dialogue.Say(NpcName, "Rawat dia baik-baik. Menang bertarung membuatnya naik level, lalu berevolusi.");
        yield return Dialogue.Say(NpcName, "Batu Simpan di sebelahku menyimpan perjalananmu dan memulihkan partnermu.");
        if (toast != null) toast.Show("Penjaga Batas sudah membuka jalan ke hutan");
    }

    public static Color ElementColor(Element e) => e switch
    {
        Element.Pyro => new Color32(255, 138, 76, 255),
        Element.Aqua => new Color32(111, 182, 255, 255),
        Element.Sprout => new Color32(127, 214, 106, 255),
        _ => new Color32(255, 224, 102, 255),
    };
}
