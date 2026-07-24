namespace Mana.Interfaces
{
    public interface IManaService
    {
        public void IncreaseMana(int mana, bool isOverMax = false);
        public void SetBonusMana(int mana);
        public bool IsEnoughMana(int mana);
        public int GetCurrentMana();
        public int GetMaxMana();
        public void RestoreAllMana();
        public void AddBonusMana(int mana);
        public void RestoreMana(int value);
        public void ResetMana(int round = 0);
        public void IncreaseMaxMana(int mana);
        public void SetMaximumMana(int mana);
        
    }
}
