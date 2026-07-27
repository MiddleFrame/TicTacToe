namespace AudioSystem
{
    public interface IAudioService
    {
        bool IsMuted { get; }
        bool Play(string presetId, float volumeScale = 1f);
        void SetMuted(bool muted);
        void SetSfxVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxMuted(bool muted);
        void SetMusicMuted(bool muted);
    }
}
