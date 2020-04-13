using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Upgrade")]
    public class Upgrade : UnlockableComponent
    {
        public CurrencyTuple Cost;
        public UpgradePerk[] UpgradePerk;
    }

    [Serializable]
    public class UpgradePerk
    {
        public Building TargetBuilding;
        public Clickable TargetClickable;
        public Currency TargetCurrency;
        public Operation Operation;
        public float Amount;
    }

    public enum Operation
    {
        Add,
        Multiply
    }
}