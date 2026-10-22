// P6 — menerjemahkan state game (objek) <-> SaveData (teks).
using UnityEngine;

public static class SaveMapper
{
    public static SaveData Capture(GameServices s, Vector2 playerPosition)
    {
        MonsterInstance partner = s.Party.Partner;
        return new SaveData
        {
            playerPosition = playerPosition,
            partnerId = partner != null ? partner.Data.Id : null,
            partnerLevel = partner != null ? partner.Level : 0,
            partnerExp = partner != null ? partner.Exp : 0,
            partnerHp = partner != null ? partner.CurrentHP : 0,
            potions = s.Party.Potions,
            playtime = s.Party.Playtime,
            flags = s.Flags.ToList(),
        };
    }

    public static void Apply(SaveData data, GameServices s)
    {
        s.Party.Potions = data.potions;
        s.Party.Playtime = data.playtime;
        s.Flags.Load(data.flags);
        MonsterData species = s.Database.GetById(data.partnerId);
        s.Party.SetPartner(species == null
            ? null
            : new MonsterInstance(species, data.partnerLevel, data.partnerExp, data.partnerHp));
    }
}
