// P7 — masuk gua: cahaya global meredup, cahaya di sekitar pemain menyala.
// Versi demo: cahaya tambahan (kristal, Raijugar) ikut menyala saat gua gelap.
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightZone : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private Light2D[] extraLights;           // versi demo
    [SerializeField] private float insideIntensity = 0.15f;
    [SerializeField] private float fadeSpeed = 2f;

    private float target = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) target = insideIntensity;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) target = 1f;
    }

    void Update()
    {
        globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, target, fadeSpeed * Time.deltaTime);
        float dark = 1f - globalLight.intensity;
        playerLight.intensity = dark;
        foreach (Light2D l in extraLights) if (l != null) l.intensity = dark;
    }
}
