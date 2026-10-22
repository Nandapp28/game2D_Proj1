// P2 (modul 7 pertemuan) — data kecepatan pemain (60 & 96 piksel/detik pada PPU 16).
using UnityEngine;

[CreateAssetMenu(fileName = "Cfg_Player", menuName = "Pulau Monster/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [SerializeField] private float walkSpeed = 3.75f;
    [SerializeField] private float runSpeed = 6f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
}
