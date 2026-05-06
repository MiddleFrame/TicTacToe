using System;

namespace FinishLine.Interfaces
{
    public interface IFinishLineService
    {
        public void MasterChecker(int figure, bool isInQueue = true, bool isNeedEvent = true);

        public void SetNetworkEventAction(Action action);
        public void SetNewGameState(Action<GameplayState> action);
        public void SetPredicateIsEqualGameState(Predicate<GameplayState> action);
    
    }
}