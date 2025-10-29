using Game.Core;

namespace Game.Services
{
    public class MoveValidator
    {
        private readonly GridService _gridService;
        
        public MoveValidator(GridService gridService)
        {
            _gridService = gridService;
        }
        
        public bool CanMove(GridPosition from, Direction direction, out MoveResult result)
        {
            GridPosition targetPosition = from.Move(direction);
            
            if (_gridService.IsWalkable(targetPosition))
            {
                result = new MoveResult(targetPosition, false, new GridPosition(0, 0));
                return true;
            }
            
            if (_gridService.TryGetBox(targetPosition, out _))
            {
                GridPosition boxTargetPosition = targetPosition.Move(direction);
                
                if (_gridService.IsWalkable(boxTargetPosition))
                {
                    result = new MoveResult(targetPosition, true, boxTargetPosition);
                    return true;
                }
            }
            
            result = default;
            return false;
        }
    }
    
    public readonly struct MoveResult
    {
        public readonly GridPosition PlayerPosition;
        public readonly bool PushesBox;
        public readonly GridPosition BoxPosition;
        
        public MoveResult(GridPosition playerPosition, bool pushesBox, GridPosition boxPosition)
        {
            PlayerPosition = playerPosition;
            PushesBox = pushesBox;
            BoxPosition = boxPosition;
        }
    }
}