// P6 — isi dialog sebagai data. Awalan file: Dlg_ (mis. Dlg_Tetua_Starter).
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 4)] public string text;
}

[CreateAssetMenu(fileName = "Dlg_", menuName = "Pulau Monster/Dialogue")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();
    [SerializeField] private List<string> choices = new List<string>();     // kosong = tanpa pilihan

    public IReadOnlyList<DialogueLine> Lines => lines;
    public IReadOnlyList<string> Choices => choices;
}
