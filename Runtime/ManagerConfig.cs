using System;

namespace uClicker
{
    [Serializable]
    public class ManagerConfig
    {
        public Currency[] Currencies;
        public Clickable[] Clickables;
        public Building[] AvailableBuildings;
        public Upgrade[] AvailableUpgrades;
        public float BuildingCostIncrease = 0.15f;
    }
}