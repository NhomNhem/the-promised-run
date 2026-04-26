using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility.Data
{
    public class ScriptableStringEvent : ScriptableVariableEvent<string>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableString _variable;
        
        protected override UnityEvent<string> GetChangedEvent() => _variable.ValueChanged;
        protected override string GetValue() => _variable.GetValue();
    }
}
