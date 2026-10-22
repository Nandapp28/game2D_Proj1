// P4 — state pemain yang bertahan antar-scene: monster partner & jumlah Ramuan.
// Versi demo: event PotionsChanged (HUD "Ramuan ×N") dan waktu bermain.
using System;

public class PlayerParty
{
    public const int StartingPotions = 3;

    private int potions = StartingPotions;

    public MonsterInstance Partner { get; private set; }
    public float Playtime { get; set; }                         // detik, ikut disimpan

    public int Potions
    {
        get => potions;
        set { potions = Math.Max(0, value); PotionsChanged?.Invoke(potions); }
    }

    public event Action<MonsterInstance> PartnerChanged;       // P5: HUD
    public event Action<int> PotionsChanged;                   // versi demo: HUD

    public void SetPartner(MonsterInstance monster)
    {
        Partner = monster;
        PartnerChanged?.Invoke(monster);
    }

    public void Reset()                                         // P6: Mulai baru
    {
        Potions = StartingPotions;
        Playtime = 0f;
        SetPartner(null);
    }
}
