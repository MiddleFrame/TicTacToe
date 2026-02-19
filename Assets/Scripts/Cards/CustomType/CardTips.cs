using System;
using System.Collections;
using TMPro;
using UnityEngine;
using ScreenScaler;
using ScreenScaler.Interfaces;
using Zenject;

public class CardTips : MonoBehaviour
{
    [SerializeField]
    private RectTransform _tipRect;

    [SerializeField]
    private TextMeshProUGUI _tipsText;

    [SerializeField]
    private CanvasGroup _tipCanvas;


    [SerializeField]
    private float _awaitTime;

    private bool _isAlphaCoroutineWork;
    private UnityEngine.Coroutine _alphaCoroutine;

    private const int ALPHA_COUNT_FRAME = 25;

    #region Dependency

    private IScreenScaler _screenScaler;

    [Inject]
    private void Construct(IScreenScaler screenScaler)
    {
        _screenScaler = screenScaler;
    }

    #endregion

    public void ShowTip(string textTip, bool instantly = true)
    {
        gameObject.SetActive(true);

        _tipsText.text = textTip;
        if (instantly)
        {
            _tipCanvas.alpha = 1;
        }
        else
        {
            if (_isAlphaCoroutineWork) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(AwaitBeforeLerp(_awaitTime, 1));
        }
    }

    public void HideTip(bool instantly)
    {
        if (instantly)
        {
            _tipCanvas.alpha = 0;
            gameObject.SetActive(false);
        }
        else
        {
            if (!gameObject.activeInHierarchy) return;
            if (_isAlphaCoroutineWork) StopCoroutine(_alphaCoroutine);
            _alphaCoroutine = StartCoroutine(
                _tipCanvas.AlphaWithLerp(
                    _tipCanvas.alpha,
                    0,
                    ALPHA_COUNT_FRAME,
                    () => gameObject.SetActive(false)));
        }
    }

    private IEnumerator AwaitBeforeLerp(float awaitTime, float finalAlpha)
    {
        yield return new WaitForSeconds(awaitTime);
        Action callback = (finalAlpha == 0) ? () => { gameObject.SetActive(false); } : null;
        yield return
            StartCoroutine(_tipCanvas.AlphaWithLerp(_tipCanvas.alpha, finalAlpha, ALPHA_COUNT_FRAME, callback));
    }

    private void OnDisable()
    {
        _isAlphaCoroutineWork = false;
    }
}