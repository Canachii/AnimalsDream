using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundId
{
    UI_ButtonHover,
    UI_ButtonClick,
    Lobby_BGM,
    Race_Countdown,
    Race_BGM,
    Race_GoalIn,
    Race_GameEnd,
    Event_PenguinSkill,
    Event_HorseSkill,
    Event_ZebraSkill,
    Event_SpiderSkill,
    Trap_JumpPad,
    Trap_DisappearingPlatform,
    Trap_Fan,
    Trap_Flame,
    Trap_Thunder,
    Trap_Warning
}

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    private class SoundEntry
    {
        public SoundId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool isBgm;
        public bool spatial3D;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Sound List")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

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
    private readonly Dictionary<SoundId, SoundEntry> soundMap = new Dictionary<SoundId, SoundEntry>();
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

        BuildSoundMap();
        Ensure3DPool();
    }

    public void PlayBgm(SoundId id, bool restart = true)
    {
        if (!TryGetSound(id, out var entry)) return;
        if (!entry.isBgm)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is not marked as BGM.");
            return;
        }
        PlayBgmClip(entry.clip, entry.volume, restart);
    }

    public void StopBgm()
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void FadeBgm(SoundId id, float fadeOutSeconds, float fadeInSeconds)
    {
        if (!TryGetSound(id, out var entry)) return;
        if (!entry.isBgm)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is not marked as BGM.");
            return;
        }
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmFadeRoutine = StartCoroutine(CoFadeBgm(entry.clip, fadeOutSeconds, fadeInSeconds, entry.volume));
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

    public void PlaySfx(SoundId id, float pitch = 1f)
    {
        if (!TryGetSound(id, out var entry)) return;
        if (entry.isBgm)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is marked as BGM; use PlayBgm.");
            return;
        }
        if (entry.spatial3D)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is 3D; use PlaySfxAtPoint.");
            return;
        }
        if (entry.clip == null) return;
        sfx2DSource.pitch = pitch;
        sfx2DSource.PlayOneShot(entry.clip, sfxVolume * Mathf.Clamp01(entry.volume));
    }

    public void PlaySfxAtPoint(SoundId id, Vector3 position, float pitch = 1f)
    {
        if (!TryGetSound(id, out var entry)) return;
        if (entry.isBgm)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is marked as BGM; use PlayBgm.");
            return;
        }
        if (!entry.spatial3D)
        {
            Debug.LogWarning($"[AudioManager] Sound {id} is 2D; use PlaySfx.");
            return;
        }
        if (entry.clip == null) return;
        var source = GetFree3DSource();
        source.transform.position = position;
        source.pitch = pitch;
        source.volume = sfxVolume * Mathf.Clamp01(entry.volume);
        source.clip = entry.clip;
        source.Play();
    }

    private void PlayBgmClip(AudioClip clip, float volume, bool restart)
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

    private void BuildSoundMap()
    {
        soundMap.Clear();
        for (int i = 0; i < sounds.Count; i++)
        {
            var entry = sounds[i];
            if (!soundMap.ContainsKey(entry.id))
                soundMap.Add(entry.id, entry);
        }
    }

    private bool TryGetSound(SoundId id, out SoundEntry entry)
    {
        if (soundMap.Count == 0) BuildSoundMap();
        if (!soundMap.TryGetValue(id, out entry))
        {
            Debug.LogWarning($"[AudioManager] Missing sound entry: {id}");
            return false;
        }
        return true;
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
