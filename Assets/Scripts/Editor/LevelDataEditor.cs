#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using Game.Data;
using Game.Core;

namespace Game.Editor
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private TileType _selectedTileType = TileType.Floor;
        private PaintMode _paintMode = PaintMode.Tiles;
        private Vector2 _scrollPosition;
        
        private int _tempWidth;
        private int _tempHeight;
        private bool _dimensionsChanged;
        
        private enum PaintMode
        {
            Tiles,
            PlayerStart,
            Boxes
        }
        
        private void OnEnable()
        {
            LevelData levelData = (LevelData)target;
            _tempWidth = levelData.Width;
            _tempHeight = levelData.Height;
        }
        
        public override void OnInspectorGUI()
        {
            LevelData levelData = (LevelData)target;
            
            EditorGUILayout.LabelField("Level Dimensions", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            _tempWidth = EditorGUILayout.IntSlider("Width", _tempWidth, 3, 25);
            _tempHeight = EditorGUILayout.IntSlider("Height", _tempHeight, 3, 25);
            
            if (EditorGUI.EndChangeCheck())
            {
                _dimensionsChanged = true;
            }
            
            if (_dimensionsChanged && (_tempWidth != levelData.Width || _tempHeight != levelData.Height))
            {
                EditorGUILayout.HelpBox($"Resize to {_tempWidth}x{_tempHeight}? This will clear all data.", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Resize"))
                {
                    Undo.RecordObject(levelData, "Resize Level");
                    levelData.Resize(_tempWidth, _tempHeight);
                    _dimensionsChanged = false;
                }
                if (GUILayout.Button("Cancel"))
                {
                    _tempWidth = levelData.Width;
                    _tempHeight = levelData.Height;
                    _dimensionsChanged = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Paint Mode", EditorStyles.boldLabel);
            _paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode", _paintMode);
            
            if (_paintMode == PaintMode.Tiles)
            {
                _selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", _selectedTileType);
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill Floor"))
            {
                Undo.RecordObject(levelData, "Fill Floor");
                FillAllTiles(levelData, TileType.Floor);
            }
            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Clear Level", 
                    "Clear all tiles, boxes, and player position?", "Yes", "No"))
                {
                    Undo.RecordObject(levelData, "Clear Level");
                    levelData.Resize(levelData.Width, levelData.Height);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            DrawLevelInfo(levelData);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("P = Player, B = Box", EditorStyles.miniLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));
            DrawGrid(levelData);
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawLevelInfo(LevelData levelData)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Player Start: {levelData.PlayerStartPosition}");
            EditorGUILayout.LabelField($"Boxes: {levelData.BoxStartPositions.Count}");
            
            int goalCount = 0;
            for (int y = 0; y < levelData.Height; y++)
            {
                for (int x = 0; x < levelData.Width; x++)
                {
                    if (levelData.GetTile(new GridPosition(x, y)) == TileType.Goal)
                        goalCount++;
                }
            }
            EditorGUILayout.LabelField($"Goals: {goalCount}");
            
            if (goalCount != levelData.BoxStartPositions.Count)
            {
                EditorGUILayout.HelpBox("Box count should match goal count!", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGrid(LevelData levelData)
        {
            float cellSize = 25f;
            
            for (int y = levelData.Height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < levelData.Width; x++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    DrawCell(levelData, pos, cellSize);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawCell(LevelData levelData, GridPosition pos, float cellSize)
        {
            TileType currentTile = levelData.GetTile(pos);
            bool isPlayerStart = pos.Equals(levelData.PlayerStartPosition);
            bool hasBox = levelData.BoxStartPositions.Any(bp => bp.Equals(pos));
            
            Color backgroundColor = currentTile switch
            {
                TileType.Floor => new Color(0.9f, 0.9f, 0.9f),
                TileType.Wall => new Color(0.3f, 0.3f, 0.3f),
                TileType.Goal => new Color(1f, 0.9f, 0.3f),
                _ => Color.black
            };
            
            GUI.backgroundColor = backgroundColor;
            
            string label = "";
            if (isPlayerStart) label = "P";
            else if (hasBox) label = "B";
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = isPlayerStart ? Color.blue : (hasBox ? new Color(0.6f, 0.3f, 0f) : Color.clear) }
            };
            
            if (GUILayout.Button(label, buttonStyle, GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
            {
                Undo.RecordObject(levelData, "Paint Cell");
                HandleCellClick(levelData, pos);
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        private void HandleCellClick(LevelData levelData, GridPosition pos)
        {
            switch (_paintMode)
            {
                case PaintMode.Tiles:
                    levelData.SetTile(pos, _selectedTileType);
                    break;
                    
                case PaintMode.PlayerStart:
                    levelData.SetPlayerStart(pos);
                    break;
                    
                case PaintMode.Boxes:
                    bool hasBox = levelData.BoxStartPositions.Any(bp => bp.Equals(pos));
                    if (hasBox)
                        levelData.RemoveBox(pos);
                    else
                        levelData.AddBox(pos);
                    break;
            }
        }
        
        private void FillAllTiles(LevelData levelData, TileType tileType)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                for (int x = 0; x < levelData.Width; x++)
                {
                    levelData.SetTile(new GridPosition(x, y), tileType);
                }
            }
        }
    }
}
#endif