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
                manager.AvailableBuildings = new Building[buildingGUIDs.Length];
                for (int i = 0; i < buildingGUIDs.Length; i++)
                {
                    string guid = buildingGUIDs[i];
                    manager.AvailableBuildings[i] =
                        AssetDatabase.LoadAssetAtPath<Building>(AssetDatabase.GUIDToAssetPath(guid));
                }

                Array.Sort(manager.AvailableBuildings, BuildingSorter);
            }

            if (GUILayout.Button("Populate Upgrades"))
            {
                string[] upgradeGUIDs = AssetDatabase.FindAssets("t:Upgrade");
                manager.AvailableUpgrades = new Upgrade[upgradeGUIDs.Length];
                for (int i = 0; i < upgradeGUIDs.Length; i++)
                {
                    string guid = upgradeGUIDs[i];
                    manager.AvailableUpgrades[i] =
                        AssetDatabase.LoadAssetAtPath<Upgrade>(AssetDatabase.GUIDToAssetPath(guid));
                }

                Array.Sort(manager.AvailableUpgrades, UpgradeSorter);
            }

            if (GUILayout.Button("Reset Progress"))
            {
                manager.EarnedBuildings = new BuildingContainer[0];
                manager.EarnedUpgrades = new Upgrade[0];
                foreach (Building availableBuilding in manager.AvailableBuildings)
                {
                    availableBuilding.Unlocked = false;
                }

                foreach (Upgrade availableUpgrade in manager.AvailableUpgrades)
                {
                    availableUpgrade.Unlocked = false;
                }

                manager.TotalAmount = 0;
            }

            if (GUILayout.Button("Save"))
            {
                PlayerPrefs.SetString("_save", JsonUtility.ToJson(manager));
            }

            if (GUILayout.Button("Load"))
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("_save"), manager);
            }
        }

        private int UpgradeSorter(Upgrade x, Upgrade y)
        {
            return x.Cost.CompareTo(y.Cost);
        }

        private int BuildingSorter(Building x, Building y)
        {
            return x.Cost.CompareTo(y.Cost);
        }
    }
}