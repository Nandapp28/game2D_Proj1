// Versi demo — Pak Dagang: kunjungan pertama memberi 3 Ramuan gratis (flag "gift").
using System.Collections;
using UnityEngine;

public class MerchantNpc : Npc
{
    [SerializeField] private int giftPotions = 3;
    [SerializeField] private AudioClip giftClip;

    private PlayerParty party;
    private AudioService audioService;

    public void ConstructMerchant(PlayerParty playerParty, AudioService audio)
    {
        party = playerParty;
        audioService = audio;
    }

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.Gift))
        {
            Flags.Set(StoryFlags.Gift);
            party.Potions += giftPotions;
            if (audioService != null) audioService.PlaySfx(giftClip, 0f);
            yield return Dialogue.Say(NpcName, "Petualang baru? Bawa 3 Ramuan ini. Gratis untuk hari pertama!");
            yield return Dialogue.Say("", $"Kamu sekarang punya {party.Potions} Ramuan.");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Ramuan memulihkan 40% HP. Pakai di tengah pertarungan kalau terdesak.");
        }
    }
}
