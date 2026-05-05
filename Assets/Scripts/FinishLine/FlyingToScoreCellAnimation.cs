using System;
using System.Collections;
using System.Collections.Generic;
using Field.Interfaces;
using FinishLine.Interfaces;
using Players.Interfaces;
using Score.Interfaces;
using Theme.Interfaces;
using UIPages.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace FinishLine
{
    public class FlyingToScoreCellAnimation : ICellClearAnimationService
    {
        private const bool ENABLE_TARGET_TRACE_LOGS = false;
        private const float ICON_SIZE_FACTOR = 0.72f;
        private const float SCALE_UP_FACTOR = 1.04f;
        private const int START_DELAY_FRAMES = 15;
        private const int SCALE_UP_FRAMES = 18;
        private const int HOLD_AFTER_SCALE_FRAMES = 18; // ~0.5s at 60 FPS
        private const int FLY_FRAMES = 28;
        private const float SINE_WAVES = 1.2f;
        private const float SINE_AMPLITUDE_FACTOR = 0.35f;
        private const float SCORE_APPLY_THRESHOLD = 0.92f;
        private readonly IPlayerService _playerService;
        private readonly IScoreService _scoreService;
        private readonly IFieldService _fieldService;
        private readonly IFieldFigureService _fieldFigureService;
        private readonly IInGameUIService _inGameUIService;
        private readonly IThemeService _themeService;

        public FlyingToScoreCellAnimation(
            IPlayerService playerService,
            IScoreService scoreService,
            IFieldService fieldService,
            IFieldFigureService fieldFigureService,
            IInGameUIService inGameUIService,
            IThemeService themeService)
        {
            _playerService = playerService;
            _scoreService = scoreService;
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
            _inGameUIService = inGameUIService;
            _themeService = themeService;
        }

        public float AnimationFrames => 0f;

        public IEnumerator Play(List<List<Vector2Int>> lines, Action<List<Vector2Int>, int> drawLineAction)
        {
            _ = drawLineAction;
            int currentPlayer = _playerService.GetCurrentPlayer().SideId;
            List<Vector2Int> uniqueCells = CollectUniqueCells(lines);
            if (uniqueCells.Count == 0)
            {
                yield break;
            }

            RectTransform targetRect = _inGameUIService.GetScoreSliderRectTransform(currentPlayer);
            RectTransform animationLayer = ResolveAnimationLayer(targetRect);

            List<FlyingIconData> icons = SpawnIcons(uniqueCells, animationLayer);
            if (icons.Count == 0)
            {
                ApplyScoreAndClear(uniqueCells, currentPlayer);
                yield break;
            }

            for (int i = 0; i < uniqueCells.Count; i++)
            {
                _fieldFigureService.SetFigure(uniqueCells[i], CellFigure.None, isQueue: false);
            }

            yield return AnimateIconsToTarget(icons, targetRect, animationLayer, currentPlayer);
        }

        private static List<Vector2Int> CollectUniqueCells(List<List<Vector2Int>> lines)
        {
            List<Vector2Int> uniqueCells = new();
            foreach (List<Vector2Int> line in lines)
            {
                foreach (Vector2Int cell in line)
                {
                    if (uniqueCells.IndexOf(cell) == -1)
                    {
                        uniqueCells.Add(cell);
                    }
                }
            }

            return uniqueCells;
        }

        private List<FlyingIconData> SpawnIcons(List<Vector2Int> cells, RectTransform layer)
        {
            List<FlyingIconData> icons = new();
            if (layer == null) return icons;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cellId = cells[i];
                CellFigure cellFigure = _fieldFigureService.GetCellFigure(cellId);
                if (cellFigure == CellFigure.None) continue;

                Sprite sprite = _themeService.GetSprite(cellFigure);
                if (sprite == null) continue;

                Cell cellLink = _fieldService.GetCellLink(cellId);
                RectTransform sourceRect = cellLink.transform as RectTransform;
                if (sourceRect == null) continue;

                GameObject iconObject = new("ScoreFlyIcon");
                RectTransform iconRect = iconObject.AddComponent<RectTransform>();
                iconRect.SetParent(layer, false);
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.localScale = Vector3.one;
                Image sourceFigureImage = cellLink.transform.GetChild(2).GetComponent<Image>();
                iconRect.sizeDelta = sourceFigureImage != null
                    ? sourceFigureImage.rectTransform.rect.size
                    : sourceRect.rect.size;
                iconRect.sizeDelta *= ICON_SIZE_FACTOR;
                if (iconRect.sizeDelta.sqrMagnitude <= 0.001f)
                {
                    float fallbackSize = _fieldService.GetCellSize() * 0.8f;
                    iconRect.sizeDelta = new Vector2(fallbackSize, fallbackSize);
                }

                Image iconImage = iconObject.AddComponent<Image>();
                iconImage.sprite = sprite;
                iconImage.raycastTarget = false;
                iconImage.preserveAspect = true;
                iconImage.color = sourceFigureImage != null ? sourceFigureImage.color : Color.white;

                Vector3 sourceWorldPosition = sourceFigureImage != null
                    ? sourceFigureImage.rectTransform.position
                    : sourceRect.position;
                Vector2 startScreenPosition = GetScreenPointFromRect(sourceWorldPosition, sourceRect);
                Vector2 layerStartLocal = ConvertWorldToLayerLocal(sourceWorldPosition, sourceRect, layer);
                iconRect.localPosition = new Vector3(layerStartLocal.x, layerStartLocal.y, 0f);

                RectTransform targetRect = _inGameUIService.GetScoreSliderRectTransform(_playerService.GetCurrentPlayer().SideId);
                Vector3 targetWorldPosition = targetRect != null
                    ? targetRect.position
                    : _inGameUIService.GetScoreSliderWorldPosition(_playerService.GetCurrentPlayer().SideId);
                Vector2 layerTargetLocal = ConvertWorldToLayerLocal(targetWorldPosition, targetRect, layer);
                Vector2 toTarget = layerTargetLocal - new Vector2(iconRect.localPosition.x, iconRect.localPosition.y);
                if (ENABLE_TARGET_TRACE_LOGS)
                {
                    Debug.Log(
                        $"[FlyTargetTrace] figure={cellFigure} cell={cellId} currentPlayer={_playerService.GetCurrentPlayer().SideId} " +
                        $"sourceWorld={sourceWorldPosition} targetWorld={targetWorldPosition} " +
                        $"startLocal={iconRect.localPosition} targetLocal={layerTargetLocal} toTarget={toTarget} " +
                        $"targetRect={(targetRect != null ? targetRect.name : "null")} layer={(layer != null ? layer.name : "null")}");
                }
                Vector2 perpendicular = new Vector2(-toTarget.y, toTarget.x).normalized;
                if (perpendicular.sqrMagnitude < 0.01f) perpendicular = Vector2.up;
                float direction = (i % 2 == 0) ? 1f : -1f;
                float amplitude = iconRect.sizeDelta.y * SINE_AMPLITUDE_FACTOR;

                icons.Add(new FlyingIconData
                {
                    Image = iconImage,
                    StartPosition = iconRect.localPosition,
                    StartScale = iconRect.localScale,
                    Perpendicular = perpendicular * direction,
                    SineAmplitude = amplitude,
                    StartFrame = i * START_DELAY_FRAMES
                });
            }

            return icons;
        }

        private IEnumerator AnimateIconsToTarget(
            List<FlyingIconData> icons,
            RectTransform targetRect,
            RectTransform layer,
            int currentPlayer)
        {
            Vector3 targetWorldPosition = targetRect != null
                ? targetRect.position
                : _inGameUIService.GetScoreSliderWorldPosition(currentPlayer);
            Vector2 layerTarget2D = ConvertWorldToLayerLocal(targetWorldPosition, targetRect, layer);
            Vector3 layerTargetPosition = new Vector3(layerTarget2D.x, layerTarget2D.y, 0f);
            int totalFrames = SCALE_UP_FRAMES + HOLD_AFTER_SCALE_FRAMES + FLY_FRAMES +
                              START_DELAY_FRAMES * Mathf.Max(0, icons.Count - 1);
            for (int frame = 0; frame <= totalFrames; frame++)
            {
                for (int i = 0; i < icons.Count; i++)
                {
                    FlyingIconData icon = icons[i];
                    if (icon.Image == null || icon.IsFinished) continue;

                    int localFrame = frame - icon.StartFrame;
                    if (localFrame < 0) continue;

                    Image image = icons[i].Image;
                    RectTransform rect = image.rectTransform;
                    if (localFrame <= SCALE_UP_FRAMES)
                    {
                        float tScale = Mathf.Clamp01(localFrame / (float) SCALE_UP_FRAMES);
                        rect.localPosition = icon.StartPosition;
                        rect.localScale = Vector3.LerpUnclamped(
                            icon.StartScale,
                            icon.StartScale * SCALE_UP_FACTOR,
                            EaseOutBack(tScale));
                        continue;
                    }

                    if (localFrame <= SCALE_UP_FRAMES + HOLD_AFTER_SCALE_FRAMES)
                    {
                        rect.localPosition = icon.StartPosition;
                        rect.localScale = icon.StartScale * SCALE_UP_FACTOR;
                        continue;
                    }

                    int flyFrame = localFrame - SCALE_UP_FRAMES - HOLD_AFTER_SCALE_FRAMES;
                    float tFly = Mathf.Clamp01(flyFrame / (float) FLY_FRAMES);
                    float eased = EaseInOutCubic(tFly);
                    Vector3 basePosition = Vector3.LerpUnclamped(icon.StartPosition, layerTargetPosition, eased);
                    float sineOffset = Mathf.Sin(tFly * Mathf.PI * 2f * SINE_WAVES) *
                                       icon.SineAmplitude * (1f - tFly);
                    rect.localPosition = basePosition + (Vector3) (icon.Perpendicular * sineOffset);
                    rect.localScale = Vector3.LerpUnclamped(
                        icon.StartScale * SCALE_UP_FACTOR,
                        Vector3.zero,
                        eased);

                    if (tFly >= SCORE_APPLY_THRESHOLD)
                    {
                        if (!icon.IsScored)
                        {
                            _scoreService.AddScore(currentPlayer, 1);
                            _inGameUIService.UpdateScore(_scoreService.GetScore(1), _scoreService.GetScore(2));
                            icon.IsScored = true;
                        }

                        icon.IsFinished = true;
                        UnityEngine.Object.Destroy(image.gameObject);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < icons.Count; i++)
            {
                FlyingIconData icon = icons[i];
                if (!icon.IsScored)
                {
                    _scoreService.AddScore(currentPlayer, 1);
                    _inGameUIService.UpdateScore(_scoreService.GetScore(1), _scoreService.GetScore(2));
                    icon.IsScored = true;
                }

                if (icon.Image != null)
                {
                    UnityEngine.Object.Destroy(icon.Image.gameObject);
                }
            }
        }

        private RectTransform ResolveAnimationLayer(RectTransform targetRect)
        {
            if (targetRect != null)
            {
                Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
                if (targetCanvas != null) return targetCanvas.transform as RectTransform;
            }

            return _inGameUIService.GetScoreFlyAnimationLayer();
        }

        private void ApplyScoreAndClear(List<Vector2Int> uniqueCells, int currentPlayer)
        {
            _scoreService.AddScore(currentPlayer, uniqueCells.Count);
            _inGameUIService.UpdateScore(_scoreService.GetScore(1), _scoreService.GetScore(2));

            for (int i = 0; i < uniqueCells.Count; i++)
            {
                _fieldFigureService.SetFigure(uniqueCells[i], CellFigure.None, isQueue: false);
            }
        }

        private static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float p = t - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        private static Vector2 GetScreenPointFromRect(Vector3 worldPoint, RectTransform sourceRect)
        {
            Canvas sourceCanvas = sourceRect != null ? sourceRect.GetComponentInParent<Canvas>() : null;
            Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;
            return RectTransformUtility.WorldToScreenPoint(sourceCamera, worldPoint);
        }

        private static Vector2 ConvertWorldToLayerLocal(
            Vector3 worldPoint,
            RectTransform sourceRect,
            RectTransform layer)
        {
            if (layer == null) return new Vector2(worldPoint.x, worldPoint.y);

            Vector2 screenPoint = GetScreenPointFromRect(worldPoint, sourceRect);

            Canvas layerCanvas = layer.GetComponentInParent<Canvas>();
            Camera layerCamera = layerCanvas != null && layerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? layerCanvas.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPoint, layerCamera, out Vector2 layerLocal);
            return layerLocal;
        }

        private class FlyingIconData
        {
            public Image Image;
            public Vector3 StartPosition;
            public Vector3 StartScale;
            public Vector2 Perpendicular;
            public float SineAmplitude;
            public int StartFrame;
            public bool IsScored;
            public bool IsFinished;
        }
    }
}
