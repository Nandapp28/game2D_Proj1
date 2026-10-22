// Alat uji: IMoveInput yang dikendalikan dari kode (menggantikan keyboard saat uji otomatis).
// Bukti manfaat interface: PlayerMovement tidak perlu diubah sama sekali.
using UnityEngine;

public class TestMoveInput : IMoveInput
{
    public static readonly TestMoveInput Instance = new TestMoveInput();
    public Vector2 Move { get; set; }
    public bool IsRunning { get; set; }

    public static void Drive(Vector2 move, bool run = false)
    {
        Instance.Move = move;
        Instance.IsRunning = run;
        foreach (PlayerMovement p in Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            p.Construct(Instance);
    }
}
