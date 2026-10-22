// P4 placeholder → P5 BattleSystem + presenter (MVP) → P6 audio → P7 feel. Versi demo: latar hutan / gua.
using UnityEngine;

public class BattleInstaller : SceneInstaller
{
    [SerializeField] private BattleSystem battle;
    [SerializeField] private BattleView view;
    [SerializeField] private BattleAudio battleAudio;      // P6
    [SerializeField] private BattleFeel feel;              // P7
    [SerializeField] private AudioClip normalMusic;        // P6
    [SerializeField] private AudioClip bossMusic;          // P6
    [SerializeField] private SpriteRenderer background;    // versi demo
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;

    public override void Install(GameServices s)
    {
        background.sprite = s.Battle.IsBoss ? caveBackground : forestBackground;
        battle.Construct(s.Party, s.Battle, s.Scenes);
        new BattlePresenter(battle, view);
        battleAudio.Construct(battle, s.Audio);
        feel.Construct(battle);                             // P7
        if (s.Audio != null) s.Audio.PlayBattleMusic(s.Battle.IsBoss ? bossMusic : normalMusic);
        battle.Begin();
    }
}
