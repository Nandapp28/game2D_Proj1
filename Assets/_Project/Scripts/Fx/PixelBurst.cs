// Versi demo — percikan partikel kotak 1–2 piksel (daun di rumput tinggi, kilau Batu Simpan, starter baru).
// Memakai pool SpriteRenderer sederhana: objek dipakai ulang, tidak ada Instantiate setiap percikan.
using System.Collections.Generic;
using UnityEngine;

public class PixelBurst : MonoBehaviour
{
    [SerializeField] private Sprite pixel;                     // UI_Pixel (4×4 putih, PPU 16)
    [SerializeField] private string sortingLayer = "Characters";
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private float gravity = 2.5f;             // unit/detik² (40 px/detik² di demo)

    private class Particle
    {
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life, max;
    }

    private readonly List<Particle> active = new List<Particle>();
    private readonly Stack<Particle> free = new Stack<Particle>();

    // speed dan ukuran dalam piksel demo (1 piksel = 1/16 unit)
    public void Play(Vector3 position, Color color, int count, float speed, float life)
    {
        for (int i = 0; i < count; i++)
        {
            Particle p = free.Count > 0 ? free.Pop() : Create();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float s = Random.Range(speed * 0.3f, speed) / 16f;
            p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * s;
            p.life = p.max = life;
            p.sr.color = color;
            p.sr.transform.position = position;
            p.sr.transform.localScale = Vector3.one * (Random.value < 0.5f ? 0.25f : 0.5f);
            p.sr.gameObject.SetActive(true);
            active.Add(p);
        }
    }

    private Particle Create()
    {
        var go = new GameObject("Px");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pixel;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;
        return new Particle { sr = sr };
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Particle p = active[i];
            p.life -= dt;
            if (p.life <= 0f)
            {
                p.sr.gameObject.SetActive(false);
                active.RemoveAt(i);
                free.Push(p);
                continue;
            }
            p.velocity.y -= gravity * dt;
            p.sr.transform.position += (Vector3)(p.velocity * dt);
            Color c = p.sr.color;
            c.a = p.life / p.max;
            p.sr.color = c;
        }
    }
}
