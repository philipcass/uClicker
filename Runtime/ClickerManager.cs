using System;
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
            public Currency Currency;
            public Clickable Clickable;
            public Building[] AvailableBuildings;
            public Upgrade[] AvailableUpgrades;
        }

        [Serializable]
        public class ManagerState : ISerializationCallbackReceiver
        {
            [NonSerialized] public Building[] EarnedBuildings = new Building[0];
            public int[] EarnedBuildingsCount = new int[0];
            [NonSerialized] public Upgrade[] EarnedUpgrades = new Upgrade[0];
            public float TotalAmount;
            [SerializeField] private GUIDContainer[] _earnedBuildings;
            [SerializeField] private GUIDContainer[] _earnedUpgrades;

            public void OnBeforeSerialize()
            {
                _earnedBuildings = Array.ConvertAll(EarnedBuildings, input => input.GUIDContainer);
                _earnedUpgrades = Array.ConvertAll(EarnedUpgrades, input => input.GUIDContainer);
            }

            public void OnAfterDeserialize()
            {
                EarnedBuildings = Array.ConvertAll(_earnedBuildings, input => (Building) RuntimeLookup[input.Guid]);
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
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL && SaveSettings.SaveType == SaveSettings.SaveTypeEnum.SaveToFile)
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

        public void Click()
        {
            float amount = Config.Clickable.Amount;

            ApplyClickPerks(ref amount);
            ApplyCurrencyPerk(ref amount);

            bool updated = UpdateTotal(amount);
            UpdateUnlocks();
            if (updated)
            {
                OnTick.Invoke();
            }
        }

        public void Tick()
        {
            float amount = PerSecondAmount();

            bool updated = UpdateTotal(amount);
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

            int indexOf = Array.IndexOf(State.EarnedBuildings, building);

            float cost = indexOf == -1 ? building.Cost : BuildingCost(building);

            if (!Deduct(cost))
            {
                return;
            }

            if (indexOf >= 0)
            {
                State.EarnedBuildingsCount[indexOf]++;
            }
            else
            {
                Array.Resize(ref State.EarnedBuildings, State.EarnedBuildings.Length + 1);
                Array.Resize(ref State.EarnedBuildingsCount, State.EarnedBuildingsCount.Length + 1);
                State.EarnedBuildings[State.EarnedBuildings.Length - 1] = building;
                State.EarnedBuildingsCount[State.EarnedBuildingsCount.Length - 1] = 1;
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

            if (!Deduct(upgrade.Cost))
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
            int indexOf = Array.IndexOf(State.EarnedBuildings, building);

            return (int) (building.Cost * Mathf.Pow(1 + Config.Currency.PercentIncr,
                indexOf == -1 ? 0 : State.EarnedBuildingsCount[indexOf]));
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

        private bool Deduct(float cost)
        {
            if (State.TotalAmount < cost)
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

        private bool UpdateTotal(float amount)
        {
            State.TotalAmount += amount;
            return amount != 0;
        }

        private float PerSecondAmount()
        {
            float amount = 0;

            ApplyBuildingPerks(ref amount);
            ApplyCurrencyPerk(ref amount);
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
            return IsUnlocked(building.Requirements);
        }

        private bool CanUpgrade(Upgrade upgrade)
        {
            bool unlocked = true;
            unlocked &= Array.IndexOf(State.EarnedUpgrades, upgrade) == -1;
            unlocked &= IsUnlocked(upgrade.Requirements);

            return unlocked;
        }

        private bool IsUnlocked(Requirement[] requirements)
        {
            bool unlocked = true;
            foreach (Requirement requirement in requirements)
            {
                unlocked &= requirement.UnlockUpgrade == null ||
                            Array.IndexOf(State.EarnedUpgrades, requirement.UnlockUpgrade) != -1;
                unlocked &= requirement.UnlockBuilding == null ||
                            Array.IndexOf(State.EarnedBuildings, requirement.UnlockBuilding) != -1;
                unlocked &= State.TotalAmount >= requirement.UnlockAmount;
            }

            return unlocked;
        }

        private void ApplyClickPerks(ref float amount)
        {
            foreach (Upgrade upgrade in State.EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetClickable == Config.Clickable)
                    {
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

        private void ApplyBuildingPerks(ref float amount)
        {
            for (int i = 0; i < State.EarnedBuildings.Length; i++)
            {
                Building building = State.EarnedBuildings[i];
                int buildingCount = State.EarnedBuildingsCount[i];
                amount += building.Amount * buildingCount;

                foreach (Upgrade upgrade in State.EarnedUpgrades)
                {
                    foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                    {
                        if (upgradePerk.TargetBuilding == building)
                        {
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
        }

        private void ApplyCurrencyPerk(ref float amount)
        {
            foreach (Upgrade upgrade in State.EarnedUpgrades)
            {
                foreach (UpgradePerk upgradePerk in upgrade.UpgradePerk)
                {
                    if (upgradePerk.TargetCurrency == Config.Currency)
                    {
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

        #endregion
    }
}