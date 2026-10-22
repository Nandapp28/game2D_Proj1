// P5 — HUD di Scn_World: potret, nama, level, HP, EXP partner, dan nama area.
// Versi demo: angka HP ("27/40") dan jumlah Ramuan ("Ramuan ×3") di bawah nama area.
using UnityEngine;
using UnityEngine.UI;

public class PartnerHud : MonoBehaviour
{
    [SerializeField] private GameObject partnerRoot;
    [SerializeField] private Image portrait;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image hpFill;          // Image Type: Filled, Horizontal
    [SerializeField] private Text hpText;           // versi demo
    [SerializeField] private Image expFill;         // Image Type: Filled, Horizontal
    [SerializeField] private Text areaText;
    [SerializeField] private Text potionText;       // versi demo

    private PlayerParty party;
    private MonsterInstance shown;

    public void Construct(PlayerParty playerParty)
    {
        party = playerParty;
        party.PartnerChanged += Bind;
        party.PotionsChanged += ShowPotions;
        Bind(party.Partner);
        ShowPotions(party.Potions);
    }

    void OnDestroy()
    {
        if (party != null) { party.PartnerChanged -= Bind; party.PotionsChanged -= ShowPotions; }
        if (shown != null) shown.Changed -= Refresh;
    }

    public void ShowArea(AreaZone zone) => areaText.text = zone.AreaName;

    private void ShowPotions(int count) => potionText.text = $"Ramuan ×{count}";

    private void Bind(MonsterInstance monster)
    {
        if (shown != null) shown.Changed -= Refresh;
        shown = monster;
        partnerRoot.SetActive(monster != null);
        if (monster == null) return;
        monster.Changed += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        portrait.sprite = shown.Data.Frames[0];
        nameText.text = shown.Data.DisplayName;
        levelText.text = $"Lv{shown.Level}";
        float hp = (float)shown.CurrentHP / shown.MaxHP;
        hpFill.fillAmount = hp;
        hpFill.color = HpColors.For(hp);
        hpText.text = $"{shown.CurrentHP}/{shown.MaxHP}";
        expFill.fillAmount = (float)shown.Exp / shown.ExpToNext;
    }
}
