using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Data;
using Game.Services;

namespace Game.Gameplay
{
    public class GameBoard : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelData _levelData;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private Box _boxPrefab;
        
        private GridService _gridService;
        private MoveValidator _moveValidator;
        private WinConditionChecker _winChecker;
        private UndoSystem _undoSystem;
        
        private Player _player;
        private readonly List<Box> _boxes = new();
        
        private Transform _tilesParent;
        private Transform _entitiesParent;
        
        public GridService GridService => _gridService;
        public MoveValidator MoveValidator => _moveValidator;
        
        private void Awake()
        {
            _gridService = new GridService(_levelData);
            _moveValidator = new MoveValidator(_gridService);
            _winChecker = new WinConditionChecker(_gridService);
            _undoSystem = new UndoSystem();
            
            CreateLevel();
        }
        
        private void CreateLevel()
        {
            _tilesParent = new GameObject("Tiles").transform;
            _tilesParent.SetParent(transform);
            
            _entitiesParent = new GameObject("Entities").transform;
            _entitiesParent.SetParent(transform);
            
            for (int y = 0; y < _levelData.Height; y++)
            {
                for (int x = 0; x < _levelData.Width; x++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    TileType tile = _levelData.GetTile(pos);
                    
                    GameObject prefab = tile switch
                    {
                        TileType.Floor => _floorPrefab,
                        TileType.Wall => _wallPrefab,
                        TileType.Goal => _goalPrefab,
                        _ => null
                    };
                    
                    if (prefab != null)
                    {
                        var tileObj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, _tilesParent);
                        tileObj.name = $"{tile}_{x}_{y}";
                    }
                }
            }
            
            GridPosition playerPos = _levelData.PlayerStartPosition;
            _player = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity, _entitiesParent);
            _player.Initialize(this, playerPos);
            _gridService.SetPlayerPosition(playerPos);
            
            foreach (var boxPos in _levelData.BoxStartPositions)
            {
                var box = Instantiate(_boxPrefab, Vector3.zero, Quaternion.identity, _entitiesParent);
                box.Initialize(this, boxPos);
                _gridService.RegisterBox(boxPos, box);
                _boxes.Add(box);
            }
            
            CenterCamera();
            _undoSystem.SaveState(CreateBoardState());
        }
        
        private void CenterCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float centerX = (_levelData.Width - 1) / 2f;
                float centerY = (_levelData.Height - 1) / 2f;
                cam.transform.position = new Vector3(centerX, centerY, -10);
                
                float verticalSize = _levelData.Height / 2f + 1f;
                cam.orthographicSize = verticalSize;
            }
        }
        
        public void OnMoveCompleted()
        {
            _undoSystem.SaveState(CreateBoardState());
            
            if (_winChecker.IsLevelComplete())
            {
                Debug.Log("<color=#00FF00>[GameBoard] Level Complete!</color>");
                _player.SetCanMove(false);
            }
        }
        
        public void Undo()
        {
            if (!_undoSystem.CanUndo()) return;
            
            BoardState state = _undoSystem.Undo();
            RestoreBoardState(state);
        }
        
        public void ResetLevel()
        {
            while (_undoSystem.CanUndo())
            {
                BoardState state = _undoSystem.Undo();
                RestoreBoardState(state);
            }
        }
        
        private void RestoreBoardState(BoardState state)
        {
            _player.SetPosition(state.PlayerPosition);
            _gridService.SetPlayerPosition(state.PlayerPosition);
            
            for (int i = 0; i < _boxes.Count && i < state.BoxPositions.Length; i++)
            {
                GridPosition oldPos = _boxes[i].CurrentPosition;
                GridPosition newPos = state.BoxPositions[i];
                
                _gridService.UnregisterBox(oldPos);
                _boxes[i].SetPosition(newPos);
                _gridService.RegisterBox(newPos, _boxes[i]);
            }
            
            _player.SetCanMove(true);
        }
        
        private BoardState CreateBoardState()
        {
            var boxPositions = _boxes.Select(b => b.CurrentPosition).ToArray();
            return new BoardState(_gridService.GetPlayerPosition(), boxPositions);
        }
    }
}