using UnityEditor;

namespace uClicker.Editor
{
    class ClickerPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                ClickerComponent component = AssetDatabase.LoadAssetAtPath<ClickerComponent>(assetPath);
                if (component == null)
                {
                    continue;
                }

                SerializedObject so = new SerializedObject(component);
                so.Update();
                SerializedProperty serializedProperty = so.FindProperty("GUIDContainer._serializedGuid");
                if (string.IsNullOrEmpty(serializedProperty.stringValue))
                {
                    serializedProperty.stringValue = System.Guid.NewGuid().ToString();
                    so.ApplyModifiedProperties();
                    AssetDatabase.ImportAsset(assetPath);
                }
                else
                {
                    ClickerComponent.Lookup[component.GUIDContainer.Guid] = component;
                }
            }
        }
    }
}