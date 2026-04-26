using System;
using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// A base class for creating ScriptableObjects that hold enum values.
    /// </summary>
    public abstract class ScriptableEnum : ScriptableInt
    {
        public TEnum GetEnumValue<TEnum>() => (TEnum)Enum.ToObject(typeof(TEnum), GetValue());
    }
    
    /// <summary>
    /// A base class for creating ScriptableObjects that hold specific enum values.
    /// </summary>
    public abstract class ScriptableEnum<TEnum> : ScriptableEnum where TEnum : Enum
    {
        [Serializable]
        public class EnumValueChangedEvent : UnityEngine.Events.UnityEvent<TEnum> { }
        
        [Serializable]
        public class StringValueChangedEvent : UnityEngine.Events.UnityEvent<string> { }
        
        [SerializeField]
        private EnumValueChangedEvent _enumValueChanged;
        
        [SerializeField]
        private StringValueChangedEvent _stringValueChanged;
        
        public EnumValueChangedEvent EnumValueChanged => _enumValueChanged;
        
        public StringValueChangedEvent StringValueChanged => _stringValueChanged;

        public TEnum GetEnumValue() => GetEnumValue<TEnum>();

        public override void SetValue(int newValue)
        {
            base.SetValue(newValue);
            OnEnumValueChanged(newValue);
            OnStringValueChanged(newValue);
        }

        public void SetValue(TEnum newValue) => SetValue(Convert.ToInt32(newValue));

        public override string ToString() => GetEnumValue().ToString();

        private void OnEnumValueChanged(int newValue)
        {
            if (_enumValueChanged == null)
                return;

            var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), newValue);
            _enumValueChanged.Invoke(enumValue);
        }

        private void OnStringValueChanged(int newValue)
        {
            if (_stringValueChanged == null)
                return;
            
            var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), newValue);
            var stringValue = enumValue.ToString();
            _stringValueChanged.Invoke(stringValue);
        }
        
        public static implicit operator TEnum(ScriptableEnum<TEnum> scriptableEnum)
        {
            return scriptableEnum.GetEnumValue();
        }
    }
}
