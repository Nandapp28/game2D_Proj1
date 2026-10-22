// Versi demo — pesan singkat di atas layar ("Temui Tetua Rahmat ..."), hilang sendiri setelah 2,6 detik.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToastView : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Text label;
    [SerializeField] private float seconds = 2.6f;

    void Awake() => panel.gameObject.SetActive(false);

    public void Show(string text)
    {
        StopAllCoroutines();
        StartCoroutine(Run(text));
    }

    private IEnumerator Run(string text)
    {
        label.text = text;
        panel.sizeDelta = new Vector2(label.preferredWidth + 72f, panel.sizeDelta.y);
        panel.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(seconds);
        panel.gameObject.SetActive(false);
    }
}
