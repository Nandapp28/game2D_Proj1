// P5 — warna bar HP: hijau > 50%, kuning > 20%, merah selebihnya (warna versi demo).
using UnityEngine;

public static class HpColors
{
    public static readonly Color High = new Color32(111, 208, 140, 255);   // #6FD08C
    public static readonly Color Mid = new Color32(255, 198, 26, 255);     // #FFC61A
    public static readonly Color Low = new Color32(255, 93, 93, 255);      // #FF5D5D

    public static Color For(float ratio) => ratio > 0.5f ? High : ratio > 0.2f ? Mid : Low;
}
