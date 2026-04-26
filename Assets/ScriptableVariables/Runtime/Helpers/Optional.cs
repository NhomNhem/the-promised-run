using System;
using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// A serializable optional value type. Usefull in situations where a value may or may not be preset
    /// and checking for its availability is expensive (e.g. null checks on UnityEngine.Objects).
    /// </summary>
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField]
        private bool _hasValue;

        [SerializeField]
        private T _value;

        /// <summary>
        /// Does this Optional contain a valid value?
        /// </summary>
        public bool HasValue => _hasValue;
        
        /// <summary>
        /// The value contained in this Optional.
        /// </summary>
        public T Value => _value;

        public Optional(T value)
        {
            _hasValue = true;
            _value = value;
        }
        
        /// <summary>
        /// Use this to create an Optional without a value.
        /// </summary>
        public static Optional<T> None()
        {
            return new Optional<T>
            {
                _hasValue = false,
                _value = default
            };
        }

        public bool TryGetValue(out T value)
        {
            value = GetValueOrDefault();
            return (_hasValue);
        }
        
        public T GetValueOrDefault(T defaultValue = default) => _hasValue ? _value : defaultValue;

        public override string ToString() => _hasValue ? _value.ToString() : "(None)";

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }
    }
}
