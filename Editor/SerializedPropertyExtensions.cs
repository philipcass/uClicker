using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace uClicker.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static Type GetPropertyType(this SerializedProperty property)
        {
            string[] slices = property.propertyPath.Split('.');
            System.Type type = property.serializedObject.targetObject.GetType();

            for (int i = 0; i < slices.Length; i++)
                if (slices[i] == "Array")
                {
                    i++; //skips "data[x]"
                    type = type.GetElementType(); //gets info on array elements
                }
                else
                {
                    type = type.GetField(slices[i],
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                        BindingFlags.Instance).FieldType;
                }

            return type;
        }
    }
}