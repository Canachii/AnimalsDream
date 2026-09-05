using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

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
    Trap_Fan,
    Trap_Flame,
    Trap_Thunder,   // 번개 피격
    Trap_Warning    // 번개 경고
}

public class AudioManager : MonoBehaviour
{
    public enum SoundBus { BGM, SFX, UI }

    [System.Serializable]
    private class SoundEntry
    {
        public SoundId id;
        public AudioClip clip;

        [Range(0f, 1f)] public float volume = 1f;

        public SoundBus bus = SoundBus.SFX;

        [Tooltip("SFX일 때만 의미 있음. true면 3D로 재생")]
        public bool spatial3D;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Sound List")]
    [SerializeField] private List<SoundEntry> sounds = new();

    [Header("Mixer (권장)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;

    [Tooltip("Expose된 파라미터 이름")]
    [SerializeField] private string masterVolParam = "MasterVol";
    [SerializeField] private string bgmVolParam = "BgmVol";
    [SerializeField] private string sfxVolParam = "SfxVol";
    [SerializeField] private string uiVolParam = "UiVol";

    [Header("Default Volumes (0~1)")]
    [Range(0f, 1f)][SerializeField] private float defaultMaster = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultBgm = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultSfx = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultUi = 1f;

    [Header("BGM Source")]
    [SerializeField] private float bgmFadeOut = 0.25f;
    [SerializeField] private float bgmFadeIn = 0.35f;

    [Header("2D Pools (pitch 꼬임 방지)")]
    [SerializeField] private int sfx2DPoolSize = 6;
    [SerializeField] private int ui2DPoolSize = 4;

    [Header("3D Pool")]
    [SerializeField] private int sfx3DPoolSize = 10;
    [SerializeField] private int sfx3DPoolMax = 16; // 무한 증가 방지
    [SerializeField] private float sfx3DMinDistance = 1f;
    [SerializeField] private float sfx3DMaxDistance = 20f;

    private AudioSource bgmSource;
    private readonly List<AudioSource> sfx2DPool = new();
    private readonly List<AudioSource> ui2DPool = new();
    private readonly List<AudioSource> sfx3DPool = new();

    private readonly Dictionary<SoundId, SoundEntry> soundMap = new();
    private int _nextLoopHandle = 1;
    private readonly Dictionary<int, AudioSource> _loopSources = new();

    private Coroutine bgmFadeRoutine;
    private float currentBgmMul = 1f; // 현재 트랙(entry.volume) 배수 유지용

    // 저장 키
    private const string KEY_MASTER = "Audio_Master";
    private const string KEY_BGM = "Audio_Bgm";
    private const string KEY_SFX = "Audio_Sfx";
    private const string KEY_UI = "Audio_Ui";

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSoundMap();

        // Sources
        bgmSource = CreateChildSource("BGM_Source", spatial: false, output: bgmGroup);
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        Ensure2DPool(sfx2DPool, sfx2DPoolSize, "SFX2D_", sfxGroup);
        Ensure2DPool(ui2DPool, ui2DPoolSize, "UI2D_", uiGroup);
        Ensure3DPool();

        // Load volumes
        float master = PlayerPrefs.GetFloat(KEY_MASTER, defaultMaster);
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, defaultBgm);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, defaultSfx);
        float ui = PlayerPrefs.GetFloat(KEY_UI, defaultUi);

        SetMasterVolume01(master, save: false);
        SetBgmVolume01(bgm, save: false);
        SetSfxVolume01(sfx, save: false);
        SetUiVolume01(ui, save: false);
    }

    private void OnValidate()
    {
        // 에디터에서 중복/누락 빨리 잡기
        BuildSoundMap();
    }

    // -------------------- Public API: Volume --------------------
    public void SetMasterVolume01(float value, bool save = true) => SetMixerVolume01(masterVolParam, value, KEY_MASTER, save);
    public void SetBgmVolume01(float value, bool save = true) => SetMixerVolume01(bgmVolParam, value, KEY_BGM, save);
    public void SetSfxVolume01(float value, bool save = true) => SetMixerVolume01(sfxVolParam, value, KEY_SFX, save);
    public void SetUiVolume01(float value, bool save = true) => SetMixerVolume01(uiVolParam, value, KEY_UI, save);

    private void SetMixerVolume01(string param, float value01, string key, bool save)
    {
        value01 = Mathf.Clamp01(value01);

        if (audioMixer != null && !string.IsNullOrEmpty(param))
        {
            audioMixer.SetFloat(param, Linear01ToDb(value01));
        }

        if (save)
            PlayerPrefs.SetFloat(key, value01);
    }

    private float Linear01ToDb(float v01)
    {
        if (v01 <= 0.0001f) return -80f; // 거의 무음
        return Mathf.Log10(v01) * 20f;   // 1 -> 0dB
    }

    // -------------------- Public API: Play --------------------
    public void Play(SoundId id, float pitch = 1f)
    {
        if (!TryGetSound(id, out var entry)) return;

        switch (entry.bus)
        {
            case SoundBus.BGM:
                PlayBgm(id, fade: true, restart: true);
                break;

            case SoundBus.UI:
                Play2D(ui2DPool, entry, pitch);
                break;

            case SoundBus.SFX:
                if (entry.spatial3D)
                    Debug.LogWarning($"[AudioManager] {id} is 3D. Use PlayAtPoint(id, position).");
                else
                    Play2D(sfx2DPool, entry, pitch);
                break;
        }
    }

    public void PlayAtPoint(SoundId id, Vector3 position, float pitch = 1f)
    {
        if (!TryGetSound(id, out var entry)) return;

        if (entry.bus == SoundBus.BGM)
        {
            Debug.LogWarning($"[AudioManager] {id} is BGM. Use PlayBgm.");
            return;
        }

        if (!entry.spatial3D)
        {
            Debug.LogWarning($"[AudioManager] {id} is 2D. Use Play(id).");
            return;
        }

        Play3D(entry, position, pitch);
    }

    // -------------------- BGM --------------------
    public void PlayBgm(SoundId id, bool fade = true, bool restart = true)
    {
        if (!TryGetSound(id, out var entry)) return;
        if (entry.bus != SoundBus.BGM)
        {
            Debug.LogWarning($"[AudioManager] {id} is not BGM.");
            return;
        }

        if (entry.clip == null) return;

        currentBgmMul = Mathf.Clamp01(entry.volume);

        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);

        if (!fade)
        {
            if (bgmSource.clip != entry.clip || restart)
            {
                bgmSource.clip = entry.clip;
                bgmSource.Play();
            }
            bgmSource.volume = currentBgmMul;
            return;
        }

        bgmFadeRoutine = StartCoroutine(CoFadeToBgm(entry.clip, bgmFadeOut, bgmFadeIn, restart));
    }

    public void StopBgm(bool fade = true)
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);

        if (!fade)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        bgmFadeRoutine = StartCoroutine(CoFadeToBgm(null, bgmFadeOut, 0f, restart: true));
    }

    private IEnumerator CoFadeToBgm(AudioClip next, float outSec, float inSec, bool restart)
    {
        float start = bgmSource.volume;

        if (bgmSource.isPlaying && outSec > 0f)
        {
            float t = 0f;
            while (t < outSec)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(start, 0f, t / outSec);
                yield return null;
            }
        }

        // swap
        if (next == null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            bgmSource.volume = 0f;
            bgmFadeRoutine = null;
            yield break;
        }

        if (bgmSource.clip != next || restart)
        {
            bgmSource.Stop();
            bgmSource.clip = next;
            bgmSource.Play();
        }

        // fade in to currentBgmMul
        if (inSec > 0f)
        {
            float t = 0f;
            while (t < inSec)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, currentBgmMul, t / inSec);
                yield return null;
            }
        }

        bgmSource.volume = currentBgmMul;
        bgmFadeRoutine = null;
    }

    // -------------------- Internals: 2D/3D --------------------
    private void Play2D(List<AudioSource> pool, SoundEntry entry, float pitch)
    {
        if (entry.clip == null) return;

        var src = GetFree2DSource(pool);
        src.pitch = pitch;
        src.PlayOneShot(entry.clip, Mathf.Clamp01(entry.volume));
    }

    private void Play3D(SoundEntry entry, Vector3 position, float pitch)
    {
        if (entry.clip == null) return;

        var src = GetFree3DSource();
        src.transform.position = position;
        src.pitch = pitch;
        src.volume = Mathf.Clamp01(entry.volume);
        src.clip = entry.clip;
        src.Play();
    }

    private AudioSource GetFree2DSource(List<AudioSource> pool)
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].isPlaying) return pool[i];

        // 다 쓰는 중이면 0번 재사용(겹침은 나지만 pitch 꼬임은 최소화)
        return pool.Count > 0 ? pool[0] : bgmSource;
    }

    private AudioSource GetFree3DSource()
    {
        for (int i = 0; i < sfx3DPool.Count; i++)
            if (!sfx3DPool[i].isPlaying) return sfx3DPool[i];

        if (sfx3DPool.Count >= sfx3DPoolMax)
            return sfx3DPool[0]; // 최대치면 재사용(소리 끊김 가능)

        var extra = CreateChildSource($"SFX3D_{sfx3DPool.Count}", spatial: true, output: sfxGroup);
        sfx3DPool.Add(extra);
        return extra;
    }

    // -------------------- Map / Pools --------------------
    private void BuildSoundMap()
    {
        soundMap.Clear();

        for (int i = 0; i < sounds.Count; i++)
        {
            var e = sounds[i];
            if (e == null) continue;

            if (soundMap.ContainsKey(e.id))
            {
                Debug.LogWarning($"[AudioManager] Duplicate SoundId in list: {e.id}");
                continue;
            }

            soundMap.Add(e.id, e);
        }
    }

    private bool TryGetSound(SoundId id, out SoundEntry entry)
    {
        if (!soundMap.TryGetValue(id, out entry))
        {
            Debug.LogWarning($"[AudioManager] Missing SoundEntry: {id}");
            return false;
        }
        return true;
    }

    private void Ensure2DPool(List<AudioSource> pool, int size, string prefix, AudioMixerGroup group)
    {
        while (pool.Count < size)
            pool.Add(CreateChildSource($"{prefix}{pool.Count}", spatial: false, output: group));
    }

    private void Ensure3DPool()
    {
        while (sfx3DPool.Count < sfx3DPoolSize)
            sfx3DPool.Add(CreateChildSource($"SFX3D_{sfx3DPool.Count}", spatial: true, output: sfxGroup));
    }

    private AudioSource CreateChildSource(string name, bool spatial, AudioMixerGroup output)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = output;
        src.spatialBlend = spatial ? 1f : 0f;

        if (spatial)
        {
            src.minDistance = sfx3DMinDistance;
            src.maxDistance = sfx3DMaxDistance;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.dopplerLevel = 0f; // 레이싱에서 피치 튐 방지
        }

        return src;
    }
    public int StartLoop3D(SoundId id, Transform follow, Vector3 localOffset = default, float pitch = 1f)
    {
        if (!TryGetSound(id, out var entry)) return -1;
        if (entry.bus == SoundBus.BGM)
        {
            Debug.LogWarning($"[AudioManager] {id}는 BGM입니다. Loop3D가 아닌 PlayBgm을 사용하세요.");
            return -1;
        }
        if (entry.clip == null)
        {
            Debug.LogWarning($"[AudioManager] {id}의 clip이 비어있습니다.");
            return -1;
        }

        // 루프용 오브젝트 생성(몇 개 안 생기므로 풀 대신 생성/파괴로 충분)
        var go = new GameObject($"Loop3D_{id}");
        if (follow != null)
        {
            go.transform.SetParent(follow, false);
            go.transform.localPosition = localOffset;
        }

        var src = go.AddComponent<AudioSource>();

        // 3D 세팅 (AudioManager의 거리값 사용)
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 1f;
        src.minDistance = sfx3DMinDistance;
        src.maxDistance = sfx3DMaxDistance;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.dopplerLevel = 0f;

        // 믹서 그룹 연결 (SFX로)
        if (sfxGroup != null) src.outputAudioMixerGroup = sfxGroup;

        // 클립/볼륨/피치 적용
        src.clip = entry.clip;
        src.volume = Mathf.Clamp01(entry.volume); // 엔트리 볼륨
        src.pitch = pitch;

        src.Play();

        int handle = _nextLoopHandle++;
        _loopSources[handle] = src;
        return handle;
    }
    public void StopLoop(int handle)
    {
        if (handle < 0) return;

        if (_loopSources.TryGetValue(handle, out var src) && src != null)
        {
            src.Stop();
            Destroy(src.gameObject);
        }
        _loopSources.Remove(handle);
    }
    public void StopAllNonBgm()
    {
        // 2D SFX / UI 끄기 (PlayOneShot 포함)
        StopPool(sfx2DPool);
        StopPool(ui2DPool);

        // 3D SFX 풀 끄기
        for (int i = 0; i < sfx3DPool.Count; i++)
        {
            if (sfx3DPool[i] != null) sfx3DPool[i].Stop();
        }

        // Loop3D 핸들로 재생 중인 것들 끄기
        var handles = _loopSources.Keys.ToArray();
        foreach (var h in handles)
            StopLoop(h);
    }

    private void StopPool(List<AudioSource> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null) pool[i].Stop();
        }
    }
}
