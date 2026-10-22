// P4 — monster yang "hidup" saat game berjalan. Class C# biasa (bukan MonoBehaviour),
// sehingga bisa dibawa dari dunia ke battle dan disimpan ke file.
using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int CurrentHP { get; private set; }

    public event Action Changed;                                  // P5: UI berlangganan

    private float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
    public bool IsFainted => CurrentHP <= 0;

    public MonsterInstance(MonsterData data, int level, int exp = 0, int hp = -1)
    {
        Data = data;
        Level = Mathf.Max(1, level);
        Exp = exp;
        CurrentHP = hp < 0 ? MaxHP : Mathf.Min(hp, MaxHP);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }

    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Changed?.Invoke();
        return CurrentHP - before;
    }

    public void HealFull() => Heal(MaxHP);

    public List<GrowthEvent> GainExp(int amount)
    {
        var events = new List<GrowthEvent>();
        Exp += amount;
        while (Exp >= ExpToNext)
        {
            Exp -= ExpToNext;
            int oldMax = MaxHP;
            Level++;
            CurrentHP += MaxHP - oldMax;
            events.Add(new GrowthEvent(GrowthType.LevelUp, $"{Data.DisplayName} naik ke Lv {Level}!", Data, Data));

            if (Data.EvolvesTo == null || Level < Data.EvolveLevel) continue;   // belum berevolusi
            MonsterData from = Data;
            Data = Data.EvolvesTo;
            CurrentHP = MaxHP;
            events.Add(new GrowthEvent(GrowthType.Evolved, $"{from.DisplayName} berevolusi menjadi {Data.DisplayName}!", from, Data));
        }
        Changed?.Invoke();
        return events;
    }
}
