using System;
using TMPro;
using UnityEngine;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(TMP_Dropdown), typeof(int), DisplayName = "Default Integer Variable")]
    [CreateAssetMenu(fileName = "ScriptableInt", menuName = "OpenUtility/Scriptable Variable/Int")]
    public class ScriptableInt : ScriptableVariable<int>, ICanLoadValueFromPlayerPrefs
    {
        [Serializable]
        public class ChangedEvent : UnityEngine.Events.UnityEvent<int> { }

        [Header("State")]
        [SerializeField]
        private int _value;

        [Header("Optional")]
        [SerializeField]
        private Optional<string> _playerPref;
        
        [Header("Event")]
        [SerializeField]
        private ChangedEvent _valueChanged;

        public ChangedEvent ValueChanged => _valueChanged;
        public Optional<string> PlayerPref => _playerPref;
        
        protected int value { get; private set; }

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

        public override int GetValue() => value;

        public override void SetValue(int newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
            OnValueChanged(newValue);
        }

        public virtual void SetValueWithoutNotify(int newValue)
        {
            SetValueInternal(newValue);
            SetPlayerPrefIfNeeded();
        }

        protected void SetValueInternal(int newValue) => value = newValue;

        protected void OnValueChanged(int newValue) => _valueChanged?.Invoke(newValue);

        private void SetValueFromPlayerPref(int defaultValue)
        {
            var key = _playerPref.Value;
            var data = PlayerPrefs.GetInt(key, defaultValue);
            SetValueInternal(data);
        }

        private void SetPlayerPrefIfNeeded()
        {
            if (!_playerPref.HasValue)
                return;

            var key = _playerPref.Value;
            PlayerPrefs.SetInt(key, value);
        }

        public override string ToString() => value.ToString();
        
        public static implicit operator int(ScriptableInt scriptableInt) => scriptableInt.GetValue();
    }
}
