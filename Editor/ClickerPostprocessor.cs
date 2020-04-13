using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    /// <summary>
    /// Postprocessor responsible for maintaining the valid GUIDs for clicker components
    /// </summary>
    class ClickerPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool rebuildRuntimeDB = false;
            foreach (string assetPath in importedAssets)
            {
                ClickerComponent component = AssetDatabase.LoadAssetAtPath<ClickerComponent>(assetPath);
                if (component == null)
                {
                    continue;
                }

                Guid clickerGuid;
                int index = ClickerSettings.Instance.ClickerComponentAssetGUIDs.IndexOf(
                    AssetDatabase.AssetPathToGUID(assetPath));
                if (index >= 0)
                {
                    clickerGuid = ClickerSettings.Instance.ClickerComponentGUIDContainers[index].Guid;
                }
                else
                {
                    clickerGuid = System.Guid.NewGuid();
                }

                SerializedObject so = new SerializedObject(component);
                so.Update();
                SerializedProperty serializedProperty = so.FindProperty("GUIDContainer._serializedGuid");

                if (clickerGuid.ToString() != serializedProperty.stringValue)
                {
                    // Update DB as this is a new GUID
                    if (index == -1)
                    {
                        ClickerSettings.Instance.ClickerComponentAssetGUIDs.Add(
                            AssetDatabase.AssetPathToGUID(assetPath));
                        ClickerSettings.Instance.ClickerComponentGUIDContainers.Add(new GUIDContainer(clickerGuid));
                    }

                    serializedProperty.stringValue = clickerGuid.ToString();
                    so.ApplyModifiedProperties();
                    AssetDatabase.ImportAsset(assetPath);
                    rebuildRuntimeDB = true;
                }
            }

            foreach (string assetPath in deletedAssets)
            {
                int index = ClickerSettings.Instance.ClickerComponentAssetGUIDs.IndexOf(
                    AssetDatabase.AssetPathToGUID(assetPath));
                if (index >= 0)
                {
                    Debug.LogFormat("Removing GUID {0}", assetPath);
                    ClickerSettings.Instance.ClickerComponentAssetGUIDs.RemoveAt(index);
                    ClickerSettings.Instance.ClickerComponentGUIDContainers.RemoveAt(index);
                    rebuildRuntimeDB = true;
                }
            }

            ClickerSettings.Instance.Save();
            EditorUtility.SetDirty(ClickerSettings.Instance);

            if (rebuildRuntimeDB)
            {
                ClickerComponent.RuntimeLookup.Clear();
                foreach (ClickerComponent clickerComponent in AssetDatabase.FindAssets("t:ClickerComponent")
                    .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ClickerComponent>))
                {
                    clickerComponent.OnAfterDeserialize();
                }
            }
        }
    }
}