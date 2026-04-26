using UnityEditor;

namespace OpenUtility.Data.Editor
{
    [CustomEditor(typeof(ScriptableGameObject))]
    public class ScriptableGameObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty _sourceProperty;
        private SerializedProperty _prefabProperty;
        private SerializedProperty _dontDestroyOnLoadProperty;
        private SerializedProperty _instantiateLazyProperty;
        
        private void OnEnable()
        {
            _sourceProperty = serializedObject.FindProperty("_source");
            _prefabProperty = serializedObject.FindProperty("_prefab");
            _dontDestroyOnLoadProperty = serializedObject.FindProperty("_dontDestroyOnLoad");
            _instantiateLazyProperty = serializedObject.FindProperty("_instantiateLazy");
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(_sourceProperty);
            var source = (ScriptableGameObject.Source)_sourceProperty.enumValueIndex;

            EditorGUILayout.Space();
            
            switch (source)
            {
                case ScriptableGameObject.Source.PREFAB:
                    OnPrefabSourceGUI();
                    break;
                
                case ScriptableGameObject.Source.SCENE:
                    OnSceneSourceGUI();
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void OnPrefabSourceGUI()
        {
            EditorGUILayout.LabelField("Project References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_prefabProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Insantiation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_dontDestroyOnLoadProperty);
            EditorGUILayout.PropertyField(_instantiateLazyProperty);

            if (EditorApplication.isPlaying)
                return;
            
            EditorGUILayout.Space();
            
            bool usesLazyInstantiation = _instantiateLazyProperty.boolValue;
            if (usesLazyInstantiation)
            {
                EditorGUILayout.HelpBox("The instance will be created automatically at runtime upon first retrieval.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Use the 'CreateValue' method from script to create your scene instance.", MessageType.Info);
            }
        }

        private void OnSceneSourceGUI()
        {
            EditorGUILayout.LabelField("Instance Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_dontDestroyOnLoadProperty);
        }
    }
}
