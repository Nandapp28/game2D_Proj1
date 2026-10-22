// Alat uji: menyuntikkan tombol keyboard ke Input System saat Play Mode (dipakai untuk uji otomatis & screenshot).
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class TestInput
{
    public static void Hold(params Key[] keys)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys));
        InputSystem.Update();
    }

    public static void Release() => Hold();

    public static void Tap(Key key)
    {
        Hold(key);
        Release();
    }
}
