using System;
using TMPro;
using UnityEngine;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(TMP_Text), typeof(string), DisplayName = "Default String Variable")]
    [ScriptableVariableBinder(typeof(TMP_InputField), typeof(string), DisplayName = "Default String Variable")]
    [CreateAssetMenu(fileName = "ScriptableString", menuName = "OpenUtility/Scriptable Variable/String")]
    public class ScriptableString : ScriptableVariable<string>, ICanLoadValueFromPlayerPrefs
    {
        [Serializable]
        public class ChangedEvent : UnityEngine.Events.UnityEvent<string> { }

        [Header("State")]
        [SerializeField]
        private string _value;
        
        [Header("Optional")]
        [SerializeField, Space]
        private Optional<string> _playerPref;

        [Header("Event")]
        [SerializeField]
        private ChangedEvent _valueChanged;

        public ChangedEvent ValueChanged => _valueChanged;
        public Optional<string> PlayerPref => _playerPref;
        
        protected string value { get; private set; }

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

        public override string GetValue() => value;

        public override void SetValue(string newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
            OnValueChanged(newValue);
        }

        public virtual void SetValueWithoutNotify(string newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
        }

        protected void SetValueInternal(string newValue) => value = newValue;

        protected void OnValueChanged(string newValue) => _valueChanged?.Invoke(newValue);

        private void SetPlayerPrefIfNeeded()
        {
            if (!_playerPref.HasValue) 
                return;
            
            var key = _playerPref.Value;
            PlayerPrefs.SetString(key, value);
        }

        private void SetValueFromPlayerPref(string defaultValue)
        {
            var key = _playerPref.Value;
            var data = PlayerPrefs.GetString(key, defaultValue);
            SetValueInternal(data);
        }

        public override string ToString() => value;
        
        public static implicit operator string(ScriptableString scriptableString) => scriptableString.GetValue();
    }
}
