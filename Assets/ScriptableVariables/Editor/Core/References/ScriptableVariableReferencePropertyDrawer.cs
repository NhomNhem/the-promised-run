#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OpenUtility.Data.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableVariableReference<>), true)]
    public class ScriptableVariableReferencePropertyDrawer : PropertyDrawer
    {
        private bool? _hasNestedData = null;
        private SerializedProperty _scriptableVariableProperty = null;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("_localValue");
            return GetHasNestedData(valueProperty) ? GetNestedPropertyHeight(property) : GetNonNestedPropertyHeight(property);
        }

        private float GetNestedPropertyHeight(SerializedProperty property)
        {
            SerializedProperty valueSource = property.FindPropertyRelative("_valueSource");
            VariableValueSource valueSourceValue = (VariableValueSource)valueSource.enumValueIndex;
            
            if (valueSourceValue == VariableValueSource.Shared)
                return (GetNonNestedPropertyHeight(property));

            SerializedProperty valueProperty = property.FindPropertyRelative("_localValue");
            float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
            if (!property.isExpanded)
                return (valuePropertyHeight);
            
            float valueSourceHeight = EditorGUI.GetPropertyHeight(valueSource);
            return (EditorGUI.GetPropertyHeight(valueProperty) + EditorGUIUtility.standardVerticalSpacing + valueSourceHeight);
        }

        private float GetNonNestedPropertyHeight(SerializedProperty property)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
                return totalHeight;

            SerializedProperty valueSource = property.FindPropertyRelative("_valueSource");
            float valueSourceHeight = EditorGUI.GetPropertyHeight(valueSource);
            totalHeight += valueSourceHeight + EditorGUIUtility.standardVerticalSpacing;

            return (totalHeight);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("_localValue");
            if (GetHasNestedData(valueProperty))
            {
                OnNestedPropertyGUI(position, property, label);
            }
            else
            {
                OnNonNestedPropertyGUI(position, property, label);
            }
        }
        
        private void OnNestedPropertyGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueSourceProperty = property.FindPropertyRelative("_valueSource");
            VariableValueSource valueSourceValue = (VariableValueSource)valueSourceProperty.enumValueIndex;
            if (valueSourceValue == VariableValueSource.Shared)
            {
                OnNonNestedPropertyGUI(position, property, label);
            }
            else
            {
                SerializedProperty valueProperty = property.FindPropertyRelative("_localValue");
                
                EditorGUI.BeginProperty(position, label, valueProperty);
                EditorGUI.PropertyField(position, valueProperty, label, true);
                property.isExpanded = valueProperty.isExpanded;
                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    
                    float height = EditorGUI.GetPropertyHeight(valueSourceProperty);
                    float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
                    float yOffset = valuePropertyHeight + EditorGUIUtility.standardVerticalSpacing;
                    Rect valueSourceRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(valueSourceRect, valueSourceProperty, new GUIContent("Source"));

                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.EndProperty();
            }
        }

        private void OnNonNestedPropertyGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            Rect contentRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
            
            SerializedProperty valueSourceProperty = property.FindPropertyRelative("_valueSource");
            VariableValueSource valueSourceValue = (VariableValueSource)valueSourceProperty.enumValueIndex;
            OnPropertyValueGUI(contentRect, property, valueSourceValue);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                OnValueSourceGUI(position, valueSourceProperty);
                
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void OnValueSourceGUI(Rect position, SerializedProperty valueSourceProperty)
        {
            float height = EditorGUI.GetPropertyHeight(valueSourceProperty);
            float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect valueSourceRect = new Rect(position.x, position.y + yOffset, position.width, height);
            EditorGUI.PropertyField(valueSourceRect, valueSourceProperty, new GUIContent("Source"));
        }

        private void OnPropertyValueGUI(Rect position, SerializedProperty property, VariableValueSource valueSource)
        {
            SerializedProperty valueProperty;
            switch (valueSource)
            {
                case VariableValueSource.Local:
                    valueProperty = property.FindPropertyRelative("_localValue");
                    break;
                
                case VariableValueSource.Shared:
                    valueProperty = GetScriptableVariableProperty(property);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            float height = EditorGUI.GetPropertyHeight(valueProperty);
            position.height = height;
            EditorGUI.PropertyField(position, valueProperty, GUIContent.none, true);
        }


        private SerializedProperty GetScriptableVariableProperty(SerializedProperty property)
        {
            if (_scriptableVariableProperty != null)
                return (_scriptableVariableProperty);
            
            _scriptableVariableProperty = GetValue();
            return (_scriptableVariableProperty);
            
            SerializedProperty GetValue()
            {
                SerializedProperty value = property.FindPropertyRelative("_variable");
                if (value != null)
                    return (value);
                
                Type type = fieldInfo.FieldType;
                foreach (MemberInfo member in type.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member.MemberType != MemberTypes.Field)
                        continue;

                    FieldInfo field = member as FieldInfo;
                    if (field == null)
                        continue;

                    Type fieldType = field.FieldType;
                    Type baseType = fieldType.BaseType;
                    if (!baseType.IsGenericType)
                        continue;

                    Type genericType = baseType.GetGenericTypeDefinition();
                    if (genericType != typeof(ScriptableVariable<>))
                        continue;

                    SerializedProperty variableProperty = property.FindPropertyRelative(field.Name);
                    if (variableProperty != null)
                        return (variableProperty);
                }
            
                Debug.LogWarning($"Could not find ScriptableVariable field in VariableReference of type {property.type}. Make sure it derives directly from VariableReference<T>.");

                return (null);
            }
        }
        
        private bool GetHasNestedData(SerializedProperty property)
        {
            if (_hasNestedData.HasValue)
                return (_hasNestedData.Value);

            _hasNestedData = GetValue();
            return (_hasNestedData.Value);
            
            bool GetValue()
            {
                if (property == null)
                    return false;

                if (!property.hasVisibleChildren || property.propertyType == SerializedPropertyType.String)
                    return false;

                SerializedProperty iterator = property.Copy();
                SerializedProperty end = iterator.GetEndProperty();
                
                if (!iterator.NextVisible(true))
                    return false;

                while (!SerializedProperty.EqualContents(iterator, end))
                {
                    if (iterator.depth > property.depth)
                        return true;

                    if (!iterator.NextVisible(false))
                        break;
                }

                return false;
            }
        }
    }
}

#endif