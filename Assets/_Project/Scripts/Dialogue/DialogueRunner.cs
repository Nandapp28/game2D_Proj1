// P6 — memutar dialog: efek mesin tik, maju dengan E/Spasi/Enter (atau klik di mana saja), pilihan opsional.
// Tinggal di Scn_Bootstrap agar bisa dipakai semua scene.
// Versi demo: selain DialogueData, dialog bisa ditulis sebagai urutan (coroutine) dengan Say, Narrate, Choose:
//     dialogue.Run(Talk());
//     IEnumerator Talk() { yield return dialogue.Say("Tetua Rahmat", "Halo!"); ... }
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView view;
    [SerializeField] private float charsPerSecond = 55f;
    [SerializeField] private AudioClip blip;               // suara ketik (opsional)

    private InputGate gate;
    private AudioService audioService;
    private bool clicked;
    private int picked = -1;

    public bool IsRunning { get; private set; }

    public void Construct(InputGate inputGate, AudioService audio)
    {
        gate = inputGate;
        audioService = audio;
        view.Hide();
        view.Clicked += () => clicked = true;
        view.ChoicePicked += index => picked = index;
    }

    // Menjalankan satu urutan dialog. Pemain berhenti (gate dikunci) sampai urutan selesai.
    public void Run(IEnumerator sequence)
    {
        if (!IsRunning && sequence != null) StartCoroutine(RunRoutine(sequence));
    }

    // Versi P6 tetap ada: memutar DialogueData (dipakai Sari dan Batu Simpan).
    public void Play(DialogueData data, Action<int> onChoice = null)
    {
        if (data != null) Run(PlayData(data, onChoice));
    }

    private IEnumerator RunRoutine(IEnumerator sequence)
    {
        IsRunning = true;
        gate.Lock();                       // pemain berhenti, interaksi lain diabaikan
        yield return null;                 // abaikan tombol yang membuka dialog ini
        clicked = false;
        yield return sequence;
        view.Hide();
        yield return null;                 // tombol penutup tidak langsung membuka dialog lagi
        gate.Unlock();
        IsRunning = false;
    }

    public IEnumerator PlayData(DialogueData data, Action<int> onChoice = null)
    {
        int last = data.Lines.Count - 1;
        for (int i = 0; i <= last; i++)
        {
            DialogueLine line = data.Lines[i];
            if (i == last && data.Choices.Count > 0)
            {
                int choice = -1;
                yield return Choose(line.speaker, line.text, new List<string>(data.Choices).ToArray(), c => choice = c);
                onChoice?.Invoke(choice);
            }
            else yield return Say(line.speaker, line.text);
        }
    }

    public IEnumerator Say(string speaker, string text)
    {
        view.Show(false);
        yield return TypeLine(speaker, text, true);
    }

    // Narasi: kotak biru tanpa nama, tetap terlihat walau layar sedang hitam.
    public IEnumerator Narrate(string text)
    {
        view.Show(true);
        yield return TypeLine("", text, true);
    }

    public IEnumerator Choose(string speaker, string prompt, string[] options, Action<int> result)
    {
        view.Show(false);
        yield return TypeLine(speaker, prompt, false);
        picked = -1;
        view.ShowChoices(options);
        yield return new WaitUntil(() => picked >= 0);
        view.HideChoices();
        clicked = false;
        result?.Invoke(picked);
    }

    private bool AdvancePressed()
    {
        bool pressed = clicked || MenuInput.Confirm;
        clicked = false;
        return pressed;
    }

    private IEnumerator TypeLine(string speaker, string text, bool waitForKey)
    {
        view.SetLine(speaker, text);
        float shown = 0f;
        int lastBlip = 0;
        while (shown < text.Length)
        {
            if (AdvancePressed()) break;                          // tekan = tampilkan semua
            shown += charsPerSecond * Time.unscaledDeltaTime;
            view.SetVisibleCharacters((int)shown);
            if (audioService != null && (int)shown / 3 > lastBlip)
            {
                lastBlip = (int)shown / 3;
                audioService.PlaySfx(blip, 0.1f);
            }
            yield return null;
        }
        view.SetVisibleCharacters(text.Length);
        if (!waitForKey) yield break;
        view.SetContinueHint(true);
        yield return null;
        while (!AdvancePressed()) yield return null;              // tunggu tombol untuk lanjut
        view.SetContinueHint(false);
    }
}
