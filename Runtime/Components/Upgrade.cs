using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Upgrade")]
    public class Upgrade : UnlockableComponent
    {
        public CurrencyTuple Cost;
        public Requirement[] Requirements;
        public UpgradePerk[] UpgradePerk;
    }

    [Serializable]
    public class Requirement
    {
        public CurrencyTuple UnlockAmount;
        public Building UnlockBuilding;
        public Upgrade UnlockUpgrade;
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