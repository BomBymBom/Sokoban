using System.Linq;

namespace Game.Services
{
    public class WinConditionChecker
    {
        private readonly GridService _gridService;
        
        public WinConditionChecker(GridService gridService)
        {
            _gridService = gridService;
        }
        
        public bool IsLevelComplete()
        {
            return _gridService.GetAllBoxPositions().All(pos => _gridService.IsGoal(pos));
        }
    }
}