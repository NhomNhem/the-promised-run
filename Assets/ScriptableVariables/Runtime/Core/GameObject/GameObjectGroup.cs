using System;
using System.Collections.Generic;
using OpenUtility.Exceptions;
using UnityEngine;
using UnityEngine.Pool;

namespace OpenUtility.Data
{
    /// <summary>
    /// Represents a group of game objects. Used with the 'GroupGameObject' component to
    /// organize game objects into groups and access their components.
    /// </summary>
    [CreateAssetMenu(fileName = "GameObjectGroup", menuName = "OpenUtility/GameObject/GameObjectGroup")]
    public class GameObjectGroup : ScriptableList<ScriptableGameObject>
    {
        /// <summary>
        /// Adds a new game object to the list.
        /// </summary>
        public void Add(GameObject gameObject)
        {
            ThrowIf.UnityObjectNull(gameObject);

            var instance = ScriptableGameObject.CreateInstance(gameObject);
            
            Add(instance);
        }
        
        /// <summary>
        /// Removes the game object from the list.
        /// </summary>
        public void Remove(GameObject gameObject)
        {
            ThrowIf.UnityObjectNull(gameObject);

            IList<ScriptableGameObject> list = GetValue();
            
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ScriptableGameObject instance = list[i];
                if (instance == null)
                    continue;

                GameObject gameObjectValue = instance.GetValue();
                if (gameObjectValue == gameObject) 
                    continue;
                
                list.RemoveAt(i);
                break;
            }
        }
        
        /// <summary>
        /// Tries to get the first component of type T found on any of the game objects in the group.
        /// Returns true if a component was found, false otherwise.
        /// </summary>
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = null;

            if (Application.exitCancellationToken.IsCancellationRequested)
                return (false);
            
            IList<ScriptableGameObject> list = GetValue();
            if (list.Count == 0)
                return (false);
            
            for (int i = 0; i < list.Count; i++)
            {
                ScriptableGameObject instance = list[i];
                if (instance == null)
                    continue;
                
                if (instance.TryGetComponent(out component))
                    return (true);
            }

            return (false);
        }
        
        /// <summary>
        /// Returns the first component of type T found on any of the game objects in the group. Returns null if no component is found.
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return (null);
            
            IList<ScriptableGameObject> list = GetValue();
            if (list.Count == 0)
                return (null);

            for (int i = 0; i < list.Count; i++)
                if (list[i].TryGetComponent(out T component))
                    return (component);

            return (null);
        }

        /// <summary>
        /// Returns an array of all components of type T found on any of the game objects in the group.
        /// Returns an empty array if no components are found.
        /// </summary>
        public T[] GetComponents<T>() where T : Component
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return (Array.Empty<T>());
            
            using var pooled = ListPool<T>.Get(out List<T> components);

            IList<ScriptableGameObject> list = GetValue();

            for (int i = 0; i < list.Count; i++)
                if (list[i].TryGetComponent(out T component))
                    components.Add(component);

            return (components.ToArray());
        }

        /// <summary>
        /// Adds components of type T found on any of the game objects in the group to given list.
        /// </summary>
        public void GetComponents<T>(List<T> components) where T : Component
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return;
            
            IList<ScriptableGameObject> list = GetValue();

            for (int i = 0; i < list.Count; i++)
                if (list[i].TryGetComponent(out T component))
                    components.Add(component);
        }
    }
}