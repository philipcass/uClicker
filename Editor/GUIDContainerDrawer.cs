using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CustomPropertyDrawer(typeof(GUIDContainer), true)]
    public class GUIDContainerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guid = new System.Guid(property.FindPropertyRelative("_serializedGuid").stringValue);
            ClickerComponent clickerComponent;
            if (ClickerComponent.Lookup.TryGetValue(guid, out clickerComponent))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(position, "Target", clickerComponent, typeof(ClickerComponent), false);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}