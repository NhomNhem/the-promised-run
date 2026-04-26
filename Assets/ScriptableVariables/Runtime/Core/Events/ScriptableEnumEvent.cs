using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public class ScriptableEnumEvent : ScriptableVariableEvent<int>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableEnum _variable;
        
        protected override UnityEvent<int> GetChangedEvent() => _variable.ValueChanged;

        protected override int GetValue() => _variable.GetValue();
    }
}
