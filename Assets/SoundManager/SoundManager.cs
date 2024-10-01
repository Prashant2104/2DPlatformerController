using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mute")]
    [SerializeField]
    private bool m_muted;
    public bool Muted { get { return m_muted; } }
    public bool SetMute { set { m_muted = value; ToggleAudio(m_muted); } }

    [Header("BGM")]
    [SerializeField]
    private AudioSource m_musicSource;
    [SerializeField, Range(0f, 1f)]
    private float m_bgmVolume;
    public float MusicVolume {
        get { return m_bgmVolume; }
        set {
            m_bgmVolume = Mathf.Clamp01(value);
            m_musicSource.volume = m_bgmVolume;
        }
    }
    [SerializeField]
    private float m_musicCrossfadeTime = 2f;

    [SerializeField, Space]
    private List<Music> m_musics = new List<Music>();
    Dictionary<BGM, Music> m_musicDictionary;

    [Space, Header("SFX")]
    [SerializeField]
    private AudioSource m_sfxSource;
    [SerializeField, Range(0f, 1f)]
    private float m_sfxVolume;
    public float EffectsVolume {
        get { return m_sfxVolume; }
        set {
            m_sfxVolume = Mathf.Clamp01(value);
            m_sfxSource.volume = m_sfxVolume;
        }
    }

    [SerializeField, Space]
    private List<Sounds> m_sounds = new List<Sounds>();
    Dictionary<SFX, Sounds> m_soundDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        m_soundDictionary = new Dictionary<SFX, Sounds>();
        foreach (var item in m_sounds)
        {
            m_soundDictionary.Add(item.sfx, item);
        }

        m_musicDictionary = new Dictionary<BGM, Music>();
        foreach (var item in m_musics)
        {
            m_musicDictionary.Add(item.bgm, item);
        }
        m_musicSource.volume = m_bgmVolume;
        PlayMusic(BGM.Bgm1);
    }

    public void ChangeMusicVolume(float valueToAdd)
    {
        m_bgmVolume += valueToAdd;
        m_bgmVolume = Mathf.Clamp01(m_bgmVolume);
        m_musicSource.volume = m_bgmVolume;
    }

    public void ChangeEffectsVolume(float valueToAdd)
    {
        m_sfxVolume += valueToAdd;
        m_sfxVolume = Mathf.Clamp01(m_sfxVolume);
        m_sfxSource.volume = m_sfxVolume;
    }

    public void PlaySfx(SFX sfx)
    {
        if (m_muted || !m_sfxSource) return;

        if (m_soundDictionary.ContainsKey(sfx))
        {
            Sounds sound = m_soundDictionary[sfx];
            int r = Random.Range(0, sound.clips.Length);
            if (!m_sfxSource.isPlaying)
            {
                m_sfxSource.clip = sound.clips[r];
                m_sfxSource.volume = m_sfxVolume * sound.volumeMultiplier;
                m_sfxSource.pitch = Eerp(sound.pitchRange.x, sound.pitchRange.y);
                m_sfxSource.Play();
            }
            else
            {
                AudioSource tempSource = new GameObject().AddComponent<AudioSource>();
                tempSource.clip = sound.clips[r];
                tempSource.volume = m_sfxVolume * sound.volumeMultiplier;
                tempSource.pitch = Eerp(sound.pitchRange.x, sound.pitchRange.y);
                tempSource.Play();
                StartCoroutine(WaitForSFX(tempSource.clip.length, tempSource));
            }
        }
    }

    IEnumerator WaitForSFX(float delay, AudioSource source)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            Destroy(source.gameObject);
        }
    }

    public void PlayMusic(BGM bgm)
    {
        if (!m_musicSource) return;

        if (m_musicDictionary.ContainsKey(bgm))
        {
            StartCoroutine(FadeInOutAudio(m_musicSource, m_musicDictionary[bgm].clip));
        }
    }

    IEnumerator FadeInOutAudio(AudioSource source, AudioClip newClip)
    {
        float startVol = source.volume;
        while (source.volume > 0)
        {
            source.volume = Mathf.MoveTowards(source.volume, 0, 2 * Time.deltaTime / m_musicCrossfadeTime);
            yield return null;
        }
        source.clip = newClip;
        source.Play();
        while (source.volume < startVol)
        {
            source.volume = Mathf.MoveTowards(source.volume, startVol, 2 * Time.deltaTime / m_musicCrossfadeTime);
            yield return null;
        }
        source.volume = startVol;
    }

    // interpolates in log scale (multiplicatively linear) by @FreyaHolmer
    static float Eerp(float a, float b)
    {
        float t = Random.Range(0f, 1f);
        return a * Mathf.Exp(t * Mathf.Log(b / a));
    }
    
    void ToggleAudio(bool mute)
    {
        if (!m_musicSource) return;
        if (mute)
        {
            m_musicSource.Pause();
        }
        else
        {
            m_musicSource.Play();
        }
    }
}