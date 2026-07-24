namespace Score.Interfaces
{
    public interface IScoreService
    {
        public int GetScore(int player);
        public int GetMaxScore(int player);
        public void SetScore(int player, int value);
        public void AddScore(int player, int value);
        public void ClearAllScore();
        public void AddPlayer(int player);
        public void RemovePlayer(int player);
        public void SetMaxScore(int player, int value);
    }
}
