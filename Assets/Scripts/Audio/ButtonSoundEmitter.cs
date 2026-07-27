using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AudioSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSoundEmitter : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        [SerializeField]
        [Tooltip("Leave enabled to use the global ui.click preset.")]
        private bool _useDefaultPreset = true;

        [SerializeField]
        [Tooltip("Optional per-button preset ID. No AudioClip reference is required.")]
        private string _presetId = SoundPresetIds.UiClick;

        private Button _button;
        private IAudioService _audioService;

        public void Initialize(IAudioService audioService)
        {
            _audioService = audioService;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            PlayClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            _ = eventData;
            PlayClick();
        }

        private void PlayClick()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button == null || !_button.IsActive() || !_button.interactable) return;

            string id = _useDefaultPreset || string.IsNullOrWhiteSpace(_presetId)
                ? SoundPresetIds.UiClick
                : _presetId;
            _audioService?.Play(id);
        }
    }
}
