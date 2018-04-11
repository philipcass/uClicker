using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CustomEditor(typeof(Component), true)]
    [CanEditMultipleObjects]
    public class ComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Component c = target as Component;
            if (!EditorGUIUtility.editingTextField)
            {
                if (!ClickerSettings.Instance.MapNameToFileNames || string.IsNullOrEmpty(c.Name) || c.name == c.Name)
                {
                    return;
                }

                string assetPath = AssetDatabase.GetAssetPath(c.GetInstanceID());
                AssetDatabase.RenameAsset(assetPath, c.Name);
            }
        }
    }
}