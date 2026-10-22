// P3 versi 1 (menoleh + Debug.Log) → P6: memutar DialogueData, bisa berganti sesuai flag.
// Versi demo: Talk() dibuat virtual — NPC khusus (Tetua, Penjaga, Pak Dagang) cukup mewarisi Npc
// dan menulis ulang Talk() sebagai urutan dialog (inheritance + polymorphism).
using System.Collections;
using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Warga";
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite faceDown, faceLeft, faceRight, faceUp;
    [SerializeField] private DialogueData dialogue;            // P6
    [SerializeField] private DialogueData dialogueAfterFlag;   // P6 (opsional)
    [SerializeField] private string flagKey;                   // P6 (opsional), mis. has_starter

    protected DialogueRunner Dialogue { get; private set; }
    protected StoryFlags Flags { get; private set; }

    public string NpcName => npcName;

    public void Construct(DialogueRunner dialogueRunner, StoryFlags storyFlags)
    {
        Dialogue = dialogueRunner;
        Flags = storyFlags;
        OnConstructed();
    }

    protected virtual void OnConstructed() { }

    public void Interact(PlayerInteractor who)
    {
        if (Dialogue.IsRunning) return;
        Face(who.transform.position - transform.position);
        Dialogue.Run(Talk());
    }

    // Isi percakapan. Versi dasar: DialogueData biasa (atau versi "after" jika flag sudah menyala).
    protected virtual IEnumerator Talk()
    {
        bool useAfter = dialogueAfterFlag != null && !string.IsNullOrEmpty(flagKey) && Flags.Has(flagKey);
        yield return Dialogue.PlayData(useAfter ? dialogueAfterFlag : dialogue);
    }

    protected void Face(Vector2 d)
    {
        body.sprite = Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? (d.x > 0 ? faceRight : faceLeft)
            : (d.y > 0 ? faceUp : faceDown);
    }
}
