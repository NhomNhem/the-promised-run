using System.Globalization;
using OpenUtility.Data;
using TMPro;
using UnityEngine;

namespace OpenUtility
{
    [ScriptableVariableBinder(typeof(TMP_Text), typeof(float), BindingGoal.DetermineValue, DisplayName = "Default Decimal Variable")]
    public class DefaultDecimalTextEventBinding : DecimalTextEventBinding
    {
        [Header("Optional")]
        [SerializeField]
        private Optional<string> _format;
        
        protected override string ConvertDecimalToText(float newValue)
        {
            return (_format.HasValue ? newValue.ToString(_format.Value) : newValue.ToString(CultureInfo.InvariantCulture));
        }
    }
}
