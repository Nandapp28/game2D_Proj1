// P6 — menulis dan membaca file save di Application.persistentDataPath.
using System;
using System.IO;
using UnityEngine;

public class SaveService
{
    public const int CurrentVersion = 1;

    // Diisi TitleInstaller saat "Lanjutkan", dipakai WorldInstaller untuk posisi pemain.
    public SaveData PendingLoad { get; set; }

    private static string PathFor(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public bool HasSave(int slot = 0) => File.Exists(PathFor(slot));

    public bool Save(SaveData data, int slot = 0)
    {
        data.version = CurrentVersion;
        data.savedAt = DateTime.Now.ToString("s");
        try
        {
            File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
            return true;
        }
        catch (IOException e)
        {
            Debug.LogError($"Gagal menyimpan: {e.Message}");
            return false;
        }
    }

    public SaveData Load(int slot = 0)
    {
        if (!HasSave(slot)) return null;
        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)));
            return data.version == CurrentVersion ? data : Migrate(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"File save rusak: {e.Message}");
            return null;
        }
    }

    // Tempat mengubah data versi lama ke format terbaru saat struktur SaveData berubah.
    private static SaveData Migrate(SaveData old)
    {
        old.version = CurrentVersion;
        return old;
    }
}
