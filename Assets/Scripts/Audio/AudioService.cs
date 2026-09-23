using System;
using System.Collections.Generic;
using DefenderOfDreams.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace DefenderOfDreams.Audio
{
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [Header("4.9 — Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioMixer mixer;

        [Header("Volume (0..1)")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;

        [Header("Directional cue (4.9.3)")]
        [SerializeField] private bool showDirectionVisual = true;
        [SerializeField] private float directionCueDuration = 1.2f;

        public static event Action<Vector2, float> SoundEmitted;
        public static event Action<Vector2, float, float> DirectionalCue;

        private readonly List<OneShot> _pending = new List<OneShot>();
        private float _masterDb;
        private float _sfxDb;
        private float _musicDb;

        private struct OneShot
        {
            public AudioClip clip;
            public Vector2 worldPos;
            public float volume;
            public float delay;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
            if (musicSource == null)
            {
                var musicGo = new GameObject("MusicSource");
                musicGo.transform.SetParent(transform, false);
                musicSource = musicGo.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            if (ambientSource == null)
            {
                var ambGo = new GameObject("AmbientSource");
                ambGo.transform.SetParent(transform, false);
                ambientSource = ambGo.AddComponent<AudioSource>();
                ambientSource.loop = true;
            }

            sfxSource.playOnAwake = false;
            ApplyVolumes();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                p.delay -= Time.unscaledDeltaTime;
                if (p.delay <= 0f)
                {
                    PlayNow(p.clip, p.worldPos, p.volume);
                    _pending.RemoveAt(i);
                }
                else
                {
                    _pending[i] = p;
                }
            }
        }

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); ApplyVolumes(); }
        public void SetSfxVolume(float v) { sfxVolume = Mathf.Clamp01(v); ApplyVolumes(); }
        public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); ApplyVolumes(); }

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;
        public bool DirectionVisualEnabled => showDirectionVisual;

        public void SetDirectionVisual(bool enabled) => showDirectionVisual = enabled;

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null)
                return;
            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }

        public void PlaySfxAt(AudioClip clip, Vector2 worldPos, float volume = 1f)
        {
            if (clip == null)
                return;
            _pending.Add(new OneShot { clip = clip, worldPos = worldPos, volume = volume, delay = 0f });
        }

        public void PlaySfxAtDelayed(AudioClip clip, Vector2 worldPos, float volume, float delay)
        {
            if (clip == null)
                return;
            _pending.Add(new OneShot { clip = clip, worldPos = worldPos, volume = volume, delay = delay });
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
                return;
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (ambientSource == null || clip == null)
                return;
            if (ambientSource.clip == clip && ambientSource.isPlaying)
                return;
            ambientSource.clip = clip;
            ambientSource.volume = masterVolume * 0.8f;
            ambientSource.Play();
        }

        private void PlayNow(AudioClip clip, Vector2 worldPos, float volume)
        {
            if (sfxSource == null)
                return;

            float pan = ComputePan(worldPos);
            sfxSource.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

            var temp = new GameObject("SfxPan")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            temp.transform.position = sfxSource.transform.position;
            var src = temp.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = Mathf.Clamp01(volume * sfxVolume);
            src.panStereo = pan;
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            src.pitch = 0.95f + UnityEngine.Random.value * 0.1f;
            src.Play();
            Destroy(temp, clip.length + 0.1f);

            SoundEmitted?.Invoke(worldPos, volume);
            if (showDirectionVisual)
                DirectionalCue?.Invoke(worldPos, pan, directionCueDuration);
        }

        private static float ComputePan(Vector2 worldPos)
        {
            var cam = Camera.main;
            if (cam == null)
                return 0f;
            float dx = worldPos.x - cam.transform.position.x;
            float halfWidth = cam.orthographic
                ? cam.orthographicSize * cam.aspect
                : 5f;
            return Mathf.Clamp(dx / Mathf.Max(0.01f, halfWidth), -1f, 1f);
        }

        private void ApplyVolumes()
        {
            _masterDb = ToDb(masterVolume);
            _sfxDb = ToDb(sfxVolume);
            _musicDb = ToDb(musicVolume);

            if (mixer != null)
            {
                mixer.SetFloat("MasterVolume", _masterDb);
                mixer.SetFloat("SfxVolume", _sfxDb);
                mixer.SetFloat("MusicVolume", _musicDb);
            }

            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        private static float ToDb(float v)
        {
            if (v <= 0.0001f)
                return -80f;
            return Mathf.Log10(v) * 20f;
        }
    }
}
