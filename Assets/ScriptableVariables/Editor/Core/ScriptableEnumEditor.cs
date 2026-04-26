#if UNITY_EDITOR

using System;
using UnityEditor;

namespace OpenUtility.Data.Editor
{
    [CustomEditor(typeof(ScriptableEnum<>), true)]
    public class ScriptableEnumEditor : UnityEditor.Editor
    {
        private Type _enumValueType;

        private void OnEnable()
        {
            _enumValueType = GetEnumValueType(target.GetType());
        }

        public override void OnInspectorGUI()
        {
            if (_enumValueType == null)
            {
                base.OnInspectorGUI();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.Update();
            
                EditorGUI.BeginDisabledGroup(true);
                SerializedProperty script = serializedObject.FindProperty("m_Script");
                EditorGUILayout.PropertyField(script);
                EditorGUI.EndDisabledGroup();
                
                
                EditorGUILayout.LabelField("State", EditorStyles.boldLabel);
                SerializedProperty value = serializedObject.FindProperty("_value");
                Enum enumValue = (Enum)Enum.ToObject(_enumValueType, value.intValue);
                Enum newEnumValue = EditorGUILayout.EnumPopup(value.displayName, enumValue);
                
                DrawPropertiesExcluding(serializedObject, "m_Script", "_value");
                
                if (EditorGUI.EndChangeCheck())
                {
                    int newIntValue = Convert.ToInt32(newEnumValue);
                    value.intValue = newIntValue;

                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        
        public static Type GetEnumValueType(Type scriptableEnumType)
        {
            while (scriptableEnumType != null)
            {
                if (scriptableEnumType.IsGenericType && scriptableEnumType.GetGenericTypeDefinition() == typeof(ScriptableEnum<>))
                {
                    return scriptableEnumType.GetGenericArguments()[0];
                }
                
                scriptableEnumType = scriptableEnumType.BaseType;
            }
            return null;
        }
    }
}

#endif