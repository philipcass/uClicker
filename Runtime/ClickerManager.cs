using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace uClicker
{
    /// <summary>
    /// The "Game.cs" for this library. Deals with game logic, saving, loading... you name it - it does it!
    /// Also responsible for triggering the GUIDContainer system - anything referenced in the ManagerConfig will be loaded by this object
    /// This will populate the runtime GUIDContainer DB and make saving and loading work - so if you use the runtime DB, make sure this is loaded before you use it! 
    /// </summary>
    [CreateAssetMenu(menuName = "uClicker/Manager")]
    public class ClickerManager : ClickerComponent
    {
        public ManagerSaveSettings SaveSettings = new ManagerSaveSettings();
        public ManagerConfig Config = new ManagerConfig();
        public ManagerState State = new ManagerState();

        public UnityEvent OnTick = new UnityEvent();
        public UnityEvent OnBuyUpgrade = new UnityEvent();
        public UnityEvent OnBuyBuilding = new UnityEvent();

        #region Unity Events

#if UNITY_EDITOR
        /// <summary>
        /// Validates platform settings and switches to platform compatible settings automatically
        /// </summary>
        private void OnValidate()
        {
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL &&
                SaveSettings.SaveType == ManagerSaveSettings.SaveTypeEnum.SaveToFile)
            {
                Debug.LogWarning("Cannot save to file on WebGL, changing to SaveToPlayerPrefs");
                SaveSettings.SaveType = ManagerSaveSettings.SaveTypeEnum.SaveToPlayerPrefs;
            }
        }
#endif

        /// <summary>
        /// Inits Currency totals for manager on startup
        /// </summary>
        public void OnEnable()
        {
            foreach (var configClickable in Config.Currencies)
            {
                UpdateTotal(configClickable, 0);
            }
        }

        /// <summary>
        /// Clear save on unload so we don't try deserializing the save between play/stop
        /// </summary>
        private void OnDisable()
        {
            State = new ManagerState();
        }

        #endregion

        #region Public Game Logic

        public void Click(Clickable clickable)
        {
            float amount = clickable.Amount;
            Currency currency = clickable.Currency;

            ApplyClickPerks(clickable, ref amount);
            ApplyCurrencyPerk(currency, ref amount);

            bool updated = UpdateTotal(currency, amount);
            UpdateUnlocks();
            if (updated)
            {
                OnTick.Invoke();
            }
        }

        public void Tick()
        {
            bool updated = false;
            foreach (Currency currency in Config.Currencies)
            {
                float amount = PerSecondAmount(currency);

                updated = UpdateTotal(currency, amount);
            }

            UpdateUnlocks();
            if (updated)
            {
                OnTick.Invoke();
            }
        }

        public void BuyBuilding(Building building)
        {
            if (building == null)
            {
                return;
            }

            if (!BuildingAvailable(building))
            {
                return;
            }

            CurrencyTuple cost = BuildingCost(building);

            if (!Deduct(cost.Currency, cost.Amount))
            {
                return;
            }

            int buildingCount;
            State.EarnedBuildings.TryGetValue(building, out buildingCount);
            State.EarnedBuildings[building] = buildingCount + 1;

            UpdateUnlocks();
            OnBuyBuilding.Invoke();
        }

        public void BuyUpgrade(Upgrade upgrade)
        {
            if (upgrade == null)
            {
                return;
            }

            if (!UpgradeAvailable(upgrade))
            {
                return;
            }

            if (!Deduct(upgrade.Cost.Currency, upgrade.Cost.Amount))
            {
                return;
            }

            State.EarnedUpgrades.Add(upgrade);
            UpdateUnlocks();
            OnBuyUpgrade.Invoke();
        }

        public bool CanBuy(ClickerComponent component)
        {
            CurrencyTuple cost;
            if (component is Building)
            {
                Building building = component as Building;
                if (!BuildingAvailable(building))
                {
                    return false;
                }

                cost = BuildingCost(building);
            }
            else if (component is Upgrade)
            {
                Upgrade upgrade = (component as Upgrade);
                if (!UpgradeAvailable(upgrade))
                {
                    return false;
                }

                cost = upgrade.Cost;
            }
            else
            {
                return true;
            }

            float amount;
            State.CurrencyCurrentTotals.TryGetValue(cost.Currency, out amount);
            return amount >= cost.Amount;
        }

        public CurrencyTuple BuildingCost(Building building)
        {
            int count;
            if (!State.EarnedBuildings.TryGetValue(building, out count))
            {
                count = 0;
            }

            CurrencyTuple currencyTuple = building.Cost;
            currencyTuple.Amount = (int) currencyTuple.Amount * Mathf.Pow(1 + Config.BuildingCostIncrease, count);
            return currencyTuple;
        }

        /// <summary>
        /// Serializes state and saves to target <see cref="ManagerSaveSettings.SaveTypeEnum"/> in <see cref="SaveSettings"/>
        /// </summary>
        public void SaveProgress()
        {
            string value = JsonUtility.ToJson(State, true);
            switch (SaveSettings.SaveType)
            {
                case ManagerSaveSettings.SaveTypeEnum.SaveToPlayerPrefs:
                    PlayerPrefs.SetString(SaveSettings.SaveName, value);
                    break;
                case ManagerSaveSettings.SaveTypeEnum.SaveToFile:
                    File.WriteAllText(SaveSettings.FullSavePath, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Loads from target <see cref="ManagerSaveSettings.SaveTypeEnum"/> in <see cref="SaveSettings"/>
        /// </summary>
        /// <exception cref="ArgumentException">When save does not exist</exception>
        public void LoadProgress()
        {
            string json;
            switch (SaveSettings.SaveType)
            {
                case ManagerSaveSettings.SaveTypeEnum.SaveToPlayerPrefs:
                    if (!PlayerPrefs.HasKey(SaveSettings.SaveName))
                    {
                        throw new ArgumentException($"Save '{SaveSettings.SaveName}' is null");
                    }

                    json = PlayerPrefs.GetString(SaveSettings.SaveName);
                    break;
                case ManagerSaveSettings.SaveTypeEnum.SaveToFile:
                    if (!File.Exists(SaveSettings.FullSavePath))
                    {
                        throw new ArgumentException($"Save '{SaveSettings.FullSavePath}' is null");
                    }

#if DEBUG
                    Debug.LogFormat("Loading Save from file: {0}", SaveSettings.FullSavePath);
#endif

                    json = File.ReadAllText(SaveSettings.FullSavePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            JsonUtility.FromJsonOverwrite(json, State);
            UpdateUnlocks();
            OnTick.Invoke();
            OnBuyBuilding.Invoke();
            OnBuyUpgrade.Invoke();
        }

        /// <summary>
        /// Clear progress from target <see cref="ManagerSaveSettings.SaveTypeEnum"/> in <see cref="SaveSettings"/>
        /// </summary>
        public void ClearProgress()
        {
            switch (SaveSettings.SaveType)
            {
                case ManagerSaveSettings.SaveTypeEnum.SaveToPlayerPrefs:
                    PlayerPrefs.DeleteKey(SaveSettings.SaveName);
                    break;
                case ManagerSaveSettings.SaveTypeEnum.SaveToFile:
                    File.Delete(SaveSettings.FullSavePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Internal Logic

        private bool Deduct(Currency costCurrency, float cost)
        {
            if (State.CurrencyCurrentTotals[costCurrency] < cost)
            {
                return false;
            }

            bool updated = UpdateTotal(costCurrency, -cost);
            if (updated)
            {
                OnTick.Invoke();
            }

            return true;
        }

        private bool UpdateTotal(Currency currency, float amount)
        {
            float total;
            State.CurrencyCurrentTotals.TryGetValue(currency, out total);
            total += amount;
            State.CurrencyCurrentTotals[currency] = total;

            if (amount > 0)
            {
                float historicalTotal;
                State.CurrencyHistoricalTotals.TryGetValue(currency, out historicalTotal);
                State.CurrencyHistoricalTotals[currency] = historicalTotal + amount;
            }

            return total != 0;
        }

        private float PerSecondAmount(Currency currency)
        {
            float amount = 0;

            ApplyBuildingPerks(currency, ref amount);
            ApplyCurrencyPerk(currency, ref amount);
            return amount;
        }

        private void UpdateUnlocks()
        {
            foreach (Building availableBuilding in Config.AvailableBuildings)
            {
                availableBuilding.Unlocked |= BuildingAvailable(availableBuilding);
            }

            foreach (Upgrade availableUpgrade in Config.AvailableUpgrades)
            {
                availableUpgrade.Unlocked |= UpgradeAvailable(availableUpgrade);
            }
        }

        private bool BuildingAvailable(Building building)
        {
            return IsUnlocked(building.RequirementGroups);
        }

        private bool UpgradeAvailable(Upgrade upgrade)
        {
            bool unlocked = true;
            unlocked &= !State.EarnedUpgrades.Contains(upgrade);
            unlocked &= IsUnlocked(upgrade.RequirementGroups);

            return unlocked;
        }

        private bool IsUnlocked(RequirementGroup[] requirementGroups)
        {
            bool compareStarted = false;
            // if empty it's unlocked
            bool groupsUnlocked = requirementGroups.Length == 0;
            foreach (RequirementGroup requirementGroup in requirementGroups)
            {
                if (!compareStarted)
                {
                    compareStarted = true;
                    groupsUnlocked = requirementGroup.GroupOperand == RequirementOperand.And;
                }

                bool unlocked = true;
                foreach (Requirement requirement in requirementGroup.Requirements)
                {
                    switch (requirement.RequirementType)
                    {
                        case RequirementType.Currency:
                            unlocked &= requirement.UnlockAmount.Currency == null ||
                                        (State.CurrencyHistoricalTotals.ContainsKey(requirement.UnlockAmount
                                             .Currency) &&
                                         State.CurrencyHistoricalTotals[requirement.UnlockAmount.Currency] >=
                                         requirement.UnlockAmount.Amount);
                            break;
                        case RequirementType.Building:
                            unlocked &= requirement.UnlockBuilding.Building == null ||
                                        State.EarnedBuildings.ContainsKey(requirement.UnlockBuilding.Building) &&
                                        State.EarnedBuildings[requirement.UnlockBuilding.Building] >=
                                        requirement.UnlockBuilding.Amount;
                            break;
                        case RequirementType.Upgrade:
                            unlocked &= requirement.UnlockUpgrade == null ||
                                        State.EarnedUpgrades.Contains(requirement.UnlockUpgrade);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (requirementGroup.GroupOperand == RequirementOperand.And)
                {
                    groupsUnlocked &= unlocked;
                }
                else
                {
                    groupsUnlocked |= unlocked;
                }
            }

            return groupsUnlocked;
        }

        private void ApplyClickPerks(Clickable clickable, ref float amount)
        {
            foreach (Upgrade upgrade in State.EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetClickable != clickable)
                    {
                        continue;
                    }

                    switch (upgradePerk.Operation)
                    {
                        case Operation.Add:
                            amount += upgradePerk.Amount;
                            break;
                        case Operation.Multiply:
                            amount *= upgradePerk.Amount;
                            break;
                    }
                }
            }
        }

        private void ApplyBuildingPerks(Currency currency, ref float amount)
        {
            foreach (KeyValuePair<Building, int> kvp in State.EarnedBuildings)
            {
                Building building = kvp.Key;
                if (building.YieldAmount.Currency != currency)
                {
                    continue;
                }

                int buildingCount = kvp.Value;
                amount += building.YieldAmount.Amount * buildingCount;

                foreach (Upgrade upgrade in State.EarnedUpgrades)
                {
                    foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                    {
                        if (upgradePerk.TargetBuilding != building)
                        {
                            continue;
                        }

                        switch (upgradePerk.Operation)
                        {
                            case Operation.Add:
                                amount += upgradePerk.Amount;
                                break;
                            case Operation.Multiply:
                                amount *= upgradePerk.Amount;
                                break;
                        }
                    }
                }
            }
        }

        private void ApplyCurrencyPerk(Currency currency, ref float amount)
        {
            foreach (Upgrade upgrade in State.EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetCurrency != currency)
                    {
                        continue;
                    }

                    switch (upgradePerk.Operation)
                    {
                        case Operation.Add:
                            amount += upgradePerk.Amount;
                            break;
                        case Operation.Multiply:
                            amount *= upgradePerk.Amount;
                            break;
                    }
                }
            }
        }

        #endregion
    }
}