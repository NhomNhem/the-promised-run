using System;

namespace OpenUtility.Data
{
    /// <summary>
    /// The goal of the binding - either to receive a value from the component or to determine the value of the component.
    /// </summary>
    [Flags]
    public enum BindingGoal
    {
        None = 0,
        
        /// <summary>
        /// The goal of the binding is to only receive a value from the component (e.g. get the text from an InputField).
        /// </summary>
        ReceiveValue = 1 << 0,
        
        /// <summary>
        /// The goal of the binding is to only determine the value of the component (e.g. set the text of an InputField).
        /// </summary>
        DetermineValue = 1 << 1,
        
        /// <summary>
        /// The goal of the binding is to both receive and determine the value of the component.
        /// </summary>
        ReceiveAndDetermineValue = ReceiveValue | DetermineValue,
    }
    
    /// <summary>
    /// Use to mark a binding class for a ScriptableVariableBinding type. Add the type of component
    /// to bind to in the constructor (e.g. TMP_InputField).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ScriptableVariableBinder : Attribute
    {
        public Type TypeOfComponentToBindTo { get; }
        public Type TypeOfValue { get; }
        
        public BindingGoal Goal { get; }
        
        /// <summary>
        /// A display name for the binding option in the inspector. Should be unique compered to other display names.
        /// </summary>
        public string DisplayName { get; set; }

        public Type TypeOfScriptableVariable
        {
            get
            {
                if (_customScriptableVariableType != null)
                    return (_customScriptableVariableType);

                if (TypeOfValue == typeof(int))
                    return (typeof(ScriptableInt));

                if (TypeOfValue == typeof(float))
                    return (typeof(ScriptableFloat));

                if (TypeOfValue == typeof(string))
                    return (typeof(ScriptableString));

                throw new NotImplementedException($"No default scriptable variable implementation available for type {TypeOfValue.Name}. Make sure to the the 'typeOfScriptableVariable' parameter.");
            }
        }

        private readonly Type _customScriptableVariableType;
        
        /// <summary>
        /// Constructor for ScriptableVariableBinder attribute.
        /// </summary>
        /// <param name="typeOfComponentToBindTo">The component the binder is used on (e.g. Slider)</param>
        /// <param name="typeOfValue">The type of value that is being converted to (e.g. int)</param>
        public ScriptableVariableBinder(Type typeOfComponentToBindTo, Type typeOfValue) : this(typeOfComponentToBindTo, typeOfValue, null, BindingGoal.ReceiveAndDetermineValue)
        {
        }
        
        /// <summary>
        /// Constructor for ScriptableVariableBinder attribute.
        /// </summary>
        /// <param name="typeOfComponentToBindTo">The component the binder is used on (e.g. Slider)</param>
        /// <param name="typeOfValue">The type of value that is being converted to (e.g. int)</param>
        /// <param name="goal">The goal of the binding.</param>
        public ScriptableVariableBinder(Type typeOfComponentToBindTo, Type typeOfValue, BindingGoal goal) : this(typeOfComponentToBindTo, typeOfValue, null, goal)
        {
        }
        
        /// <summary>
        /// Constructor for ScriptableVariableBinder attribute.
        /// </summary>
        /// <param name="typeOfComponentToBindTo">The component the binder is used on (e.g. Slider)</param>
        /// <param name="typeOfValue">The type of value that is being converted to (e.g. int)</param>
        /// <param name="customScriptableVariableType">The type of scriptable variable. Set this when you are using a custom type of scriptable variable.</param>
        /// <param name="goal">The goal of the binding.</param>
        public ScriptableVariableBinder(Type typeOfComponentToBindTo, Type typeOfValue, Type customScriptableVariableType, BindingGoal goal)
        {
            TypeOfComponentToBindTo = typeOfComponentToBindTo;
            TypeOfValue = typeOfValue;
            Goal = goal;
            _customScriptableVariableType = customScriptableVariableType;
        }
    }
}
