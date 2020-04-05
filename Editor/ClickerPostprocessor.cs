using UnityEditor;

namespace uClicker.Editor
{
    class ClickerPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                ClickerComponent component = AssetDatabase.LoadAssetAtPath<ClickerComponent>(assetPath);
                if (component == null)
                {
                    continue;
                }

                SerializedObject so = new SerializedObject(component);
                SerializedProperty serializedProperty = so.FindProperty("_serializedGuid");
                if (serializedProperty.arraySize == 0)
                {
                    serializedProperty.arraySize = 16;
                    byte[] bs = System.Guid.NewGuid().ToByteArray();
                    for (int i = 0; i < bs.Length; i++)
                    {
                        byte b = bs[i];
                        serializedProperty.GetArrayElementAtIndex(i).intValue = b;
                    }
                }

                so.ApplyModifiedProperties();
            }
        }
    }
}