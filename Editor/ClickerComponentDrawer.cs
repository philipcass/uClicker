using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    /// <summary>
    /// CustomPropertyDrawer that enables dropdowns for ClickerComponents
    /// </summary>
    [CustomPropertyDrawer(typeof(ClickerComponent), true)]
    public class ClickerComponentDrawer : PropertyDrawer
    {
        private ClickerComponent[] _backingOptions;
        private string[] _displayedOptions;
        private static ClickerComponent[] _clickerComponents;

        private ClickerComponent[] Components
        {
            get
            {
                if (_backingOptions != null)
                {
                    return _backingOptions;
                }

                Type fieldType = this.fieldInfo.FieldType;
                if (fieldType.IsArray)
                {
                    fieldType = fieldType.GetElementType();
                }

                _backingOptions = _clickerComponents.Where(component => component.GetType() == fieldType).Prepend(null)
                    .ToArray();
                return _backingOptions;
            }
        }

        private string[] DisplayedOptions
        {
            get
            {
                if (_displayedOptions != null)
                {
                    return _displayedOptions;
                }

                _displayedOptions = Array.ConvertAll(Components, input => input != null ? input.name : "None");
                return _displayedOptions;
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod]
        private static void OnScriptsReloaded()
        {
            _clickerComponents = AssetDatabase.FindAssets("t:ClickerComponent").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ClickerComponent>).ToArray();
        }

        class ClickerPostprocessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                HashSet<ClickerComponent> hash = new HashSet<ClickerComponent>(_clickerComponents);
                foreach (ClickerComponent obj in importedAssets.Select(AssetDatabase.LoadAssetAtPath<ClickerComponent>))
                {
                    hash.Add(obj);
                }

                _clickerComponents = hash.Where(component => component != null).ToArray();
                foreach (UnityEditor.Editor item in ActiveEditorTracker.sharedTracker.activeEditors)
                {
                    //TODO: Update active editors
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ClickerSettings.Instance.UseCustomInspector)
            {
                EditorGUI.PropertyField(position, property);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int index = EditorGUI.Popup(position,
                Array.IndexOf(Components, property.objectReferenceValue as ClickerComponent),
                DisplayedOptions.Length > 0 ? DisplayedOptions : new[] {"None Available"});
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = Components[index];
            }

            EditorGUI.EndProperty();
        }
    }
}