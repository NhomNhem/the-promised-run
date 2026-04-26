using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenUtility.Data
{
    /// <summary>
    /// Used to share a reference to a GameObject across different scenes and prefabs. Either set the source to 'Prefab',
    /// assign a prefab to the 'prefab' field and instantiate it at runtime or set the source to 'Scene' and assign a
    /// value directly to the variable by adding a 'ShareGameObject' component to a GameObject in a scene.
    /// </summary>
    [CreateAssetMenu(fileName = "ScriptableGameObject", menuName = "OpenUtility/GameObject/ScriptableGameObject")]
    public class ScriptableGameObject : ScriptableVariable<GameObject>
    {
        public enum Source
        {
            [InspectorName("Prefab")]
            PREFAB,
            
            [InspectorName("Scene")]
            SCENE  
        }

        [Header("Settings")]
        [SerializeField]
        private Source _source = Source.PREFAB;
        
        [SerializeField, Tooltip("The prefab used to create an instance of the game object.")]
        private GameObject _prefab;
        
        [SerializeField, Tooltip("Should the instance not be destroyed when its scene is unloaded?")]
        private bool _dontDestroyOnLoad;

        [SerializeField, Tooltip("Should the instance be created automatically when no instance is set yet?")]
        private bool _instantiateLazy;

        public bool HasValue => _instance.HasValue && !Application.exitCancellationToken.IsCancellationRequested;

        private Optional<GameObject> _instance;
        private Optional<Transform> _parent;

        /// <summary>
        /// The scene the game object instance is currently being used in. null if 'dontDestroyOnLoad' is set.
        /// </summary>
        private Scene? _scene;

        private void OnEnable()
        {
#if UNITY_EDITOR
            _instance = Optional<GameObject>.None();

            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif
            
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (Application.exitCancellationToken.IsCancellationRequested)
                return;
            
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// Tries to get a component of type T from the current instance. Returns true if the component was found, false otherwise.
        /// </summary>
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = null;
            
            GameObject instance = GetValue();
            if (instance == null)
                return (false);
            
            return (instance.TryGetComponent(out component));
        }
        
        /// <summary>
        /// Returns the component of type T from the current instance.
        /// Returns null if no instance is set or if the component was not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : Component => GetValue()?.GetComponent<T>(); 
        
        /// <summary>
        /// Returns the component of type T from the current instance or any of its children.
        /// Returns null if no instance is set or if the component was not found.
        /// </summary>
        public T GetComponentInChildren<T>() where T : Component => GetValue()?.GetComponentInChildren<T>();
        
        /// <summary>
        /// Returns an array of all components of type T from the current instance.
        /// Returns null if no instance is set or an empty array if no components were found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>() => GetValue()?.GetComponents<T>();
        
        /// <summary>
        /// Returns an array of all components of type T from the current instance and any of its children.
        /// Returns null if no instance is set or an empty array if no components were found.
        /// </summary>
        public T[] GetComponentsInChildren<T>() => GetValue()?.GetComponentsInChildren<T>();

        /// <summary>
        /// Assigns the given parent to a created or set value. If the instance is already set, assigns the
        /// given parent immediately.
        /// </summary>
        public void SetParent(Transform parent)
        {
            _parent = parent;
            
            if (_instance.TryGetValue(out GameObject instance))
                instance.transform.SetParent(parent);
        }

        /// <summary>
        /// Creates a new instance of the game object using the assigned prefab.
        /// If 'dontDestroyOnLoad' is set, the instance will persist across scene loads.
        /// Returns the created instance.
        /// </summary>
        public GameObject CreateValue()
        {
#if UNITY_EDITOR
            if (_source == Source.SCENE)
                Debug.LogWarning($"[{name}] Creating instance from prefab even though source is set to 'Scene'.");
#endif
            if (_instance.TryGetValue(out GameObject instanceToDestroy))
            {
                Debug.Log($"[{name}] Replacing instance '{instanceToDestroy.name}' with new instance.");
                
                Destroy(instanceToDestroy);
            }

            if (_prefab == null)
            {
                Debug.LogWarning($"[{name} Can't create value because prefab did not have a value.");
                return (null);
            }
            
            _instance = _parent.TryGetValue(out Transform parent) ? Instantiate(_prefab, parent) : Instantiate(_prefab);

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(_instance.Value);

                _scene = null;
            }
            else
            {
                _scene = _instance.Value.scene;
            }
            
            return (_instance.Value);
        }
        
        /// <summary>
        /// Destroys the current instance of the game object. Does nothing if no instance is set.
        /// </summary>
        public void DestroyValue()
        {
            if (!_instance.HasValue)
                return;
            
            Destroy(_instance.Value);
            
            _instance = Optional<GameObject>.None();
            _scene = null;
        }

        /// <summary>
        /// Sets the given game object as the current instance. Destroys the previous instance if there was one.
        /// If 'dontDestroyOnLoad' is set, the scene reference will be cleared and the instance will persist across scene loads.
        /// </summary>
        public override void SetValue(GameObject newValue)
        {
#if UNITY_EDITOR
            if (_source == Source.PREFAB)
                Debug.LogWarning($"[{name}] Setting instance from scene even though source is set to 'Prefab'.");
#endif
            if (_instance.TryGetValue(out GameObject instanceToDestroy) && instanceToDestroy != null)
            {
                Debug.Log($"[{name}] Replacing instance '{instanceToDestroy.name}' with new instance {newValue.name}.");
                
                Destroy(instanceToDestroy);
            }
            
            if (_parent.TryGetValue(out Transform parent))
                newValue.transform.SetParent(parent);
            
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(newValue);

                _scene = null;
            }
            else
            {
                _scene = newValue.scene;
            }

            _instance = newValue;
        }

        /// <summary>
        /// Returns the current instance of the game object. If 'instantiateLazy' is set and no instance is set yet,
        /// creates a new instance using the assigned prefab. Returns the current instance.
        /// </summary>
        public override GameObject GetValue()
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return (null);
            
            if (_instance.HasValue && _instance.Value == null)
                return (null);
            
            if (_instantiateLazy)
                return (_instance.TryGetValue(out GameObject instance) ? instance : CreateValue());

            return (_instance.GetValueOrDefault());
        }

        public override string ToString() => _instance.TryGetValue(out GameObject instance) ? instance.name : $"{name} -> (null)";

        private void OnSceneUnloaded(Scene unloadedScene)
        {
            if (_scene.HasValue && unloadedScene == _scene.Value)
                _instance = Optional<GameObject>.None();
        }
        
        public static ScriptableGameObject CreateInstance(GameObject instance)
        {
            var variable = CreateInstance<ScriptableGameObject>();
            variable._source = Source.SCENE;
            variable.SetValue(instance);
            return (variable);
        }
    }
}
