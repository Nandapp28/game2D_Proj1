// Versi demo — tombol menu (judul, pilihan dialog, menu battle, menu jeda) langsung dari keyboard/gamepad.
// E / Spasi / Enter = pilih, Esc / X / Backspace = batal, panah atau WASD = pindah pilihan.
using UnityEngine;
using UnityEngine.InputSystem;

public static class MenuInput
{
    private static Keyboard K => Keyboard.current;
    private static Gamepad G => Gamepad.current;

    public static bool Up => Pressed(K?.upArrowKey, K?.wKey) || (G != null && G.dpad.up.wasPressedThisFrame);
    public static bool Down => Pressed(K?.downArrowKey, K?.sKey) || (G != null && G.dpad.down.wasPressedThisFrame);
    public static bool Left => Pressed(K?.leftArrowKey, K?.aKey) || (G != null && G.dpad.left.wasPressedThisFrame);
    public static bool Right => Pressed(K?.rightArrowKey, K?.dKey) || (G != null && G.dpad.right.wasPressedThisFrame);
    public static bool Confirm => Pressed(K?.eKey, K?.spaceKey, K?.enterKey, K?.numpadEnterKey, K?.zKey) || (G != null && G.buttonSouth.wasPressedThisFrame);
    public static bool Cancel => Pressed(K?.escapeKey, K?.xKey, K?.backspaceKey) || (G != null && G.buttonEast.wasPressedThisFrame);
    public static bool Mute => Pressed(K?.mKey);

    private static bool Pressed(params UnityEngine.InputSystem.Controls.KeyControl[] keys)
    {
        foreach (var k in keys) if (k != null && k.wasPressedThisFrame) return true;
        return false;
    }
}
