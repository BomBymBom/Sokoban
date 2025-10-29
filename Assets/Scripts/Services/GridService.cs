using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Gameplay;

namespace Game.Services
{
    public class GridService
    {
        private readonly LevelData _levelData;
        private readonly Dictionary<GridPosition, Box> _boxes = new();
        private GridPosition _playerPosition;
        
        public GridService(LevelData levelData)
        {
            _levelData = levelData;
        }
        
        public void RegisterBox(GridPosition position, Box box)
        {
            _boxes[position] = box;
        }
        
        public void UnregisterBox(GridPosition position)
        {
            _boxes.Remove(position);
        }
        
        public bool TryGetBox(GridPosition position, out Box box)
        {
            return _boxes.TryGetValue(position, out box);
        }
        
        public void SetPlayerPosition(GridPosition position)
        {
            _playerPosition = position;
        }
        
        public GridPosition GetPlayerPosition() => _playerPosition;
        
        public bool IsWalkable(GridPosition position)
        {
            if (!_levelData.IsInBounds(position)) return false;
            
            TileType tile = _levelData.GetTile(position);
            if (tile == TileType.Wall || tile == TileType.Empty) return false;
            
            return !_boxes.ContainsKey(position);
        }
        
        public bool IsGoal(GridPosition position)
        {
            return _levelData.GetTile(position) == TileType.Goal;
        }
        
        public IEnumerable<GridPosition> GetAllBoxPositions() => _boxes.Keys;
    }
}