using UnityEngine;

namespace NullBorn.RPG.Economy
{
    /// <summary>
    /// Créditos de CPU — moneda del Runtime Market.
    /// </summary>
    public class CpuCredits : MonoBehaviour
    {
        public static CpuCredits Instance { get; private set; }

        [SerializeField] private int amount = 40;

        public int Amount => amount;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool TrySpend(int cost)
        {
            if (cost <= 0) return true;
            if (amount < cost) return false;
            amount -= cost;
            return true;
        }

        public void Add(int value)
        {
            amount = Mathf.Max(0, amount + value);
        }

        public void SetAmount(int value)
        {
            amount = Mathf.Max(0, value);
        }
    }
}
