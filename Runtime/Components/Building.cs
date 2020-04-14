using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Building")]
    public class Building : UnlockableComponent
    {
        public CurrencyTuple Cost;
        public CurrencyTuple YieldAmount;
    }

    [Serializable]
    public struct BuildingTuple
    {
        public Building Building;
        public int Amount;
    }
}