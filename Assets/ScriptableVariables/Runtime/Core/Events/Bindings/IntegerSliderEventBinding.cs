using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public abstract class IntegerSliderEventBinding : ScriptableVariableEvent<float>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableInt _variable;

        private readonly UnityEvent<float> _changedEvent = new UnityEvent<float>();

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

        protected abstract float ConvertIntegerToDecimal(int newValue);

        protected override UnityEvent<float> GetChangedEvent() => _changedEvent;

        protected override float GetValue()
        {
            int value = _variable.GetValue();
            return (ConvertIntegerToDecimal(value));
        }

        private void AddListener() => _variable.ValueChanged.AddListener(OnValueChanged);

        private void RemoveListener() => _variable.ValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(int newValue)
        {
            float converted = ConvertIntegerToDecimal(newValue);
            _changedEvent?.Invoke(converted);
        }
    }
}