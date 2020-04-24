using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Currency")]
    public class Currency : ClickerComponent
    {
    }

    [Serializable]
    public struct CurrencyTuple
    {
        public Currency Currency;
        public float Amount;

        public override string ToString()
        {
            return string.Format("{0} {1}s", Amount, Currency.name);
        }
    }
}