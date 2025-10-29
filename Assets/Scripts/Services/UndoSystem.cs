using System.Collections.Generic;
using System.Linq;
using Game.Core;

namespace Game.Gameplay
{
    public class UndoSystem
    {
        private readonly Stack<BoardState> _stateHistory = new();
        private const int MAX_UNDO_STACK = 100;
        
        public void SaveState(BoardState state)
        {
            _stateHistory.Push(state);
            
            if (_stateHistory.Count > MAX_UNDO_STACK)
            {
                var temp = new Stack<BoardState>(_stateHistory.Take(MAX_UNDO_STACK).Reverse());
                _stateHistory.Clear();
                
                foreach (var s in temp)
                    _stateHistory.Push(s);
            }
        }
        
        public bool CanUndo() => _stateHistory.Count > 1;
        
        public BoardState Undo()
        {
            if (!CanUndo()) return default;
            
            _stateHistory.Pop();
            return _stateHistory.Peek();
        }
        
        public void Reset()
        {
            _stateHistory.Clear();
        }
    }
    
    public readonly struct BoardState
    {
        public readonly GridPosition PlayerPosition;
        public readonly GridPosition[] BoxPositions;
        
        public BoardState(GridPosition playerPosition, GridPosition[] boxPositions)
        {
            PlayerPosition = playerPosition;
            BoxPositions = boxPositions;
        }
    }
}