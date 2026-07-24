using Mana.Interfaces;
using UnityEngine;

namespace Mana
{
    public class ManaController : IManaService
    {
        private const int START_MANAPOOL = 3;

        private const int GROW_PER_ROUND = 1;

        private const int MIN_MANA = 0;
        private const int MAX_MANA = 10;

        private int _manapool = 1;

        private int _bonusMana;

        /// <summary>
        /// 
        /// </summary>
        private int _currentMana;
        
        // /// <summary>
        // /// 
        // /// </summary>
        // [SerializeField]
        // private List<Image> _manaPointsFill;              

        public bool IsEnoughMana(int mana)
        {
            return mana <= Mathf.Max(0, GetCurrentMana());
        }

        public void IncreaseMaxMana(int mana)
        {
            _manapool +=mana;
        } 

        public void SetMaximumMana(int mana)
        {
            _manapool = Mathf.Clamp(mana, MIN_MANA, MAX_MANA);
            _bonusMana = 0;
            _currentMana = Mathf.Min(_currentMana, _manapool);
        }
        
        public int IncreaseMaxManaCallBack(int mana)
        {
            int startManapool = _manapool + mana;
            _manapool = Mathf.Clamp(_manapool+mana,MIN_MANA,MAX_MANA);
            return startManapool - _manapool;
        }
        
        

        public void IncreaseMana(int mana, bool isOverMax = false)
        {
            if (_currentMana + mana > _manapool + _bonusMana)
            {
                int diff = _currentMana + mana - _manapool - _bonusMana;
                _currentMana += mana;
                if (isOverMax)
                {
                    AddBonusMana(diff);
                }
                else
                {
                    _currentMana -= diff;
                }
            }
            else
                _currentMana += mana;
        }

        public void RestoreAllMana()
        {
            _currentMana = _manapool + _bonusMana;
        }

        public void ResetMana(int round = 0)
        {
            _manapool = START_MANAPOOL + round * GROW_PER_ROUND;
            _bonusMana = 0;
        }


        public void SetBonusMana(int mana)
        {
            _bonusMana = mana;
        }

        public void AddBonusMana(int mana)
        {
            _bonusMana +=mana;
        }

        public void RestoreMana(int value)
        {
            _currentMana = Mathf.Min(_currentMana + value, _manapool + _bonusMana);
        }

        public int GetCurrentMana()
        {
         return Mathf.Clamp(_currentMana,MIN_MANA,GetMaxMana());    
        }
        
        public int GetMaxMana()
        {
         return Mathf.Clamp(_manapool + _bonusMana,MIN_MANA,MAX_MANA);    
        }
    }
}
