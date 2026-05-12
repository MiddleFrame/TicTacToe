using System.Collections.Generic;
using UnityEngine;

namespace Field.Interfaces
{
    public interface IFieldService
    {
        public Vector2Int GetFieldSize();
        public bool IsExistEmptyCell();
        
        public void Initialization(int round = 0);
        public bool CheckIsInField(Vector2 pos);
        public bool IsInFieldHeight(float h);
        public List<Cell> GetAllEmptyNeighbours(Cell cell);
        public Vector2Int GetIdFromPosition(Vector2 pos, bool isFindBorder);
    
        public Cell IsOnField(Vector2Int currentId, Vector2Int step);
        public Cell GetNextCell(Vector2Int currentId, Vector2Int step);

        public float GetCellSize();
        public Vector2 GetCellPosition(Vector2Int id);
        public Vector2 GetCellPosition(int x, int y);
        
        public Cell GetCellLink(Vector2Int id);
        public Cell GetCellLink(int x,int y);
        public void SetFieldDragHighlightState(bool state);
        public void UpdateFieldDragHighlight();
    }
}