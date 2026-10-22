// P4 — tabel kelemahan elemen (sama dengan kolom strongAgainst di MonsterDatabase.csv).
public static class ElementChart
{
    public static float Multiplier(Element attacker, Element defender)
    {
        if (Beats(attacker, defender)) return 1.5f;
        if (Beats(defender, attacker)) return 0.75f;
        return 1f;
    }

    private static bool Beats(Element a, Element b) =>
        (a == Element.Pyro && b == Element.Sprout) ||
        (a == Element.Sprout && b == Element.Aqua) ||
        (a == Element.Aqua && b == Element.Pyro) ||
        (a == Element.Volt && b == Element.Aqua);
}
