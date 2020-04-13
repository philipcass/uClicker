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
    }
}