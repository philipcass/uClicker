using System;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CustomPropertyDrawer(typeof(RequirementType), true)]
    public class RequirementTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.enumValueIndex = GUI.Toolbar(position, property.enumValueIndex, property.enumDisplayNames);
        }
    }

    [CustomPropertyDrawer(typeof(RequirementGroup), true)]
    public class RequirementGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty requirementsArray = property.FindPropertyRelative("Requirements");

            position = EditorGUI.IndentedRect(position);
            GUI.BeginGroup(position, EditorStyles.helpBox);
            Rect groupPosition = new Rect();
            groupPosition.y = 8;
            groupPosition.width = position.width - 16;

            groupPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(groupPosition, property.FindPropertyRelative("GroupOperand"));
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

            groupPosition.height = EditorGUIUtility.singleLineHeight;
            requirementsArray.arraySize =
                EditorGUI.IntField(groupPosition, "Requirements", requirementsArray.arraySize);
            groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

            groupPosition = EditorGUI.IndentedRect(groupPosition);

            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                SerializedProperty requirement = requirementsArray.GetArrayElementAtIndex(i);

                SerializedProperty requirementTypeProp = requirement.FindPropertyRelative("RequirementType");
                groupPosition.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(groupPosition, requirementTypeProp, true);
                groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty reqObject = GetActiveRequirement(requirement);
                groupPosition.height = EditorGUI.GetPropertyHeight(reqObject);
                EditorGUI.PropertyField(groupPosition, reqObject, true);
                groupPosition.y += groupPosition.height + EditorGUIUtility.standardVerticalSpacing;
            }

            GUI.EndGroup();
            EditorGUI.EndProperty();
        }

        private static SerializedProperty GetActiveRequirement(SerializedProperty requirement)
        {
            SerializedProperty requirementTypeProp = requirement.FindPropertyRelative("RequirementType");
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
            SerializedProperty requirementsArray = property.FindPropertyRelative("Requirements");
            for (int i = 0; i < requirementsArray.arraySize; i++)
            {
                SerializedProperty requirement = requirementsArray.GetArrayElementAtIndex(i);
                SerializedProperty reqObject = GetActiveRequirement(requirement);

                float reqHeight = EditorGUI.GetPropertyHeight(reqObject) + EditorGUIUtility.singleLineHeight +
                                  EditorGUIUtility.standardVerticalSpacing;

                totalHeight += reqHeight;
            }

            return totalHeight;
        }
    }
}