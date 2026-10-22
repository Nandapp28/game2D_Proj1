// Versi demo — menu jeda (Esc) di dunia: Lanjut, Simpan sekarang, Kembali ke judul.
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;        // layar redup + panel
    [SerializeField] private TextMenu menu;

    private GameServices services;
    private bool open;

    public void Construct(GameServices gameServices)
    {
        services = gameServices;
        menu.Picked += OnPicked;
        root.SetActive(false);
    }

    void Update()
    {
        if (services == null) return;
        if (!open)
        {
            bool canOpen = services.Scenes.CurrentScene == SceneFlow.WorldScene && !services.Scenes.IsBusy
                           && !services.Scenes.InBattle && !services.Input.IsLocked && services.Player != null;
            if (canOpen && MenuInput.Cancel) Open();
        }
        else if (MenuInput.Cancel) Close();
    }

    private void Open()
    {
        open = true;
        services.Input.Lock();                      // pemain berhenti selama menu terbuka
        root.SetActive(true);
        menu.Open(new[] { "Lanjut", "Simpan sekarang", "Kembali ke judul" });
    }

    private void Close()
    {
        open = false;
        menu.Close();
        root.SetActive(false);
        services.Input.Unlock();
    }

    private void OnPicked(int index)
    {
        Close();
        if (index == 1)
        {
            bool saved = services.Save.Save(SaveMapper.Capture(services, services.Player.position));
            services.Toast.Show(saved ? "Tersimpan" : "Penyimpanan gagal");
        }
        else if (index == 2)
        {
            services.Scenes.GoTo(SceneFlow.TitleScene);
        }
    }
}
