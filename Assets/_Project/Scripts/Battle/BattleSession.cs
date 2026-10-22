// P4 — "surat pengantar" dari dunia ke battle: siapa lawannya, dan siapa yang dikabari hasilnya.
// Versi demo: event Ended (dunia: pindah ke desa saat kalah, ending setelah boss) dan waktu aman 2,2 detik.
using System;
using UnityEngine;

public enum BattleResult { None, Win, Lose, Run }

public class BattleSession
{
    public const float SafeSeconds = 2.2f;                    // monster liar tidak mengejar setelah battle

    private Action<BattleResult> onFinished;
    private float safeUntil;

    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsSafe => Time.time < safeUntil;

    public event Action<BattleResult, bool> Ended;           // (hasil, apakah boss)

    public void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> finished)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = finished;
    }

    public void Finish(BattleResult result)
    {
        Action<BattleResult> callback = onFinished;
        bool wasBoss = IsBoss;
        onFinished = null;
        Enemy = null;
        IsBoss = false;
        safeUntil = Time.time + SafeSeconds;
        callback?.Invoke(result);
        Ended?.Invoke(result, wasBoss);
    }
}
