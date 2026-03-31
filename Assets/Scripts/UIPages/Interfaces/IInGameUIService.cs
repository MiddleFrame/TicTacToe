using System;
using System.Collections;

namespace UIPages.Interfaces
{
    public interface IInGameUIService
    {
        public void UpdateScore(int score1Player, int score2Player);
        public void SetupMaxScore(int score1Player, int score2Player);
        public void UpdatePlayerRP(int score1Player, int score2Player);
        public IEnumerator ShowRoundOverAnimation();
        public IEnumerator IShowNewTurnAnimation(CellFigure cellFigure);
        public void NewTurn();
        public bool GetIsGameOverShowed();
        public void StateGameOverPanel(bool state, int value = 0);
        public void StopTimer();
        public void SetSideBannerTurn(int side);

        public void SetRestartGameAction(Action action);
        public void SetReturnHomeAction(Action action);
        public void SetEndTurnAction(Action action);

        public void SetIsOnlineGame(bool state);


    }
}