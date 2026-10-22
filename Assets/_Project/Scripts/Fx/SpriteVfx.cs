// P7 — efek sprite sheet sederhana (VFX_HitSlash_4f_32x32): putar frame lalu lapor selesai.
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteVfx : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameTime = 0.06f;

    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Play(Action finished) => StartCoroutine(Run(finished));

    private IEnumerator Run(Action finished)
    {
        foreach (Sprite f in frames)
        {
            sr.sprite = f;
            yield return new WaitForSecondsRealtime(frameTime);   // tetap jalan saat hit-stop
        }
        finished?.Invoke();
    }
}
