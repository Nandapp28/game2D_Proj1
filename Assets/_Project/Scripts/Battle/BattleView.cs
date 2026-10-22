// P5 — View (pola MVP): hanya menampilkan dan melaporkan pilihan. Tidak ada aturan battle di sini.
// Versi demo: pesan diketik 55 huruf/detik, menu teks 2×2 (TextMenu), tirai pembuka, kilat putih,
// dan cincin emas saat evolusi.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleView : MonoBehaviour
{
    [SerializeField] private Text messageText;
    [SerializeField] private Text hintText;                // "Jurus elemen: Semburan Api"
    [SerializeField] private TextMenu menu;                // Serang, Jurus, Ramuan, Kabur
    [SerializeField] private MonsterPanel playerPanel;
    [SerializeField] private MonsterPanel enemyPanel;
    [SerializeField] private RectTransform wipeTop;
    [SerializeField] private RectTransform wipeBottom;
    [SerializeField] private Image flash;
    [SerializeField] private RectTransform[] rings;        // 3 cincin evolusi
    [SerializeField] private float charsPerSecond = 55f;

    private Coroutine typing;
    private int lastPick;

    public event Action<int> ActionClicked;
    public MonsterPanel PlayerPanel => playerPanel;
    public MonsterPanel EnemyPanel => enemyPanel;

    void Awake()
    {
        menu.Picked += index => { lastPick = index; ActionClicked?.Invoke(index); };
        foreach (RectTransform r in rings) r.gameObject.SetActive(false);
        flash.color = new Color(1f, 1f, 1f, 0f);
    }

    public void ShowMessage(string text)
    {
        hintText.enabled = false;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type(text));
    }

    private IEnumerator Type(string text)
    {
        for (float shown = 0f; shown < text.Length; shown += charsPerSecond * Time.deltaTime)
        {
            messageText.text = text.Substring(0, (int)shown);
            yield return null;
        }
        messageText.text = text;
    }

    public void ShowMenu(IReadOnlyList<string> labels, IReadOnlyList<bool> usable, string prompt, string hint)
    {
        if (typing != null) StopCoroutine(typing);
        messageText.text = prompt;
        hintText.text = hint;
        hintText.enabled = true;
        int start = usable[lastPick] ? lastPick : 0;        // ingat pilihan terakhir (seperti demo)
        menu.Open(new List<string>(labels).ToArray(), new List<bool>(usable).ToArray(), start);
    }

    public void HideMenu()
    {
        menu.Close();
        hintText.enabled = false;
    }

    public void PlayIntro() => StartCoroutine(Wipe());

    public void PlayFlash() => StartCoroutine(Flash());

    public void PlayEvolveRings(float seconds) => StartCoroutine(Rings(seconds));

    private IEnumerator Wipe()
    {
        for (float t = 0f; t < 0.48f; t += Time.deltaTime)
        {
            float h = (1f - t / 0.48f) * 360f;              // dua tirai menutup dari tengah layar
            wipeTop.sizeDelta = new Vector2(wipeTop.sizeDelta.x, h);
            wipeBottom.sizeDelta = new Vector2(wipeBottom.sizeDelta.x, h);
            yield return null;
        }
        wipeTop.sizeDelta = wipeBottom.sizeDelta = new Vector2(wipeTop.sizeDelta.x, 0f);
    }

    private IEnumerator Flash()
    {
        for (float a = 1f; a > 0f; a -= Time.deltaTime * 2f)
        {
            flash.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        flash.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator Rings(float seconds)
    {
        foreach (RectTransform r in rings) r.gameObject.SetActive(true);
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            for (int i = 0; i < rings.Length; i++)
            {
                float radius = (Time.time * 125f + i * 20f) % 60f;            // piksel demo
                rings[i].sizeDelta = Vector2.one * radius * 8f;               // diameter × 4 unit kanvas
            }
            yield return null;
        }
        foreach (RectTransform r in rings) r.gameObject.SetActive(false);
    }
}
