using System;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Field.Interfaces;
using FinishLine.Interfaces;
using Players.Interfaces;
using UIPages.Interfaces;
using UnityEngine;
using Score.Interfaces;

namespace FinishLine
{
    public class FinishLineLineCellAnimation : ICellClearAnimationService
    {
        private readonly IPlayerService _playerService;
        private readonly IScoreService _scoreService;
        private readonly IFieldFigureService _fieldFigureService;
        private readonly IInGameUIService _inGameUIService;
        private readonly IAudioService _audioService;

        public FinishLineLineCellAnimation(
            IPlayerService playerService,
            IScoreService scoreService,
            IFieldFigureService fieldFigureService,
            IInGameUIService inGameUIService,
            IAudioService audioService)
        {
            _playerService = playerService;
            _scoreService = scoreService;
            _fieldFigureService = fieldFigureService;
            _inGameUIService = inGameUIService;
            _audioService = audioService;
        }

        public float AnimationFrames => FinishLineObject.FINISH_COUNT_FRAME;

        public IEnumerator Play(List<List<Vector2Int>> lines, Action<List<Vector2Int>, int> drawLineAction)
        {
            int currentPlayer = _playerService.GetCurrentPlayer().SideId;
            List<Vector2Int> uniqueCells = new();

            foreach (List<Vector2Int> line in lines)
            {
                _audioService.Play(SoundPresetIds.DamageErase);
                int lineScore = 0;

                foreach (Vector2Int cell in line)
                {
                    if (uniqueCells.IndexOf(cell) != -1) continue;

                    uniqueCells.Add(cell);
                    lineScore += 1;
                }

                drawLineAction?.Invoke(line, lineScore);
            }

            float frame = 0f;
            while (frame < FinishLineObject.FINISH_COUNT_FRAME)
            {
                frame++;
                yield return null;
            }

            _scoreService.AddScore(currentPlayer, uniqueCells.Count);
            _inGameUIService.UpdateScore(_scoreService.GetScore(1), _scoreService.GetScore(2));

            foreach (Vector2Int cell in uniqueCells)
            {
                _fieldFigureService.SetFigure(cell, CellFigure.None, isQueue: false);
            }
        }
    }
}
