using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class Box : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _onGoalColor = new Color(0.2f, 0.8f, 0.2f);
        
        private GameBoard _gameBoard;
        private GridPosition _currentPosition;
        
        public GridPosition CurrentPosition => _currentPosition;
        
        public void Initialize(GameBoard gameBoard, GridPosition startPosition)
        {
            _gameBoard = gameBoard;
            SetPosition(startPosition);
        }
        
        public void SetPosition(GridPosition position)
        {
            _currentPosition = position;
            transform.position = new Vector3(position.X, position.Y, -0.1f);
            
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            bool isOnGoal = _gameBoard.GridService.IsGoal(_currentPosition);
            _spriteRenderer.color = isOnGoal ? _onGoalColor : _normalColor;
        }
    }
}