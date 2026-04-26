using System;
using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// A base class for creating scriptable variables of various types.
    /// </summary>
    public abstract class ScriptableVariable<T> : ScriptableObject
    {
        public abstract T GetValue();
        public virtual void SetValue(T newValue) => throw new NotImplementedException($"Setter for {typeof(T)} is not implemented.");
    }
}
