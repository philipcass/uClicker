using System;
using System.Collections.Generic;
using UnityEditor;

namespace uClicker.Editor
{
    public class ClickerSettingsEditor : UnityEditor.EditorWindow
    {
        private UnityEditor.Editor _cachedEditor;

        [MenuItem("uClicker/Settings")]
        static void Init()
        {
            ClickerSettingsEditor window = (ClickerSettingsEditor) GetWindow(typeof(ClickerSettingsEditor));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            if (_cachedEditor == null || _cachedEditor.target != ClickerSettings.Instance)
                _cachedEditor = UnityEditor.Editor.CreateEditor(ClickerSettings.Instance);
            _cachedEditor.DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                ClickerSettings.Instance.Save();
            }

            foreach (KeyValuePair<Guid, ClickerComponent> kvp in ClickerComponent.Lookup)
            {
                EditorGUILayout.LabelField("Key", kvp.Key.ToString());
                EditorGUILayout.ObjectField("Value", kvp.Value, typeof(ClickerComponent), false);
            }
        }
    }
}