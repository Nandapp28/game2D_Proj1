// Versi demo — monster partner ikut berjalan di samping pemain (posisi berganti sesuai arah hadap).
using UnityEngine;

public class PartnerFollower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private float frameSeconds = 0.25f;

    private PlayerParty party;
    private PlayerMovement player;

    public void Construct(PlayerParty playerParty, PlayerMovement playerMovement)
    {
        party = playerParty;
        player = playerMovement;
    }

    void LateUpdate()
    {
        MonsterInstance m = party != null ? party.Partner : null;
        body.enabled = m != null;
        if (m == null) return;

        Vector2 f = player.Facing;
        // offset dalam piksel demo (16 piksel = 1 unit): bawah, kiri, kanan, atas
        Vector2 offset = f.y < 0 ? new Vector2(-12, 2) : f.x < 0 ? new Vector2(10, 4)
                       : f.x > 0 ? new Vector2(-10, 4) : new Vector2(12, -2);
        transform.position = player.transform.position + (Vector3)(offset / 16f);

        int frame = Mathf.FloorToInt(Time.time / frameSeconds) % 2;          // idle 2 frame
        body.sprite = m.Data.Frames[frame];
        body.flipX = f.x < 0;
        float scale = m.Data.Frames[0].rect.width <= 16f ? 1f : 0.75f;     // tahap 2–3 sedikit dikecilkan
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
