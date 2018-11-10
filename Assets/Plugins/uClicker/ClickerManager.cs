using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace uClicker
{
    [Serializable]
    public class BuildingContainer
    {
        public Building Building;
        public int Count;
    }

    [CreateAssetMenu(menuName = "uClicker/Manager")]
    public class ClickerManager : ScriptableObject
    {
        public Currency Currency;
        public Clickable Clickable;
        public Building[] AvailableBuildings;
        public Upgrade[] AvailableUpgrades;

        public BuildingContainer[] EarnedBuildings;
        public Upgrade[] EarnedUpgrades;

        public UnityEvent OnTick;
        public UnityEvent OnBuyUpgrade;
        public UnityEvent OnBuyBuilding;

        public float TotalAmount;

        public void Click()
        {
            float amount = Clickable.Amount;

            ApplyClickPerks(ref amount);
            ApplyCurrencyPerk(ref amount);

            bool updated = UpdateTotal(amount);
            UpdateUnlocks();
            if (updated)
            {
                OnTick.Invoke();
            }
        }

        private bool UpdateTotal(float amount)
        {
            TotalAmount += amount;
            return amount != 0;
        }

        public void Tick()
        {
            var amount = PerSecondAmount();

            bool updated = UpdateTotal(amount);
            UpdateUnlocks();
            if (updated)
            {
                OnTick.Invoke();
            }
        }

        public float PerSecondAmount()
        {
            float amount = 0;

            ApplyBuildingPerks(ref amount);
            ApplyCurrencyPerk(ref amount);
            return amount;
        }

        public void BuyBuilding(string id)
        {
            Building building = GetBuildingByName(id);

            if (building == null)
            {
                return;
            }

            if (!CanBuild(building))
            {
                return;
            }

            BuildingContainer buildingContainer = GetBuildingContainer(building);

            float cost = buildingContainer == null ? building.Cost : BuildingCost(building);

            if (!Deduct(cost))
            {
                return;
            }

            if (buildingContainer != null)
            {
                buildingContainer.Count++;
            }
            else
            {
                Array.Resize(ref EarnedBuildings, EarnedBuildings.Length + 1);
                EarnedBuildings[EarnedBuildings.Length - 1] = new BuildingContainer
                {
                    Building = building,
                    Count = 1
                };
            }

            UpdateUnlocks();
            OnBuyBuilding.Invoke();
        }

        private Building GetBuildingByName(string id)
        {
            Building building = null;

            foreach (Building availableBuilding in AvailableBuildings)
            {
                if (availableBuilding.Name == id)
                {
                    building = availableBuilding;
                }
            }

            return building;
        }

        public BuildingContainer GetBuildingContainer(Building building)
        {
            BuildingContainer buildingContainer = null;
            foreach (BuildingContainer container in EarnedBuildings)
            {
                if (container.Building == building)
                {
                    buildingContainer = container;
                    break;
                }
            }

            return buildingContainer;
        }

        public void BuyUpgrade(string id)
        {
            Upgrade upgrade = null;
            foreach (Upgrade availableUpgrade in AvailableUpgrades)
            {
                if (availableUpgrade.name == id)
                {
                    upgrade = availableUpgrade;
                    break;
                }
            }

            if (upgrade == null)
            {
                return;
            }

            if (!CanUpgrade(upgrade))
            {
                return;
            }

            if (!Deduct(upgrade.Cost))
            {
                return;
            }

            Array.Resize(ref EarnedUpgrades, EarnedUpgrades.Length + 1);
            EarnedUpgrades[EarnedUpgrades.Length - 1] = upgrade;
            UpdateUnlocks();
            OnBuyUpgrade.Invoke();
        }

        private void UpdateUnlocks()
        {
            foreach (var availableBuilding in AvailableBuildings)
            {
                availableBuilding.Unlocked |= CanBuild(availableBuilding);
            }

            foreach (var availableUpgrade in AvailableUpgrades)
            {
                availableUpgrade.Unlocked |= CanUpgrade(availableUpgrade);
            }
        }

        private bool CanBuild(Building building)
        {
            return IsUnlocked(building.Requirements);
        }

        private bool CanUpgrade(Upgrade upgrade)
        {
            bool unlocked = true;
            unlocked &= Array.IndexOf(EarnedUpgrades, upgrade) == -1;
            unlocked &= IsUnlocked(upgrade.Requirements);

            return unlocked;
        }

        private bool IsUnlocked(Requirement[] requirements)
        {
            bool unlocked = true;
            foreach (Requirement requirement in requirements)
            {
                unlocked &= requirement.UnlockUpgrade == null ||
                            Array.IndexOf(EarnedUpgrades, requirement.UnlockUpgrade) != -1;
                unlocked &= requirement.UnlockBuilding == null ||
                            Array.IndexOf(EarnedBuildings, requirement.UnlockBuilding) != -1;
                unlocked &= TotalAmount >= requirement.UnlockAmount;
            }

            return unlocked;
        }

        private bool Deduct(float cost)
        {
            if (TotalAmount < cost)
            {
                return false;
            }

            bool updated = UpdateTotal(-cost);
            if (updated)
            {
                OnTick.Invoke();
            }

            return true;
        }

        private void ApplyClickPerks(ref float amount)
        {
            foreach (Upgrade upgrade in EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetClickable == Clickable)
                    {
                        if (upgradePerk.Operation == Operation.Add)
                        {
                            amount += upgradePerk.Amount;
                        }
                        else
                        {
                            amount *= upgradePerk.Amount;
                        }
                    }
                }
            }
        }

        private void ApplyBuildingPerks(ref float amount)
        {
            foreach (BuildingContainer building in EarnedBuildings)
            {
                amount += building.Building.Amount * building.Count;

                foreach (Upgrade upgrade in EarnedUpgrades)
                {
                    foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                    {
                        if (upgradePerk.TargetBuilding == building.Building)
                        {
                            if (upgradePerk.Operation == Operation.Add)
                            {
                                amount += upgradePerk.Amount;
                            }
                            else
                            {
                                amount *= upgradePerk.Amount;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyCurrencyPerk(ref float amount)
        {
            foreach (Upgrade upgrade in EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetCurrency == Currency)
                    {
                        if (upgradePerk.Operation == Operation.Add)
                        {
                            amount += upgradePerk.Amount;
                        }
                        else
                        {
                            amount *= upgradePerk.Amount;
                        }
                    }
                }
            }
        }

        public int BuildingCost(Building building)
        {
            BuildingContainer buildingContainer = GetBuildingContainer(building);
            return (int) (building.Cost * Mathf.Pow(1 + Currency.PercentIncr,
                              buildingContainer == null ? 0 : buildingContainer.Count));
        }
    }
}