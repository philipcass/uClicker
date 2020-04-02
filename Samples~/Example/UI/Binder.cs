using uClicker;
using UnityEngine;
using UnityEngine.UI;

public class Binder : MonoBehaviour
{
    [SerializeField] private ClickerManager _clickerManager;

    public Text Name;
    public Text Cost;
    private UnlockableComponent _component;

    public void Buy()
    {
        if (_component is Upgrade)
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
        _component = availableUpgrade;
        this.Name.text = availableUpgrade.Name;
        this.Cost.text = availableUpgrade.Cost.ToString();
    }

    public void Bind(Building availableBuilding)
    {
        _component = availableBuilding;
        this.Name.text = availableBuilding.Name;
        this.Cost.text = _clickerManager.BuildingCost(availableBuilding).ToString();
    }
}