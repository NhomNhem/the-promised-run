using OpenUtility.Data;
using UnityEngine;
using UnityEngine.Events;

namespace OpenUtility
{
    public class ScriptableIntEvent : ScriptableVariableEvent<int>
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableInt _variable;
        
        protected override UnityEvent<int> GetChangedEvent() => _variable.ValueChanged;
        protected override int GetValue() => _variable.GetValue();
    }
}
