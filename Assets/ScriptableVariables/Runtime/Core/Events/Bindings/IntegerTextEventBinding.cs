using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public abstract class IntegerTextEventBinding : ScriptableVariableEvent<string>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableInt _variable;

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

        protected abstract string ConvertIntegerToText(int newValue);

        protected override UnityEvent<string> GetChangedEvent() => _changedEvent;

        protected override string GetValue()
        {
            int value = _variable.GetValue();
            return (ConvertIntegerToText(value));
        }

        private void AddListener() => _variable.ValueChanged.AddListener(OnValueChanged);

        private void RemoveListener() => _variable.ValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(int newValue)
        {
            string converted = ConvertIntegerToText(newValue);
            _changedEvent?.Invoke(converted);
        }
    }
}
