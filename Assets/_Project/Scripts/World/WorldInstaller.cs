// P2 dibuat, P3 interactor, P4 SceneInstaller + monster liar, P5 HUD & area, P6 dialog, flag, save, musik.
// Versi demo: narasi pembuka, partner mengikuti pemain, kembali ke desa saat kalah, dan ending setelah Raijugar kalah.
using System.Collections;
using UnityEngine;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerHud hud;                    // P5
    [SerializeField] private PartnerFollower follower;          // versi demo
    [SerializeField] private PixelBurst burst;                  // versi demo
    [SerializeField] private Vector2 respawnPoint = new Vector2(21.5f, 6f);   // di depan Batu Simpan

    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;
        s.Player = player.transform;
        player.Construct(s.Input);
        interactor.Construct(s.Input);
        hud.Construct(s.Party);
        follower.Construct(s.Party, player);

        foreach (Npc npc in FindInScene<Npc>()) npc.Construct(s.Dialogue, s.Flags);                       // P6
        foreach (StarterGiver giver in FindInScene<StarterGiver>())
            giver.ConstructStarter(s.Party, s.Toast, s.Audio, burst, player.transform);
        foreach (MerchantNpc merchant in FindInScene<MerchantNpc>()) merchant.ConstructMerchant(s.Party, s.Audio);
        foreach (FlagToggle toggle in FindInScene<FlagToggle>()) toggle.Construct(s.Flags);
        foreach (SaveStone stone in FindInScene<SaveStone>()) stone.Construct(s, player.transform, burst);
        foreach (WildMonster wild in FindInScene<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, s.Flags, s.Input, s.Dialogue, player.transform);
        foreach (AreaZone zone in FindInScene<AreaZone>())
        {
            zone.Construct(s.Audio);                                                                      // P6
            zone.Entered += hud.ShowArea;
        }
        s.Battle.Ended += OnBattleEnded;

        if (s.Save.PendingLoad != null)                         // P6: lanjut dari file save
        {
            player.Teleport(s.Save.PendingLoad.playerPosition, Vector2.down);
            s.Save.PendingLoad = null;
            s.Toast.Show("Perjalanan dilanjutkan");
        }
        else if (!s.Flags.Has(StoryFlags.IntroSeen))            // versi demo: narasi di atas layar hitam
        {
            s.Scenes.HoldFade = true;
            StartCoroutine(Intro());
        }
    }

    void OnDestroy()
    {
        if (services != null) { services.Battle.Ended -= OnBattleEnded; services.Player = null; }
    }

    private IEnumerator Intro()
    {
        services.Dialogue.Run(IntroLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;
        services.Flags.Set(StoryFlags.IntroSeen);
        yield return services.Scenes.ReleaseFade();
        services.Toast.Show("Temui Tetua Rahmat (kanan jalan desa)");
    }

    private IEnumerator IntroLines()
    {
        yield return services.Dialogue.Narrate("Di ujung Pulau Batam ada desa kecil yang hidup berdampingan dengan monster.");
        yield return services.Dialogue.Narrate("Hari ini kamu akan mendapat monster partner pertamamu. Temui Tetua Rahmat di desa.");
    }

    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose)
            player.Teleport(respawnPoint, Vector2.up);          // partner sudah dipulihkan BattleSystem
        else if (result == BattleResult.Win && wasBoss)
            StartCoroutine(Ending());
    }

    private IEnumerator Ending()
    {
        while (services.Scenes.IsBusy) yield return null;      // tunggu layar dunia terang lagi
        services.Dialogue.Run(EndingLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;
        services.Save.Save(SaveMapper.Capture(services, player.transform.position));
        services.Scenes.GoTo(SceneFlow.EndScene);
    }

    private IEnumerator EndingLines()
    {
        yield return services.Dialogue.Narrate("Raijugar menunduk dan membiarkanmu lewat. Gua Petir kini tenang.");
    }
}
