// P5 — setiap pilihan di menu battle adalah turunan BattleAction (inheritance + polymorphism).
// Versi demo: teks dan jeda pesan disamakan dengan demo, ada event Attacked (animasi menerjang).
using System.Collections;
using UnityEngine;

public abstract class BattleAction
{
    public abstract string Label { get; }
    public virtual int Priority => 0;                               // >0 = jalan lebih dulu
    public virtual string MenuLabel(BattleSystem battle) => Label;
    public virtual bool CanUse(BattleSystem battle) => true;
    public abstract IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target);
}

public class AttackAction : BattleAction
{
    public override string Label => "Serang";
    protected virtual int Power => 14;
    protected virtual string Move(MonsterInstance user) => null;    // null = serangan biasa
    protected virtual float ElementBonus(MonsterInstance user, MonsterInstance target) => 1f;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string who = (user == battle.Enemy && !battle.IsBoss ? "Lawan " : "") + user.Data.DisplayName;
        string move = Move(user);
        yield return battle.Say(move == null ? $"{who} menyerang!" : $"{who} memakai {move}!", 0.7f);
        battle.RaiseAttacked(user);
        yield return new WaitForSeconds(0.2f);
        if (Random.value < 0.06f)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break;
        }
        float bonus = ElementBonus(user, target);
        int damage = DamageFormula.Calculate(user, target, Power, bonus * battle.DamageScale(user), out bool critical);
        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical, move != null ? user.Data.Element : (Element?)null);
        yield return new WaitForSeconds(0.45f);
        if (critical) yield return battle.Say("Serangan kritis!", 0.7f);
        if (bonus > 1f) yield return battle.Say("Sangat efektif!", 0.7f);
        else if (bonus < 1f) yield return battle.Say("Kurang efektif...", 0.7f);
    }
}

public class SkillAction : AttackAction
{
    public override string Label => "Jurus";
    protected override int Power => 22;
    public override string MenuLabel(BattleSystem battle) => $"Jurus ({battle.SkillUses})";
    public override bool CanUse(BattleSystem battle) => battle.SkillUses > 0;
    protected override string Move(MonsterInstance user) => user.Data.SkillName;
    protected override float ElementBonus(MonsterInstance user, MonsterInstance target) =>
        ElementChart.Multiplier(user.Data.Element, target.Data.Element);

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (user == battle.Player) battle.ConsumeSkill();
        yield return base.Execute(battle, user, target);
    }
}

public class PotionAction : BattleAction
{
    public override string Label => "Ramuan";
    public override int Priority => 1;
    public override string MenuLabel(BattleSystem battle) => $"Ramuan ({battle.Party.Potions})";
    public override bool CanUse(BattleSystem battle) => battle.Party.Potions > 0;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        battle.Party.Potions--;
        int healed = user.Heal(Mathf.RoundToInt(user.MaxHP * 0.4f));
        battle.RaiseHealed(user);
        yield return battle.Say($"Ramuan dipakai. HP pulih {healed}.", 0.8f);
    }
}

public class RunAction : BattleAction
{
    public override string Label => "Kabur";
    public override int Priority => 2;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (battle.IsBoss)
        {
            yield return battle.Say("Tidak bisa kabur dari pertarungan ini!", 0.9f);
        }
        else if (Random.value < 0.7f)
        {
            battle.MarkRun();
            yield return battle.Say("Berhasil kabur!", 0.7f);
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.7f);
        }
    }
}
