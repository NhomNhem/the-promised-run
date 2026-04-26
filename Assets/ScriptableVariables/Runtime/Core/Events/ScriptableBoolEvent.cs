using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public class ScriptableBoolEvent : ScriptableVariableEvent<bool>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableBool _variable;
        
        protected override UnityEvent<bool> GetChangedEvent() => _variable.ValueChanged;
        protected override bool GetValue() => _variable.GetValue();
    }
}
