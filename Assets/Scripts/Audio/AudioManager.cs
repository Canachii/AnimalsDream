using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float bgmVolume = 1f;

    [Header("SFX 2D")]
    [SerializeField] private AudioSource sfx2DSource;
    [SerializeField] private float sfxVolume = 1f;

    [Header("SFX 3D")]
    [SerializeField] private int sfx3DPoolSize = 8;
    [SerializeField] private float sfx3DMinDistance = 1f;
    [SerializeField] private float sfx3DMaxDistance = 20f;

    private readonly List<AudioSource> sfx3DPool = new List<AudioSource>();
    private Coroutine bgmFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null) bgmSource = CreateChildSource("BGM", false);
        if (sfx2DSource == null) sfx2DSource = CreateChildSource("SFX_2D", false);

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        sfx2DSource.playOnAwake = false;

        SetBgmVolume(bgmVolume);
        SetSfxVolume(sfxVolume);

        Ensure3DPool();
    }

    public void PlayBgm(AudioClip clip, float volume = 1f, bool restart = true)
    {
        if (clip == null) return;
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);

        if (bgmSource.clip != clip || restart)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        bgmSource.volume = bgmVolume * Mathf.Clamp01(volume);
    }

    public void StopBgm()
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void FadeBgm(AudioClip clip, float fadeOutSeconds, float fadeInSeconds, float targetVolume = 1f)
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmFadeRoutine = StartCoroutine(CoFadeBgm(clip, fadeOutSeconds, fadeInSeconds, targetVolume));
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        sfx2DSource.pitch = pitch;
        sfx2DSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volume));
    }

    public void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var source = GetFree3DSource();
        source.transform.position = position;
        source.pitch = pitch;
        source.volume = sfxVolume * Mathf.Clamp01(volume);
        source.clip = clip;
        source.Play();
    }

    private IEnumerator CoFadeBgm(AudioClip clip, float fadeOutSeconds, float fadeInSeconds, float targetVolume)
    {
        float startVolume = bgmSource.volume;
        float t = 0f;

        if (bgmSource.isPlaying && fadeOutSeconds > 0f)
        {
            while (t < fadeOutSeconds)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutSeconds);
                yield return null;
            }
        }

        bgmSource.Stop();
        bgmSource.clip = clip;

        if (clip != null)
        {
            bgmSource.Play();
        }

        t = 0f;
        float endVolume = bgmVolume * Mathf.Clamp01(targetVolume);
        if (clip != null && fadeInSeconds > 0f)
        {
            while (t < fadeInSeconds)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, endVolume, t / fadeInSeconds);
                yield return null;
            }
        }

        bgmSource.volume = endVolume;
        bgmFadeRoutine = null;
    }

    private void Ensure3DPool()
    {
        while (sfx3DPool.Count < sfx3DPoolSize)
        {
            var source = CreateChildSource($"SFX_3D_{sfx3DPool.Count}", true);
            sfx3DPool.Add(source);
        }
    }

    private AudioSource GetFree3DSource()
    {
        for (int i = 0; i < sfx3DPool.Count; i++)
        {
            if (!sfx3DPool[i].isPlaying)
                return sfx3DPool[i];
        }

        var extra = CreateChildSource($"SFX_3D_{sfx3DPool.Count}", true);
        sfx3DPool.Add(extra);
        return extra;
    }

    private AudioSource CreateChildSource(string name, bool spatial)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var source = go.AddComponent<AudioSource>();
        source.spatialBlend = spatial ? 1f : 0f;
        source.minDistance = sfx3DMinDistance;
        source.maxDistance = sfx3DMaxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        return source;
    }
}
