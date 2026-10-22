// P3 — membaca action "Interact" (keyboard E pada InputSystem_Actions bawaan).
using UnityEngine.InputSystem;

public class InputSystemActionInput : IActionInput
{
    private readonly InputAction interact;

    public InputSystemActionInput()
    {
        interact = InputSystem.actions.FindAction("Interact");
    }

    // WasPressedThisFrame bernilai true tepat pada frame tombol ditekan.
    public bool InteractPressed => interact.WasPressedThisFrame();
}
