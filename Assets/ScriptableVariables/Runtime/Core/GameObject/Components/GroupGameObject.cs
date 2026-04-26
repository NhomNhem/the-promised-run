using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// Adds the game object to the specified group on Awake. Use this component to organize game objects
    /// into groups and access their components through the 'GameObjectGroup' scriptable object.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class GroupGameObject : MonoBehaviour
    {
        [Header("Project References")]
        [SerializeField]
        private GameObjectGroup _group;

        private void Awake()
        {
            _group.Add(gameObject);  
        }

        private void OnDestroy()
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return;
            
            _group.Remove(gameObject);
        }
    }
}
