// P6 — penanda alur cerita. HashSet: cek "sudah terjadi?" dalam O(1).
using System;
using System.Collections.Generic;

public class StoryFlags
{
    public const string IntroSeen = "intro_seen";
    public const string HasStarter = "has_starter";
    public const string BossDefeated = "boss_defeated";
    public const string Gift = "gift";                          // versi demo: hadiah Ramuan dari Pak Dagang

    private readonly HashSet<string> flags = new HashSet<string>();

    public event Action<string> Changed;

    public bool Has(string key) => flags.Contains(key);

    public void Set(string key)
    {
        if (flags.Add(key)) Changed?.Invoke(key);
    }

    public List<string> ToList() => new List<string>(flags);   // P6 save: untuk disimpan

    public void Load(IEnumerable<string> keys)                  // P6 save: dari file save
    {
        flags.Clear();
        foreach (string key in keys) flags.Add(key);
    }
}
