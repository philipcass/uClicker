using System;
using uClicker;
using UnityEditor;
using UnityEngine;

namespace Clicker.Editor
{
    [CustomEditor(typeof(ClickerManager))]
    [CanEditMultipleObjects]
    public class ClickerManagerEditor : UnityEditor.Editor
    {
        private string _save;

        public override void OnInspectorGUI()
        {
            ClickerManager manager = this.target as ClickerManager;
            base.OnInspectorGUI();

            if (GUILayout.Button("Populate Buildings"))
            {
                string[] buildingGUIDs = AssetDatabase.FindAssets("t:Building");
                manager.Config.AvailableBuildings = new Building[buildingGUIDs.Length];
                for (int i = 0; i < buildingGUIDs.Length; i++)
                {
                    string guid = buildingGUIDs[i];
                    manager.Config.AvailableBuildings[i] =
                        AssetDatabase.LoadAssetAtPath<Building>(AssetDatabase.GUIDToAssetPath(guid));
                }

                Array.Sort(manager.Config.AvailableBuildings, BuildingSorter);
            }

            if (GUILayout.Button("Populate Upgrades"))
            {
                string[] upgradeGUIDs = AssetDatabase.FindAssets("t:Upgrade");
                manager.Config.AvailableUpgrades = new Upgrade[upgradeGUIDs.Length];
                for (int i = 0; i < upgradeGUIDs.Length; i++)
                {
                    string guid = upgradeGUIDs[i];
                    manager.Config.AvailableUpgrades[i] =
                        AssetDatabase.LoadAssetAtPath<Upgrade>(AssetDatabase.GUIDToAssetPath(guid));
                }

                Array.Sort(manager.Config.AvailableUpgrades, UpgradeSorter);
            }

            if (GUILayout.Button("Reset Progress"))
            {
                manager.State.EarnedBuildings.Clear();
                manager.State.EarnedUpgrades.Clear();
                foreach (Building availableBuilding in manager.Config.AvailableBuildings)
                {
                    availableBuilding.Unlocked = false;
                }

                foreach (Upgrade availableUpgrade in manager.Config.AvailableUpgrades)
                {
                    availableUpgrade.Unlocked = false;
                }

                manager.State.CurrencyCurrentTotals.Clear();
            }

            if (GUILayout.Button("Save"))
            {
                manager.SaveProgress();
            }

            if (GUILayout.Button("Load"))
            {
                manager.LoadProgress();
            }
        }

        private int UpgradeSorter(Upgrade x, Upgrade y)
        {
            return x.Cost.Amount.CompareTo(y.Cost.Amount);
        }

        private int BuildingSorter(Building x, Building y)
        {
            return x.Cost.Amount.CompareTo(y.Cost.Amount);
        }
    }
}