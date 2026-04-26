using System;
using OpenUtility.Exceptions;
using UnityEngine;

namespace OpenUtility.Data
{
    /// <summary>
    /// Defines the source of a variable's value. Can be either local (On the script itself)
    /// or shared (From a ScriptableObject).
    /// </summary>
    public enum VariableValueSource
    {
        /// <summary>
        /// The variable's source is on the instance itself.
        /// </summary>
        Local,
        
        /// <summary>
        /// The variable's source is a shared ScriptableObject.
        /// </summary>
        Shared
    }
    
    [Serializable]
    public class FloatReference : ScriptableVariableReference<float>
    {
        [SerializeField]
        private ScriptableFloat _variable;

        protected override ScriptableVariable<float> GetScriptableVariable() => _variable;
    }
    
    [Serializable]
    public class IntReference : ScriptableVariableReference<int>
    {
        [SerializeField]
        private ScriptableInt _variable;

        protected override ScriptableVariable<int> GetScriptableVariable() => _variable;
    }
    
    [Serializable]
    public class StringReference : ScriptableVariableReference<string>
    {
        [SerializeField]
        private ScriptableString _variable;

        protected override ScriptableVariable<string> GetScriptableVariable() => _variable;
    }
    
    [Serializable]
    public class BoolReference : ScriptableVariableReference<bool>
    {
        [SerializeField]
        private ScriptableBool _variable;

        protected override ScriptableVariable<bool> GetScriptableVariable() => _variable;
    }
    
    /// <summary>
    /// Provides a designer-friendly way to reference either a local value or a shared ScriptableVariable value.
    /// Set T to the type of variable you want to reference.
    /// </summary>
    public abstract class ScriptableVariableReference<T>
    {
        [SerializeField]
        private VariableValueSource _valueSource;
        
        [SerializeField]
        private T _localValue;

        /// <summary>
        /// Whether this references a local value or a shared one.
        /// </summary>
        public bool IsLocal
        {
            get => _valueSource == VariableValueSource.Local;
            set => _valueSource = value ? VariableValueSource.Local : VariableValueSource.Shared;
        }
        
        /// <summary>
        /// The source of the variable's value.
        /// </summary>
        public VariableValueSource ValueSource => _valueSource;

        /// <summary>
        /// The local value used when IsLocal is true.
        /// </summary>
        public T LocalValue
        {
            get => _localValue;
            set => _localValue = value;
        }
        
        /// <summary>
        /// The shared scriptable variable when the value source is shared.
        /// </summary>
        public ScriptableVariable<T> SharedVariable => GetScriptableVariable();

        /// <summary>
        /// The value of the variable reference, either local or shared based on IsLocal.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsLocal)
                    return (_localValue);
                
                ScriptableVariable<T> variable = GetScriptableVariable();
                
                ThrowIf.SystemObjectNull(variable);

                return (variable.GetValue());
            }
            set
            {
                if (IsLocal)
                {
                    _localValue = value;
                }
                else
                {
                    ScriptableVariable<T> variable = GetScriptableVariable();
                    
                    ThrowIf.SystemObjectNull(variable);
                    
                    variable.SetValue(value);
                }
            }
        }

        protected abstract ScriptableVariable<T> GetScriptableVariable();

        public static implicit operator T(ScriptableVariableReference<T> reference)
        {
            return reference.Value;
        }
    }
}
