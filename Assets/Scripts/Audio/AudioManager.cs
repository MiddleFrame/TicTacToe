using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace AudioSystem
{
    public sealed class AudioManager : MonoBehaviour, IAudioService, IInitializable, IDisposable
    {
        private const string LibraryResourcePath = "Audio/SoundLibrary";
        private const string SfxVolumeKey = "Audio.SfxVolume";
        private const string MusicVolumeKey = "Audio.MusicVolume";
        private const string SfxMutedKey = "Audio.SfxMuted";
        private const string MusicMutedKey = "Audio.MusicMuted";
        private const int InitialVoiceCount = 12;
        private const int MaxVoiceCount = 24;
        private const float ButtonScanInterval = 0.35f;

        private readonly Dictionary<string, SoundPreset> _presets =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> _lastPlayTimes =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly List<AudioSource> _voicePool = new();

        private SoundLibrary _library;
        private AudioSource _musicSource;
        private float _sfxVolume;
        private float _musicVolume;
        private bool _sfxMuted;
        private bool _musicMuted;
        private float _nextButtonScanTime;

        public bool IsMuted => _sfxMuted && _musicMuted;

        public void Initialize()
        {
            DontDestroyOnLoad(gameObject);
            _library = Resources.Load<SoundLibrary>(LibraryResourcePath);
            if (_library == null)
            {
                Debug.LogError($"Sound library not found at Resources/{LibraryResourcePath}.asset");
                return;
            }

            BuildPresetLookup();
            CreateSources();
            LoadUserSettings();
            StartBackgroundMusic();

            SceneManager.sceneLoaded += OnSceneLoaded;
            ScanAndWireButtons();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextButtonScanTime) return;
            _nextButtonScanTime = Time.unscaledTime + ButtonScanInterval;
            ScanAndWireButtons();
        }

        public bool Play(string presetId, float volumeScale = 1f)
        {
            if (_sfxMuted || _library == null || string.IsNullOrWhiteSpace(presetId)) return false;
            if (!_presets.TryGetValue(presetId, out SoundPreset preset)) return false;
            if (preset.Clips == null || preset.Clips.Length == 0) return false;

            float now = Time.unscaledTime;
            if (_lastPlayTimes.TryGetValue(presetId, out float lastPlay) &&
                now - lastPlay < preset.Cooldown)
            {
                return false;
            }

            int activeForPreset = 0;
            for (int i = 0; i < _voicePool.Count; i++)
            {
                AudioSource voice = _voicePool[i];
                if (voice.isPlaying && voice.gameObject.name == presetId) activeForPreset++;
            }

            if (activeForPreset >= preset.MaxSimultaneousVoices) return false;

            AudioSource source = GetAvailableVoice();
            if (source == null) return false;

            AudioClip clip = preset.Clips[UnityEngine.Random.Range(0, preset.Clips.Length)];
            if (clip == null) return false;

            source.gameObject.name = presetId;
            source.clip = clip;
            source.pitch = UnityEngine.Random.Range(
                Mathf.Min(preset.PitchMin, preset.PitchMax),
                Mathf.Max(preset.PitchMin, preset.PitchMax));
            source.volume = Mathf.Clamp01(preset.Volume * _sfxVolume * volumeScale);
            source.Play();
            _lastPlayTimes[presetId] = now;
            return true;
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
        }

        public void SetMuted(bool muted)
        {
            SetSfxMuted(muted);
            SetMusicMuted(muted);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
            ApplyMusicVolume();
        }

        public void SetSfxMuted(bool muted)
        {
            _sfxMuted = muted;
            PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
            if (!muted) return;

            for (int i = 0; i < _voicePool.Count; i++)
            {
                _voicePool[i].Stop();
            }
        }

        public void SetMusicMuted(bool muted)
        {
            _musicMuted = muted;
            PlayerPrefs.SetInt(MusicMutedKey, muted ? 1 : 0);
            ApplyMusicVolume();
        }

        private void BuildPresetLookup()
        {
            _presets.Clear();
            IReadOnlyList<SoundPreset> presets = _library.Presets;
            for (int i = 0; i < presets.Count; i++)
            {
                SoundPreset preset = presets[i];
                if (preset == null || string.IsNullOrWhiteSpace(preset.Id)) continue;
                _presets[preset.Id] = preset;
            }
        }

        private void CreateSources()
        {
            for (int i = 0; i < InitialVoiceCount; i++)
            {
                CreateVoice();
            }

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.ignoreListenerPause = true;
        }

        private AudioSource CreateVoice()
        {
            GameObject voiceObject = new("AudioVoice");
            voiceObject.transform.SetParent(transform, false);
            AudioSource source = voiceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            _voicePool.Add(source);
            return source;
        }

        private AudioSource GetAvailableVoice()
        {
            for (int i = 0; i < _voicePool.Count; i++)
            {
                if (!_voicePool[i].isPlaying) return _voicePool[i];
            }

            return _voicePool.Count < MaxVoiceCount ? CreateVoice() : null;
        }

        private void LoadUserSettings()
        {
            _sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            _musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
            _sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) != 0;
            _musicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) != 0;
        }

        private void StartBackgroundMusic()
        {
            if (_library.BackgroundMusic == null) return;
            _musicSource.clip = _library.BackgroundMusic;
            ApplyMusicVolume();
            _musicSource.Play();
        }

        private void ApplyMusicVolume()
        {
            if (_musicSource == null || _library == null) return;
            _musicSource.volume = _musicMuted
                ? 0f
                : _library.BackgroundMusicVolume * _musicVolume;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _ = scene;
            _ = mode;
            _nextButtonScanTime = 0f;
            ScanAndWireButtons();
        }

        private void ScanAndWireButtons()
        {
            Button[] buttons = FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null) continue;

                ButtonSoundEmitter emitter = button.GetComponent<ButtonSoundEmitter>();
                if (emitter == null)
                {
                    emitter = button.gameObject.AddComponent<ButtonSoundEmitter>();
                }

                emitter.Initialize(this);
            }
        }
    }
}
