using System;
using TMPro;
using UnityEngine;

namespace UIPages
{
    public class DevModePasswordPopup : MonoBehaviour
    {
        [SerializeField]
        private GameObject _root;

        [SerializeField]
        private TMP_InputField _passwordInput;

        [SerializeField]
        private TextMeshProUGUI _errorText;

        public event Action<string> PasswordSubmitted;
        public event Action Closed;

        public void Open()
        {
            SetVisible(true);
            SetErrorVisible(false);
            if (_passwordInput == null) return;
            _passwordInput.text = string.Empty;
            _passwordInput.ActivateInputField();
        }

        public void Close()
        {
            SetVisible(false);
            Closed?.Invoke();
        }

        public void OnSubmitButtonClick()
        {
            string password = _passwordInput != null ? _passwordInput.text : string.Empty;
            PasswordSubmitted?.Invoke(password);
        }

        public void OnCloseButtonClick()
        {
            Close();
        }

        public void ShowInvalidPassword()
        {
            SetErrorVisible(true);
        }

        private void SetVisible(bool state)
        {
            if (_root != null)
            {
                _root.SetActive(state);
            }
            else
            {
                gameObject.SetActive(state);
            }
        }

        private void SetErrorVisible(bool state)
        {
            if (_errorText == null) return;
            _errorText.gameObject.SetActive(state);
        }
    }
}
