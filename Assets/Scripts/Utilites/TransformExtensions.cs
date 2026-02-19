using System;
using System.Collections;
using System.Collections.Generic;
using ModestTree.Util;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerator ScaleWithLerp(this Transform transform, Vector2 initialScale, Vector2 finalScale,
        int countFrame, Action callBack = null)
    {
        float percentage = 0;
        int frame = 0;
        while (frame <= countFrame)
        {
            percentage = (float) frame / (float) countFrame;
            transform.localScale = Vector2.Lerp(initialScale, finalScale, percentage);
            frame += 1;
            yield return null;
        }

        callBack?.Invoke();
    }

    public static IEnumerator PositionWithLerp(this Transform transform, Vector3 initalPosition, Vector3 finalPosition,
        int countFrame, Action callBack = null)
    {
        float percentage = 0;
        int frame = 0;
        while (frame <= countFrame)
        {
            percentage = (float) frame / (float) countFrame;
            transform.position = Vector2.Lerp(initalPosition, finalPosition, percentage);
            frame += 1;
            yield return null;
        }

        callBack?.Invoke();
    }

    public static IEnumerator LocalPositionWithLerp(this Transform transform, Vector3 initalPosition,
        Vector3 finalPosition,
        int countFrame, Action callBack = null)
    {
        int frame = 0;
        while (frame <= countFrame)
        {
            var percentage = (float) frame / (float) countFrame;
            transform.localPosition = Vector2.Lerp(initalPosition, finalPosition, percentage);
            frame += 1;
            yield return null;
        }

        callBack?.Invoke();
    }

    public static IEnumerator RotationWithLerp(this Transform transform, Quaternion initalRotation,
        Quaternion finalRotation,
        int countFrame, Action callBack = null)
    {
        float percentage = 0;
        int frame = 0;
        while (frame <= countFrame)
        {
            percentage = (float) frame / (float) countFrame;
            transform.rotation = Quaternion.Lerp(initalRotation, finalRotation, percentage);
            frame += 1;
            yield return null;
        }

        callBack?.Invoke();
    }
}