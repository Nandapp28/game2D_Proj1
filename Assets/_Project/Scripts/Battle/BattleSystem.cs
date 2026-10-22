// P5 — logika battle bergiliran. Tidak menyentuh UI: dunia luar cukup mendengar event.
// Versi demo: urutan pesan, jeda, dan evolusi bertahap ("Apa? X mulai bersinar...") disamakan dengan demo.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private int skillUsesPerBattle = 3;
    [SerializeField] private float wildDamageScale = 0.8f;       // monster liar biasa sedikit dilunakkan
    [SerializeField] private float secondsPerChar = 0.018f;      // pesan panjang tampil lebih lama
    [SerializeField] private float evolveSeconds = 2.4f;

    public event Action Started;
    public event Action<string> MessageShown;
    public event Action ChoiceRequested;
    public event Action<MonsterInstance> Attacked;               // versi demo: animasi menerjang
    public event Action<MonsterInstance, int> Hit;
    public event Action Missed;
    public event Action<MonsterInstance> Healed;
    public event Action<MonsterInstance> Fainted;                // versi demo: tenggelam & memudar
    public event Action Victory;                                 // versi demo: jingle menang
    public event Action<MonsterData> LevelUp;                   // data tampilan (sebelum evolusi)
    public event Action<MonsterData, MonsterData> Evolving;      // versi demo: animasi 2,4 detik
    public event Action<MonsterInstance> Evolved;
    public event Action<BattleResult> Ended;

    private readonly AttackAction attack = new AttackAction();
    private readonly SkillAction skill = new SkillAction();
    private BattleAction[] playerActions;

    private BattleSession session;
    private SceneFlow scenes;
    private BattleAction chosen;
    private BattleResult result;
    private int enemySkillUses;

    public PlayerParty Party { get; private set; }
    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public int SkillUses { get; private set; }
    public bool IsBoss => session.IsBoss;
    public bool LastHitCritical { get; private set; }
    public Element? LastHitElement { get; private set; }         // null = serangan biasa
    public IReadOnlyList<BattleAction> PlayerActions => playerActions;

    public void Construct(PlayerParty party, BattleSession battleSession, SceneFlow sceneFlow)
    {
        Party = party;
        session = battleSession;
        scenes = sceneFlow;
        playerActions = new BattleAction[] { attack, skill, new PotionAction(), new RunAction() };
    }

    public void Begin() => StartCoroutine(Run());

    public void Choose(BattleAction action)
    {
        if (chosen == null && action.CanUse(this)) chosen = action;
    }

    // Pesan tampil, lalu lanjut sendiri setelah (hold + panjang teks × 18 ms). E/Spasi/klik mempercepat.
    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);
        float wait = hold + text.Length * secondsPerChar;
        for (float t = 0f; t < wait; t += Time.deltaTime)
        {
            if (t > text.Length * secondsPerChar && MenuInput.Confirm) break;
            yield return null;
        }
    }

    public float DamageScale(MonsterInstance user) => user == Enemy && !IsBoss ? wildDamageScale : 1f;
    public void ConsumeSkill() => SkillUses--;
    public void MarkRun() => result = BattleResult.Run;
    public void RaiseAttacked(MonsterInstance user) => Attacked?.Invoke(user);
    public void RaiseMissed() => Missed?.Invoke();
    public void RaiseHealed(MonsterInstance target) => Healed?.Invoke(target);

    public void RaiseHit(MonsterInstance target, int damage, bool critical = false, Element? element = null)
    {
        LastHitCritical = critical;
        LastHitElement = element;
        Hit?.Invoke(target, damage);
    }

    private IEnumerator Run()
    {
        Player = Party.Partner;
        Enemy = session.Enemy;
        SkillUses = skillUsesPerBattle;
        enemySkillUses = 2;
        result = BattleResult.None;
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f);                      // tirai pembuka

        yield return Say(IsBoss
            ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!"
            : $"{Enemy.Data.DisplayName} liar Lv{Enemy.Level} muncul!", 0.9f);
        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosen = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosen != null);

            if (chosen is RunAction && IsBoss)                       // boss: giliran tidak hilang
            {
                yield return chosen.Execute(this, Player, Enemy);
                continue;
            }

            BattleAction enemyAction = PickEnemyAction();
            bool playerFirst = chosen.Priority > 0 || Player.Speed > Enemy.Speed
                               || (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f);
            if (playerFirst)
            {
                yield return Turn(chosen, Player, Enemy);
                yield return Turn(enemyAction, Enemy, Player);
            }
            else
            {
                yield return Turn(enemyAction, Enemy, Player);
                yield return Turn(chosen, Player, Enemy);
            }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f);
        scenes.EndBattle(result);
    }

    private IEnumerator Turn(BattleAction action, MonsterInstance user, MonsterInstance target)
    {
        if (result != BattleResult.None || user.IsFainted) yield break;   // battle sudah selesai
        yield return action.Execute(this, user, target);
        if (Enemy.IsFainted) yield return Win();
        else if (Player.IsFainted) yield return Lose();
    }

    private BattleAction PickEnemyAction()
    {
        if (enemySkillUses > 0 && UnityEngine.Random.value < 0.35f)
        {
            enemySkillUses--;
            return skill;
        }
        return attack;
    }

    private IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy);
        yield return new WaitForSeconds(0.6f);
        yield return Say(IsBoss ? $"{Enemy.Data.DisplayName} kalah!" : $"{Enemy.Data.DisplayName} liar kalah!", 0.8f);
        Victory?.Invoke();
        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say($"{Player.Data.DisplayName} mendapat {exp} EXP.", 0.9f);
        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);
                yield return Say(growth.Message, 0.9f);
                continue;
            }
            yield return Say($"Apa? {growth.From.DisplayName} mulai bersinar...", 0.9f);
            Evolving?.Invoke(growth.From, growth.To);
            yield return new WaitForSeconds(evolveSeconds);
            Evolved?.Invoke(Player);
            yield return Say(growth.Message, 1.2f);
        }
        if (!IsBoss)
        {
            int healed = Player.Heal(Mathf.RoundToInt(Player.MaxHP * 0.25f));
            if (healed > 0) yield return Say($"{Player.Data.DisplayName} beristirahat sejenak dan pulih {healed} HP.", 0.8f);
        }
    }

    private IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say($"{Player.Data.DisplayName} pingsan... Kamu berlari kembali ke desa.", 1.2f);
        Player.HealFull();
    }
}
