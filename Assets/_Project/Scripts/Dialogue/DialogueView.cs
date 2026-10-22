// P6 — kotak dialog (Canvas_Dialogue di Scn_Bootstrap). Memakai UI Text (legacy):
// efek mesin tik dibuat dengan memotong string (TMP punya maxVisibleCharacters).
// Versi demo: gaya navy-emas, label nama emas, panah ▼ berkedip, pilihan berbentuk teks (TextMenu),
// gaya narasi (biru, tampil di atas layar hitam), dan klik di mana saja = lanjut.
using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject root;              // penangkap klik layar penuh + kotak
    [SerializeField] private Button clickCatcher;          // klik di mana saja = lanjut
    [SerializeField] private Image box;
    [SerializeField] private Sprite normalPanel;           // UI_Panel_Navy
    [SerializeField] private Sprite narrationPanel;        // UI_Panel_Narr
    [SerializeField] private GameObject speakerTag;
    [SerializeField] private RectTransform speakerTagRect;
    [SerializeField] private Text speakerText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Text continueHint;            // "▼"
    [SerializeField] private RectTransform choicesPanel;
    [SerializeField] private TextMenu choices;
    [SerializeField] private int normalOrder = 20;
    [SerializeField] private int narrationOrder = 110;     // di atas Fader (100)

    private string fullText = "";
    private bool hintOn;

    public event Action Clicked;
    public event Action<int> ChoicePicked;

    void Awake()
    {
        clickCatcher.onClick.AddListener(() => Clicked?.Invoke());
        choices.Picked += i => ChoicePicked?.Invoke(i);
    }

    void Update()
    {
        if (hintOn) continueHint.enabled = Mathf.FloorToInt(Time.unscaledTime / 0.3f) % 2 == 1;   // berkedip
    }

    public void Show(bool narration)
    {
        root.SetActive(true);
        box.sprite = narration ? narrationPanel : normalPanel;
        canvas.sortingOrder = narration ? narrationOrder : normalOrder;
    }

    public void Hide()
    {
        root.SetActive(false);
        HideChoices();
        SetContinueHint(false);
    }

    public void SetLine(string speaker, string text)
    {
        bool named = !string.IsNullOrEmpty(speaker);
        speakerTag.SetActive(named);
        speakerText.text = speaker;
        if (named)                                          // lebar label mengikuti panjang nama
            speakerTagRect.sizeDelta = new Vector2(Mathf.Min(488f, speaker.Length * 20f + 64f), speakerTagRect.sizeDelta.y);
        RectTransform body = bodyText.rectTransform;
        body.offsetMax = new Vector2(body.offsetMax.x, named ? -40f : -32f);
        fullText = text;
        bodyText.text = "";
        SetContinueHint(false);
    }

    public void SetVisibleCharacters(int count) =>
        bodyText.text = fullText.Substring(0, Mathf.Clamp(count, 0, fullText.Length));

    public void SetContinueHint(bool visible)
    {
        hintOn = visible;
        continueHint.enabled = false;
    }

    public void ShowChoices(string[] options)
    {
        SetContinueHint(false);
        choicesPanel.sizeDelta = new Vector2(choicesPanel.sizeDelta.x, options.Length * 52f + 40f);
        choicesPanel.gameObject.SetActive(true);
        choices.Open(options);
    }

    public void HideChoices()
    {
        choices.Close();
        choicesPanel.gameObject.SetActive(false);
    }
}
