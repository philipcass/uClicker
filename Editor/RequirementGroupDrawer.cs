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

            position = EditorGUI.IndentedRect(position);
            GUI.BeginGroup(position, EditorStyles.helpBox);
            Rect groupPosition = new Rect();
            groupPosition.y = 8;
            groupPosition.width = position.width - 16;

            groupPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(groupPosition, property.FindPropertyRelative("GroupOperand"));
            groupPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            groupPosition.height = EditorGUIUtility.singleLineHeight;
            requirementsArray.arraySize =
                EditorGUI.IntField(groupPosition, "Requirements", requirementsArray.arraySize);
            groupPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            groupPosition = EditorGUI.IndentedRect(groupPosition);

            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                var requirement = requirementsArray.GetArrayElementAtIndex(i);
                var requirementTypeProp = requirement.FindPropertyRelative("RequirementType");
                RequirementType type = (RequirementType) requirementTypeProp.enumValueIndex;

                groupPosition.height = EditorGUIUtility.singleLineHeight;
                type = (RequirementType) GUI.Toolbar(groupPosition, (int) type, requirementTypeProp.enumDisplayNames);
                groupPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var reqObject = GetActiveRequirement(requirement);

                var reqHeight = EditorGUI.GetPropertyHeight(reqObject);

                groupPosition.height = reqHeight;
                EditorGUI.PropertyField(groupPosition, reqObject, true);
                groupPosition.y += reqHeight + EditorGUIUtility.standardVerticalSpacing;

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
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var requirementsArray = property.FindPropertyRelative("Requirements");
            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                var requirement = requirementsArray.GetArrayElementAtIndex(i);
                var reqObject = GetActiveRequirement(requirement);

                var reqHeight = EditorGUI.GetPropertyHeight(reqObject) + EditorGUIUtility.singleLineHeight +
                                EditorGUIUtility.standardVerticalSpacing;

                totalHeight += reqHeight;
            }

            return totalHeight;
        }
    }
}