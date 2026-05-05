using System.Collections.Generic;
using UnityEngine;

namespace FinishLine.Interfaces
{
    public interface IFinishLineMasterCheckerService
    {
        public List<List<Vector2Int>> FindLinesToClear(CellFigure figure, IReadOnlyList<List<Vector2Int>> linesInProgress);
    }
}
