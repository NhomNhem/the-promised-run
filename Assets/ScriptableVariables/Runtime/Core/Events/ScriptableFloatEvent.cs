using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public class ScriptableFloatEvent : ScriptableVariableEvent<float>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableFloat _variable;
        
        protected override UnityEvent<float> GetChangedEvent() => _variable.ValueChanged;
        protected override float GetValue() => _variable.GetValue();
    }
}
