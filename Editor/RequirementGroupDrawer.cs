using System;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CustomPropertyDrawer(typeof(RequirementGroup), true)]
    public class RequirementGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            var requirementsArray = property.FindPropertyRelative("Requirements");

            EditorGUI.indentLevel++;

            var groupPosition = EditorGUI.IndentedRect(position);
            groupPosition.height = GetPropertyHeight(property, label);
            GUI.BeginGroup(groupPosition, EditorStyles.helpBox);
            position.y = 8;
            position.width -= 64;

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RequirementOperand"));
            position.y += EditorGUIUtility.singleLineHeight;

            position.height = EditorGUIUtility.singleLineHeight;
            requirementsArray.arraySize = EditorGUI.IntField(position, "Requirements", requirementsArray.arraySize);
            position.y += EditorGUIUtility.singleLineHeight;

            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                var requirement = requirementsArray.GetArrayElementAtIndex(i);
                var requirementTypeProp = requirement.FindPropertyRelative("RequirementType");
                RequirementType type = (RequirementType) requirementTypeProp.enumValueIndex;

                position.height = EditorGUIUtility.singleLineHeight;
                type = (RequirementType) GUI.Toolbar(position, (int) type, requirementTypeProp.enumDisplayNames);
                position.y += EditorGUIUtility.singleLineHeight;

                var reqObject = GetActiveRequirement(requirement);

                var reqHeight = EditorGUI.GetPropertyHeight(reqObject);

                position.height = reqHeight;
                EditorGUI.PropertyField(position, reqObject, true);
                position.y += reqHeight;

                requirementTypeProp.enumValueIndex = (int) type;
            }

            GUI.EndGroup();
            EditorGUI.EndProperty();
        }

        private static SerializedProperty GetActiveRequirement(SerializedProperty requirement)
        {
            var requirementTypeProp = requirement.FindPropertyRelative("RequirementType");
            RequirementType type = (RequirementType) requirementTypeProp.intValue;

            SerializedProperty reqObject;
            switch (type)
            {
                case RequirementType.Currency:
                    reqObject = requirement.FindPropertyRelative("UnlockAmount");
                    break;
                case RequirementType.Building:
                    reqObject = requirement.FindPropertyRelative("UnlockBuilding");
                    break;
                case RequirementType.Upgrade:
                    reqObject = requirement.FindPropertyRelative("UnlockUpgrade");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return reqObject;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0;
            totalHeight += EditorGUIUtility.singleLineHeight;
            totalHeight += EditorGUIUtility.singleLineHeight;
            totalHeight += EditorGUIUtility.singleLineHeight;
            var requirementsArray = property.FindPropertyRelative("Requirements");
            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                var requirement = requirementsArray.GetArrayElementAtIndex(i);
                var reqObject = GetActiveRequirement(requirement);

                var reqHeight = EditorGUI.GetPropertyHeight(reqObject) + EditorGUIUtility.singleLineHeight;

                totalHeight += reqHeight;
            }

            return totalHeight;
        }
    }
}