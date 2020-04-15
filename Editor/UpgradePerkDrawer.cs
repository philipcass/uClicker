using System;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CustomPropertyDrawer(typeof(UpgradeType), true)]
    public class UpgradeTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.enumValueIndex = GUI.Toolbar(position, property.enumValueIndex, property.enumDisplayNames);
        }
    }

    [CustomPropertyDrawer(typeof(UpgradePerk), true)]
    public class UpgradePerkDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect groupPosition, SerializedProperty property, GUIContent label)
        {
            groupPosition = EditorGUI.IndentedRect(groupPosition);
            SerializedProperty requirementTypeProp = property.FindPropertyRelative("Type");
            groupPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(groupPosition, requirementTypeProp, true);
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty reqObject = GetActiveRequirement(property);
            groupPosition.height = EditorGUI.GetPropertyHeight(reqObject);
            EditorGUI.PropertyField(groupPosition, reqObject, true);
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

            reqObject = property.FindPropertyRelative("Operation");
            groupPosition.height = EditorGUI.GetPropertyHeight(reqObject);
            EditorGUI.PropertyField(groupPosition, reqObject, true);
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

            reqObject = property.FindPropertyRelative("Amount");
            groupPosition.height = EditorGUI.GetPropertyHeight(reqObject);
            EditorGUI.PropertyField(groupPosition, reqObject, true);
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static SerializedProperty GetActiveRequirement(SerializedProperty requirement)
        {
            SerializedProperty requirementTypeProp = requirement.FindPropertyRelative("Type");
            UpgradeType type = (UpgradeType) requirementTypeProp.intValue;

            SerializedProperty reqObject;
            switch (type)
            {
                case UpgradeType.Currency:
                    reqObject = requirement.FindPropertyRelative("TargetCurrency");
                    break;
                case UpgradeType.Building:
                    reqObject = requirement.FindPropertyRelative("TargetBuilding");
                    break;
                case UpgradeType.Clickable:
                    reqObject = requirement.FindPropertyRelative("TargetClickable");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return reqObject;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing) * 4;
        }
    }
}