using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicExtensions
{
    public static IEnumerator AlphaWithLerpByDuration(this Graphic graphic, float initialAlpha, float finalAlpha,
        float duration, bool useUnscaledTime = true, Action callback = null)
    {
        if (duration <= 0f)
        {
            Color instantColor = graphic.color;
            instantColor.a = finalAlpha;
            graphic.color = instantColor;
            callback?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Color color = graphic.color;
            color.a = Mathf.Lerp(initialAlpha, finalAlpha, t);
            graphic.color = color;
            yield return null;
        }

        Color finalColor = graphic.color;
        finalColor.a = finalAlpha;
        graphic.color = finalColor;
        callback?.Invoke();
    }
}
