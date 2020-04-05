using uClicker;
using UnityEngine;
using UnityEngine.UI;

public class Binder : MonoBehaviour
{
    [SerializeField] private ClickerManager _clickerManager;

    public Text Name;
    public Text Cost;
    private UnlockableComponent _clickerComponent;

    public void Buy()
    {
        if (_clickerComponent is Upgrade)
        {
            _clickerManager.BuyUpgrade(Name.text);
        }
        else
        {
            _clickerManager.BuyBuilding(Name.text);
        }
    }

    public void Bind(Upgrade availableUpgrade)
    {
        _clickerComponent = availableUpgrade;
        this.Name.text = availableUpgrade.name;
        this.Cost.text = availableUpgrade.Cost.ToString();
    }

    public void Bind(Building availableBuilding)
    {
        _clickerComponent = availableBuilding;
        this.Name.text = availableBuilding.name;
        this.Cost.text = _clickerManager.BuildingCost(availableBuilding).ToString();
    }
}