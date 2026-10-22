// P4 — Satu file = satu tahap evolusi.
using UnityEngine;

[CreateAssetMenu(fileName = "Mon_", menuName = "Pulau Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private string id;                  // P4: "MON_001" (kolom id di CSV)
    [SerializeField] private string displayName;
    [SerializeField] private Element element;
    [SerializeField] private Sprite[] frames;            // Idle0, Idle1, Attack, Hurt
    [SerializeField] private int baseHP, baseATK, baseDEF, baseSPD;
    [SerializeField] private string skillName = "Semburan Api";
    [SerializeField] private MonsterData evolvesTo;
    [SerializeField] private int evolveLevel;

    public string Id => id;
    public string DisplayName => displayName;
    public Element Element => element;
    public Sprite[] Frames => frames;
    public int BaseHP => baseHP;
    public int BaseATK => baseATK;
    public int BaseDEF => baseDEF;
    public int BaseSPD => baseSPD;
    public string SkillName => skillName;
    public MonsterData EvolvesTo => evolvesTo;
    public int EvolveLevel => evolveLevel;
}
