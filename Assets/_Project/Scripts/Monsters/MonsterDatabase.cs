// P4 — daftar semua MonsterData, dicari lewat id (dipakai Save/Load di P6).
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    public IReadOnlyList<MonsterData> All => monsters;

    public MonsterData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return monsters.Find(m => m.Id == id);
    }
}
