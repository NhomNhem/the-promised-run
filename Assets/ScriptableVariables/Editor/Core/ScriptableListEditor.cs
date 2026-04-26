using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OpenUtility.Data.Editor
{
    [CustomEditor(typeof(ScriptableList<>), true)]
    public class ScriptableListEditor : UnityEditor.Editor
    {
        private PropertyInfo _valueProperty;

        private void OnEnable()
        {
            _valueProperty = target.GetType().GetProperty("value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            EditorApplication.update += RepaintWhilePlaying;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= RepaintWhilePlaying;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            DrawRuntimeState();

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeState()
        {
            EditorGUILayout.LabelField("Runtime List State", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("List state is only available in Play Mode.", MessageType.Info);
                return;
            }
            
            var value = (IEnumerable)_valueProperty.GetValue(target);
            if (value == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            EditorGUI.BeginDisabledGroup(true); 
            
            int index = 0;
            foreach (object item in value)
            {
                EditorGUILayout.LabelField($"[{index}]", item?.ToString() ?? "(null)");
                index++;
            }

            if (index == 0)
                EditorGUILayout.HelpBox("List is currently empty.", MessageType.Info);

            EditorGUI.EndDisabledGroup();
        }
        
        private void RepaintWhilePlaying()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}