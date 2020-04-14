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
        [Serializable]
        public class ManagerConfig
        {
            public Currency[] Currencies;
            public Clickable[] Clickables;
            public Building[] AvailableBuildings;
            public Upgrade[] AvailableUpgrades;
            public float BuildingCostIncrease = 0.15f;
        }

        [Serializable]
        public class ManagerState : ISerializationCallbackReceiver
        {
            [NonSerialized] public Dictionary<Building, int> EarnedBuildings = new Dictionary<Building, int>();
            [NonSerialized] public Upgrade[] EarnedUpgrades = new Upgrade[0];

            [NonSerialized]
            public Dictionary<Currency, float> CurrencyCurrentTotals = new Dictionary<Currency, float>();

            [NonSerialized]
            public Dictionary<Currency, float> CurrencyHistoricalTotals = new Dictionary<Currency, float>();

            [SerializeField] private GUIDContainer[] _earnedBuildings;
            [SerializeField] private int[] _earnedBuildingsCount = new int[0];
            [SerializeField] private GUIDContainer[] _earnedUpgrades;
            [SerializeField] private GUIDContainer[] _currencies;
            [SerializeField] private float[] _currencyCurrentTotals = new float[0];
            [SerializeField] private float[] _currencyHistoricalTotals = new float[0];

            public void OnBeforeSerialize()
            {
                Array.Resize(ref _earnedBuildings, EarnedBuildings.Count);
                Array.Resize(ref _earnedBuildingsCount, EarnedBuildings.Count);
                int index = 0;
                foreach (KeyValuePair<Building, int> kvp in EarnedBuildings)
                {
                    _earnedBuildings[index] = kvp.Key.GUIDContainer;
                    _earnedBuildingsCount[index] = kvp.Value;
                    index++;
                }

                Array.Resize(ref _currencies, CurrencyCurrentTotals.Count);
                Array.Resize(ref _currencyCurrentTotals, CurrencyCurrentTotals.Count);
                Array.Resize(ref _currencyHistoricalTotals, CurrencyCurrentTotals.Count);
                index = 0;
                foreach (KeyValuePair<Currency, float> kvp in CurrencyCurrentTotals)
                {
                    _currencies[index] = kvp.Key.GUIDContainer;
                    _currencyCurrentTotals[index] = kvp.Value;
                    float historicalTotal;
                    CurrencyHistoricalTotals.TryGetValue(kvp.Key, out historicalTotal);
                    _currencyHistoricalTotals[index] = historicalTotal;
                    index++;
                }

                _earnedUpgrades = Array.ConvertAll(EarnedUpgrades, input => input.GUIDContainer);
            }

            public void OnAfterDeserialize()
            {
                for (int i = 0; i < _earnedBuildings.Length; i++)
                {
                    EarnedBuildings[(Building) RuntimeLookup[_earnedBuildings[i].Guid]] = _earnedBuildingsCount[i];
                }

                for (int i = 0; i < _currencies.Length; i++)
                {
                    CurrencyCurrentTotals[(Currency) RuntimeLookup[_currencies[i].Guid]] = _currencyCurrentTotals[i];
                    CurrencyHistoricalTotals[(Currency) RuntimeLookup[_currencies[i].Guid]] =
                        _currencyHistoricalTotals[i];
                }

                EarnedUpgrades = Array.ConvertAll(_earnedUpgrades, input => (Upgrade) RuntimeLookup[input.Guid]);
            }
        }

        public SaveSettings SaveSettings = new SaveSettings();
        public ManagerConfig Config;
        public ManagerState State;

        public UnityEvent OnTick;
        public UnityEvent OnBuyUpgrade;
        public UnityEvent OnBuyBuilding;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL &&
                SaveSettings.SaveType == SaveSettings.SaveTypeEnum.SaveToFile)
            {
                Debug.LogWarning("Cannot save to file on WebGL, changing to SaveToPlayerPrefs");
                SaveSettings.SaveType = SaveSettings.SaveTypeEnum.SaveToPlayerPrefs;
            }
        }
#endif

        private void OnDisable()
        {
            // Clear save on unload so we don't try deserializing the save between play/stop
            State = new ManagerState();
        }

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

            bool containsKey = State.EarnedBuildings.ContainsKey(building);
            float cost = !containsKey ? building.Cost.Amount : BuildingCost(building);

            if (!Deduct(building.Cost.Currency, cost))
            {
                return;
            }

            if (containsKey)
            {
                State.EarnedBuildings[building]++;
            }
            else
            {
                State.EarnedBuildings[building] = 1;
            }

            UpdateUnlocks();
            OnBuyBuilding.Invoke();
        }

        public void BuyUpgrade(string id)
        {
            Upgrade upgrade = null;
            foreach (Upgrade availableUpgrade in Config.AvailableUpgrades)
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

            if (!Deduct(upgrade.Cost.Currency, upgrade.Cost.Amount))
            {
                return;
            }

            Array.Resize(ref State.EarnedUpgrades, State.EarnedUpgrades.Length + 1);
            State.EarnedUpgrades[State.EarnedUpgrades.Length - 1] = upgrade;
            UpdateUnlocks();
            OnBuyUpgrade.Invoke();
        }

        public int BuildingCost(Building building)
        {
            int count;
            if (!State.EarnedBuildings.TryGetValue(building, out count))
            {
                count = 0;
            }

            return (int) (building.Cost.Amount * Mathf.Pow(1 + Config.BuildingCostIncrease, count));
        }

        public void SaveProgress()
        {
            string value = JsonUtility.ToJson(State, true);
            switch (SaveSettings.SaveType)
            {
                case SaveSettings.SaveTypeEnum.SaveToPlayerPrefs:
                    PlayerPrefs.SetString(SaveSettings.SaveName, value);
                    break;
                case SaveSettings.SaveTypeEnum.SaveToFile:
                    File.WriteAllText(SaveSettings.FullSavePath, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void LoadProgress()
        {
            string json;
            switch (SaveSettings.SaveType)
            {
                case SaveSettings.SaveTypeEnum.SaveToPlayerPrefs:
                    json = PlayerPrefs.GetString(SaveSettings.SaveName);
                    break;
                case SaveSettings.SaveTypeEnum.SaveToFile:
                    if (!File.Exists(SaveSettings.FullSavePath))
                    {
                        return;
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

        private Building GetBuildingByName(string id)
        {
            Building building = null;

            foreach (Building availableBuilding in Config.AvailableBuildings)
            {
                if (availableBuilding.name == id)
                {
                    building = availableBuilding;
                }
            }

            return building;
        }

        private void UpdateUnlocks()
        {
            foreach (Building availableBuilding in Config.AvailableBuildings)
            {
                availableBuilding.Unlocked |= CanBuild(availableBuilding);
            }

            foreach (Upgrade availableUpgrade in Config.AvailableUpgrades)
            {
                availableUpgrade.Unlocked |= CanUpgrade(availableUpgrade);
            }
        }

        private bool CanBuild(Building building)
        {
            return IsUnlocked(building.RequirementGroups);
        }

        private bool CanUpgrade(Upgrade upgrade)
        {
            bool unlocked = true;
            unlocked &= Array.IndexOf(State.EarnedUpgrades, upgrade) == -1;
            unlocked &= IsUnlocked(upgrade.RequirementGroups);

            return unlocked;
        }

        private bool IsUnlocked(RequirementGroup[] requirementGroups)
        {
            bool compareStarted = false;
            // if empty it's unlocked
            bool groupsUnlocked = requirementGroups.Length == 0;
            foreach (var requirementGroup in requirementGroups)
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
                                        Array.IndexOf(State.EarnedUpgrades, requirement.UnlockUpgrade) != -1;
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