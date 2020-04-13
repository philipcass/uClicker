using System;
using System.Collections.Generic;
using uClicker;
using UnityEngine;

public class PopulateUpgrades : MonoBehaviour
{
    [SerializeField] private ClickerManager _clickerManager;
    [SerializeField] private GameObject _prefab;

    [NonSerialized] private List<Binder> _objects = new List<Binder>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < _clickerManager.Config.AvailableUpgrades.Length; i++)
        {
            Upgrade availableUpgrade = _clickerManager.Config.AvailableUpgrades[i];
            GameObject go = Instantiate(_prefab, this.transform);
            Binder binder = go.GetComponent<Binder>();
            binder.Bind(availableUpgrade);
            go.SetActive(availableUpgrade.Unlocked);
            _objects.Add(binder);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _clickerManager.Config.AvailableUpgrades.Length; i++)
        {
            _objects[i].gameObject.SetActive(_clickerManager.Config.AvailableUpgrades[i].Unlocked &&
                                             Array.IndexOf(_clickerManager.State.EarnedUpgrades,
                                                 _clickerManager.Config.AvailableUpgrades[i]) == -1);
        }
    }
}