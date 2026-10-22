// P1 v1–v3 → P2 v4 (IMoveInput + PlayerConfig) → P3 v5: membuka Velocity & Facing untuk animasi dan interaksi.
// Versi demo: Teleport (kembali ke desa setelah kalah, posisi dari file save).
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Rigidbody2D rb;
    private IMoveInput input;
    private Vector2 velocity;

    public Vector2 Velocity => velocity;                               // P3
    public Vector2 Facing { get; private set; } = Vector2.down;        // P3

    public void Construct(IMoveInput moveInput) => input = moveInput;

    void Update()
    {
        if (input == null) return;
        Vector2 move = input.Move;
        float speed = input.IsRunning ? config.RunSpeed : config.WalkSpeed;
        velocity = move * speed;
        if (move.sqrMagnitude > 0.01f) Facing = ToCardinal(move);      // P3
    }

    void FixedUpdate() => rb.linearVelocity = velocity;

    public void Teleport(Vector2 position, Vector2 facing)
    {
        rb.position = position;
        transform.position = position;
        velocity = Vector2.zero;
        Facing = facing;
    }

    // P3 — arah diagonal dibulatkan ke 4 arah sesuai sprite yang tersedia.
    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
}
