// P4 — layar hitam untuk transisi. Letakkan di Canvas milik Scn_Bootstrap.
using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.35f;

    public IEnumerator FadeOut() => Fade(group.alpha, 1f);
    public IEnumerator FadeIn() => Fade(group.alpha, 0f);

    private IEnumerator Fade(float from, float to)
    {
        group.blocksRaycasts = true;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}
