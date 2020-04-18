using uClicker;
using UnityEngine;
using UnityEngine.UI;

public class Binder : MonoBehaviour
{
    [SerializeField] private ClickerManager _clickerManager;

    public Text Name;
    public Text Cost;
    public Text Description;
    private UnlockableComponent _clickerComponent;
    private Button _button;

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
        this.Cost.text = availableUpgrade.Cost.Amount.ToString();
        this.Description.text = GenerateUpgradeString(availableUpgrade.UpgradePerk);
        _button = this.GetComponent<Button>();
        _clickerManager.OnTick.AddListener(IsActive);
        IsActive();
    }

    public void Bind(Building availableBuilding)
    {
        _clickerComponent = availableBuilding;
        this.Name.text = availableBuilding.name;
        this.Cost.text = _clickerManager.BuildingCost(availableBuilding).ToString();
        this.Description.text = string.Format("+{0} {1}s per second", availableBuilding.YieldAmount.Amount,
            availableBuilding.YieldAmount.Currency.name);
        _button = this.GetComponent<Button>();
        _clickerManager.OnTick.AddListener(IsActive);
        IsActive();
    }

    private void IsActive()
    {
        _button.interactable = _clickerManager.CanBuy(_clickerComponent);
    }

    private string GenerateUpgradeString(UpgradePerk[] availableUpgradeUpgradePerk)
    {
        string text = "";
        foreach (var upgradePerk in availableUpgradeUpgradePerk)
        {
            ClickerComponent component = upgradePerk.TargetBuilding ??
                                         (ClickerComponent) upgradePerk.TargetClickable ?? upgradePerk.TargetCurrency;
            text += string.Format("{0}s {1} to {2}", upgradePerk.Operation, upgradePerk.Amount, component.name);
        }

        return text;
    }
}