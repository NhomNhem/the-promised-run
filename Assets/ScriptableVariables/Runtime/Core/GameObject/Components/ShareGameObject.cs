using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// Sets the value of a 'ScriptableGameObject' variable to this game object,
    /// making it accessable across different scenes and prefabs.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class ShareGameObject : MonoBehaviour
    {
        [Header("Project References")]
        [SerializeField]
        private ScriptableGameObject _variable;

        private void Awake()
        {
            _variable.SetValue(gameObject);
        }
    }
}
