using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Upgrade")]
    public class Upgrade : UnlockableComponent
    {
        public Currency Currency;
        public int Cost;
        public Requirement[] Requirements;
        public UpgradePerk[] UpgradePerk;
    }

    [Serializable]
    public class Requirement
    {
        public float UnlockAmount;
        public Building UnlockBuilding;
        public Upgrade UnlockUpgrade;
    }

    [Serializable]
    public class UpgradePerk
    {
        public Building TargetBuilding;
        public Currency TargetCurrency;
        public Clickable TargetClickable;
        public Operation Operation;
        public float Amount;
    }

    public enum Operation
    {
        Add,
        Multiply
    }
}