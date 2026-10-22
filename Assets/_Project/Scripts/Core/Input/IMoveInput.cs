// P2 (modul 7 pertemuan) — kontrak input gerak. PlayerMovement hanya mengenal interface ini.
using UnityEngine;

public interface IMoveInput
{
    Vector2 Move { get; }
    bool IsRunning { get; }
}
