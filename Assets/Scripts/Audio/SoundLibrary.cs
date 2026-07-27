using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    [Serializable]
    public sealed class SoundPreset
    {
        [Tooltip("Stable ID called from code, for example: damage.impact")]
        public string Id;

        [Tooltip("A random variation is selected on every play.")]
        public AudioClip[] Clips;

        [Range(0f, 1f)]
        public float Volume = 1f;

        [Range(0.5f, 2f)]
        public float PitchMin = 1f;

        [Range(0.5f, 2f)]
        public float PitchMax = 1f;

        [Min(0f)]
        [Tooltip("Minimum unscaled time between starts of this preset.")]
        public float Cooldown;

        [Min(1)]
        [Tooltip("Prevents repeated gameplay events from becoming too loud.")]
        public int MaxSimultaneousVoices = 4;
    }

    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "TicTacToe/Audio/Sound Library")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [SerializeField]
        private List<SoundPreset> _presets = new();

        [Header("Music")]
        [SerializeField]
        private AudioClip _backgroundMusic;

        [SerializeField, Range(0f, 1f)]
        private float _backgroundMusicVolume = 0.22f;

        public IReadOnlyList<SoundPreset> Presets => _presets;
        public AudioClip BackgroundMusic => _backgroundMusic;
        public float BackgroundMusicVolume => _backgroundMusicVolume;

#if UNITY_EDITOR
        public void Configure(
            List<SoundPreset> presets,
            AudioClip backgroundMusic,
            float backgroundMusicVolume)
        {
            _presets = presets;
            _backgroundMusic = backgroundMusic;
            _backgroundMusicVolume = backgroundMusicVolume;
        }
#endif
    }
}
