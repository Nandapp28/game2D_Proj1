// Versi demo — daftar pilihan berbentuk teks ("▶ Mulai baru"). Dipakai layar judul, menu jeda,
// pilihan dialog, dan menu battle. Bisa dengan keyboard (panah/WASD + E/Spasi/Enter) atau mouse (klik).
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextMenu : MonoBehaviour
{
    [SerializeField] private Text[] rows;                 // satu Text per pilihan (masing-masing punya Button)
    [SerializeField] private int columns = 1;             // 2 = grid 2×2 (menu battle)
    [SerializeField] private bool showCursor = true;      // tanda ▶ di depan pilihan aktif
    [SerializeField] private Color normal = new Color32(237, 233, 247, 255);
    [SerializeField] private Color selected = new Color32(255, 198, 26, 255);
    [SerializeField] private Color disabled = new Color32(92, 86, 128, 255);
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip okClip;
    [SerializeField] private AudioClip cancelClip;

    private string[] labels = new string[0];
    private bool[] usable = new bool[0];
    private int count;
    private bool acceptInput;

    public int Selected { get; private set; }
    public event Action<int> Picked;

    public static AudioService Sfx;                      // diisi GameBootstrap (opsional)

    void Awake()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            Button b = rows[i].GetComponent<Button>();
            if (b != null) b.onClick.AddListener(() => Click(index));
            // Catatan: jangan memakai "??" untuk component Unity (objek yang sudah dihancurkan tidak benar-benar null).
            var trigger = rows[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = rows[i].gameObject.AddComponent<EventTrigger>();
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => Hover(index));
            trigger.triggers.Add(enter);
        }
    }

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        labels = options;
        count = Mathf.Min(options.Length, rows.Length);
        usable = enabled ?? new bool[count];
        if (enabled == null) for (int i = 0; i < count; i++) usable[i] = true;
        for (int i = 0; i < rows.Length; i++) rows[i].gameObject.SetActive(i < count);
        Selected = Mathf.Clamp(start, 0, count - 1);
        if (!usable[Selected]) Selected = FirstUsable();
        gameObject.SetActive(true);
        acceptInput = false;                              // abaikan tombol yang membuka menu ini
        Redraw();
    }

    public void Close()
    {
        count = 0;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (count == 0) return;
        if (!acceptInput) { acceptInput = true; return; }
        int rowsCount = Mathf.CeilToInt(count / (float)columns);
        if (columns == 1)
        {
            if (MenuInput.Down) Move((Selected + 1) % count);
            if (MenuInput.Up) Move((Selected + count - 1) % count);
        }
        else
        {
            int c = Selected % columns, r = Selected / columns;
            if (MenuInput.Right && c < columns - 1) Move(Selected + 1);
            if (MenuInput.Left && c > 0) Move(Selected - 1);
            if (MenuInput.Down && r < rowsCount - 1) Move(Selected + columns);
            if (MenuInput.Up && r > 0) Move(Selected - columns);
        }
        if (MenuInput.Confirm) Pick(Selected);
    }

    private void Move(int index)
    {
        if (index < 0 || index >= count) return;
        Selected = index;
        if (Sfx != null) Sfx.PlaySfx(moveClip, 0f);
        Redraw();
    }

    private void Hover(int index) { if (index < count && index != Selected) Move(index); }

    private void Click(int index)
    {
        if (index >= count || !acceptInput) return;
        Selected = index;
        Redraw();
        Pick(index);
    }

    private void Pick(int index)
    {
        if (!usable[index])
        {
            if (Sfx != null) Sfx.PlaySfx(cancelClip, 0f);       // pilihan redup: tidak bisa dipilih
            return;
        }
        if (Sfx != null) Sfx.PlaySfx(okClip, 0f);
        Picked?.Invoke(index);
    }

    private int FirstUsable()
    {
        for (int i = 0; i < count; i++) if (usable[i]) return i;
        return 0;
    }

    private void Redraw()
    {
        for (int i = 0; i < count; i++)
        {
            bool on = i == Selected;
            rows[i].text = (showCursor ? (on ? "▶ " : "  ") : "") + labels[i];
            rows[i].color = !usable[i] ? disabled : on ? selected : normal;
        }
    }
}
