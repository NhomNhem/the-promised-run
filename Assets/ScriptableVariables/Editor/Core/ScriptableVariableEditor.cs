#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OpenUtility.Data.Editor
{
    [CustomEditor(typeof(ScriptableString), true)]
    public class ScriptableStringEditor : ScriptableVariableEditor { }
    
    [CustomEditor(typeof(ScriptableInt), true)]
    public class ScriptableIntEditor : ScriptableVariableEditor { }
    
    [CustomEditor(typeof(ScriptableFloat), true)]
    public class ScriptableFloatEditor : ScriptableVariableEditor { }
    
    [CustomEditor(typeof(ScriptableBool),true)]
    public class ScriptableBoolEditor : ScriptableVariableEditor { }
    
    public class ScriptableVariableEditor : UnityEditor.Editor
    {
        private SerializedProperty _valueProperty;
        
        private void OnEnable()
        {
            _valueProperty = serializedObject.FindProperty("_value");
            EditorApplication.update += RepaintWhilePlaying;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintWhilePlaying;
        }
        
        private void RepaintWhilePlaying()
        {
            if (Application.isPlaying)
                Repaint();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(_valueProperty, new GUIContent("Default"));
            DrawRuntimeValue();
            
            DrawPropertiesExcluding(serializedObject, "m_Script", "_value");

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeValue()
        {
            if (!Application.isPlaying)
                return;

            EditorGUI.BeginDisabledGroup(true);
            switch (target)
            {
                case ScriptableBool boolean:
                    EditorGUILayout.Toggle("Current", boolean);
                    break;
                
                case ScriptableFloat single:
                    EditorGUILayout.FloatField("Current", single);
                    break;
                
                case ScriptableInt integer:
                    EditorGUILayout.IntField("Current", integer);
                    break;
                
                case ScriptableString str:
                    EditorGUILayout.TextField("Current", str);
                    break;
                
                default:
                    EditorGUILayout.LabelField("Current", "N/A");
                    break;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}

#endif