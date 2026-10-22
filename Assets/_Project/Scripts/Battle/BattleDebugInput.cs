// P5 — kontrol sementara sebelum UI battle selesai: tombol 1–4 memilih aksi, Console menampilkan pesan.
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleDebugInput : MonoBehaviour
{
    [SerializeField] private BattleSystem battle;

    void OnEnable() => battle.MessageShown += Log;
    void OnDisable() => battle.MessageShown -= Log;

    private static void Log(string message) => Debug.Log($"[Battle] {message}");

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || battle.PlayerActions == null) return;
        if (keyboard.digit1Key.wasPressedThisFrame) Pick(0);
        if (keyboard.digit2Key.wasPressedThisFrame) Pick(1);
        if (keyboard.digit3Key.wasPressedThisFrame) Pick(2);
        if (keyboard.digit4Key.wasPressedThisFrame) Pick(3);
    }

    private void Pick(int index) => battle.Choose(battle.PlayerActions[index]);
}
