// P6 — satu-satunya pemutar musik & SFX. Tinggal di Scn_Bootstrap (bersama AudioListener).
// Volume disimpan di PlayerPrefs (versi demo: slider diganti tombol M = mute). Versi ini tanpa AudioMixer agar praktikum ringkas;
// pengayaan: route AudioSource ke grup Music/SFX sebuah AudioMixer lalu atur via SetFloat (dB).
using System.Collections;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    public const string MusicKey = "MusicVolume";
    public const string SfxKey = "SfxVolume";

    [SerializeField] private AudioSource musicA;     // Loop aktif, Play On Awake mati
    [SerializeField] private AudioSource musicB;     // Loop aktif, Play On Awake mati
    [SerializeField] private AudioSource sfx;
    [SerializeField] private float crossfadeTime = 1f;

    private AudioSource activeMusic;
    private AudioClip worldMusic;
    private float musicVolume = 0.8f;
    private float sfxVolume = 0.8f;

    public AudioClip CurrentMusic => activeMusic != null ? activeMusic.clip : null;

    void Awake()
    {
        activeMusic = musicA;
        musicVolume = GetVolume(MusicKey);
        sfxVolume = GetVolume(SfxKey);
        sfx.volume = sfxVolume;
    }

    public void PlayWorldMusic(AudioClip clip)
    {
        worldMusic = clip;
        PlayMusic(clip);
    }

    public void PlayBattleMusic(AudioClip clip) => PlayMusic(clip);
    public void ResumeWorldMusic() => PlayMusic(worldMusic);

    public void PlaySfx(AudioClip clip, float pitchJitter = 0.05f)
    {
        if (clip == null) return;
        sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfx.PlayOneShot(clip);
    }

    // Versi demo: tombol M — semua suara mati/nyala (AudioListener.volume memengaruhi semua AudioSource).
    public bool Muted { get; private set; }

    public bool ToggleMute()
    {
        Muted = !Muted;
        AudioListener.volume = Muted ? 0f : 1f;
        return Muted;
    }

    public float GetVolume(string key) => PlayerPrefs.GetFloat(key, 0.8f);

    public void SetVolume(string key, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(key, value);
        if (key == MusicKey) { musicVolume = value; activeMusic.volume = value; }
        else { sfxVolume = value; sfx.volume = value; }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (activeMusic.clip == clip && activeMusic.isPlaying) return;
        StopAllCoroutines();
        StartCoroutine(Crossfade(clip));
    }

    private IEnumerator Crossfade(AudioClip clip)
    {
        AudioSource from = activeMusic;
        AudioSource to = activeMusic == musicA ? musicB : musicA;
        activeMusic = to;
        to.clip = clip;
        to.volume = 0f;
        to.Play();
        float startFrom = from.volume;
        for (float t = 0f; t < crossfadeTime; t += Time.unscaledDeltaTime)   // tetap jalan saat timeScale 0
        {
            float k = t / crossfadeTime;
            from.volume = startFrom * (1f - k);
            to.volume = musicVolume * k;
            yield return null;
        }
        from.Stop();
        to.volume = musicVolume;
    }
}
