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
        for (var i = 0; i < _clickerManager.AvailableUpgrades.Length; i++)
        {
            var availableUpgrade = _clickerManager.AvailableUpgrades[i];
            GameObject go = Instantiate(_prefab, this.transform);
            var binder = go.GetComponent<Binder>();
            binder.Bind(availableUpgrade);
            go.SetActive(availableUpgrade.Unlocked);
            _objects.Add(binder);
        }
    }

    private void Update()
    {
        for (var i = 0; i < _clickerManager.AvailableUpgrades.Length; i++)
        {
            _objects[i].gameObject.SetActive(_clickerManager.AvailableUpgrades[i].Unlocked &&
                                             Array.IndexOf(_clickerManager.EarnedUpgrades,
                                                 _clickerManager.AvailableUpgrades[i]) == -1);
        }
    }
}