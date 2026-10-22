// ALAT UJI SEMENTARA (dibuat Claude saat menguji modul) — boleh dihapus.
// Menekan tombol keyboard virtual dan menggerakkan Player ke titik tujuan saat Play Mode.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class UjiDriver : MonoBehaviour, IMoveInput
{
    public static UjiDriver I;
    public static readonly List<string> Log = new List<string>();
    public Vector2 Move { get; private set; }
    public bool IsRunning { get; private set; }
    public bool Busy { get; private set; }

    static Keyboard Kb
    {
        get
        {
            foreach (var d in InputSystem.devices) if (d.name == "PMTestKeyboard") return (Keyboard)d;
            return InputSystem.AddDevice<Keyboard>("PMTestKeyboard");
        }
    }

    public static UjiDriver Get()
    {
        if (I == null) { var go = new GameObject("UjiDriver"); DontDestroyOnLoad(go); I = go.AddComponent<UjiDriver>(); }
        return I;
    }

    public static void Tap(Key key, int count = 1, float interval = 0.3f) => Get().StartCoroutine(Get().TapCo(key, count, interval));
    public static void Hold(params Key[] keys) => InputSystem.QueueStateEvent(Kb, new KeyboardState(keys));
    public static void Release() => InputSystem.QueueStateEvent(Kb, new KeyboardState());

    IEnumerator TapCo(Key key, int count, float interval)
    {
        Busy = true;
        for (int i = 0; i < count; i++)
        {
            InputSystem.QueueStateEvent(Kb, new KeyboardState(key));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(Kb, new KeyboardState());
            yield return new WaitForSecondsRealtime(interval);
        }
        Busy = false;
    }

    // Gerak lurus per sumbu (x dulu lalu y) menuju target; berhenti bila sampai atau macet.
    public static void WalkTo(Vector2 target, bool run = false, bool yFirst = false)
    {
        var d = Get();
        foreach (PlayerMovement p in FindObjectsByType<PlayerMovement>()) p.Construct(d);
        d.StartCoroutine(d.WalkCo(target, run, yFirst));
    }

    IEnumerator WalkCo(Vector2 target, bool run, bool yFirst)
    {
        Busy = true; IsRunning = run;
        var p = FindAnyObjectByType<PlayerMovement>().transform;
        for (int axis = 0; axis < 2; axis++)
        {
            bool doY = yFirst ? axis == 0 : axis == 1;
            float stuck = 0f, last = float.MaxValue;
            while (true)
            {
                float diff = doY ? target.y - p.position.y : target.x - p.position.x;
                if (Mathf.Abs(diff) < 0.08f) break;
                Move = doY ? new Vector2(0, Mathf.Sign(diff)) : new Vector2(Mathf.Sign(diff), 0);
                yield return new WaitForSeconds(0.1f);
                float now = doY ? p.position.y : p.position.x;
                if (Mathf.Abs(now - last) < 0.01f) stuck += 0.1f; else stuck = 0f;
                last = now;
                if (stuck > 0.6f) { Log.Add($"macet di {p.position} menuju {target}"); break; }
            }
            Move = Vector2.zero;
            yield return null;
        }
        Move = Vector2.zero; Busy = false;
        Log.Add($"tiba {p.position}");
    }

    public static void Face(Vector2 dir) { var d = Get(); d.StartCoroutine(d.FaceCo(dir)); }
    IEnumerator FaceCo(Vector2 dir) { Move = dir * 0.2f; yield return null; yield return null; Move = Vector2.zero; }
}
