using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    public class ClickerSettingsEditor : UnityEditor.EditorWindow
    {
        private SerializedObject _serializedObject;

        private string[] _propertyBlacklist = new[]
            {"m_Script", "ClickerComponentGUIDContainers", "ClickerComponentAssetGUIDs"};

        private bool _debugOpen;
        private bool _runtimeListOpen;
        private Vector2 _scrollPosition;

        [MenuItem("uClicker/Settings")]
        static void Init()
        {
            ClickerSettingsEditor window = (ClickerSettingsEditor) GetWindow(typeof(ClickerSettingsEditor));
            window.Show();
        }

        private void OnGUI()
        {
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                _serializedObject = _serializedObject ?? new SerializedObject(ClickerSettings.Instance);
                _serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty iterator = _serializedObject.GetIterator();

                EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

                for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    if (Array.IndexOf(_propertyBlacklist, iterator.propertyPath) == -1)
                        EditorGUILayout.PropertyField(iterator, true);
                }

                _serializedObject.ApplyModifiedProperties();
                if (EditorGUI.EndChangeCheck())
                {
                    ClickerSettings.Instance.Save();
                }

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Debug Values", EditorStyles.boldLabel);

                _debugOpen = EditorGUILayout.Foldout(_debugOpen, "Debug");
                if (_debugOpen)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(_serializedObject.FindProperty("ClickerComponentGUIDContainers"),
                            true);
                        EditorGUILayout.PropertyField(_serializedObject.FindProperty("ClickerComponentAssetGUIDs"),
                            true);

                        _runtimeListOpen = EditorGUILayout.Foldout(_runtimeListOpen, "Runtime DB");
                        if (_runtimeListOpen)
                        {
                            EditorGUI.indentLevel++;
                            foreach (KeyValuePair<Guid, ClickerComponent> kvp in ClickerComponent.RuntimeLookup)
                            {
                                EditorGUILayout.LabelField("Key", kvp.Key.ToString());
                                EditorGUILayout.ObjectField("Value", kvp.Value, typeof(ClickerComponent), false);
                            }

                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }
    }
}