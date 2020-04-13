using System;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    /// <summary>
    /// CustomPropertyDrawer that turns GUIDContainers into clickercomponents (as the GUIDs are pretty useless)
    /// </summary>
    [CustomPropertyDrawer(typeof(GUIDContainer), true)]
    public class GUIDContainerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Guid guid = new System.Guid(property.FindPropertyRelative("_serializedGuid").stringValue);
            ClickerComponent clickerComponent;
            if (ClickerComponent.RuntimeLookup.TryGetValue(guid, out clickerComponent))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(position, "Target", clickerComponent, typeof(ClickerComponent), false);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}