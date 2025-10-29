using UnityEngine;
using Game.Core;
using Game.Services;

namespace Game.Gameplay
{
    public class Player : MonoBehaviour
    {
        private GameBoard _gameBoard;
        private GridPosition _currentPosition;
        private bool _canMove = true;
        
        public GridPosition CurrentPosition => _currentPosition;
        
        public void Initialize(GameBoard gameBoard, GridPosition startPosition)
        {
            _gameBoard = gameBoard;
            _currentPosition = startPosition;
            transform.position = new Vector3(startPosition.X, startPosition.Y, -0.2f);
        }
        
        private void Update()
        {
            if (!_canMove) return;
            
            Direction? direction = null;
            
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                direction = Direction.Up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                direction = Direction.Down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                direction = Direction.Left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                direction = Direction.Right;
            else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
            {
                _gameBoard.Undo();
                return;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _gameBoard.ResetLevel();
                return;
            }
            
            if (direction.HasValue)
                TryMove(direction.Value);
        }
        
        private void TryMove(Direction direction)
        {
            if (!_gameBoard.MoveValidator.CanMove(_currentPosition, direction, out MoveResult result))
                return;
            
            GridPosition oldPosition = _currentPosition;
            _currentPosition = result.PlayerPosition;
            transform.position = new Vector3(_currentPosition.X, _currentPosition.Y, -0.2f);
            _gameBoard.GridService.SetPlayerPosition(_currentPosition);
            
            if (result.PushesBox)
            {
                if (_gameBoard.GridService.TryGetBox(result.PlayerPosition, out Box box))
                {
                    _gameBoard.GridService.UnregisterBox(result.PlayerPosition);
                    box.SetPosition(result.BoxPosition);
                    _gameBoard.GridService.RegisterBox(result.BoxPosition, box);
                }
            }
            
            _gameBoard.OnMoveCompleted();
        }
        
        public void SetPosition(GridPosition position)
        {
            _currentPosition = position;
            transform.position = new Vector3(position.X, position.Y, -0.2f);
        }
        
        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }
    }
}