// P6 — menyalakan/mematikan objek sesuai flag (mis. tanda "!" di atas Tetua hilang setelah dapat starter).
using UnityEngine;

public class FlagToggle : MonoBehaviour
{
    [SerializeField] private string flagKey = StoryFlags.HasStarter;
    [SerializeField] private GameObject[] showWhenSet;
    [SerializeField] private GameObject[] hideWhenSet;

    private StoryFlags flags;

    public void Construct(StoryFlags storyFlags)
    {
        flags = storyFlags;
        flags.Changed += OnChanged;
        Apply();
    }

    void OnDestroy()
    {
        if (flags != null) flags.Changed -= OnChanged;
    }

    private void OnChanged(string key)
    {
        if (key == flagKey) Apply();
    }

    private void Apply()
    {
        bool set = flags.Has(flagKey);
        foreach (GameObject go in showWhenSet) go.SetActive(set);
        foreach (GameObject go in hideWhenSet) go.SetActive(!set);
    }
}
