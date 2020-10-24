using System;

namespace uClicker
{
    [Serializable]
    public class ManagerConfig
    {
        public Currency[] Currencies = new Currency[0];
        public Clickable[] Clickables = new Clickable[0];
        public Building[] AvailableBuildings = new Building[0];
        public Upgrade[] AvailableUpgrades = new Upgrade[0];
        public float BuildingCostIncrease = 0.15f;
    }
}