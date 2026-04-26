using System;
using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public abstract class ScriptableVariableEvent<T> : MonoBehaviour
    {
        [Serializable]
        public class ChangedEvent : UnityEvent<T> { }
        
        [Header("Event")]
        [SerializeField]
        private ChangedEvent _valueChanged;
        
        public ChangedEvent ValueChanged => _valueChanged;
        
        protected virtual void OnEnable()
        {
            GetChangedEvent().AddListener(OnValueChanged);
        }

        protected virtual void OnDisable()
        {
            GetChangedEvent().RemoveListener(OnValueChanged);
        }

        private void Start()
        {
            OnValueChanged(GetValue());
        }

        protected abstract UnityEvent<T> GetChangedEvent();
        protected abstract T GetValue();
        
        private void OnValueChanged(T newValue) => _valueChanged?.Invoke(newValue);
    }
}
