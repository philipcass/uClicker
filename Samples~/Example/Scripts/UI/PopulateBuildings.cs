using System;
using System.Collections.Generic;
using uClicker;
using UnityEngine;

public class PopulateBuildings : MonoBehaviour
{
    [SerializeField] private ClickerManager _clickerManager;
    [SerializeField] private GameObject _prefab;

    [NonSerialized] private List<Binder> _objects = new List<Binder>();

    void Start()
    {
        for (int i = 0; i < _clickerManager.Config.AvailableBuildings.Length; i++)
        {
            Building availableBuilding = _clickerManager.Config.AvailableBuildings[i];
            GameObject go = Instantiate(_prefab, this.transform);
            Binder binder = go.GetComponent<Binder>();
            binder.Bind(availableBuilding);
            go.SetActive(availableBuilding.Unlocked);
            _objects.Add(binder);
        }

        _clickerManager.OnBuyBuilding.AddListener(UpdateBuildingCost);
        _clickerManager.OnTick.AddListener(OnTick);
    }

    private void UpdateBuildingCost()
    {
        for (int i = 0; i < _clickerManager.Config.AvailableBuildings.Length; i++)
        {
            _objects[i].Bind(_clickerManager.Config.AvailableBuildings[i]);
        }
    }

    private void OnTick()
    {
        for (int i = 0; i < _clickerManager.Config.AvailableBuildings.Length; i++)
        {
            _objects[i].gameObject.SetActive(_clickerManager.Config.AvailableBuildings[i].Unlocked);
        }
    }
}