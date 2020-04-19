using System;
using System.Collections.Generic;
using UnityEngine;

namespace uClicker
{
    [Serializable]
    public class ManagerState : ISerializationCallbackReceiver
    {
        [NonSerialized] public Dictionary<Building, int> EarnedBuildings = new Dictionary<Building, int>();
        [NonSerialized] public List<Upgrade> EarnedUpgrades = new List<Upgrade>();
        [NonSerialized] public Dictionary<Currency, float> CurrencyCurrentTotals = new Dictionary<Currency, float>();
        [NonSerialized] public Dictionary<Currency, float> CurrencyHistoricalTotals = new Dictionary<Currency, float>();

        [SerializeField] private List<GUIDContainer> _earnedBuildings = new List<GUIDContainer>();
        [SerializeField] private List<int> _earnedBuildingsCount = new List<int>();
        [SerializeField] private List<GUIDContainer> _earnedUpgrades = new List<GUIDContainer>();
        [SerializeField] private List<GUIDContainer> _currencies = new List<GUIDContainer>();
        [SerializeField] private List<float> _currencyCurrentTotals = new List<float>();
        [SerializeField] private List<float> _currencyHistoricalTotals = new List<float>();

        public void OnBeforeSerialize()
        {
            _earnedBuildings.Clear();
            _earnedBuildingsCount.Clear();
            foreach (KeyValuePair<Building, int> kvp in EarnedBuildings)
            {
                _earnedBuildings.Add(kvp.Key.GUIDContainer);
                _earnedBuildingsCount.Add(kvp.Value);
            }

            _currencies.Clear();
            _currencyCurrentTotals.Clear();
            _currencyHistoricalTotals.Clear();
            foreach (KeyValuePair<Currency, float> kvp in CurrencyCurrentTotals)
            {
                _currencies.Add(kvp.Key.GUIDContainer);
                _currencyCurrentTotals.Add(kvp.Value);
                float historicalTotal;
                CurrencyHistoricalTotals.TryGetValue(kvp.Key, out historicalTotal);
                _currencyHistoricalTotals.Add(historicalTotal);
            }

            _earnedUpgrades = EarnedUpgrades.ConvertAll(input => input.GUIDContainer);
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < _earnedBuildings.Count; i++)
            {
                EarnedBuildings[(Building) ClickerComponent.RuntimeLookup[_earnedBuildings[i].Guid]] =
                    _earnedBuildingsCount[i];
            }

            for (int i = 0; i < _currencies.Count; i++)
            {
                CurrencyCurrentTotals[(Currency) ClickerComponent.RuntimeLookup[_currencies[i].Guid]] =
                    _currencyCurrentTotals[i];
                CurrencyHistoricalTotals[(Currency) ClickerComponent.RuntimeLookup[_currencies[i].Guid]] =
                    _currencyHistoricalTotals[i];
            }

            EarnedUpgrades = _earnedUpgrades.ConvertAll(input => (Upgrade) ClickerComponent.RuntimeLookup[input.Guid]);
        }
    }
}