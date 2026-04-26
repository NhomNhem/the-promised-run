using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// Base class for binding a scriptable int variable to a component where TElementData is the type of data
    /// the component uses (e.g. float for Slider).
    /// </summary>
    /// <typeparam name="TElementData">The type of data the component uses (e.g. float for Slider).</typeparam>
    public abstract class ScriptableIntBinding<TElementData> : MonoBehaviour
    {
        [Header("Variable")]
        [SerializeField]
        private ScriptableInt _variable;

        protected ScriptableInt variable => _variable;

        public abstract void SetValue(TElementData newValue);
    }
}
