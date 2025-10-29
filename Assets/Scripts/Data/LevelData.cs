using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Level", menuName = "Sokoban/Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private int _width = 10;
        [SerializeField] private int _height = 10;
        [SerializeField] private TileType[] _tiles;
        [SerializeField] private int _playerStartX;
        [SerializeField] private int _playerStartY;
        [SerializeField] private List<SerializableGridPosition> _boxStartPositions = new();
        
        public int Width => _width;
        public int Height => _height;
        
        public GridPosition PlayerStartPosition => new GridPosition(_playerStartX, _playerStartY);
        
        public IReadOnlyList<GridPosition> BoxStartPositions
        {
            get
            {
                var positions = new GridPosition[_boxStartPositions.Count];
                for (int i = 0; i < _boxStartPositions.Count; i++)
                {
                    positions[i] = new GridPosition(_boxStartPositions[i].X, _boxStartPositions[i].Y);
                }
                return positions;
            }
        }
        
        public TileType GetTile(GridPosition position)
        {
            if (!IsInBounds(position)) return TileType.Empty;
            
            int index = position.Y * _width + position.X;
            return _tiles != null && index < _tiles.Length ? _tiles[index] : TileType.Empty;
        }
        
        public bool IsInBounds(GridPosition position)
        {
            return position.X >= 0 && position.X < _width 
                && position.Y >= 0 && position.Y < _height;
        }
        
        private void OnValidate()
        {
            if (_tiles == null || _tiles.Length != _width * _height)
            {
                _tiles = new TileType[_width * _height];
            }
        }
        
#if UNITY_EDITOR
        public void SetTile(GridPosition position, TileType tileType)
        {
            if (!IsInBounds(position)) return;
            
            int index = position.Y * _width + position.X;
            _tiles[index] = tileType;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        public void SetPlayerStart(GridPosition position)
        {
            _playerStartX = position.X;
            _playerStartY = position.Y;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        public void AddBox(GridPosition position)
        {
            var serializablePos = new SerializableGridPosition { X = position.X, Y = position.Y };
            
            foreach (var existingPos in _boxStartPositions)
            {
                if (existingPos.X == position.X && existingPos.Y == position.Y)
                    return;
            }
            
            _boxStartPositions.Add(serializablePos);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        public void RemoveBox(GridPosition position)
        {
            for (int i = _boxStartPositions.Count - 1; i >= 0; i--)
            {
                if (_boxStartPositions[i].X == position.X && _boxStartPositions[i].Y == position.Y)
                {
                    _boxStartPositions.RemoveAt(i);
                    break;
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        public void Resize(int width, int height)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _tiles = new TileType[_width * _height];
            _boxStartPositions.Clear();
            _playerStartX = 0;
            _playerStartY = 0;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
    
    [System.Serializable]
    public struct SerializableGridPosition
    {
        public int X;
        public int Y;
    }
}