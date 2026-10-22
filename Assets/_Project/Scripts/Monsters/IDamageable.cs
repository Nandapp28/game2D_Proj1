// P4 — apa pun yang bisa menerima damage.
public interface IDamageable
{
    int CurrentHP { get; }
    void TakeDamage(int amount);
}
