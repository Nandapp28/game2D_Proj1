// P4 — wadah semua service yang dibuat di Composition Root (GameBootstrap).
// Hanya installer yang menerima wadah ini; component lain menerima service satu per satu lewat Construct.
using UnityEngine;

public class GameServices
{
    public InputGate Input;              // IMoveInput + IActionInput yang bisa dikunci
    public IActionInput RawAction;       // P6: tombol asli (tidak ikut terkunci)
    public MonsterDatabase Database;
    public PlayerParty Party;
    public BattleSession Battle;
    public SceneFlow Scenes;
    public StoryFlags Flags;             // P6
    public DialogueRunner Dialogue;      // P6
    public SaveService Save;             // P6
    public AudioService Audio;           // P6
    public ToastView Toast;              // versi demo: pesan singkat di atas layar
    public Transform Player;             // versi demo: diisi WorldInstaller (menu jeda perlu posisi pemain)
}
