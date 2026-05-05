using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinishLine.Interfaces
{
    public interface ICellClearAnimationService
    {
        public float AnimationFrames { get; }

        public IEnumerator Play(List<List<Vector2Int>> lines, Action<List<Vector2Int>, int> drawLineAction);
    }
}
