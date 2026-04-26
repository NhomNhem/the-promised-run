using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// A base class for creating scriptable dictionary variables of various key-value types.
    /// </summary>
    public abstract class ScriptableDictionary<TKey, TValue> : ScriptableVariable<IDictionary<TKey, TValue>>
    {
        [Serializable]
        private struct KeyValuePair
        {
            public TKey key;
            public TValue value;
        }

        [Header("State")]
        [SerializeField]
        private KeyValuePair[] _values = Array.Empty<KeyValuePair>();
        
        [Header("Settigs")]
        [SerializeField, Tooltip("Optionally determine capacity by this value instead of the 'values' property")]
        private Optional<int> _capacity;

        [SerializeField, Tooltip("Remove mono behaviours from the list if they are destroyed, leaving no null references.")]
        private bool _cleanupOnDestroy;
        
        protected IDictionary<TKey, TValue> value { get; private set; }

        protected virtual void OnEnable()
        {
            value ??= CreateValue(_capacity.HasValue ? _capacity.Value : _values.Length);
            
            RebuildDictionary();
        }

        protected virtual void OnValidate()
        {
            OnEnable();
        }

        protected abstract IDictionary<TKey, TValue> CreateValue(int capacity);

        public override IDictionary<TKey, TValue> GetValue() => value;
        public TValue GetValue(TKey key) => GetValue()[key];
        public TValue GetValue(TKey key, TValue defaultValue) =>  GetValue().TryGetValue(key, out TValue result) ? result : defaultValue; 

        public override void SetValue(IDictionary<TKey, TValue> newValue) => value = newValue;
        public void SetValue(TKey key, TValue newValue) =>  GetValue()[key] = newValue;

        public void Add(TKey key, TValue newValue)
        {
            if (_cleanupOnDestroy && newValue is MonoBehaviour behaviour)
            {
                behaviour.destroyCancellationToken.Register(OnDestroy, key);

                void OnDestroy(object data) => Remove((TKey)data);
            }
            
            GetValue().Add(key, newValue);
        }
        
        public void Remove(TKey key)
        {
            GetValue().Remove(key);
        }

        public TValue this[TKey key]
        {
            get => GetValue(key);
            set
            {
                if (this.value.ContainsKey(key))
                {
                    SetValue(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        private void RebuildDictionary()
        {
            for (int i = 0; i < _values.Length; i++)
                value[_values[i].key] = _values[i].value;
        }
    }
}