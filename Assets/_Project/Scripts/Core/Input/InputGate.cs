// P4 — decorator: meneruskan input asli, atau nol saat dikunci (battle P4–P5, dialog & cutscene P6).
using UnityEngine;

public class InputGate : IMoveInput, IActionInput
{
    private readonly IMoveInput move;
    private readonly IActionInput action;
    private int locks;

    public InputGate(IMoveInput move, IActionInput action)
    {
        this.move = move;
        this.action = action;
    }

    public bool IsLocked => locks > 0;
    public void Lock() => locks++;
    public void Unlock() => locks = Mathf.Max(0, locks - 1);

    public Vector2 Move => IsLocked ? Vector2.zero : move.Move;
    public bool IsRunning => !IsLocked && move.IsRunning;
    public bool InteractPressed => !IsLocked && action.InteractPressed;
}
