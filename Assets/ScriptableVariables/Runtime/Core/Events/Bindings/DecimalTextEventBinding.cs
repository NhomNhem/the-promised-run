using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public abstract class DecimalTextEventBinding : ScriptableVariableEvent<string>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableFloat _variable;

        private readonly UnityEvent<string> _changedEvent = new UnityEvent<string>();

        protected override void OnEnable()
        {
            base.OnEnable();
            AddListener();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            RemoveListener();
        }

        protected abstract string ConvertDecimalToText(float newValue);

        protected override UnityEvent<string> GetChangedEvent() => _changedEvent;

        protected override string GetValue()
        {
            float value = _variable.GetValue();
            return (ConvertDecimalToText(value));
        }

        private void AddListener() => _variable.ValueChanged.AddListener(OnValueChanged);

        private void RemoveListener() => _variable.ValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(float newValue)
        {
            string converted = ConvertDecimalToText(newValue);
            _changedEvent?.Invoke(converted);
        }
    }
}
