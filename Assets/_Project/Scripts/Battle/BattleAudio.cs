// P6 — suara battle mendengarkan event BattleSystem; BattleSystem tidak tahu soal audio.
// Versi demo: suara meleset, naik level, jingle menang tepat setelah lawan kalah.
using UnityEngine;

public class BattleAudio : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip criticalClip;
    [SerializeField] private AudioClip missClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip levelClip;
    [SerializeField] private AudioClip evolveClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip faintClip;

    private BattleSystem battle;
    private AudioService audioService;

    public void Construct(BattleSystem battleSystem, AudioService audio)
    {
        if (audio == null) return;
        battle = battleSystem;
        audioService = audio;
        battle.Hit += OnHit;
        battle.Missed += OnMissed;
        battle.Healed += OnHealed;
        battle.LevelUp += OnLevelUp;
        battle.Evolving += OnEvolving;
        battle.Victory += OnVictory;
        battle.Fainted += OnFainted;
    }

    void OnDestroy()
    {
        if (battle == null) return;
        battle.Hit -= OnHit;
        battle.Missed -= OnMissed;
        battle.Healed -= OnHealed;
        battle.LevelUp -= OnLevelUp;
        battle.Evolving -= OnEvolving;
        battle.Victory -= OnVictory;
        battle.Fainted -= OnFainted;
    }

    private void OnHit(MonsterInstance target, int damage) =>
        audioService.PlaySfx(battle.LastHitCritical && criticalClip != null ? criticalClip : hitClip);
    private void OnMissed() => audioService.PlaySfx(missClip);
    private void OnHealed(MonsterInstance target) => audioService.PlaySfx(healClip, 0f);
    private void OnLevelUp(MonsterData shown) => audioService.PlaySfx(levelClip, 0f);
    private void OnEvolving(MonsterData from, MonsterData to) => audioService.PlaySfx(evolveClip, 0f);
    private void OnVictory() => audioService.PlaySfx(victoryClip, 0f);
    private void OnFainted(MonsterInstance monster) => audioService.PlaySfx(faintClip, 0f);
}
