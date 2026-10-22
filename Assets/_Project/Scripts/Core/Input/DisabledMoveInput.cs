// P2 (modul 7 pertemuan) (tugas) — implementasi kosong untuk membuktikan manfaat interface.
using UnityEngine;

public class DisabledMoveInput : IMoveInput
{
    public Vector2 Move => Vector2.zero;
    public bool IsRunning => false;
}
