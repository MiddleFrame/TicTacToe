using System.Collections.Generic;
using Field.Interfaces;
using FinishLine.Interfaces;
using UnityEngine;

namespace FinishLine
{
    public class FinishLineMasterChecker : IFinishLineMasterCheckerService
    {
        private const int CURRENT_GOAL_LINE = 3;
        private static readonly Vector2Int InvalidCell = new(-1, -1);

        private readonly IFieldService _fieldService;
        private readonly IFieldFigureService _fieldFigureService;

        public FinishLineMasterChecker(IFieldService fieldService, IFieldFigureService fieldFigureService)
        {
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
        }

        public List<List<Vector2Int>> FindLinesToClear(CellFigure figure, IReadOnlyList<List<Vector2Int>> linesInProgress)
        {
            List<Vector2Int> verticalList = new();
            List<Vector2Int> horizontalList = new();
            List<Vector2Int> diagonalRightList = new();
            List<Vector2Int> diagonalLeftList = new();
            List<List<Vector2Int>> linesFind = new();

            for (int x = 0; x < _fieldService.GetFieldSize().x; x++)
            {
                for (int y = 0; y < _fieldService.GetFieldSize().y; y++)
                {
                    Vector2Int currentId = new Vector2Int(x, y);
                    TryCollectLine(currentId, figure, verticalList, new Vector2Int(0, 1), linesFind);
                    TryCollectLine(currentId, figure, horizontalList, new Vector2Int(1, 0), linesFind);
                    TryCollectLine(currentId, figure, diagonalRightList, new Vector2Int(1, 1), linesFind);

                    Vector2Int mirroredId = new Vector2Int(x, _fieldService.GetFieldSize().y - y - 1);
                    TryCollectLine(mirroredId, figure, diagonalLeftList, new Vector2Int(1, -1), linesFind);
                }
            }

            List<List<Vector2Int>> linesResult = new();
            foreach (List<Vector2Int> line in linesFind)
            {
                if (line.Count < CURRENT_GOAL_LINE) continue;
                if (ContainsSameLine(linesInProgress, line)) continue;
                linesResult.Add(line);
            }

            return linesResult;
        }

        private void TryCollectLine(
            Vector2Int currentId,
            CellFigure figure,
            List<Vector2Int> scannedCells,
            Vector2Int step,
            List<List<Vector2Int>> linesFind)
        {
            if (_fieldFigureService.GetCellFigure(currentId) != figure || _fieldFigureService.GetIsCellClear(currentId)) return;
            if (scannedCells.IndexOf(currentId) != -1) return;

            Vector2Int nextVal = GetNextCellId(currentId, step);
            if (nextVal == InvalidCell) return;

            List<Vector2Int> newLine = new();
            Vector2Int currentLocal = currentId;
            newLine.Add(currentLocal);
            scannedCells.Add(currentLocal);

            while (nextVal != InvalidCell)
            {
                newLine.Add(nextVal);
                scannedCells.Add(nextVal);
                currentLocal = nextVal;
                nextVal = GetNextCellId(currentLocal, step);
            }

            linesFind.Add(newLine);
        }

        private Vector2Int GetNextCellId(Vector2Int currentId, Vector2Int step)
        {
            Vector2Int nextId = currentId + step;
            if (nextId.x < 0 || nextId.y < 0 || nextId.x >= _fieldService.GetFieldSize().x ||
                nextId.y >= _fieldService.GetFieldSize().y) return InvalidCell;
            if (_fieldFigureService.GetIsCellClear(nextId)) return InvalidCell;
            if (_fieldFigureService.IsCellBlocked(nextId)) return InvalidCell;
            if (_fieldFigureService.GetCellFigure(nextId) != _fieldFigureService.GetCellFigure(currentId)) return InvalidCell;

            return nextId;
        }

        private static bool ContainsSameLine(IReadOnlyList<List<Vector2Int>> lines, List<Vector2Int> line)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i][0] == line[0] && lines[i][^1] == line[^1]) return true;
            }

            return false;
        }
    }
}
