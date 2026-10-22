// P4 — catatan pertumbuhan setelah mendapat EXP (naik level / evolusi).
// Versi demo: From/To dipakai animasi evolusi (sprite lama ↔ sprite baru bergantian).
public enum GrowthType { LevelUp, Evolved }

public readonly struct GrowthEvent
{
    public readonly GrowthType Type;
    public readonly string Message;
    public readonly MonsterData From;
    public readonly MonsterData To;

    public GrowthEvent(GrowthType type, string message, MonsterData from = null, MonsterData to = null)
    {
        Type = type;
        Message = message;
        From = from;
        To = to;
    }
}
