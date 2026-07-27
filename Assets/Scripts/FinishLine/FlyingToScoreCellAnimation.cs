using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
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
        private const bool ENABLE_SIZE_TRACE_LOGS = false;
        private const float ICON_SIZE_FACTOR = 1.0f;
        private const float SCALE_UP_FACTOR = 1.15f;
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
        private readonly IAudioService _audioService;

        public FlyingToScoreCellAnimation(
            IPlayerService playerService,
            IScoreService scoreService,
            IFieldService fieldService,
            IFieldFigureService fieldFigureService,
            IInGameUIService inGameUIService,
            IThemeService themeService,
            IAudioService audioService)
        {
            _playerService = playerService;
            _scoreService = scoreService;
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
            _inGameUIService = inGameUIService;
            _themeService = themeService;
            _audioService = audioService;
        }

        public float AnimationFrames => 3f;

        public IEnumerator Play(List<List<Vector2Int>> lines, Action<List<Vector2Int>, int> drawLineAction)
        {
            _ = drawLineAction;
            List<Vector2Int> uniqueCells = CollectUniqueCells(lines);
            if (uniqueCells.Count == 0)
            {
                yield break;
            }

            int currentPlayer = _playerService.GetCurrentPlayer().SideId;
            RectTransform targetRect = _inGameUIService.GetScoreSliderRectTransform(currentPlayer);
            RectTransform animationLayer = ResolveAnimationLayer(targetRect);
            Canvas animationCanvas = _inGameUIService.GetScoreFlyAnimationCanvas();
           
            Canvas targetCanvas = _inGameUIService.GetScoreSliderCanvas(currentPlayer);
           
            if (ENABLE_SIZE_TRACE_LOGS)
            {
                Debug.Log(
                    $"[FlySizeTrace][Env] resolution={Screen.width}x{Screen.height} aspect={(float)Screen.width / Screen.height:F3} " +
                    $"layer={(animationLayer != null ? animationLayer.name : "null")} " +
                    $"layerCanvas={(animationCanvas != null ? animationCanvas.name : "null")} " +
                    $"layerCanvasScale={(animationCanvas != null ? animationCanvas.scaleFactor.ToString("F3") : "n/a")} " +
                    $"targetRect={(targetRect != null ? targetRect.name : "null")} currentPlayer={currentPlayer}");
            }

            List<FlyingIconData> icons = SpawnIcons(uniqueCells, animationLayer, animationCanvas);
            if (icons.Count == 0)
            {
                ApplyScoreAndClear(uniqueCells);
                yield break;
            }

            for (int i = 0; i < uniqueCells.Count; i++)
            {
                _fieldFigureService.SetFigure(uniqueCells[i], CellFigure.None, isQueue: false);
            }

            yield return AnimateIconsToTarget(icons);
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

        private List<FlyingIconData> SpawnIcons(List<Vector2Int> cells, RectTransform layer, Canvas layerCanvas)
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
                Image sourceFigureImage = cellLink.FigureImage;
                RectTransform sourceIconRect = sourceFigureImage != null
                    ? sourceFigureImage.rectTransform
                    : sourceRect;
                Canvas sourceCanvas = cellLink.ParentCanvas;
                if (sourceCanvas == null)
                {
                    sourceCanvas = sourceIconRect.GetComponentInParent<Canvas>();
                }
                Vector2 sizeInLayerByWorldCorners = GetRectSizeInLayerSpace(sourceIconRect, sourceCanvas, layer, layerCanvas);
                iconRect.sizeDelta = sizeInLayerByWorldCorners;
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
                Vector2 startScreenPosition = GetScreenPoint(sourceWorldPosition, sourceCanvas);
                Vector2 layerStartLocal = ConvertWorldToLayerLocal(sourceWorldPosition, sourceCanvas, layer, layerCanvas);
                iconRect.localPosition = new Vector3(layerStartLocal.x, layerStartLocal.y, 0f);

                if (ENABLE_SIZE_TRACE_LOGS)
                {
                    Vector2 sourceRectSize = sourceRect.rect.size;
                    Vector2 sourceIconRectSize = sourceIconRect.rect.size;
                    Vector2 sourceWorldSize = GetRectWorldSize(sourceIconRect);
                    Vector2 sourceScreenSize = GetRectScreenSize(sourceIconRect, sourceCanvas);
                    Vector2 sourceToLayerByScreen = GetRectSizeInLayerSpaceByScreen(sourceIconRect, sourceCanvas, layer, layerCanvas);
                    Debug.Log(
                        $"[FlySizeTrace][Cell] figure={cellFigure} cell={cellId} " +
                        $"sourceRectSize={sourceRectSize} sourceIconRectSize={sourceIconRectSize} " +
                        $"sourceWorldSize={sourceWorldSize} sourceScreenSizePx={sourceScreenSize} " +
                        $"toLayerByWorldCorners={sizeInLayerByWorldCorners} toLayerByScreen={sourceToLayerByScreen} " +
                        $"finalIconSize={iconRect.sizeDelta} factor={ICON_SIZE_FACTOR:F2} " +
                        $"sourceCanvas={(sourceCanvas != null ? sourceCanvas.name : "null")} " +
                        $"sourceCanvasScale={(sourceCanvas != null ? sourceCanvas.scaleFactor.ToString("F3") : "n/a")} " +
                        $"layerCanvas={(layerCanvas != null ? layerCanvas.name : "null")} " +
                        $"layerCanvasScale={(layerCanvas != null ? layerCanvas.scaleFactor.ToString("F3") : "n/a")} " +
                        $"sourceLossyScale={sourceIconRect.lossyScale} layerLossyScale={(layer != null ? layer.lossyScale.ToString() : "n/a")}");
                }

                int scoreSide = ResolveFigureScoreSide(cellFigure);
                int damageTargetSide = GetOppositeSide(scoreSide);
                RectTransform targetRect = _inGameUIService.GetScoreSliderRectTransform(damageTargetSide);
                Canvas targetCanvas = _inGameUIService.GetScoreSliderCanvas(damageTargetSide);
                if (targetCanvas == null && targetRect != null)
                {
                    targetCanvas = targetRect.GetComponentInParent<Canvas>();
                }
                Vector3 targetWorldPosition = targetRect != null
                    ? targetRect.position
                    : _inGameUIService.GetScoreSliderWorldPosition(damageTargetSide);
                Vector2 layerTargetLocal = ConvertWorldToLayerLocal(targetWorldPosition, targetCanvas, layer, layerCanvas);
                Vector2 toTarget = layerTargetLocal - new Vector2(iconRect.localPosition.x, iconRect.localPosition.y);
                if (ENABLE_TARGET_TRACE_LOGS)
                {
                    Debug.Log(
                        $"[FlyTargetTrace] figure={cellFigure} cell={cellId} scoreSide={scoreSide} damageTargetSide={damageTargetSide} " +
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
                    TargetPosition = new Vector3(layerTargetLocal.x, layerTargetLocal.y, 0f),
                    ScoreSide = scoreSide,
                    Perpendicular = perpendicular * direction,
                    SineAmplitude = amplitude,
                    StartFrame = i * START_DELAY_FRAMES
                });
            }

            return icons;
        }

        private IEnumerator AnimateIconsToTarget(List<FlyingIconData> icons)
        {
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
                    Vector3 basePosition = Vector3.LerpUnclamped(icon.StartPosition, icon.TargetPosition, eased);
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
                            _audioService.Play(SoundPresetIds.DamageImpact);
                            _scoreService.AddScore(icon.ScoreSide, 1);
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
                    _audioService.Play(SoundPresetIds.DamageImpact);
                    _scoreService.AddScore(icon.ScoreSide, 1);
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
                if (targetCanvas != null)
                {
                    return targetCanvas.transform as RectTransform;
                }
            }

            RectTransform configuredLayer = _inGameUIService.GetScoreFlyAnimationLayer();
            if (configuredLayer != null)
            {
                return configuredLayer;
            }

            return targetRect;
        }

        private int ResolveFigureScoreSide(CellFigure figure)
        {
            if (figure == CellFigure.P1) return 1;
            if (figure == CellFigure.P2) return 2;
            return _playerService.GetCurrentPlayer().SideId;
        }

        private static int GetOppositeSide(int side)
        {
            return side == 1 ? 2 : 1;
        }

        private void ApplyScoreAndClear(List<Vector2Int> uniqueCells)
        {
            int scoreP1 = 0;
            int scoreP2 = 0;
            for (int i = 0; i < uniqueCells.Count; i++)
            {
                CellFigure figure = _fieldFigureService.GetCellFigure(uniqueCells[i]);
                if (figure == CellFigure.P1) scoreP1++;
                else if (figure == CellFigure.P2) scoreP2++;
            }

            if (scoreP1 > 0) _scoreService.AddScore(1, scoreP1);
            if (scoreP2 > 0) _scoreService.AddScore(2, scoreP2);
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

        private static Vector2 GetScreenPoint(Vector3 worldPoint, Canvas sourceCanvas)
        {
            Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;
            return RectTransformUtility.WorldToScreenPoint(sourceCamera, worldPoint);
        }

        private static Vector2 ConvertWorldToLayerLocal(
            Vector3 worldPoint,
            Canvas sourceCanvas,
            RectTransform layer,
            Canvas layerCanvas)
        {
            if (layer == null) return new Vector2(worldPoint.x, worldPoint.y);

            Vector2 screenPoint = GetScreenPoint(worldPoint, sourceCanvas);
            Camera layerCamera = layerCanvas != null && layerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? layerCanvas.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPoint, layerCamera, out Vector2 layerLocal);
            return layerLocal;
        }

        private static Vector2 GetRectSizeInLayerSpace(
            RectTransform sourceRect,
            Canvas sourceCanvas,
            RectTransform layer,
            Canvas layerCanvas)
        {
            if (sourceRect == null) return Vector2.zero;
            if (layer == null) return sourceRect.rect.size;

            Vector3[] worldCorners = new Vector3[4];
            sourceRect.GetWorldCorners(worldCorners);

            Vector2 p0 = ConvertWorldToLayerLocal(worldCorners[0], sourceCanvas, layer, layerCanvas);
            Vector2 p1 = ConvertWorldToLayerLocal(worldCorners[1], sourceCanvas, layer, layerCanvas);
            Vector2 p3 = ConvertWorldToLayerLocal(worldCorners[3], sourceCanvas, layer, layerCanvas);

            float width = Vector2.Distance(p0, p3);
            float height = Vector2.Distance(p0, p1);

            return new Vector2(width, height);
        }

        private static Vector2 GetRectWorldSize(RectTransform sourceRect)
        {
            if (sourceRect == null) return Vector2.zero;
            Vector3[] worldCorners = new Vector3[4];
            sourceRect.GetWorldCorners(worldCorners);
            float width = Vector3.Distance(worldCorners[0], worldCorners[3]);
            float height = Vector3.Distance(worldCorners[0], worldCorners[1]);
            return new Vector2(width, height);
        }

        private static Vector2 GetRectScreenSize(RectTransform sourceRect, Canvas sourceCanvas)
        {
            if (sourceRect == null) return Vector2.zero;
            Vector3[] worldCorners = new Vector3[4];
            sourceRect.GetWorldCorners(worldCorners);
            Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;

            Vector2 p0 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[0]);
            Vector2 p1 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[1]);
            Vector2 p3 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[3]);
            float width = Vector2.Distance(p0, p3);
            float height = Vector2.Distance(p0, p1);
            return new Vector2(width, height);
        }

        private static Vector2 GetRectSizeInLayerSpaceByScreen(
            RectTransform sourceRect,
            Canvas sourceCanvas,
            RectTransform layer,
            Canvas layerCanvas)
        {
            if (sourceRect == null) return Vector2.zero;
            if (layer == null) return sourceRect.rect.size;

            Vector3[] worldCorners = new Vector3[4];
            sourceRect.GetWorldCorners(worldCorners);
            Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;

            Vector2 s0 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[0]);
            Vector2 s1 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[1]);
            Vector2 s3 = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldCorners[3]);

            Camera layerCamera = layerCanvas != null && layerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? layerCanvas.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, s0, layerCamera, out Vector2 l0);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, s1, layerCamera, out Vector2 l1);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, s3, layerCamera, out Vector2 l3);
            float width = Vector2.Distance(l0, l3);
            float height = Vector2.Distance(l0, l1);
            return new Vector2(width, height);
        }

        private class FlyingIconData
        {
            public Image Image;
            public Vector3 StartPosition;
            public Vector3 StartScale;
            public Vector3 TargetPosition;
            public int ScoreSide;
            public Vector2 Perpendicular;
            public float SineAmplitude;
            public int StartFrame;
            public bool IsScored;
            public bool IsFinished;
        }
    }
}
