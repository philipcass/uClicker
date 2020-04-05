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
        for (int i = 0; i < _clickerManager.AvailableBuildings.Length; i++)
        {
            Building availableBuilding = _clickerManager.AvailableBuildings[i];
            GameObject go = Instantiate(_prefab, this.transform);
            Binder binder = go.GetComponent<Binder>();
            binder.Bind(availableBuilding);
            go.SetActive(availableBuilding.Unlocked);
            _objects.Add(binder);
        }

        _clickerManager.OnBuyBuilding.AddListener(UpdateBuildingCost);
    }

    private void UpdateBuildingCost()
    {
        for (int i = 0; i < _clickerManager.AvailableBuildings.Length; i++)
        {
            _objects[i].Bind(_clickerManager.AvailableBuildings[i]);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _clickerManager.AvailableBuildings.Length; i++)
        {
            _objects[i].gameObject.SetActive(_clickerManager.AvailableBuildings[i].Unlocked &&
                                             Array.IndexOf(_clickerManager.EarnedBuildings,
                                                 _clickerManager.AvailableBuildings[i]) == -1);
        }
    }
}