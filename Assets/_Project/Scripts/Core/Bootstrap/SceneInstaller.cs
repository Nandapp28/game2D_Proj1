// P4 — kelas induk semua installer (inheritance + polymorphism).
// Aturan: installer diletakkan di GameObject ROOT scene-nya.
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneInstaller : MonoBehaviour
{
    public abstract void Install(GameServices services);

    // Mencari component bertipe T di scene ini saja, termasuk yang nonaktif.
    protected IEnumerable<T> FindInScene<T>()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (T component in root.GetComponentsInChildren<T>(true))
                yield return component;
    }
}
