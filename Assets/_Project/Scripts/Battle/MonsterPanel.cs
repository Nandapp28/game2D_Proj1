// P5 — satu sisi battle: sprite di dunia (SpriteRenderer) + info di UI (nama, level, HP).
// Versi demo: animasi idle 2 frame, menerjang, berkedip putih, pingsan (tenggelam + memudar),
// evolusi (sprite lama ↔ baru makin cepat), angka HP, bar EXP, dan elemen lawan.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPanel : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private bool faceRight;               // partner pemain menghadap lawan
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text elementText;             // opsional (panel lawan)
    [SerializeField] private Text hpText;                  // opsional (panel pemain)
    [SerializeField] private Image hpFill;                 // Image Type: Filled, Horizontal
    [SerializeField] private Image expFill;                // opsional (panel pemain)
    [SerializeField] private float lungeDistance = 0.875f; // 14 px
    [SerializeField] private float lungeSeconds = 0.26f;

    private MonsterInstance shown;
    private MonsterData shownData;
    private MonsterData evolveData;                        // diisi selama animasi evolusi
    private float hpShow = -1f;
    private Vector3 home;
    private float lungeStart = -10f;
    private bool hurting, evolving, fainted;

    void Awake() => home = body.transform.localPosition;

    // displayAs: tampilkan data lain (mis. nama sebelum evolusi) tanpa mengubah monsternya.
    public void Show(MonsterInstance monster, bool boss = false, MonsterData displayAs = null)
    {
        shown = monster;
        shownData = displayAs != null ? displayAs : monster.Data;
        body.flipX = faceRight;
        nameText.text = (boss ? "★ " : "") + shownData.DisplayName;
        levelText.text = $"Lv{monster.Level}";
        if (elementText != null) elementText.text = monster.Data.Element.ToString();
        if (expFill != null) expFill.fillAmount = (float)monster.Exp / monster.ExpToNext;
        if (hpShow < 0f) hpShow = monster.CurrentHP;
        ApplyScale(shownData);
    }

    // Ukuran tampil sama dengan demo: 16 px ×3, 24 px ×2,4, 32 px ×2.
    private void ApplyScale(MonsterData data)
    {
        float px = data.Frames[0].rect.width;
        float s = px <= 16f ? 3f : px <= 24f ? 2.4f : 2f;
        body.transform.localScale = new Vector3(s, s, 1f);
    }

    void Update()
    {
        if (shown == null) return;
        if (!hurting && !evolving)
            body.sprite = shownData.Frames[Mathf.FloorToInt(Time.time / 0.35f) % 2];

        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f);           // mendekat 15% per frame (60 fps)
        hpShow = Mathf.Lerp(hpShow, shown.CurrentHP, k);
        float ratio = shown.CurrentHP <= 0 ? 0f : Mathf.Clamp01(hpShow / shown.MaxHP);
        hpFill.fillAmount = ratio;
        hpFill.color = HpColors.For(ratio);
        if (hpText != null) hpText.text = $"{Mathf.Max(0, Mathf.RoundToInt(hpShow))}/{shown.MaxHP}";
        if (expFill != null) expFill.fillAmount = (float)shown.Exp / shown.ExpToNext;

        if (!fainted)
        {
            float p = (Time.time - lungeStart) / lungeSeconds;
            float dx = p < 1f ? Mathf.Sin(p * Mathf.PI) * lungeDistance * (faceRight ? 1f : -1f) : 0f;
            body.transform.localPosition = home + new Vector3(dx, 0f, 0f);
        }
    }

    public void PlayLunge() => lungeStart = Time.time;

    public void PlayHurt() => StartCoroutine(Hurt());

    public void PlayFaint() => StartCoroutine(Faint());

    public void PlayEvolve(MonsterData from, MonsterData to, float seconds) => StartCoroutine(Evolve(from, to, seconds));

    private IEnumerator Hurt()
    {
        hurting = true;
        for (float t = 0f; t < 0.4f; t += Time.deltaTime)
        {
            body.sprite = shownData.Frames[Mathf.FloorToInt(t / 0.07f) % 2 == 0 ? 3 : 0];   // frame 3 = putih
            yield return null;
        }
        hurting = false;
    }

    private IEnumerator Faint()
    {
        fainted = true;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            float k = t / 0.5f;
            body.transform.localPosition = home + new Vector3(0f, -1.25f * k, 0f);   // tenggelam 20 px
            body.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }
        body.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator Evolve(MonsterData from, MonsterData to, float seconds)
    {
        evolving = true;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            float p = t / seconds;
            float swap = Mathf.Max(0.04f, 0.22f - p * 0.2f);                       // makin lama makin cepat
            MonsterData d = Mathf.FloorToInt(Time.time / swap) % 2 == 0 ? to : from;
            bool white = p > 0.2f && Mathf.FloorToInt(Time.time / 0.09f) % 3 == 0;
            body.sprite = d.Frames[white ? 3 : 0];
            ApplyScale(d);
            yield return null;
        }
        evolving = false;
    }
}
