using System.Collections;
using System.Linq;
using uClicker;
using UnityEngine;
using UnityEngine.UI;

public class UpdateInfo : MonoBehaviour
{
    public ClickerManager Manager;
    public Text Money;
    public Text Upgrades;
    public Text Buildings;

    void Start()
    {
        Manager.OnTick.AddListener(OnTick);
        Manager.OnBuyBuilding.AddListener(OnBuyBuilding);
        Manager.OnBuyUpgrade.AddListener(OnBuyUpgrade);
        OnTick();
        OnBuyBuilding();
        OnBuyUpgrade();
    }

    private void OnTick()
    {
        Money.text = "Current Money: " + Manager.TotalAmount;
    }

    private void OnBuyUpgrade()
    {
        Upgrades.text = "Current Upgrades: " +
                        string.Join(", ", Manager.EarnedUpgrades.Select(upgrade => upgrade.name).ToArray());
    }

    private void OnBuyBuilding()
    {
        Buildings.text = "Current Buildings: " + string.Join(", ",
                             Manager.EarnedBuildings.Select(building =>
                                 string.Format("{0} {1}", building.Building.name, building.Count)).ToArray());
    }
}