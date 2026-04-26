using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(Slider), typeof(float), DisplayName = "Default Decimal Variable")]
    [CreateAssetMenu(fileName = "ScriptableFloat", menuName = "OpenUtility/Scriptable Variable/Float")]
    public class ScriptableFloat : ScriptableVariable<float>, ICanLoadValueFromPlayerPrefs
    {
        [Serializable]
        public class ChangedEvent : UnityEngine.Events.UnityEvent<float> { }

        [Header("State")]
        [SerializeField]
        private float _value;
        
        [Header("Optional")]
        [SerializeField]
        private Optional<string> _playerPref;

        [Header("Event")]
        [SerializeField]
        private ChangedEvent _valueChanged;

        public ChangedEvent ValueChanged => _valueChanged;
        public Optional<string> PlayerPref => _playerPref;
        
        protected float value { get; private set; }

        protected virtual void OnEnable()
        {
            if (_playerPref.HasValue)
            {
                SetValueFromPlayerPref(_value);
            }
            else
            {
                SetValueWithoutNotify(_value);
            }
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
            {
                SetValue(_value);
            }
            else
            {
                SetValueWithoutNotify(_value);
            }
        }
        
        public override float GetValue() => value;

        public override void SetValue(float newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
            OnValueChanged(newValue);
        }

        public virtual void SetValueWithoutNotify(float newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
        }

        protected void SetValueInternal(float newValue) => value = newValue;

        protected void OnValueChanged(float newValue) => _valueChanged?.Invoke(newValue);
        
        private void SetPlayerPrefIfNeeded()
        {
            if (!_playerPref.HasValue)
                return;

            var key = _playerPref.Value;
            PlayerPrefs.SetFloat(key, value);
        }

        private void SetValueFromPlayerPref(float defaultValue)
        {
            var key = _playerPref.Value;
            var data = PlayerPrefs.GetFloat(key, defaultValue);
            SetValueWithoutNotify(data);
        }

        public override string ToString() => value.ToString();
        
        public static implicit operator float(ScriptableFloat reference) => reference.GetValue();
    }
}
