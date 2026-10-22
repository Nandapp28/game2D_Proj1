// P6 — data yang ditulis ke file JSON. Hanya tipe sederhana: JsonUtility menyimpan
// referensi objek Unity sebagai InstanceID yang tidak berlaku di sesi berikutnya,
// jadi monster disimpan lewat id string, bukan referensi MonsterData.
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = SaveService.CurrentVersion;
    public string savedAt;
    public string scene = "Scn_World";
    public Vector2 playerPosition;

    public string partnerId;
    public int partnerLevel;
    public int partnerExp;
    public int partnerHp;

    public int potions;
    public float playtime;                 // versi demo: layar DEMO SELESAI
    public List<string> flags = new List<string>();
}
